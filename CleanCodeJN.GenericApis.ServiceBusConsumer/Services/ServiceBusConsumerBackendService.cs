using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Services;
public class ServiceBusConsumerBackendService(
    IServiceScopeFactory scopeFactory,
    IServiceBusConsumerConfigurationService service) : BackgroundService
{
    private readonly List<ServiceBusProcessor> _processors = [];
    private ServiceBusClient _client;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (service.IsLocalEnvironment())
        {
            while (true)
            {
                service.PrintLogoForDebugging();

                Console.WriteLine(service.PrintStartTextForDebugging());

                var eventMessage = new StringBuilder();
                var line = Console.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    eventMessage.Append(line);
                    line = Console.ReadLine();
                }

                using var scope = scopeFactory.CreateScope();
                var commandBus = scope.ServiceProvider.GetService<IMediator>();

                var response = await commandBus.Send(CreateRequest(eventMessage.ToString()));
            }
        }
        else
        {
            var serviceBusTopicConfigurations = service.GetServiceBusTopicConfiguration();

            _client = new ServiceBusClient(serviceBusTopicConfigurations.ConnectionString);

            foreach (var topicConfig in serviceBusTopicConfigurations.TopicConfigurations)
            {
                var processor = _client.CreateProcessor(topicConfig.Name, topicConfig.SubscriptionName, new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock,
                    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(topicConfig.MaxAutoLockRenewalDurationInMinutes),
                    PrefetchCount = topicConfig.PrefetchCount,
                    MaxConcurrentCalls = topicConfig.MaxConcurrentCalls,
                });

                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                _processors.Add(processor);
            }

            try
            {
                foreach (var processor in _processors)
                {
                    await processor.StartProcessingAsync();
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
                }

                foreach (var processor in _processors)
                {
                    await processor.StopProcessingAsync();
                }
            }
            finally
            {
                foreach (var processor in _processors)
                {
                    await processor.DisposeAsync();
                }

                await _client.DisposeAsync();
            }
        }
    }

    public async Task MessageHandler(ProcessMessageEventArgs args)
    {
        using var scope = scopeFactory.CreateScope();

        var response = new Response(ResultEnum.FAILURE_BAD_REQUEST);
        var commandBus = scope.ServiceProvider.GetService<IMediator>();
        var body = args.Message.Body.ToString();
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(body);
        var topic = jsonElement.GetProperty("Topic").GetString();
        var name = jsonElement.GetProperty("Name").GetString();
        var instanceId = jsonElement.GetProperty("InstanceId").GetString();

        try
        {
            service.LogIncomingEvent(name, body);

            response = await commandBus.Send(CreateRequest(body));

            service.LogExecutionResponse(body, response);

            if (!response.Succeeded)
            {
                await Retry(args, commandBus, body, topic, response, customDelay: response.Delay);
            }
        }
        catch (Exception e)
        {
            await Retry(args, commandBus, body, topic, response, e);
        }

        await args.CompleteMessageAsync(args.Message);
    }

    public async Task ErrorHandler(ProcessErrorEventArgs args) => await service.LogAndHandleException(args.Exception, args.Exception.ToString());

    private async Task Retry(ProcessMessageEventArgs args, IMediator commandBus, string body, string topic, Response response, Exception exception = null, TimeSpan? customDelay = null)
    {
        service.LogExecutionResponse(body, response, exception);

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(body);
        var delay = customDelay.HasValue ? DateTime.UtcNow.Add(customDelay.Value) :
            DateTime.UtcNow.AddMinutes(Convert.ToInt32(service.GetServiceBusTopicConfiguration().RetryDelayInMinutes));

        var delayRequest = new SendEventRequest(jsonElement, topic, delay);

        delayRequest.Event.RetryCount++;
        delayRequest.Event.InstanceId = delayRequest.Event.InstanceId.TrimInstanceId() + "_" + delayRequest.Event.RetryCount;

        if (delayRequest.Event.RetryCount < Convert.ToInt32(service.GetServiceBusTopicConfiguration().MaxRetryCount))
        {
            await commandBus.Send(delayRequest);
        }
        else
        {
            await args.DeadLetterMessageAsync(args.Message, service.MaxRetryMessage(args));
            service.LogMaxRetryReached(args);
        }
    }

    private IRequest<Response> CreateRequest(string body)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(body);
        var type = jsonElement.GetProperty("Type").GetString();
        var instanceId = jsonElement.GetProperty("InstanceId").GetString();

        return (IRequest<Response>)Activator
            .CreateInstance(service.GetCommandAssemblies()
            .SelectMany(x => x.GetTypes())
            .First(x => x.FullName == type), [jsonElement]);
    }
}

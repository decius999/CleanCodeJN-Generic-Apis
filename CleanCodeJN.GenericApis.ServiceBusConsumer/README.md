# Generic Web Apis for Service Bus - Extension Package
### Service Bus Consumer with IOSP and Command Pattern

> _This CleanCodeJN package for Service Bus simplifies the development of asynchronous microservices by providing a framework that leverages the power of MediatR and IOSP to consume service bus events from topics and execute commands to process these events._

### Features

- Built in Service Bus Consumer for multiple configurable Topics and Subscriptions
- Uses Mediator and IOSP to execute Commands for processing Events
- Built in Retry concept
- Custom Configuration implementation required for event logging and configuration management
- On latest .NET 9.0

### How to use

- Implement IServiceBusConsumerConfigurationService
- Add AddCleanCodeJNServiceBusConsumer<TServiceBusConsumerConfigurationService, TDataContext> to your Program.cs
- Add Service Bus Configuration to your appsettings.json and your Configuration classes
- Consume Events by just posting events to the Service Bus

# Step by step explanation

__Implement the full IServiceBusConsumerConfigurationService__
```C#
public class SampleServiceBusConsumerConfigurationService(
    IOptionsMonitor<SampleConfiguration> configuration,
    ILogger<SampleServiceBusConsumerConfigurationService> logger) : IServiceBusConsumerConfigurationService
{
    public virtual bool IsLocalEnvironment() => Environment.GetEnvironmentVariable("IS_LOCAL")?.Equals("true") ?? false;

    public virtual void PrintLogoForDebugging() => StringExtensions.PrintLogo();

    public virtual string PrintStartTextForDebugging() => "Please add the event as JSON and press ENTER twice.";

    public virtual string GetServiceBusConnectionString() => configuration.CurrentValue.ServiceBus.ConnectionString;

    public virtual ServiceBusConfiguration GetServiceBusTopicConfiguration() => configuration.CurrentValue.ServiceBus;

    public virtual void LogIncomingEvent(string name, string body) => logger.LogDebug($"EventRequest_{name.Replace(" ", string.Empty)}", body);

    public virtual string MaxRetryMessage(ProcessMessageEventArgs args) => "Max Retry reached";

    public virtual void LogMaxRetryReached(ProcessMessageEventArgs args) => logger.LogCritical(message: MaxRetryMessage(args));

    public virtual List<Assembly> GetCommandAssemblies() => [typeof(UpdateInvoiceEventRequest).Assembly];

    public virtual Task LogAndHandleException(Exception exception, string message)
    {
        logger.LogCritical(exception, message);
        return Task.CompletedTask;
    }

    public virtual void LogExecutionResponse(string body, Response response, Exception exception = null) =>
        logger.LogDebug($"EventResponse_{JsonSerializer.Deserialize<JsonElement>(body).GetProperty("Name").GetString().Replace(" ", string.Empty)}_{(response.Succeeded ? "Success" : "Failure")}", new Dictionary<string, string>
    {
        { nameof(response.Succeeded), response.Succeeded.ToString() },
        { nameof(response.Message), response.Message ?? exception?.Message },
        { nameof(response.Info), response.Info },
        { nameof(exception), exception?.StackTrace },
        { "data", body }
    });
}
```

__Or derive from ServiceBusConsumerConfigurationServiceBase for using reasonable defaults and only override the following methods__
```C#
public class SampleServiceBusConsumerConfigurationService(
    IOptionsMonitor<SampleConfiguration> configuration,
    ILogger<SampleServiceBusConsumerConfigurationService> logger) : ServiceBusConsumerConfigurationServiceBase(logger)
{
    public override ServiceBusConfiguration GetServiceBusTopicConfiguration() => configuration.CurrentValue.ServiceBus;

    public override List<Assembly> GetCommandAssemblies() => [typeof(UpdateInvoiceEventRequest).Assembly];
}
```

__Add AddCleanCodeJNServiceBusConsumer<TServiceBusConsumerConfigurationService, TDataContext> to your Program.cs__
```C#
builder.Services.RegisterServiceBusConsumer<SampleServiceBusConsumerConfigurationService>(
   builder.Configuration["ServiceBus:ConnectionString"],
   [typeof(UpdateInvoiceEventRequest).Assembly]);

builder.Services.AddCleanCodeJNServiceBusConsumer<SampleServiceBusConsumerConfigurationService, MyDbContext>(options =>
{
    options.ApplicationAssemblies =
    [
        Assembly.GetExecutingAssembly(),
    ];
    options.ServiceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"];
});
```

__Add Service Bus Configuration to your appsettings.json and your Configuration classes__
```Json
 {
    "ServiceBus": {
            "MaxRetryCount": 30,
            "RetryDelayInMinutes": 10,
            "ConnectionString": "",
            "TopicConfigurations": [
                {
                    "Name": "topicA",
                    "SubscriptionName": "general",
                    "MaxAutoLockRenewalDurationInMinutes": 15,
                    "PrefetchCount": 10,
                    "MaxConcurrentCalls": 5
                },
                {
                    "Name": "topicB",
                    "SubscriptionName": "general",
                    "MaxAutoLockRenewalDurationInMinutes": 15,
                    "PrefetchCount": 10,
                    "MaxConcurrentCalls": 5
                }
            ]
        }
 }
```

__Consume Events by just posting events to the Service Bus__
```Json
{
    "InstanceId": "<Guid>",
    "Name": "Event Name",
    "Type": "CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands.UpdateInvoiceEventRequest",
    "Environment": "Production",
    "CreatedAt": "2024-03-25 16:05:50",
    "CreatedFrom": "Event Producer",
    "RequestId": "Businnss process Id",
    "RetryCount": 0,
    "Topic": "invoices",
    "Data": 
    {
        // Payload
    }
}
```

__If you want to use the Generic Apis Commands and Repositories together with the Service Bus Consumer, than register everything as below__
```C#
var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SampleConfiguration>(builder.Configuration);

builder.Services.AddCleanCodeJNServiceBusConsumer<SampleServiceBusConsumerConfigurationService, MyDbContext>(options =>
{
    options.ApplicationAssemblies =
    [
        typeof(CleanCodeJN.GenericApis.Sample.Business.AssemblyRegistration).Assembly,
        typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly,
        typeof(CleanCodeJN.GenericApis.Sample.Domain.AssemblyRegistration).Assembly,
        Assembly.GetExecutingAssembly(),
    ];
    options.ServiceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"];
});

await builder.Build().RunAsync();
```

# Link to Sample Code
[Sample Project](https://github.com/decius999/CleanCodeJN-Generic-Apis/tree/dev/CleanCodeJN.GenericApis.ServiceBusConsumer.Sample)

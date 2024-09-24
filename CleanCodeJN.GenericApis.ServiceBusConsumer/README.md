# Generic Web Apis for Service Bus - Extension Package
### Service Bus Consumer with IOSP and Command Pattern

> _This CleanCodeJN package for Service Bus simplifies the development of asynchronous microservices by providing a framework that leverages the power of MediatR and IOSP to consume service bus events from topics and execute commands to process these events._

### Features

- Built in Service Bus Consumer for multiple configurable Topics and Subscriptions
- Uses Mediator and IOSP to execute Commands for processing Events
- Built in Retry concept
- Custom Configuration implementation required for event logging and configuration management
- On latest .NET 8.0

### How to use

- Implement IServiceBusConsumerConfigurationService
- Add RegisterServiceBusConsumer<YourServiceBusConsumerConfigurationService>() to your Program.cs
- Add Service Bus Configuration to your appsettings.json and your Configuration classes
- Consume Events by just posting events to the Service Bus

# Step by step explanation

__Implement IServiceBusConsumerConfigurationService__
```C#
public class ServiceBusConsumerConfigurationService(
    IOptionsMonitor<SampleConfiguration> configuration,
    ILogger<ServiceBusConsumerConfigurationService> logger) : IServiceBusConsumerConfigurationService
{
    public virtual bool IsLocalEnvironment() => Debugger.IsAttached;

    public virtual void PrintLogoForDebugging() => StringExtensions.PrintLogo();

    public virtual string PrintStartTextForDebugging() => "Please add the event as JSON and press ENTER twice.";

    public virtual string GetServiceBusConnectionString() => configuration.CurrentValue.ServiceBusConnectionString;

    public virtual ServiceBusConfiguration GetServiceBusTopicConfiguration() => configuration.CurrentValue.ServiceBus;

    public virtual void LogIncomingEvent(string name, string body) => logger.LogDebug(EventRequest(name), body);

    public virtual string MaxRetryMessage(ProcessMessageEventArgs args) => "Max Retry reached";

    public virtual void LogMaxRetryReached(ProcessMessageEventArgs args) => logger.LogCritical(message: "Max Retry reached");

    public List<Assembly> GetCommandAssemblies() => [typeof(SendEventRequest).Assembly];

    public virtual Task LogAndHandleException(Exception exception, string message)
    {
        logger.LogCritical(exception, message);
        return Task.CompletedTask;
    }

    public virtual void LogExecutionResponse(string body, Response response, Exception exception = null) =>
        logger.LogDebug(EventResponse(body, response), new Dictionary<string, string>
    {
        { nameof(response.Succeeded), response.Succeeded.ToString() },
        { nameof(response.Message), response.Message ?? exception?.Message },
        { nameof(response.Info), response.Info },
        { nameof(exception), exception?.StackTrace },
        { "data", body }
    });

    private static string EventResponse(string body, Response response) => $"EventResponse_{JsonSerializer.Deserialize<JsonElement>(body).GetProperty("Name").GetString().Replace(" ", string.Empty)}_{(response.Succeeded ? "Success" : "Failure")}";

    private static string EventRequest(string name) => $"EventRequest_{name.Replace(" ", string.Empty)}";
}

```

__Add RegisterServiceBusConsumer<YourServiceBusConsumerConfigurationService>() to your Program.cs__
```C#
builder.Services.RegisterServiceBusConsumer<SampleServiceBusConsumerConfigurationService>(
   "serviceBusConnectionString", 
   [Assembly.GetExecutingAssembly()]);
```

__Add Service Bus Configuration to your appsettings.json and your Configuration classes__
```Json
 {
    "ServiceBus": {
            "MaxRetryCount": 30,
            "RetryDelayInMinutes": 10,
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
    "Type": "Api.Business.EventCommands.XYZIntegrationEventRequest",
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

# Link to Sample Code
[Sample Project](https://github.com/decius999/CleanCodeJN-Generic-Apis/tree/dev/CleanCodeJN.GenericApis.ServiceBusConsumer.Sample)

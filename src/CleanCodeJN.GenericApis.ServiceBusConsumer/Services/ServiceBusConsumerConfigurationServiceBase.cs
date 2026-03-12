using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
using Microsoft.Extensions.Logging;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Services;
public abstract class ServiceBusConsumerConfigurationServiceBase(ILogger logger) : IServiceBusConsumerConfigurationService
{
    public virtual bool IsLocalEnvironment() => Environment.GetEnvironmentVariable("IS_LOCAL")?.Equals("true") ?? false;

    public virtual void PrintLogoForDebugging() => StringExtensions.PrintLogo();

    public virtual string PrintStartTextForDebugging() => "Please add the event as JSON and press ENTER twice.";

    public virtual ServiceBusConfiguration GetServiceBusTopicConfiguration() => null;

    public virtual void LogIncomingEvent(string name, string body) => logger.LogDebug($"EventRequest_{name.Replace(" ", string.Empty)}", body);

    public virtual string MaxRetryMessage(ProcessMessageEventArgs args) => "Max Retry reached";

    public virtual void LogMaxRetryReached(ProcessMessageEventArgs args) => logger.LogCritical(message: MaxRetryMessage(args));

    public virtual List<Assembly> GetCommandAssemblies() => [];

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

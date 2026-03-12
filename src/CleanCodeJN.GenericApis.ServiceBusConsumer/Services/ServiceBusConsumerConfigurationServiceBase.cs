using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
using Microsoft.Extensions.Logging;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Services;

/// <summary>
/// Base class providing default implementations of <see cref="IServiceBusConsumerConfigurationService"/> that can be overridden by application-specific subclasses.
/// </summary>
public abstract class ServiceBusConsumerConfigurationServiceBase(ILogger logger) : IServiceBusConsumerConfigurationService
{
    /// <summary>
    /// Determines whether the application is running locally by checking the IS_LOCAL environment variable.
    /// </summary>
    /// <returns><c>true</c> if IS_LOCAL equals "true"; otherwise <c>false</c>.</returns>
    public virtual bool IsLocalEnvironment() => Environment.GetEnvironmentVariable("IS_LOCAL")?.Equals("true") ?? false;

    /// <summary>
    /// Prints the ASCII art logo to the console for use during local debugging.
    /// </summary>
    public virtual void PrintLogoForDebugging() => StringExtensions.PrintLogo();

    /// <summary>
    /// Returns the default prompt text shown to the developer when running in local mode.
    /// </summary>
    /// <returns>A string instructing the user to paste a JSON event and press ENTER twice.</returns>
    public virtual string PrintStartTextForDebugging() => "Please add the event as JSON and press ENTER twice.";

    /// <summary>
    /// Returns <c>null</c> by default; override to supply the application's <see cref="ServiceBusConfiguration"/>.
    /// </summary>
    /// <returns>The <see cref="ServiceBusConfiguration"/> for the application, or <c>null</c> if not configured.</returns>
    public virtual ServiceBusConfiguration GetServiceBusTopicConfiguration() => null;

    /// <summary>
    /// Logs the name and body of an incoming Service Bus event at debug level.
    /// </summary>
    /// <param name="name">The name of the incoming event.</param>
    /// <param name="body">The raw JSON body of the message.</param>
    public virtual void LogIncomingEvent(string name, string body) => logger.LogDebug($"EventRequest_{name.Replace(" ", string.Empty)}", body);

    /// <summary>
    /// Returns the default dead-letter reason message when the maximum retry count has been reached.
    /// </summary>
    /// <param name="args">The <see cref="ProcessMessageEventArgs"/> for the failed message.</param>
    /// <returns>The string "Max Retry reached".</returns>
    public virtual string MaxRetryMessage(ProcessMessageEventArgs args) => "Max Retry reached";

    /// <summary>
    /// Logs a critical message when a Service Bus message has exhausted all retry attempts.
    /// </summary>
    /// <param name="args">The <see cref="ProcessMessageEventArgs"/> for the failed message.</param>
    public virtual void LogMaxRetryReached(ProcessMessageEventArgs args) => logger.LogCritical(message: MaxRetryMessage(args));

    /// <summary>
    /// Returns an empty list by default; override to provide the assemblies containing MediatR command handlers.
    /// </summary>
    /// <returns>An empty list of <see cref="Assembly"/> objects.</returns>
    public virtual List<Assembly> GetCommandAssemblies() => [];

    /// <summary>
    /// Logs the exception at critical level and completes the task.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">A contextual message describing the failure.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public virtual Task LogAndHandleException(Exception exception, string message)
    {
        logger.LogCritical(exception, message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs the event processing outcome, including response status and optional exception details, at debug level.
    /// </summary>
    /// <param name="body">The raw JSON body of the processed message.</param>
    /// <param name="response">The <see cref="Response"/> returned by the handler.</param>
    /// <param name="exception">An optional exception that occurred during processing.</param>
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

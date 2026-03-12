using System.Reflection;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;

/// <summary>
/// Defines the configuration and logging contract required by the Service Bus consumer infrastructure.
/// </summary>
public interface IServiceBusConsumerConfigurationService
{
    /// <summary>
    /// Determines whether the application is currently running in a local development environment.
    /// </summary>
    /// <returns><c>true</c> if the IS_LOCAL environment variable is set to "true"; otherwise <c>false</c>.</returns>
    bool IsLocalEnvironment();

    /// <summary>
    /// Prints the application logo to the console for use during local debugging sessions.
    /// </summary>
    void PrintLogoForDebugging();

    /// <summary>
    /// Returns the prompt text displayed to the developer when the consumer is running in local debug mode.
    /// </summary>
    /// <returns>A string containing the start prompt for manual event input.</returns>
    string PrintStartTextForDebugging();

    /// <summary>
    /// Returns the <see cref="ServiceBusConfiguration"/> containing connection and topic settings.
    /// </summary>
    /// <returns>The configured <see cref="ServiceBusConfiguration"/> instance.</returns>
    ServiceBusConfiguration GetServiceBusTopicConfiguration();

    /// <summary>
    /// Logs an incoming Service Bus event message.
    /// </summary>
    /// <param name="name">The name of the event being received.</param>
    /// <param name="body">The raw JSON body of the message.</param>
    void LogIncomingEvent(string name, string body);

    /// <summary>
    /// Logs the outcome of processing an event, including optional exception details.
    /// </summary>
    /// <param name="body">The raw JSON body of the processed message.</param>
    /// <param name="response">The <see cref="Response"/> returned by the handler.</param>
    /// <param name="exception">An optional exception that occurred during processing.</param>
    void LogExecutionResponse(string body, Response response, Exception exception = null);

    /// <summary>
    /// Logs a critical exception and performs any additional error handling, such as alerting or persistence.
    /// </summary>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="message">A contextual message describing the failure.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogAndHandleException(Exception exception, string message);

    /// <summary>
    /// Logs a critical entry when the maximum retry count for a message has been reached.
    /// </summary>
    /// <param name="args">The <see cref="ProcessMessageEventArgs"/> associated with the failed message.</param>
    void LogMaxRetryReached(ProcessMessageEventArgs args);

    /// <summary>
    /// Builds the dead-letter reason message used when a message exceeds the maximum retry count.
    /// </summary>
    /// <param name="args">The <see cref="ProcessMessageEventArgs"/> associated with the failed message.</param>
    /// <returns>A string describing why the message was dead-lettered.</returns>
    string MaxRetryMessage(ProcessMessageEventArgs args);

    /// <summary>
    /// Returns the list of assemblies that contain MediatR command handlers to be registered and resolved during event processing.
    /// </summary>
    /// <returns>A list of <see cref="Assembly"/> objects containing command handler implementations.</returns>
    List<Assembly> GetCommandAssemblies();
}

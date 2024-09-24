﻿using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Services;
public class SampleServiceBusConsumerConfigurationService(
    IOptionsMonitor<SampleConfiguration> configuration,
    ILogger<SampleServiceBusConsumerConfigurationService> logger) : IServiceBusConsumerConfigurationService
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

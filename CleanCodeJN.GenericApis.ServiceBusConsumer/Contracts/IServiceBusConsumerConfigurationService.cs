using System.Reflection;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
public interface IServiceBusConsumerConfigurationService
{
    bool IsLocalEnvironment();

    void PrintLogoForDebugging();

    string PrintStartTextForDebugging();

    ServiceBusConfiguration GetServiceBusTopicConfiguration();

    void LogIncomingEvent(string name, string body);

    void LogExecutionResponse(string body, Response response, Exception exception = null);

    Task LogAndHandleException(Exception exception, string message);

    void LogMaxRetryReached(ProcessMessageEventArgs args);

    string MaxRetryMessage(ProcessMessageEventArgs args);

    List<Assembly> GetCommandAssemblies();
}

using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Services;

/// <summary>
/// Creates and caches <see cref="ServiceBusSender"/> instances per topic name, reusing them across calls.
/// </summary>
public class ServiceBusSenderCreator(IServiceBusConsumerConfigurationService configurationService) : IServiceBusSenderCreator
{
    private readonly ConcurrentDictionary<string, ServiceBusSender> clientDict = new();

    /// <summary>
    /// Returns a cached <see cref="ServiceBusSender"/> for the specified topic, creating one if it does not yet exist.
    /// </summary>
    /// <param name="topicName">The name of the Service Bus topic to send messages to.</param>
    /// <returns>A <see cref="ServiceBusSender"/> for the given topic.</returns>
    public ServiceBusSender GetOrCreateSender(string topicName)
    {
        if (!clientDict.ContainsKey(topicName))
        {
            var client = new ServiceBusClient(configurationService.GetServiceBusTopicConfiguration().ConnectionString);
            clientDict[topicName] = client.CreateSender(topicName);
        }

        return clientDict[topicName];
    }
}

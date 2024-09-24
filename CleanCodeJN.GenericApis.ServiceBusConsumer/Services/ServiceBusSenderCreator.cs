using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Services;
public class ServiceBusSenderCreator(IServiceBusConsumerConfigurationService configurationService) : IServiceBusSenderCreator
{
    private readonly ConcurrentDictionary<string, ServiceBusSender> clientDict = new();

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

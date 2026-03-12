using Azure.Messaging.ServiceBus;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;
public interface IServiceBusSenderCreator
{
    ServiceBusSender GetOrCreateSender(string topicName);
}

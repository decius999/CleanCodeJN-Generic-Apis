using Azure.Messaging.ServiceBus;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;

/// <summary>
/// Defines a factory for obtaining or creating <see cref="ServiceBusSender"/> instances keyed by topic name.
/// </summary>
public interface IServiceBusSenderCreator
{
    /// <summary>
    /// Returns an existing <see cref="ServiceBusSender"/> for the specified topic, or creates and caches a new one if none exists.
    /// </summary>
    /// <param name="topicName">The name of the Service Bus topic.</param>
    /// <returns>A <see cref="ServiceBusSender"/> configured for the given topic.</returns>
    ServiceBusSender GetOrCreateSender(string topicName);
}

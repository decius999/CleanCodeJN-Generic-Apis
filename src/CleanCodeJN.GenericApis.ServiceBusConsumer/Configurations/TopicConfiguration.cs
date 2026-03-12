namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;

/// <summary>
/// Represents the configuration for a single Service Bus topic subscription.
/// </summary>
public class TopicConfiguration
{
    /// <summary>
    /// Gets or sets the name of the Service Bus topic.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the subscription name used to receive messages from the topic.
    /// </summary>
    public string SubscriptionName { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration in minutes for automatically renewing the message lock.
    /// </summary>
    public int MaxAutoLockRenewalDurationInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the number of messages to prefetch from the Service Bus topic.
    /// </summary>
    public int PrefetchCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent message processing calls allowed.
    /// </summary>
    public int MaxConcurrentCalls { get; set; }
}

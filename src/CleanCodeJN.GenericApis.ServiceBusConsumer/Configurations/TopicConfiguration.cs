namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
public class TopicConfiguration
{
    public string Name { get; set; }

    public string SubscriptionName { get; set; }

    public int MaxAutoLockRenewalDurationInMinutes { get; set; }

    public int PrefetchCount { get; set; }

    public int MaxConcurrentCalls { get; set; }
}

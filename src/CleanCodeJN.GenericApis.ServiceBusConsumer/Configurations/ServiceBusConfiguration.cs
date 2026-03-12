namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;
public class ServiceBusConfiguration
{
    public int MaxRetryCount { get; set; }

    public int RetryDelayInMinutes { get; set; }

    public string ConnectionString { get; set; }

    public List<TopicConfiguration> TopicConfigurations { get; set; }
}

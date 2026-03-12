namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Configurations;

/// <summary>
/// Holds the configuration settings required to connect to and consume messages from Azure Service Bus.
/// </summary>
public class ServiceBusConfiguration
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts before a message is dead-lettered.
    /// </summary>
    public int MaxRetryCount { get; set; }

    /// <summary>
    /// Gets or sets the delay in minutes between consecutive retry attempts.
    /// </summary>
    public int RetryDelayInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the Azure Service Bus connection string.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the list of topic configurations that the consumer should subscribe to.
    /// </summary>
    public List<TopicConfiguration> TopicConfigurations { get; set; }
}

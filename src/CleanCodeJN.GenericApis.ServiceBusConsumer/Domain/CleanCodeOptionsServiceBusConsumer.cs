using CleanCodeJN.GenericApis.Extensions;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Domain;

/// <summary>
/// Configuration options for the CleanCode JN Service Bus Consumer package, extending the base <see cref="CleanCodeOptions"/>.
/// </summary>
public class CleanCodeOptionsServiceBusConsumer : CleanCodeOptions
{
    /// <summary>
    /// Gets or sets the Azure Service Bus connection string used to connect to the message broker.
    /// </summary>
    public string ServiceBusConnectionString { get; set; }
}

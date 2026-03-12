namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;

/// <summary>
/// Represents the data payload of an invoice update event received from the service bus.
/// </summary>
public class UpdateInvoiceEventModel
{
    /// <summary>
    /// Gets or sets the identifier of the customer referenced by this event.
    /// </summary>
    public int CustomerId { get; set; }
}

using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

/// <summary>
/// Data transfer object used when updating an existing invoice via the API.
/// </summary>
public class InvoicePutDto : IDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the invoice to update.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer associated with this invoice.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the updated monetary amount of the invoice.
    /// </summary>
    public decimal Amount { get; set; }
}

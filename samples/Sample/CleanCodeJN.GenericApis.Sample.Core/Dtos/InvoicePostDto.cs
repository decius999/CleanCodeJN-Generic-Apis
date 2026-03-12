using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

/// <summary>
/// Data transfer object used when creating a new invoice via the API.
/// </summary>
public class InvoicePostDto : IDto
{
    /// <summary>
    /// Gets or sets the identifier of the customer for whom the invoice is created.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount of the new invoice.
    /// </summary>
    public decimal Amount { get; set; }
}

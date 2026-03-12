using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

/// <summary>
/// Data transfer object used when reading invoice data from the API.
/// </summary>
public class InvoiceGetDto : IDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the invoice.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer associated with this invoice.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer associated with this invoice.
    /// </summary>
    public CustomerGetDto Customer { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount of the invoice.
    /// </summary>
    public decimal Amount { get; set; }
}

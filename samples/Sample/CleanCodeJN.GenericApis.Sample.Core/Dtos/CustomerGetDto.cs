using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

/// <summary>
/// Data transfer object used when reading customer data from the API.
/// </summary>
public class CustomerGetDto : IDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the full name of the customer.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the address information of the customer.
    /// </summary>
    public AddressInfoGetDto AddressInfo { get; set; }

    /// <summary>
    /// Gets or sets the list of invoices belonging to this customer.
    /// </summary>
    public List<InvoiceGetDto> Invoices { get; set; }
}

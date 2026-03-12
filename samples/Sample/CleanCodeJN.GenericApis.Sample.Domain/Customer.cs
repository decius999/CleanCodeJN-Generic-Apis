using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Domain;

/// <summary>
/// Represents a customer entity with address and invoice information.
/// </summary>
public class Customer : IEntity<int>
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
    /// Gets or sets the address information associated with this customer.
    /// </summary>
    public AddressInfo AddressInfo { get; set; }

    /// <summary>
    /// Gets or sets the list of invoices belonging to this customer.
    /// </summary>
    public List<Invoice> Invoices { get; set; }
}

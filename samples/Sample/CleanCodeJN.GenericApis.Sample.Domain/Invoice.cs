using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Domain;

/// <summary>
/// Represents an invoice entity associated with a customer.
/// </summary>
public class Invoice : IEntity<Guid>
{
    /// <summary>
    /// Gets or sets the unique identifier of the invoice.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer who owns this invoice.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer associated with this invoice.
    /// </summary>
    public Customer Customer { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount of the invoice.
    /// </summary>
    public decimal Amount { get; set; }
}

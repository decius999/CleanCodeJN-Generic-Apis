using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Domain;

/// <summary>
/// Represents address information owned by a customer.
/// </summary>
public class AddressInfo : IEntity<int>
{
    /// <summary>
    /// Gets or sets the unique identifier of this address record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer who owns this address.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the street name of the address.
    /// </summary>
    public string Street { get; set; }

    /// <summary>
    /// Gets or sets the house number of the address.
    /// </summary>
    public string HouseNo { get; set; }

    /// <summary>
    /// Gets or sets the postal (ZIP) code of the address.
    /// </summary>
    public string Zip { get; set; }

    /// <summary>
    /// Gets or sets the city name of the address.
    /// </summary>
    public string City { get; set; }
}

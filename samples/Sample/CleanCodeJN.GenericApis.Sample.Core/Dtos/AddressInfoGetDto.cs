using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

/// <summary>
/// Data transfer object used when reading address information from the API.
/// </summary>
public class AddressInfoGetDto : IDto
{
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

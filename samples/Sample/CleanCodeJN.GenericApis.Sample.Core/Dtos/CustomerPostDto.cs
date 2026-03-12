using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

/// <summary>
/// Data transfer object used when creating a new customer via the API.
/// </summary>
public class CustomerPostDto : IDto
{
    /// <summary>Full name of the customer, e.g. 'Acme Corp'. Maximum 100 characters.</summary>
    public string Name { get; set; }

    /// <summary>AdressInfo with street, houseNo, zip and city for customer.</summary>
    public AddressInfoGetDto AddressInfo { get; set; }
}

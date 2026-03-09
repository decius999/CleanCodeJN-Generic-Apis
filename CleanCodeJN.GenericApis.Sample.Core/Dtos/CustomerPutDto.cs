using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

public class CustomerPutDto : IDto
{
    /// <summary>Unique identifier of the customer to update.</summary>
    public int Id { get; set; }

    /// <summary>New full name of the customer, e.g. 'Acme Corp'. Maximum 100 characters.</summary>
    public string Name { get; set; }

    /// <summary>AdressInfo with street, houseNo, zip and city for customer.</summary>
    public AddressInfoGetDto AddressInfo { get; set; }
}

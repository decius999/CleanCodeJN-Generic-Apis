using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;
public class AddressInfoGetDto : IDto
{
    public int CustomerId { get; set; }

    public string Street { get; set; }

    public string HouseNo { get; set; }

    public string Zip { get; set; }

    public string City { get; set; }
}

using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Domain;
public class AddressInfo : IEntity<int>
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Street { get; set; }

    public string HouseNo { get; set; }

    public string Zip { get; set; }

    public string City { get; set; }
}

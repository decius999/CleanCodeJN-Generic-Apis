using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Domain;

public class Customer : IEntity<int>
{
    public int Id { get; set; }

    public string Name { get; set; }

    public AddressInfo AddressInfo { get; set; }

    public List<Invoice> Invoices { get; set; }
}

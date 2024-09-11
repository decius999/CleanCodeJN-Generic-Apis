using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Domain;

public class Invoice : IEntity<Guid>
{
    public Guid Id { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; }

    public decimal Amount { get; set; }
}

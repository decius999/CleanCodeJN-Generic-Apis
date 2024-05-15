using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Dtos;

public class InvoiceGetDto : IDto
{
    public Guid Id { get; set; }

    public int CustomerId { get; set; }

    public CustomerGetDto Customer { get; set; }

    public decimal Amount { get; set; }
}

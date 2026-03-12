using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

public class InvoicePutDto : IDto
{
    public Guid Id { get; set; }

    public int CustomerId { get; set; }

    public decimal Amount { get; set; }
}

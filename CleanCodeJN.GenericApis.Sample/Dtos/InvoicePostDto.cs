using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Dtos;

public class InvoicePostDto : IDto
{
    public int CustomerId { get; set; }

    public decimal Amount { get; set; }
}

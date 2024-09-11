using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

public class CustomerGetDto : IDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<InvoiceGetDto> Invoices { get; set; }
}

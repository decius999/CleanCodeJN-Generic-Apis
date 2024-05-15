using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Dtos;

public class CustomerPutDto : IDto
{
    public int Id { get; set; }

    public string Name { get; set; }
}

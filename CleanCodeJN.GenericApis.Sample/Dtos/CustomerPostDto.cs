using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Dtos;

public class CustomerPostDto : IDto
{
    public string Name { get; set; }
}

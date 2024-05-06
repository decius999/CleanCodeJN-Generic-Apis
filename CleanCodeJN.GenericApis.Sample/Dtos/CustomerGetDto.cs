using CleanCodeJN.GenericApis.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Dtos;

public class CustomerGetDto : IDto
{
    public int Id { get; set; }

    public string Name { get; set; }
}

using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

public class CustomerPostDto : IDto
{
    /// <summary>Full name of the customer, e.g. 'Acme Corp'. Maximum 100 characters.</summary>
    public string Name { get; set; }
}

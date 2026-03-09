using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Sample.Core.Dtos;

public class CustomerPostDto : IDto
{
    /// <summary>Full name of the customer, e.g. 'Acme Corp'. Maximum 100 characters.</summary>
    public string Name { get; set; }

    /// <summary>Street of customer.</summary>
    public string Street { get; set; }

    /// <summary>House no of customer.</summary>
    public string HouseNo { get; set; }

    /// <summary>Zip of customer.</summary>
    public string Zip { get; set; }

    /// <summary>City of customer.</summary>
    public string City { get; set; }
}

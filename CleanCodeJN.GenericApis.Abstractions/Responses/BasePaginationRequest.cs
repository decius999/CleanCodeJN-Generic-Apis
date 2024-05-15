using CleanCodeJN.GenericApis.Abstractions.Models;

namespace CleanCodeJN.GenericApis.Abstractions.Responses;

public class BasePaginationRequest
{
    public string SortField { get; set; }

    public string SortOrder { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; } = 10_000;

    public SearchFilter Filter { get; set; }
}

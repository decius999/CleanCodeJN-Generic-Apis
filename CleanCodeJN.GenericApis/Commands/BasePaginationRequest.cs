using CleanCodeJN.GenericApis.Models;

namespace CleanCodeJN.GenericApis.Commands;

public class BasePaginationRequest
{
    public string SortField { get; set; }

    public string SortOrder { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; } = 10_000;

    public SearchFilter Filter { get; set; }
}

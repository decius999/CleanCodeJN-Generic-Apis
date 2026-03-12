using CleanCodeJN.GenericApis.Abstractions.Models;

namespace CleanCodeJN.GenericApis.Abstractions.Responses;

/// <summary>
/// Provides base pagination, sorting, and filtering parameters for paginated API requests.
/// </summary>
public class BasePaginationRequest
{
    /// <summary>
    /// Gets or sets the name of the field by which results should be sorted.
    /// </summary>
    public string SortField { get; set; }

    /// <summary>
    /// Gets or sets the sort direction, typically "asc" or "desc".
    /// </summary>
    public string SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the number of records to skip before returning results.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of records to return. Defaults to 10,000.
    /// </summary>
    public int Take { get; set; } = 10_000;

    /// <summary>
    /// Gets or sets the search filter to apply when querying results.
    /// </summary>
    public SearchFilter Filter { get; set; }
}

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// The base request for Get commands.
/// </summary>
public abstract class GetBaseRequest
{
    /// <summary>
    /// No tracking for entities.
    /// </summary>
    public bool AsNoTracking { get; init; }

    /// <summary>
    /// No global query filters.
    /// </summary>
    public bool IgnoreQueryFilters { get; init; }

    /// <summary>
    /// Use no joins on queries. Instead separate queries.
    /// </summary>
    public bool AsSplitQuery { get; init; }
}

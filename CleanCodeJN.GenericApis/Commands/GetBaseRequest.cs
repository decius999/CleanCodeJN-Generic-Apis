namespace CleanCodeJN.GenericApis.Commands;

public abstract class GetBaseRequest
{
    public bool AsNoTracking { get; init; }

    public bool IgnoreQueryFilters { get; init; }

    public bool AsSplitQuery { get; init; }
}

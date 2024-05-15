namespace CleanCodeJN.GenericApis.Abstractions.Models;

public class FilterValue
{
    public required string Field { get; init; }

    public required string Value { get; init; }

    public FilterTypeEnum Type { get; init; } = FilterTypeEnum.STRING;
}

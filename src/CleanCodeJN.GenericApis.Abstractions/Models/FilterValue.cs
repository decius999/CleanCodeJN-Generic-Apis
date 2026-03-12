namespace CleanCodeJN.GenericApis.Abstractions.Models;

/// <summary>
/// Represents a single filter criterion consisting of a field name, a value to match, and the data type of the field.
/// </summary>
public class FilterValue
{
    /// <summary>
    /// Gets the name of the field to filter on.
    /// </summary>
    public required string Field { get; init; }

    /// <summary>
    /// Gets the value to match against the specified field.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Gets the data type of the field used for type-aware comparison. Defaults to <see cref="FilterTypeEnum.STRING"/>.
    /// </summary>
    public FilterTypeEnum Type { get; init; } = FilterTypeEnum.STRING;
}

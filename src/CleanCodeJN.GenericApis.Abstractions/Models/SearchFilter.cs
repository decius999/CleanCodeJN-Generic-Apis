using System.Text.Json;

namespace CleanCodeJN.GenericApis.Abstractions.Models;

/// <summary>
/// Represents a composite search filter consisting of multiple <see cref="FilterValue"/> criteria combined by a logical condition.
/// </summary>
public class SearchFilter
{
    /// <summary>
    /// Gets or sets the logical condition used to combine the filter criteria (AND or OR).
    /// </summary>
    public FilterTypeConditionEnum Condition { get; set; }

    /// <summary>
    /// Gets or sets the list of individual filter criteria to apply.
    /// </summary>
    public required List<FilterValue> Filters { get; set; }

    /// <summary>
    /// Serializes this <see cref="SearchFilter"/> instance to a JSON string.
    /// </summary>
    /// <returns>A JSON representation of the current filter.</returns>
    public string ToJson() => JsonSerializer.Serialize(this);
}

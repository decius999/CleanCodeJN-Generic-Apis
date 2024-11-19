using System.Text.Json;

namespace CleanCodeJN.GenericApis.Abstractions.Models;

public class SearchFilter
{
    public FilterTypeConditionEnum Condition { get; set; }

    public required List<FilterValue> Filters { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);
}

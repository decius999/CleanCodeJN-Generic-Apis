using System.Text.Json;

namespace CleanCodeJN.GenericApis.Abstractions.Models;

public class SearchFilter
{
    public required List<FilterValue> Filters { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);
}

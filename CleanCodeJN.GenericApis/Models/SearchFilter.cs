using System.Text.Json;

namespace CleanCodeJN.GenericApis.Models;

public class SearchFilter
{
    public List<FilterValue> Filters { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this);
}

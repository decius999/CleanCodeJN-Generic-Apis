using System.Text.Json.Serialization;

namespace CleanCodeJN.GenericApis.DataGrid.Models;

/// <summary>Raw GraphQL HTTP request body.</summary>
public class GraphQLRequest
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;
}

/// <summary>Top-level GraphQL HTTP response envelope.</summary>
public class GraphQLResponse
{
    [JsonPropertyName("data")]
    public System.Text.Json.JsonElement? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<GraphQLError>? Errors { get; set; }
}

/// <summary>A single GraphQL error entry.</summary>
public class GraphQLError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

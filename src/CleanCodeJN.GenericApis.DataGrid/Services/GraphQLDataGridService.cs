using System.Net.Http.Json;
using System.Text.Json;
using CleanCodeJN.GenericApis.DataGrid.Models;

namespace CleanCodeJN.GenericApis.DataGrid.Services;

/// <summary>
/// Lightweight GraphQL client used by <see cref="Components.CCJNDataGrid{TDto}"/>
/// to execute auto-generated list queries against a CleanCodeJN GraphQL endpoint.
/// </summary>
public class GraphQLDataGridService(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Executes a batched GraphQL query that contains both a list field (<paramref name="entityName"/>)
    /// and a matching count field (<paramref name="entityName"/>Count) in a single HTTP request.
    /// Returns the items and the exact total count provided by the server.
    /// </summary>
    public async Task<(List<T> Items, int TotalCount)> QueryWithCountAsync<T>(
        string endpoint,
        string graphQLQuery,
        string entityName,
        string? bearerToken = null,
        CancellationToken cancellationToken = default)
    {
        using var client = httpClientFactory.CreateClient("CleanCodeJN.DataGrid");

        if (!string.IsNullOrEmpty(bearerToken))
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        var body = new GraphQLRequest { Query = graphQLQuery };

        using var response = await client.PostAsJsonAsync(endpoint, body, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<GraphQLResponse>(JsonOptions, cancellationToken);

        if (envelope?.Errors is { Count: > 0 })
            throw new InvalidOperationException(
                $"GraphQL error: {string.Join("; ", envelope.Errors.Select(e => e.Message))}");

        if (envelope?.Data is null)
            return ([], 0);

        List<T> items = [];
        int totalCount = 0;
        var countKey = $"{entityName}Count";

        foreach (var prop in envelope.Data.Value.EnumerateObject())
        {
            if (prop.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase))
                items = prop.Value.Deserialize<List<T>>(JsonOptions) ?? [];
            else if (prop.Name.Equals(countKey, StringComparison.OrdinalIgnoreCase))
                totalCount = prop.Value.GetInt32();
        }

        return (items, totalCount);
    }
}

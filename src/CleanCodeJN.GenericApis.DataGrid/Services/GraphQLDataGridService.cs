using System.Net.Http.Json;
using System.Text.Json;
using CleanCodeJN.GenericApis.DataGrid.Models;

namespace CleanCodeJN.GenericApis.DataGrid.Services;

/// <summary>
/// Lightweight GraphQL client used by <see cref="Components.CCJNDataGrid{TDto,TPostDto,TPutDto}"/>
/// to execute queries and mutations against a CleanCodeJN GraphQL endpoint.
/// </summary>
public class GraphQLDataGridService(IHttpClientFactory httpClientFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Executes a batched GraphQL query that contains both a list field and a count field in one request.
    /// </summary>
    public async Task<(List<T> Items, int TotalCount)> QueryWithCountAsync<T>(
        string endpoint,
        string graphQLQuery,
        string entityName,
        string bearerToken = null,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(bearerToken);
        var body = new GraphQLRequest { Query = graphQLQuery };

        using var response = await client.PostAsJsonAsync(endpoint, body, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<GraphQLResponse>(JsonOptions, cancellationToken);
        ThrowOnErrors(envelope);

        if (envelope?.Data is null) return ([], 0);

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

    /// <summary>
    /// Executes any GraphQL mutation (create, update, delete). Throws on GraphQL errors.
    /// </summary>
    public async Task ExecuteMutationAsync(
        string endpoint,
        string mutation,
        string bearerToken = null,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(bearerToken);
        var body = new GraphQLRequest { Query = mutation };

        using var response = await client.PostAsJsonAsync(endpoint, body, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<GraphQLResponse>(JsonOptions, cancellationToken);
        ThrowOnErrors(envelope);
    }

    private HttpClient CreateClient(string bearerToken)
    {
        var client = httpClientFactory.CreateClient("CleanCodeJN.DataGrid");
        if (!string.IsNullOrEmpty(bearerToken))
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }

    private static void ThrowOnErrors(GraphQLResponse envelope)
    {
        if (envelope?.Errors is { Count: > 0 })
            throw new InvalidOperationException(
                string.Join("; ", envelope.Errors.Select(e => e.Message)));
    }
}

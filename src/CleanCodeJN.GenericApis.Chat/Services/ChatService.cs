using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CleanCodeJN.GenericApis.Chat.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CleanCodeJN.GenericApis.Chat.Services;

/// <summary>Provides methods for communicating with the AI chat backend, including streaming responses and retrieving available MCP tools.</summary>
public class ChatService(IHttpClientFactory httpClientFactory, IOptions<ChatOptions> options)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Retrieves the list of tools available on the MCP server by sending a "tools/list" JSON-RPC request.</summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A list of <see cref="McpTool"/> instances, or an empty list if the request fails.</returns>
    public async Task<List<McpTool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            var payload = new StringContent(
                JsonSerializer.Serialize(new { jsonrpc = "2.0", id = 1, method = "tools/list", @params = new { } }),
                Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{options.Value.BackendUrl}/mcp", payload, cancellationToken);
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: cancellationToken);
            var toolsArray = json?["result"]?["tools"]?.AsArray();
            if (toolsArray is null) return [];

            return toolsArray
                .Where(t => t != null)
                .Select(t => new McpTool(
                    t!["name"]!.GetValue<string>(),
                    t["description"]?.GetValue<string>()))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>Sends the conversation history to the AI backend and asynchronously streams back <see cref="ChatStreamEvent"/> items as server-sent events.</summary>
    /// <param name="messages">The ordered list of chat messages representing the full conversation history.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous streaming operation.</param>
    /// <returns>An async enumerable of <see cref="ChatStreamEvent"/> items received from the stream.</returns>
    public async IAsyncEnumerable<ChatStreamEvent> SendAsync(
        List<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        var request = new ChatRequest(messages);

        // Build request with browser streaming enabled (required for Blazor WASM SSE)
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{options.Value.BackendUrl}/ai/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };
        httpRequest.SetBrowserResponseStreamingEnabled(true);

        HttpResponseMessage response = null;
        string errorMessage = null;
        try
        {
            response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }

        if (errorMessage is not null)
        {
            yield return new ChatStreamEvent("error", Content: errorMessage);
            yield break;
        }

        if (!response!.IsSuccessStatusCode)
        {
            yield return new ChatStreamEvent("error", Content: $"HTTP {(int)response.StatusCode}");
            yield break;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break; // end of stream
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") break;

            ChatStreamEvent ev = null;
            try { ev = JsonSerializer.Deserialize<ChatStreamEvent>(data, JsonOpts); }
            catch { /* skip malformed chunks */ }

            if (ev is not null)
                yield return ev;
        }
    }

    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient("CleanCodeJNChat");
        if (!string.IsNullOrEmpty(options.Value.BearerToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.BearerToken);
        return client;
    }
}

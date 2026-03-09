using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic.SDK;
using Anthropic.SDK.Common;
using Anthropic.SDK.Messaging;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.Models;
using Microsoft.Extensions.Options;
using Tool = Anthropic.SDK.Common.Tool;

namespace CleanCodeJN.GenericApis.Services;

public class AiProxyService(IOptions<AiProxyOptions> options, IHttpClientFactory httpClientFactory, ILogger<AiProxyService> logger)
{
    public async IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        string bearerToken,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var tools = await FetchMcpToolsAsync(bearerToken, cancellationToken);
        var messages = BuildMessages(request.Messages);
        var anthropic = new AnthropicClient(options.Value.AnthropicApiKey);

        // Agentic loop: keep going as long as Claude requests tool calls
        while (true)
        {
            var parameters = new MessageParameters
            {
                Model = options.Value.Model,
                MaxTokens = options.Value.MaxTokens,
                Messages = messages,
                Tools = tools,
                Stream = true,
            };

            var outputs = new List<MessageResponse>();

            await foreach (var res in anthropic.Messages.StreamClaudeMessageAsync(parameters, cancellationToken))
            {
                logger.LogDebug("Stream chunk: StopReason={StopReason} Delta.Type={DeltaType} Delta.Text={DeltaText} ToolCalls={ToolCallCount}",
                    res.StopReason, res.Delta?.Type, res.Delta?.Text, res.ToolCalls?.Count ?? 0);

                if (res.Delta?.Text is { Length: > 0 } text)
                {
                    yield return new ChatStreamEvent("text", Content: text);
                }

                outputs.Add(res);
            }

            // Deduplicate tool calls by Id — last chunk for metadata (id, name)
            var toolCalls = outputs
                .Where(o => o.ToolCalls is { Count: > 0 })
                .SelectMany(o => o.ToolCalls!)
                .GroupBy(tc => tc.Id)
                .Select(g => g.Last())
                .ToList();

            // Accumulate argument fragments per tool call.
            // The Anthropic streaming API sends arguments as partial JSON strings across
            // multiple chunks (input_json_delta). We concatenate the string fragments;
            // if Arguments is already a JsonObject we use it directly.
            var toolCallArgs = outputs
                .Where(o => o.ToolCalls is { Count: > 0 })
                .SelectMany(o => o.ToolCalls!)
                .Where(tc => tc.Id != null)
                .GroupBy(tc => tc.Id!)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var fragments = g
                        .Select(tc =>
                        {
                            try { return tc.Arguments is JsonValue jv ? jv.GetValue<string>() : null; }
                            catch { return null; }
                        })
                        .Where(s => s != null)
                        .ToList();
                        var accumulated = string.Concat(fragments!);
                        return !string.IsNullOrWhiteSpace(accumulated)
                            ? ParseArguments(accumulated)
                            : ParseArguments(g.Last().Arguments);
                    });

            logger.LogInformation("Stream complete. Chunks: {Count}, Tool calls: {ToolCallCount}",
                outputs.Count, toolCalls.Count);

            // Manually build assistant message with proper ToolUseContent blocks
            // (new Message(outputs) does not reliably include tool_use content blocks)
            var assistantContent = new List<ContentBase>();
            var assistantText = string.Concat(outputs
                .Where(o => o.Delta?.Text is { Length: > 0 })
                .Select(o => o.Delta!.Text));
            if (!string.IsNullOrEmpty(assistantText))
            {
                assistantContent.Add(new TextContent { Text = assistantText });
            }

            foreach (var tc in toolCalls)
            {
                var input = toolCallArgs.TryGetValue(tc.Id!, out var a) ? a : ParseArguments(tc.Arguments);
                assistantContent.Add(new ToolUseContent { Id = tc.Id, Name = tc.Name, Input = input });
            }

            messages.Add(new Message { Role = RoleType.Assistant, Content = assistantContent });

            if (toolCalls.Count == 0)
            {
                break;
            }

            // Execute each tool call via MCP, yielding events as we go
            var toolResults = new List<ContentBase>();
            foreach (var toolCall in toolCalls)
            {
                var args = toolCallArgs.TryGetValue(toolCall.Id!, out var a) ? a : ParseArguments(toolCall.Arguments);
                yield return new ChatStreamEvent("tool_call", ToolName: toolCall.Name, ToolArgs: args);

                var result = await ExecuteMcpToolAsync(toolCall.Name!, args, bearerToken, cancellationToken);
                yield return new ChatStreamEvent("tool_result", ToolName: toolCall.Name, ToolResult: result);

                toolResults.Add(new ToolResultContent
                {
                    ToolUseId = toolCall.Id,
                    Content = [new TextContent { Text = JsonSerializer.Serialize(result) }],
                });
            }

            // All tool results go into a single user message (Anthropic API requirement)
            messages.Add(new Message { Role = RoleType.User, Content = toolResults });
        }
    }

    private async Task<IList<Tool>> FetchMcpToolsAsync(string bearerToken, CancellationToken cancellationToken)
    {
        var client = CreateClient(bearerToken);

        var payload = new StringContent(
            JsonSerializer.Serialize(new { jsonrpc = "2.0", id = 1, method = "tools/list", @params = new { } }),
            Encoding.UTF8, "application/json");

        logger.LogInformation("Fetching MCP tools from {Url}", $"{options.Value.SelfBaseUrl}/mcp");
        var response = await client.PostAsync($"{options.Value.SelfBaseUrl}/mcp", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("MCP tools/list returned {Status}: {Body}", (int)response.StatusCode, body);
            return [];
        }

        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: cancellationToken);
        var toolsArray = json?["result"]?["tools"]?.AsArray();

        if (toolsArray is null)
        {
            logger.LogWarning("MCP tools/list returned no tools. Response: {Json}", json?.ToString());
            return [];
        }

        logger.LogInformation("Loaded {Count} MCP tools", toolsArray.Count);

        return toolsArray
            .Where(t => t != null)
            .Select(t =>
            {
                var description = t!["description"]?.GetValue<string>() ?? string.Empty;
                var outputSchema = t["outputSchema"];
                if (outputSchema != null)
                {
                    description += $" Returns: {outputSchema.ToJsonString()}";
                }

                var fn = new Function(t["name"]!.GetValue<string>(), description, t["inputSchema"]?.DeepClone());
                return new Tool(fn);
            })
            .ToList();
    }

    private async Task<object> ExecuteMcpToolAsync(string toolName, JsonObject args, string bearerToken, CancellationToken cancellationToken)
    {
        var client = CreateClient(bearerToken);

        var payload = new StringContent(
            JsonSerializer.Serialize(new { jsonrpc = "2.0", id = 2, method = "tools/call", @params = new { name = toolName, arguments = args } }),
            Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{options.Value.SelfBaseUrl}/mcp", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new { error = $"HTTP {(int)response.StatusCode}" };
        }

        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: cancellationToken);
        return json?["result"] ?? (object)"(no result)";
    }

    private HttpClient CreateClient(string bearerToken)
    {
        var client = httpClientFactory.CreateClient("AiProxy");
        if (!string.IsNullOrEmpty(bearerToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return client;
    }

    private static List<Message> BuildMessages(List<ChatMessage> messages) =>
        messages.Select(m => new Message(
            m.Role == "user" ? RoleType.User : RoleType.Assistant,
            m.Content,
            null)).ToList();

    // The Anthropic SDK's Function.Arguments is typed as object.
    // In streaming mode it may arrive as a JSON string, JsonElement, or JsonObject.
    private static JsonObject ParseArguments(object? arguments)
    {
        if (arguments is JsonObject jo) return jo;

        try
        {
            var json = arguments switch
            {
                string s => s,
                System.Text.Json.JsonElement el => el.GetRawText(),
                JsonNode jn => jn.ToJsonString(),
                _ => JsonSerializer.Serialize(arguments)
            };
            return JsonSerializer.Deserialize<JsonObject>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

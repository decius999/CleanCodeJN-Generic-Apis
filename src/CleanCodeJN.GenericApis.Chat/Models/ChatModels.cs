using System.Text.Json.Serialization;

namespace CleanCodeJN.GenericApis.Chat.Models;

public record ChatMessage(string Role, string Content);

public record ChatRequest(List<ChatMessage> Messages);

public record ChatStreamEvent(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("content")] string Content = null,
    [property: JsonPropertyName("toolName")] string ToolName = null,
    [property: JsonPropertyName("toolArgs")] object ToolArgs = null,
    [property: JsonPropertyName("toolResult")] object ToolResult = null);

public record McpTool(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description);

public record McpToolsResult(
    [property: JsonPropertyName("tools")] List<McpTool> Tools);

// Chat display models
public class ChatEntry
{
    public string Role { get; init; } = string.Empty;  // "user", "assistant", "tool_call", "tool_result"
    public string Text { get; set; } = string.Empty;
    public string ToolName { get; init; }
    public bool IsStreaming { get; set; }
}

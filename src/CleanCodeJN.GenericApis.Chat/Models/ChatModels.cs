using System.Text.Json.Serialization;

namespace CleanCodeJN.GenericApis.Chat.Models;

/// <summary>Represents a single message in a chat conversation, consisting of a role and text content.</summary>
/// <param name="Role">The role of the message author, such as "user" or "assistant".</param>
/// <param name="Content">The text content of the message.</param>
public record ChatMessage(string Role, string Content);

/// <summary>Represents a chat request payload containing the full conversation history to be sent to the AI backend.</summary>
/// <param name="Messages">The ordered list of chat messages comprising the conversation history.</param>
public record ChatRequest(List<ChatMessage> Messages);

/// <summary>Represents a single server-sent event received from the AI chat stream.</summary>
/// <param name="Type">The event type, such as "text", "tool_call", "tool_result", or "error".</param>
/// <param name="Content">The text content associated with the event, if any.</param>
/// <param name="ToolName">The name of the tool involved in the event, if applicable.</param>
/// <param name="ToolArgs">The arguments passed to the tool, if applicable.</param>
/// <param name="ToolResult">The result returned by the tool, if applicable.</param>
public record ChatStreamEvent(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("content")] string Content = null,
    [property: JsonPropertyName("toolName")] string ToolName = null,
    [property: JsonPropertyName("toolArgs")] object ToolArgs = null,
    [property: JsonPropertyName("toolResult")] object ToolResult = null);

/// <summary>Represents a tool exposed by the MCP (Model Context Protocol) server.</summary>
/// <param name="Name">The unique name identifier of the MCP tool.</param>
/// <param name="Description">A human-readable description of what the tool does.</param>
public record McpTool(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description);

/// <summary>Represents the response from the MCP server containing the list of available tools.</summary>
/// <param name="Tools">The list of tools available on the MCP server.</param>
public record McpToolsResult(
    [property: JsonPropertyName("tools")] List<McpTool> Tools);

// Chat display models
/// <summary>Represents a single entry in the chat UI, including messages, tool calls, and tool results.</summary>
public class ChatEntry
{
    /// <summary>The role of the entry author; one of "user", "assistant", "tool_call", or "tool_result".</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>The text content of the chat entry, which may be appended to incrementally during streaming.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>The name of the tool associated with this entry, populated for "tool_call" and "tool_result" roles.</summary>
    public string ToolName { get; init; }

    /// <summary>Indicates whether this entry is currently receiving streamed content from the AI backend.</summary>
    public bool IsStreaming { get; set; }
}

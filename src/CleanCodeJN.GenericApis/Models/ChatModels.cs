namespace CleanCodeJN.GenericApis.Models;

/// <summary>
/// Represents a single message in a chat conversation, identified by role and content.
/// </summary>
/// <param name="Role">The role of the message sender (e.g. "user" or "assistant").</param>
/// <param name="Content">The text content of the message.</param>
public record ChatMessage(string Role, string Content);

/// <summary>
/// Represents a chat request containing the conversation message history.
/// </summary>
/// <param name="Messages">The ordered list of chat messages forming the conversation.</param>
public record ChatRequest(List<ChatMessage> Messages);

// SSE event types streamed back to the client
/// <summary>
/// Represents a server-sent event streamed back to the client during an AI chat session.
/// </summary>
/// <param name="Type">The event type (e.g. "text", "tool_call", "tool_result", "error").</param>
/// <param name="Content">Optional text content for text events.</param>
/// <param name="ToolName">Optional name of the tool involved in tool-related events.</param>
/// <param name="ToolArgs">Optional arguments passed to the tool.</param>
/// <param name="ToolResult">Optional result returned by the tool.</param>
public record ChatStreamEvent(string Type, string Content = null, string ToolName = null, object ToolArgs = null, object ToolResult = null);

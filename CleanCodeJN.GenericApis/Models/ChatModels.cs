namespace CleanCodeJN.GenericApis.Models;

public record ChatMessage(string Role, string Content);

public record ChatRequest(List<ChatMessage> Messages);

// SSE event types streamed back to the client
public record ChatStreamEvent(string Type, string Content = null, string ToolName = null, object ToolArgs = null, object ToolResult = null);

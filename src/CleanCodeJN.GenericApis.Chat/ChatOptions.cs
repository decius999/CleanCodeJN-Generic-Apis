namespace CleanCodeJN.GenericApis.Chat;

/// <summary>Holds configuration options for the AI chat feature, including the backend URL, authentication, and UI settings.</summary>
public class ChatOptions
{
    /// <summary>Base URL of the backend (e.g. https://your-api.com). Used to reach /ai/chat and /mcp.</summary>
    public string BackendUrl { get; set; } = string.Empty;

    /// <summary>Show tool call cards in the chat. Default: false.</summary>
    public bool ShowToolCalls { get; set; } = false;

    /// <summary>Optional title shown in the chat header.</summary>
    public string Title { get; set; } = "AI Assistant";

    /// <summary>Bearer token forwarded to /ai/chat for authentication.</summary>
    public string BearerToken { get; set; }
}

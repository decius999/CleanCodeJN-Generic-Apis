namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Configuration options for the AI Proxy service that forwards chat requests to the Anthropic Claude API.
/// </summary>
public class AiProxyOptions
{
    /// <summary>The Anthropic API key used to call Claude.</summary>
    public string AnthropicApiKey { get; set; } = string.Empty;

    /// <summary>The Claude model to use. Defaults to claude-sonnet-4-6.</summary>
    public string Model { get; set; } = "claude-sonnet-4-6";

    /// <summary>Max tokens for Claude response. Defaults to 4096.</summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>Base URL of the backend itself (used to call /mcp internally). E.g. https://localhost:7001</summary>
    public string SelfBaseUrl { get; set; } = string.Empty;
}

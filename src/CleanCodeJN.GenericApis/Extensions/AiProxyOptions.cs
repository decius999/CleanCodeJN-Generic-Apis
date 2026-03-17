namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Configuration options for the AI Proxy service that forwards chat requests to the configured LLM provider.
/// </summary>
public class AiProxyOptions
{
    /// <summary>The API key forwarded to the active <see cref="CleanCodeJN.GenericApis.Services.ILlmProvider"/>. For the default Anthropic provider this is your Claude API key.</summary>
    public string LlmApiKey { get; set; } = string.Empty;

    /// <summary>The LLM model to use. Defaults to claude-sonnet-4-6.</summary>
    public string Model { get; set; } = "claude-sonnet-4-6";

    /// <summary>Max tokens for the LLM response. Defaults to 4096.</summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>Base URL of the backend itself (used to call /mcp internally). E.g. https://localhost:7001</summary>
    public string SelfBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The CORS policy name registered for the AI chat endpoints.
    /// Defaults to <c>"CleanCodeJNChat"</c>. Change this if the name conflicts with an existing policy in your project.
    /// </summary>
    public string CorsPolicyName { get; set; } = "CleanCodeJNChat";

    /// <summary>
    /// When <c>true</c>, disables SSL certificate validation for the internal HTTP client used by the AI proxy.
    /// Only enable this in development environments. Defaults to <c>false</c>.
    /// </summary>
    public bool DisableCertificateValidation { get; set; } = false;
}

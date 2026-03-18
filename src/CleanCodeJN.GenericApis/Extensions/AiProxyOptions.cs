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

    /// <summary>Base URL of the backend itself (used to call the MCP endpoint internally). E.g. https://localhost:7001</summary>
    public string SelfBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Path of the MCP endpoint, appended to <see cref="SelfBaseUrl"/> for internal tool calls.
    /// Must match the <c>Route</c> configured in <see cref="McpOptions"/> when using <c>UseCleanCodeJNWithMcp</c>.
    /// Defaults to <c>"/mcp"</c>.
    /// </summary>
    public string McpPath { get; set; } = "/mcp";

    /// <summary>
    /// Route at which the AI chat SSE streaming endpoint is registered.
    /// Defaults to <c>"/ai/chat"</c>.
    /// </summary>
    public string ChatRoute { get; set; } = "/ai/chat";

    /// <summary>
    /// Route at which the AI connectivity test endpoint is registered.
    /// Only used when <see cref="EnableTestEndpoint"/> is <c>true</c>.
    /// Defaults to <c>"/ai/test"</c>.
    /// </summary>
    public string TestRoute { get; set; } = "/ai/test";

    /// <summary>
    /// When <c>true</c>, registers a simple GET endpoint at <see cref="TestRoute"/> that verifies LLM connectivity.
    /// Disable in production environments. Defaults to <c>false</c>.
    /// </summary>
    public bool EnableTestEndpoint { get; set; }

    /// <summary>
    /// Allowed CORS origins for the AI chat endpoints.
    /// Defaults to <c>["*"]</c> (any origin). Restrict to specific origins in production, e.g. <c>["https://myapp.com"]</c>.
    /// </summary>
    public List<string> AllowedCorsOrigins { get; set; } = ["*"];

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

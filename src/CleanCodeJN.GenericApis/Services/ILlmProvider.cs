using CleanCodeJN.GenericApis.Models;

namespace CleanCodeJN.GenericApis.Services;

/// <summary>
/// Abstraction over an LLM provider. Implement this interface to replace the default
/// Anthropic Claude backend with any other LLM service.
/// Register your implementation <em>after</em> <c>AddCleanCodeJN</c> so it overrides the default:
/// <code>services.AddScoped&lt;ILlmProvider, MyCustomProvider&gt;();</code>
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Executes an agentic chat loop and streams events back to the caller.
    /// </summary>
    /// <param name="request">The chat request containing the conversation message history.</param>
    /// <param name="bearerToken">An optional bearer token forwarded to MCP tool calls for authentication.</param>
    /// <param name="cancellationToken">A token to cancel the streaming operation.</param>
    /// <returns>An async enumerable of <see cref="ChatStreamEvent"/> objects.</returns>
    IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        string bearerToken,
        CancellationToken cancellationToken);
}

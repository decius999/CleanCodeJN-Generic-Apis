using CleanCodeJN.GenericApis.Models;

namespace CleanCodeJN.GenericApis.Services;

/// <summary>
/// Thin facade over <see cref="ILlmProvider"/> that exposes the agentic chat loop.
/// The actual LLM interaction is handled by the registered <see cref="ILlmProvider"/>;
/// by default this is <see cref="AnthropicLlmProvider"/>.
/// </summary>
public class AiProxyService(ILlmProvider provider)
{
    /// <summary>
    /// Streams a chat response, executing MCP tool calls as needed until the provider produces a final text reply.
    /// </summary>
    /// <param name="request">The chat request containing the conversation message history.</param>
    /// <param name="bearerToken">An optional bearer token forwarded to MCP tool calls for authentication.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous streaming operation.</param>
    /// <returns>An async enumerable of <see cref="ChatStreamEvent"/> objects representing streamed response chunks.</returns>
    public IAsyncEnumerable<ChatStreamEvent> StreamAsync(
        ChatRequest request,
        string bearerToken,
        CancellationToken cancellationToken)
        => provider.StreamAsync(request, bearerToken, cancellationToken);
}

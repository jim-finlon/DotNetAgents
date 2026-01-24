namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// Chat completion service interface.
/// </summary>
public interface IChatCompletionService
{
    /// <summary>
    /// Gets the provider name (e.g., "OpenAI", "Anthropic", "Ollama").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the default model name.
    /// </summary>
    string DefaultModel { get; }

    /// <summary>
    /// Generates a chat completion.
    /// </summary>
    /// <param name="messages">Conversation messages.</param>
    /// <param name="options">Completion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Chat completion result.</returns>
    Task<ChatCompletionResult> CompleteAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a streaming chat completion.
    /// </summary>
    /// <param name="messages">Conversation messages.</param>
    /// <param name="options">Completion options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of completion chunks.</returns>
    IAsyncEnumerable<ChatCompletionChunk> StreamAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);
}


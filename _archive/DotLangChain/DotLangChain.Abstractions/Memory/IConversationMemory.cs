using DotLangChain.Abstractions.LLM;

namespace DotLangChain.Abstractions.Memory;

/// <summary>
/// Interface for conversation memory management.
/// </summary>
public interface IConversationMemory
{
    /// <summary>
    /// Adds a message to the conversation memory.
    /// </summary>
    /// <param name="message">Message to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all messages from the conversation memory.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of messages.</returns>
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all messages from the conversation memory.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearAsync(CancellationToken cancellationToken = default);
}


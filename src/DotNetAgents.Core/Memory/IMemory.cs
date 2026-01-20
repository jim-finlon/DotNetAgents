namespace DotNetAgents.Core.Memory;

/// <summary>
/// Represents a message stored in memory.
/// </summary>
public record MemoryMessage
{
    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the role of the message (e.g., "user", "assistant", "system").
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the message was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets additional metadata associated with the message.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Interface for memory stores that manage conversation history.
/// </summary>
public interface IMemory
{
    /// <summary>
    /// Adds a message to memory.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task AddMessageAsync(
        MemoryMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent messages from memory.
    /// </summary>
    /// <param name="count">The number of messages to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of messages, ordered from oldest to newest.</returns>
    Task<IReadOnlyList<MemoryMessage>> GetMessagesAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all messages from memory.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task ClearAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for persistent memory stores that can save and load state.
/// </summary>
public interface IMemoryStore : IMemory
{
    /// <summary>
    /// Saves the current memory state to persistent storage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads memory state from persistent storage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task LoadAsync(CancellationToken cancellationToken = default);
}
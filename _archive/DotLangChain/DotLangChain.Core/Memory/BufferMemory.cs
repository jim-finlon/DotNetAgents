using DotLangChain.Abstractions.LLM;
using DotLangChain.Abstractions.Memory;
using Microsoft.Extensions.Logging;

namespace DotLangChain.Core.Memory;

/// <summary>
/// Stores all conversation messages in memory (unbounded).
/// </summary>
public sealed class BufferMemory : IConversationMemory
{
    private readonly List<ChatMessage> _messages = new();
    private readonly object _lock = new();
    private readonly ILogger<BufferMemory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferMemory"/> class.
    /// </summary>
    public BufferMemory(ILogger<BufferMemory>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        lock (_lock)
        {
            _messages.Add(message);
            _logger?.LogDebug("Added message to buffer memory. Total messages: {Count}", _messages.Count);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<ChatMessage>>(_messages.ToList());
        }
    }

    /// <inheritdoc/>
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var count = _messages.Count;
            _messages.Clear();
            _logger?.LogDebug("Cleared buffer memory. Removed {Count} messages", count);
        }

        return Task.CompletedTask;
    }
}


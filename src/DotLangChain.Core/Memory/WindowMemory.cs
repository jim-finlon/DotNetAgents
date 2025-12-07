using DotLangChain.Abstractions.LLM;
using DotLangChain.Abstractions.Memory;
using Microsoft.Extensions.Logging;

namespace DotLangChain.Core.Memory;

/// <summary>
/// Keeps only the last N messages in memory (sliding window).
/// </summary>
public sealed class WindowMemory : IConversationMemory
{
    private readonly List<ChatMessage> _messages = new();
    private readonly int _maxMessages;
    private readonly object _lock = new();
    private readonly ILogger<WindowMemory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowMemory"/> class.
    /// </summary>
    /// <param name="maxMessages">Maximum number of messages to keep.</param>
    /// <param name="logger">Optional logger.</param>
    public WindowMemory(int maxMessages = 10, ILogger<WindowMemory>? logger = null)
    {
        if (maxMessages <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxMessages), "Max messages must be greater than 0");

        _maxMessages = maxMessages;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        lock (_lock)
        {
            _messages.Add(message);

            // Remove oldest messages if exceeding max
            while (_messages.Count > _maxMessages)
            {
                _messages.RemoveAt(0);
            }

            _logger?.LogDebug(
                "Added message to window memory. Messages: {Count}/{Max}",
                _messages.Count,
                _maxMessages);
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
            _logger?.LogDebug("Cleared window memory. Removed {Count} messages", count);
        }

        return Task.CompletedTask;
    }
}


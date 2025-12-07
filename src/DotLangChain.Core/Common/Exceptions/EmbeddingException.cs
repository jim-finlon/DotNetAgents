using DotLangChain.Abstractions.Common;

namespace DotLangChain.Core.Exceptions;

/// <summary>
/// Exception thrown when embedding operations fail.
/// </summary>
public class EmbeddingException : DotLangChainException
{
    /// <summary>
    /// Gets the provider name, if applicable.
    /// </summary>
    public string? ProviderName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingException"/> class.
    /// </summary>
    public EmbeddingException(string message, string? providerName = null, Exception? innerException = null)
        : base(message, "DLC002", innerException)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Creates an exception for provider unavailable.
    /// </summary>
    public static EmbeddingException ProviderUnavailable(string providerName, Exception? innerException = null)
    {
        return new EmbeddingException(
            $"Embedding provider '{providerName}' is unavailable",
            providerName: providerName,
            innerException: innerException)
        {
            ErrorCode = "DLC002-001"
        };
    }

    /// <summary>
    /// Creates an exception for rate limit exceeded.
    /// </summary>
    public static EmbeddingException RateLimitExceeded(string providerName, TimeSpan? retryAfter = null)
    {
        return new EmbeddingException(
            $"Rate limit exceeded for embedding provider {providerName}",
            providerName: providerName)
        {
            ErrorCode = "DLC002-002",
            Context = retryAfter.HasValue
                ? new Dictionary<string, object?> { ["retry_after_seconds"] = retryAfter.Value.TotalSeconds }
                : null
        };
    }

    /// <summary>
    /// Creates an exception for invalid embedding dimensions.
    /// </summary>
    public static EmbeddingException InvalidDimensions(int expected, int actual)
    {
        return new EmbeddingException(
            $"Invalid embedding dimensions: expected {expected}, got {actual}")
        {
            ErrorCode = "DLC002-003",
            Context = new Dictionary<string, object?>
            {
                ["expected_dimensions"] = expected,
                ["actual_dimensions"] = actual
            }
        };
    }
}


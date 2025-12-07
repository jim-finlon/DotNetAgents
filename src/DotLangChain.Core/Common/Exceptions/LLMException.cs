using DotLangChain.Abstractions.Common;

namespace DotLangChain.Core.Exceptions;

/// <summary>
/// Exception thrown when LLM operations fail.
/// </summary>
public class LLMException : DotLangChainException
{
    /// <summary>
    /// Gets the provider name, if applicable.
    /// </summary>
    public string? ProviderName { get; }

    /// <summary>
    /// Gets the model name, if applicable.
    /// </summary>
    public string? Model { get; }

    /// <summary>
    /// Gets the HTTP status code, if applicable.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LLMException"/> class.
    /// </summary>
    public LLMException(string message, string? providerName = null, Exception? innerException = null)
        : base(message, "DLC004", innerException)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Creates an exception for rate limit exceeded.
    /// </summary>
    public static LLMException RateLimitExceeded(string providerName, TimeSpan? retryAfter = null)
    {
        return new LLMException(
            $"Rate limit exceeded for LLM provider {providerName}",
            providerName: providerName)
        {
            ErrorCode = "DLC004-001",
            Context = retryAfter.HasValue
                ? new Dictionary<string, object?> { ["retry_after_seconds"] = retryAfter.Value.TotalSeconds }
                : null
        };
    }

    /// <summary>
    /// Creates an exception for context length exceeded.
    /// </summary>
    public static LLMException ContextLengthExceeded(string providerName, int promptTokens, int maxTokens)
    {
        return new LLMException(
            $"Context length exceeded: {promptTokens} tokens requested, {maxTokens} tokens allowed",
            providerName: providerName)
        {
            ErrorCode = "DLC004-002",
            Context = new Dictionary<string, object?>
            {
                ["prompt_tokens"] = promptTokens,
                ["max_tokens"] = maxTokens
            }
        };
    }

    /// <summary>
    /// Creates an exception for invalid response format.
    /// </summary>
    public static LLMException InvalidResponseFormat(string providerName, string? reason = null)
    {
        return new LLMException(
            $"Invalid response format from {providerName}" + (reason != null ? $": {reason}" : ""),
            providerName: providerName)
        {
            ErrorCode = "DLC004-003",
            Context = reason != null ? new Dictionary<string, object?> { ["reason"] = reason } : null
        };
    }
}


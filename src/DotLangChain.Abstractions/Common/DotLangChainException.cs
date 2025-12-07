namespace DotLangChain.Abstractions.Common;

/// <summary>
/// Base exception for all DotLangChain errors.
/// </summary>
public class DotLangChainException : Exception
{
    /// <summary>
    /// Gets the error code (format: DLC{Category}-{SubCategory}-{Specific}).
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets additional context data for the error.
    /// </summary>
    public Dictionary<string, object?>? Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DotLangChainException"/> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <param name="innerException">Inner exception.</param>
    public DotLangChainException(string message, string? errorCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DotLangChainException"/> class with context.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="errorCode">Error code.</param>
    /// <param name="context">Additional context data.</param>
    /// <param name="innerException">Inner exception.</param>
    public DotLangChainException(
        string message,
        string? errorCode,
        Dictionary<string, object?>? context,
        Exception? innerException = null)
        : this(message, errorCode, innerException)
    {
        Context = context;
    }
}


using DotLangChain.Abstractions.Common;

namespace DotLangChain.Core.Exceptions;

/// <summary>
/// Exception thrown when document operations fail.
/// </summary>
public class DocumentException : DotLangChainException
{
    /// <summary>
    /// Gets the file path that caused the error, if applicable.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Gets the file extension, if applicable.
    /// </summary>
    public string? FileExtension { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentException"/> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="filePath">Optional file path.</param>
    /// <param name="innerException">Inner exception.</param>
    public DocumentException(string message, string? filePath = null, Exception? innerException = null)
        : base(message, "DLC001", innerException)
    {
        FilePath = filePath;
        FileExtension = filePath != null ? Path.GetExtension(filePath) : null;
    }

    /// <summary>
    /// Creates an exception for unsupported document format.
    /// </summary>
    public static DocumentException UnsupportedFormat(string extension)
    {
        return new DocumentException(
            $"Unsupported document format: {extension}",
            errorCode: "DLC001-001")
        {
            Context = new Dictionary<string, object?> { ["extension"] = extension }
        };
    }

    /// <summary>
    /// Creates an exception for document load failure.
    /// </summary>
    public static DocumentException LoadFailed(string filePath, Exception innerException)
    {
        return new DocumentException(
            $"Failed to load document from {filePath}",
            filePath: filePath,
            innerException: innerException)
        {
            ErrorCode = "DLC001-002"
        };
    }

    /// <summary>
    /// Creates an exception for invalid metadata.
    /// </summary>
    public static DocumentException InvalidMetadata(string reason)
    {
        return new DocumentException(
            $"Invalid metadata: {reason}",
            errorCode: "DLC001-003")
        {
            Context = new Dictionary<string, object?> { ["reason"] = reason }
        };
    }
}


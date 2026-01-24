using DotLangChain.Abstractions.Common;

namespace DotLangChain.Core.Exceptions;

/// <summary>
/// Exception thrown when vector store operations fail.
/// </summary>
public class VectorStoreException : DotLangChainException
{
    /// <summary>
    /// Gets the collection name, if applicable.
    /// </summary>
    public string? CollectionName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorStoreException"/> class.
    /// </summary>
    public VectorStoreException(string message, string? collectionName = null, Exception? innerException = null)
        : base(message, "DLC003", innerException)
    {
        CollectionName = collectionName;
    }

    /// <summary>
    /// Creates an exception for connection failure.
    /// </summary>
    public static VectorStoreException ConnectionFailed(string? collectionName = null, Exception? innerException = null)
    {
        return new VectorStoreException(
            $"Failed to connect to vector store",
            collectionName: collectionName,
            innerException: innerException)
        {
            ErrorCode = "DLC003-001"
        };
    }

    /// <summary>
    /// Creates an exception for collection not found.
    /// </summary>
    public static VectorStoreException CollectionNotFound(string collectionName)
    {
        return new VectorStoreException(
            $"Vector store collection '{collectionName}' not found",
            collectionName: collectionName)
        {
            ErrorCode = "DLC003-002"
        };
    }

    /// <summary>
    /// Creates an exception for dimension mismatch.
    /// </summary>
    public static VectorStoreException DimensionMismatch(int expected, int actual)
    {
        return new VectorStoreException(
            $"Vector dimension mismatch: expected {expected}, got {actual}")
        {
            ErrorCode = "DLC003-003",
            Context = new Dictionary<string, object?>
            {
                ["expected_dimensions"] = expected,
                ["actual_dimensions"] = actual
            }
        };
    }
}


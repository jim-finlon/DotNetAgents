namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Represents a document with content and metadata.
/// </summary>
/// <param name="Id">Unique identifier for the document.</param>
/// <param name="Content">Extracted text content.</param>
/// <param name="Metadata">Key-value metadata dictionary (default: empty).</param>
/// <param name="SourceUri">Original source location (file path, URL).</param>
/// <param name="CreatedAt">Timestamp when document was loaded (default: UtcNow).</param>
public sealed record Document
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public DocumentMetadata Metadata { get; init; } = new();
    public string? SourceUri { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}


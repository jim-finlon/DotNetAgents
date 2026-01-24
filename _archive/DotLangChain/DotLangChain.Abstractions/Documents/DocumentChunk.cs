namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Represents a text chunk with embedding and lineage.
/// </summary>
/// <param name="Id">Unique chunk ID (typically {docId}_chunk_{index}).</param>
/// <param name="Content">The chunk text content.</param>
/// <param name="ParentDocumentId">ID of source document.</param>
/// <param name="ChunkIndex">Zero-based position in document.</param>
/// <param name="StartCharOffset">Starting character position in original.</param>
/// <param name="EndCharOffset">Ending character position in original.</param>
/// <param name="Embedding">Pre-computed embedding vector.</param>
/// <param name="Metadata">Inherited and chunk-specific metadata.</param>
public sealed record DocumentChunk
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public required string ParentDocumentId { get; init; }
    public required int ChunkIndex { get; init; }
    public int StartCharOffset { get; init; }
    public int EndCharOffset { get; init; }
    public float[]? Embedding { get; init; }
    public DocumentMetadata Metadata { get; init; } = new();
}


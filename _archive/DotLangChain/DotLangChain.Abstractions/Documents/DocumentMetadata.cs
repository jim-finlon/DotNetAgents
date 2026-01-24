namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Strongly-typed metadata container with extension support.
/// </summary>
public sealed class DocumentMetadata : Dictionary<string, object?>
{
    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string? Title
    {
        get => TryGetValue("title", out var v) ? v?.ToString() : null;
        set => this["title"] = value;
    }

    /// <summary>
    /// Gets or sets the source identifier.
    /// </summary>
    public string? Source
    {
        get => TryGetValue("source", out var v) ? v?.ToString() : null;
        set => this["source"] = value;
    }

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    public string? Author
    {
        get => TryGetValue("author", out var v) ? v?.ToString() : null;
        set => this["author"] = value;
    }

    /// <summary>
    /// Gets or sets the page number (for chunked docs).
    /// </summary>
    public int? PageNumber
    {
        get => TryGetValue("page_number", out var v) && v is int i ? i : null;
        set => this["page_number"] = value;
    }

    /// <summary>
    /// Gets or sets the chunk index in parent document.
    /// </summary>
    public int? ChunkIndex
    {
        get => TryGetValue("chunk_index", out var v) && v is int i ? i : null;
        set => this["chunk_index"] = value;
    }

    /// <summary>
    /// Gets or sets the reference to parent document ID.
    /// </summary>
    public string? ParentDocumentId
    {
        get => TryGetValue("parent_document_id", out var v) ? v?.ToString() : null;
        set => this["parent_document_id"] = value;
    }

    /// <summary>
    /// Gets or sets the content type (MIME type).
    /// </summary>
    public string? ContentType
    {
        get => TryGetValue("content_type", out var v) ? v?.ToString() : null;
        set => this["content_type"] = value;
    }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long? FileSize
    {
        get => TryGetValue("file_size", out var v) && v is long l ? l : null;
        set => this["file_size"] = value;
    }

    /// <summary>
    /// Gets or sets the original creation date.
    /// </summary>
    public DateTimeOffset? CreatedDate
    {
        get => TryGetValue("created_date", out var v) && v is DateTimeOffset d ? d : null;
        set => this["created_date"] = value;
    }

    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTimeOffset? ModifiedDate
    {
        get => TryGetValue("modified_date", out var v) && v is DateTimeOffset d ? d : null;
        set => this["modified_date"] = value;
    }
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a vector embedding for a content unit chunk (for RAG retrieval).
/// </summary>
[Table("content_embeddings")]
public class ContentEmbedding
{
    /// <summary>
    /// Gets or sets the unique identifier for the embedding.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the content unit ID this embedding belongs to.
    /// </summary>
    [Required]
    [Column("content_unit_id")]
    public Guid ContentUnitId { get; set; }

    /// <summary>
    /// Gets or sets the chunk index within the content unit.
    /// </summary>
    [Required]
    [Column("chunk_index")]
    public short ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets the chunk text.
    /// </summary>
    [Required]
    [Column("chunk_text", TypeName = "text")]
    public string ChunkText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vector embedding (1536 dimensions for BGE-large).
    /// </summary>
    [Column("embedding")]
    public Vector? Embedding { get; set; }

    /// <summary>
    /// Gets or sets metadata as JSON.
    /// </summary>
    [Column("metadata", TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets when the embedding was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the content unit this embedding belongs to.
    /// </summary>
    [ForeignKey(nameof(ContentUnitId))]
    public virtual ContentUnit ContentUnit { get; set; } = null!;
}

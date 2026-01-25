using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a message in a conversation thread.
/// </summary>
[Table("conversation_messages")]
public class ConversationMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for the message.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the thread ID this message belongs to.
    /// </summary>
    [Required]
    [Column("thread_id")]
    public Guid ThreadId { get; set; }

    /// <summary>
    /// Gets or sets the role (student, tutor, or system).
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message content.
    /// </summary>
    [Required]
    [Column("content", TypeName = "text")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets content references (UUID array).
    /// </summary>
    [Column("content_references", TypeName = "uuid[]")]
    public List<Guid> ContentReferences { get; set; } = new();

    /// <summary>
    /// Gets or sets the pedagogical intent.
    /// </summary>
    [MaxLength(50)]
    [Column("pedagogical_intent")]
    public string? PedagogicalIntent { get; set; }

    /// <summary>
    /// Gets or sets the sentiment score (-1 to 1).
    /// </summary>
    [Column("sentiment_score", TypeName = "decimal(3,2)")]
    public decimal? SentimentScore { get; set; }

    /// <summary>
    /// Gets or sets when the message was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the thread this message belongs to.
    /// </summary>
    [ForeignKey(nameof(ThreadId))]
    public virtual ConversationThread Thread { get; set; } = null!;
}

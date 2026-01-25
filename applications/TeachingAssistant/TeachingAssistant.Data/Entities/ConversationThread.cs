using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a conversation thread between a student and tutor.
/// </summary>
[Table("conversation_threads")]
public class ConversationThread
{
    /// <summary>
    /// Gets or sets the unique identifier for the thread.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the student ID.
    /// </summary>
    [Required]
    [Column("student_id")]
    public Guid StudentId { get; set; }

    /// <summary>
    /// Gets or sets the session ID (optional).
    /// </summary>
    [Column("session_id")]
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Gets or sets the subject for this conversation.
    /// </summary>
    [Column("subject")]
    public Subject? Subject { get; set; }

    /// <summary>
    /// Gets or sets the content unit ID this conversation is about.
    /// </summary>
    [Column("content_unit_id")]
    public Guid? ContentUnitId { get; set; }

    /// <summary>
    /// Gets or sets when the conversation started.
    /// </summary>
    [Column("started_at")]
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the conversation ended.
    /// </summary>
    [Column("ended_at")]
    public DateTimeOffset? EndedAt { get; set; }

    /// <summary>
    /// Gets or sets the message count.
    /// </summary>
    [Column("message_count")]
    public int MessageCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the conversation summary.
    /// </summary>
    [Column("summary", TypeName = "text")]
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets whether this conversation is flagged for review.
    /// </summary>
    [Column("flagged_for_review")]
    public bool FlaggedForReview { get; set; } = false;

    /// <summary>
    /// Gets or sets the reason for flagging.
    /// </summary>
    [Column("flag_reason", TypeName = "text")]
    public string? FlagReason { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the student this thread belongs to.
    /// </summary>
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Gets or sets the session this thread belongs to.
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual LearningSession? Session { get; set; }

    /// <summary>
    /// Gets or sets the content unit this thread is about.
    /// </summary>
    [ForeignKey(nameof(ContentUnitId))]
    public virtual ContentUnit? ContentUnit { get; set; }

    /// <summary>
    /// Gets or sets the messages in this thread.
    /// </summary>
    public virtual ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}

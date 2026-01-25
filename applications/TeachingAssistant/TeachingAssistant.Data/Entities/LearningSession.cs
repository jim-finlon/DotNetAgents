using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a learning session for a student.
/// </summary>
[Table("learning_sessions")]
public class LearningSession
{
    /// <summary>
    /// Gets or sets the unique identifier for the session.
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
    /// Gets or sets the subject for this session.
    /// </summary>
    [Column("subject")]
    public Subject? Subject { get; set; }

    /// <summary>
    /// Gets or sets the session type.
    /// </summary>
    [MaxLength(50)]
    [Column("session_type")]
    public string? SessionType { get; set; }

    /// <summary>
    /// Gets or sets when the session started.
    /// </summary>
    [Column("started_at")]
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the session ended.
    /// </summary>
    [Column("ended_at")]
    public DateTimeOffset? EndedAt { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds.
    /// </summary>
    [Column("duration_seconds")]
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the content unit IDs covered in this session.
    /// </summary>
    [Column("content_units_covered", TypeName = "uuid[]")]
    public List<Guid> ContentUnitsCovered { get; set; } = new();

    /// <summary>
    /// Gets or sets the session summary as JSON.
    /// </summary>
    [Column("summary", TypeName = "jsonb")]
    public Dictionary<string, object>? Summary { get; set; }

    /// <summary>
    /// Gets or sets whether the session was parent-approved.
    /// </summary>
    [Column("parent_approved")]
    public bool? ParentApproved { get; set; }

    /// <summary>
    /// Gets or sets when the session was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the student this session belongs to.
    /// </summary>
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Gets or sets the student responses in this session.
    /// </summary>
    public virtual ICollection<StudentResponse> Responses { get; set; } = new List<StudentResponse>();

    /// <summary>
    /// Gets or sets the conversation threads in this session.
    /// </summary>
    public virtual ICollection<ConversationThread> ConversationThreads { get; set; } = new List<ConversationThread>();
}

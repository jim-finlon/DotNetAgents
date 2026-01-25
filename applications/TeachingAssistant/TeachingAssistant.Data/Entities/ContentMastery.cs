using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a student's mastery level for a specific content unit.
/// </summary>
[Table("content_mastery")]
public class ContentMastery
{
    /// <summary>
    /// Gets or sets the unique identifier for the mastery record.
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
    /// Gets or sets the content unit ID.
    /// </summary>
    [Required]
    [Column("content_unit_id")]
    public Guid ContentUnitId { get; set; }

    /// <summary>
    /// Gets or sets the mastery level.
    /// </summary>
    [Column("mastery_level")]
    public MasteryLevel MasteryLevel { get; set; } = MasteryLevel.NotStarted;

    /// <summary>
    /// Gets or sets the mastery score (0-100).
    /// </summary>
    [Column("mastery_score", TypeName = "decimal(5,2)")]
    public decimal? MasteryScore { get; set; }

    /// <summary>
    /// Gets or sets the number of attempts.
    /// </summary>
    [Column("attempts")]
    public int Attempts { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of correct attempts.
    /// </summary>
    [Column("correct_attempts")]
    public int CorrectAttempts { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total time spent in seconds.
    /// </summary>
    [Column("total_time_seconds")]
    public int TotalTimeSeconds { get; set; } = 0;

    /// <summary>
    /// Gets or sets when the student first saw this content.
    /// </summary>
    [Column("first_seen_at")]
    public DateTimeOffset? FirstSeenAt { get; set; }

    /// <summary>
    /// Gets or sets when the student last reviewed this content.
    /// </summary>
    [Column("last_reviewed_at")]
    public DateTimeOffset? LastReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets when the student should review this content next (spaced repetition).
    /// </summary>
    [Column("next_review_at")]
    public DateTimeOffset? NextReviewAt { get; set; }

    /// <summary>
    /// Gets or sets the ease factor for spaced repetition (SM-2 algorithm).
    /// </summary>
    [Column("ease_factor", TypeName = "decimal(4,2)")]
    public decimal EaseFactor { get; set; } = 2.5m;

    /// <summary>
    /// Gets or sets the interval in days until next review.
    /// </summary>
    [Column("interval_days")]
    public int IntervalDays { get; set; } = 1;

    /// <summary>
    /// Gets or sets additional metadata as JSON.
    /// </summary>
    [Column("metadata", TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Navigation properties
    /// <summary>
    /// Gets or sets the student this mastery record belongs to.
    /// </summary>
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content unit this mastery record is for.
    /// </summary>
    [ForeignKey(nameof(ContentUnitId))]
    public virtual ContentUnit ContentUnit { get; set; } = null!;
}

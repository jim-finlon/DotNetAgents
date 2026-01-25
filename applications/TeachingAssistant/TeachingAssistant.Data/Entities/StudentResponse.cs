using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a student's response to an assessment item.
/// </summary>
[Table("student_responses")]
public class StudentResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the response.
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
    /// Gets or sets the assessment item ID.
    /// </summary>
    [Required]
    [Column("assessment_item_id")]
    public Guid AssessmentItemId { get; set; }

    /// <summary>
    /// Gets or sets the response data as JSON.
    /// </summary>
    [Required]
    [Column("response", TypeName = "jsonb")]
    public Dictionary<string, object> Response { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the response was correct.
    /// </summary>
    [Column("is_correct")]
    public bool? IsCorrect { get; set; }

    /// <summary>
    /// Gets or sets the partial credit awarded (0-100).
    /// </summary>
    [Column("partial_credit", TypeName = "decimal(5,2)")]
    public decimal? PartialCredit { get; set; }

    /// <summary>
    /// Gets or sets the time taken in seconds.
    /// </summary>
    [Column("time_taken_seconds")]
    public int? TimeTakenSeconds { get; set; }

    /// <summary>
    /// Gets or sets the number of hints used.
    /// </summary>
    [Column("hints_used")]
    public int HintsUsed { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether feedback was shown.
    /// </summary>
    [Column("feedback_shown")]
    public bool FeedbackShown { get; set; } = false;

    /// <summary>
    /// Gets or sets when the response was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the student this response belongs to.
    /// </summary>
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Gets or sets the session this response belongs to.
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual LearningSession? Session { get; set; }

    /// <summary>
    /// Gets or sets the assessment item this response is for.
    /// </summary>
    [ForeignKey(nameof(AssessmentItemId))]
    public virtual AssessmentItem AssessmentItem { get; set; } = null!;
}

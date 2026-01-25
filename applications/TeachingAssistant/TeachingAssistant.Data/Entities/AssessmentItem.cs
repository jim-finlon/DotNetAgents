using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a single assessment item (question).
/// </summary>
[Table("assessment_items")]
public class AssessmentItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the assessment item.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the assessment ID this item belongs to (optional).
    /// </summary>
    [Column("assessment_id")]
    public Guid? AssessmentId { get; set; }

    /// <summary>
    /// Gets or sets the content unit ID this item is associated with.
    /// </summary>
    [Column("content_unit_id")]
    public Guid? ContentUnitId { get; set; }

    /// <summary>
    /// Gets or sets the learning objective ID this item aligns with.
    /// </summary>
    [Column("learning_objective_id")]
    public Guid? LearningObjectiveId { get; set; }

    /// <summary>
    /// Gets or sets the question type.
    /// </summary>
    [Required]
    [Column("question_type")]
    public QuestionType QuestionType { get; set; }

    /// <summary>
    /// Gets or sets the difficulty level (1-5).
    /// </summary>
    [Required]
    [Column("difficulty")]
    public short Difficulty { get; set; }

    /// <summary>
    /// Gets or sets the Bloom's taxonomy level.
    /// </summary>
    [Required]
    [Column("bloom_level")]
    public BloomLevel BloomLevel { get; set; }

    /// <summary>
    /// Gets or sets the points value for this item.
    /// </summary>
    [Column("points", TypeName = "decimal(5,2)")]
    public decimal Points { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the question data as JSON.
    /// </summary>
    [Required]
    [Column("question_data", TypeName = "jsonb")]
    public Dictionary<string, object> QuestionData { get; set; } = new();

    /// <summary>
    /// Gets or sets the feedback data as JSON.
    /// </summary>
    [Required]
    [Column("feedback_data", TypeName = "jsonb")]
    public Dictionary<string, object> FeedbackData { get; set; } = new();

    /// <summary>
    /// Gets or sets the IRT difficulty parameter.
    /// </summary>
    [Column("irt_difficulty", TypeName = "decimal(5,3)")]
    public decimal? IrtDifficulty { get; set; }

    /// <summary>
    /// Gets or sets the IRT discrimination parameter.
    /// </summary>
    [Column("irt_discrimination", TypeName = "decimal(5,3)")]
    public decimal? IrtDiscrimination { get; set; }

    /// <summary>
    /// Gets or sets the number of times this item has been shown.
    /// </summary>
    [Column("times_shown")]
    public int TimesShown { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of times this item was answered correctly.
    /// </summary>
    [Column("times_correct")]
    public int TimesCorrect { get; set; } = 0;

    /// <summary>
    /// Gets or sets the average time taken in seconds.
    /// </summary>
    [Column("avg_time_seconds", TypeName = "decimal(8,2)")]
    public decimal? AvgTimeSeconds { get; set; }

    /// <summary>
    /// Gets or sets whether this item is active.
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets when the item was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the item was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the assessment this item belongs to.
    /// </summary>
    [ForeignKey(nameof(AssessmentId))]
    public virtual Assessment? Assessment { get; set; }

    /// <summary>
    /// Gets or sets the content unit this item is associated with.
    /// </summary>
    [ForeignKey(nameof(ContentUnitId))]
    public virtual ContentUnit? ContentUnit { get; set; }

    /// <summary>
    /// Gets or sets the learning objective this item aligns with.
    /// </summary>
    [ForeignKey(nameof(LearningObjectiveId))]
    public virtual LearningObjective? LearningObjective { get; set; }

    /// <summary>
    /// Gets or sets the student responses for this item.
    /// </summary>
    public virtual ICollection<StudentResponse> Responses { get; set; } = new List<StudentResponse>();
}

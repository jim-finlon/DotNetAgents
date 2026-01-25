using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents an assessment (collection of assessment items).
/// </summary>
[Table("assessments")]
public class Assessment
{
    /// <summary>
    /// Gets or sets the unique identifier for the assessment.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the type of assessment.
    /// </summary>
    [Required]
    [Column("type")]
    public AssessmentType Type { get; set; }

    /// <summary>
    /// Gets or sets the subject this assessment covers.
    /// </summary>
    [Required]
    [Column("subject")]
    public Subject Subject { get; set; }

    /// <summary>
    /// Gets or sets the grade band this assessment is appropriate for.
    /// </summary>
    [Required]
    [Column("grade_band")]
    public GradeBand GradeBand { get; set; }

    /// <summary>
    /// Gets or sets the title of the assessment.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the assessment.
    /// </summary>
    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the content unit IDs covered by this assessment.
    /// </summary>
    [Column("content_unit_ids", TypeName = "uuid[]")]
    public List<Guid> ContentUnitIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the time limit in minutes.
    /// </summary>
    [Column("time_limit_minutes")]
    public short? TimeLimitMinutes { get; set; }

    /// <summary>
    /// Gets or sets the passing score (percentage).
    /// </summary>
    [Column("passing_score", TypeName = "decimal(5,2)")]
    public decimal? PassingScore { get; set; }

    /// <summary>
    /// Gets or sets whether this assessment is adaptive.
    /// </summary>
    [Column("is_adaptive")]
    public bool IsAdaptive { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this assessment is active.
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets when the assessment was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the assessment items in this assessment.
    /// </summary>
    public virtual ICollection<AssessmentItem> Items { get; set; } = new List<AssessmentItem>();
}

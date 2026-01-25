using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a content unit in the curriculum (concept, lesson, assessment, etc.).
/// </summary>
[Table("content_units")]
public class ContentUnit
{
    /// <summary>
    /// Gets or sets the unique identifier for the content unit.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the type of content unit.
    /// </summary>
    [Required]
    [Column("type")]
    public ContentType Type { get; set; }

    /// <summary>
    /// Gets or sets the subject this content belongs to.
    /// </summary>
    [Required]
    [Column("subject")]
    public Subject Subject { get; set; }

    /// <summary>
    /// Gets or sets the grade band this content is appropriate for.
    /// </summary>
    [Required]
    [Column("grade_band")]
    public GradeBand GradeBand { get; set; }

    /// <summary>
    /// Gets or sets the topic path (hierarchy) as an array.
    /// </summary>
    [Required]
    [Column("topic_path", TypeName = "text[]")]
    public List<string> TopicPath { get; set; } = new();

    /// <summary>
    /// Gets or sets the title of the content unit.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the summary of the content unit.
    /// </summary>
    [Column("summary", TypeName = "text")]
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the full content as JSON.
    /// </summary>
    [Required]
    [Column("content", TypeName = "jsonb")]
    public Dictionary<string, object> Content { get; set; } = new();

    /// <summary>
    /// Gets or sets the estimated duration in minutes.
    /// </summary>
    [Column("estimated_duration_minutes")]
    public short? EstimatedDurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets NGSS standards as an array.
    /// </summary>
    [Column("ngss_standards", TypeName = "text[]")]
    public List<string> NgssStandards { get; set; } = new();

    /// <summary>
    /// Gets or sets the vocabulary tier (1-3).
    /// </summary>
    [Column("vocabulary_tier")]
    public short? VocabularyTier { get; set; }

    /// <summary>
    /// Gets or sets the source file path.
    /// </summary>
    [MaxLength(500)]
    [Column("source_file")]
    public string? SourceFile { get; set; }

    /// <summary>
    /// Gets or sets the version of the content.
    /// </summary>
    [MaxLength(20)]
    [Column("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets whether this content unit is active.
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets when the content unit was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the content unit was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the learning objectives for this content unit.
    /// </summary>
    public virtual ICollection<LearningObjective> LearningObjectives { get; set; } = new List<LearningObjective>();

    /// <summary>
    /// Gets or sets prerequisites (content units that must be mastered first).
    /// </summary>
    public virtual ICollection<Prerequisite> Prerequisites { get; set; } = new List<Prerequisite>();

    /// <summary>
    /// Gets or sets content units that require this as a prerequisite.
    /// </summary>
    public virtual ICollection<Prerequisite> RequiredBy { get; set; } = new List<Prerequisite>();

    /// <summary>
    /// Gets or sets the embeddings for this content unit.
    /// </summary>
    public virtual ICollection<ContentEmbedding> Embeddings { get; set; } = new List<ContentEmbedding>();

    /// <summary>
    /// Gets or sets the assessment items for this content unit.
    /// </summary>
    public virtual ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();

    /// <summary>
    /// Gets or sets the mastery records for this content unit.
    /// </summary>
    public virtual ICollection<ContentMastery> MasteryRecords { get; set; } = new List<ContentMastery>();
}

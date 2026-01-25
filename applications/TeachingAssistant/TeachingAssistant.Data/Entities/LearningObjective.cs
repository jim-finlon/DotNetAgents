using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a learning objective for a content unit.
/// </summary>
[Table("learning_objectives")]
public class LearningObjective
{
    /// <summary>
    /// Gets or sets the unique identifier for the learning objective.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the content unit ID this learning objective belongs to.
    /// </summary>
    [Required]
    [Column("content_unit_id")]
    public Guid ContentUnitId { get; set; }

    /// <summary>
    /// Gets or sets the description of the learning objective.
    /// </summary>
    [Required]
    [Column("description", TypeName = "text")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Bloom's taxonomy level.
    /// </summary>
    [Required]
    [Column("bloom_level")]
    public BloomLevel BloomLevel { get; set; }

    /// <summary>
    /// Gets or sets the measurable criteria for this objective.
    /// </summary>
    [Column("measurable_criteria", TypeName = "text")]
    public string? MeasurableCriteria { get; set; }

    /// <summary>
    /// Gets or sets the sequence order within the content unit.
    /// </summary>
    [Column("sequence_order")]
    public short SequenceOrder { get; set; } = 0;

    // Navigation properties
    /// <summary>
    /// Gets or sets the content unit this learning objective belongs to.
    /// </summary>
    [ForeignKey(nameof(ContentUnitId))]
    public virtual ContentUnit ContentUnit { get; set; } = null!;

    /// <summary>
    /// Gets or sets the assessment items aligned with this learning objective.
    /// </summary>
    public virtual ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
}

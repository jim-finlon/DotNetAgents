using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a prerequisite relationship between content units.
/// </summary>
[Table("prerequisites")]
public class Prerequisite
{
    /// <summary>
    /// Gets or sets the content unit ID that requires the prerequisite.
    /// </summary>
    [Required]
    [Column("content_unit_id")]
    public Guid ContentUnitId { get; set; }

    /// <summary>
    /// Gets or sets the prerequisite content unit ID.
    /// </summary>
    [Required]
    [Column("prerequisite_unit_id")]
    public Guid PrerequisiteUnitId { get; set; }

    /// <summary>
    /// Gets or sets the strength of the prerequisite (1-3, where 3 is required).
    /// </summary>
    [Column("strength")]
    public short Strength { get; set; } = 2;

    // Navigation properties
    /// <summary>
    /// Gets or sets the content unit that requires this prerequisite.
    /// </summary>
    [ForeignKey(nameof(ContentUnitId))]
    public virtual ContentUnit ContentUnit { get; set; } = null!;

    /// <summary>
    /// Gets or sets the prerequisite content unit.
    /// </summary>
    [ForeignKey(nameof(PrerequisiteUnitId))]
    public virtual ContentUnit PrerequisiteUnit { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a student's progress in a specific subject.
/// </summary>
[Table("student_subject_progress")]
public class StudentSubjectProgress
{
    /// <summary>
    /// Gets or sets the student ID.
    /// </summary>
    [Key]
    [Column("student_id")]
    public Guid StudentId { get; set; }

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    [Key]
    [Column("subject")]
    public Subject Subject { get; set; }

    /// <summary>
    /// Gets or sets the current content unit ID the student is working on.
    /// </summary>
    [Column("current_unit_id")]
    public Guid? CurrentUnitId { get; set; }

    /// <summary>
    /// Gets or sets the overall progress percentage (0-100).
    /// </summary>
    [Column("overall_progress", TypeName = "decimal(5,2)")]
    public decimal OverallProgress { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total time spent in minutes.
    /// </summary>
    [Column("total_time_minutes")]
    public int TotalTimeMinutes { get; set; } = 0;

    /// <summary>
    /// Gets or sets when the student was last active in this subject.
    /// </summary>
    [Column("last_activity_at")]
    public DateTimeOffset? LastActivityAt { get; set; }

    /// <summary>
    /// Gets or sets the current streak in days.
    /// </summary>
    [Column("streak_days")]
    public int StreakDays { get; set; } = 0;

    /// <summary>
    /// Gets or sets the longest streak in days.
    /// </summary>
    [Column("longest_streak")]
    public int LongestStreak { get; set; } = 0;

    /// <summary>
    /// Gets or sets subject-specific settings as JSON.
    /// </summary>
    [Column("settings", TypeName = "jsonb")]
    public Dictionary<string, object> Settings { get; set; } = new();

    // Navigation properties
    /// <summary>
    /// Gets or sets the student this progress belongs to.
    /// </summary>
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current content unit.
    /// </summary>
    [ForeignKey(nameof(CurrentUnitId))]
    public virtual ContentUnit? CurrentUnit { get; set; }
}

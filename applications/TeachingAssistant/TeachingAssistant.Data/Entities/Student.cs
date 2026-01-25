using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a student in the system.
/// </summary>
[Table("students")]
public class Student
{
    /// <summary>
    /// Gets or sets the unique identifier for the student.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the family ID this student belongs to.
    /// </summary>
    [Required]
    [Column("family_id")]
    public Guid FamilyId { get; set; }

    /// <summary>
    /// Gets or sets the student's name (first name only for privacy).
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the student's birthdate (optional, for age-appropriate content).
    /// </summary>
    [Column("birthdate", TypeName = "date")]
    public DateOnly? Birthdate { get; set; }

    /// <summary>
    /// Gets or sets the student's grade level (0-12).
    /// </summary>
    [Column("grade_level")]
    public int? GradeLevel { get; set; }

    /// <summary>
    /// Gets or sets the avatar ID (reference to predefined avatar).
    /// </summary>
    [MaxLength(50)]
    [Column("avatar_id")]
    public string? AvatarId { get; set; }

    /// <summary>
    /// Gets or sets student preferences as JSON.
    /// </summary>
    [Column("preferences", TypeName = "jsonb")]
    public Dictionary<string, object> Preferences { get; set; } = new()
    {
        ["theme"] = "light",
        ["text_size"] = "medium",
        ["audio_enabled"] = false,
        ["celebration_animations"] = true
    };

    /// <summary>
    /// Gets or sets accessibility settings as JSON.
    /// </summary>
    [Column("accessibility_settings", TypeName = "jsonb")]
    public Dictionary<string, object> AccessibilitySettings { get; set; } = new()
    {
        ["high_contrast"] = false,
        ["reduced_motion"] = false,
        ["screen_reader_optimized"] = false
    };

    /// <summary>
    /// Gets or sets when the student was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the student was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the student was deleted (soft delete for COPPA compliance).
    /// </summary>
    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the family this student belongs to.
    /// </summary>
    [ForeignKey(nameof(FamilyId))]
    public virtual Family Family { get; set; } = null!;

    /// <summary>
    /// Gets or sets the subject progress records for this student.
    /// </summary>
    public virtual ICollection<StudentSubjectProgress> SubjectProgress { get; set; } = new List<StudentSubjectProgress>();

    /// <summary>
    /// Gets or sets the content mastery records for this student.
    /// </summary>
    public virtual ICollection<ContentMastery> ContentMasteryRecords { get; set; } = new List<ContentMastery>();

    /// <summary>
    /// Gets or sets the learning sessions for this student.
    /// </summary>
    public virtual ICollection<LearningSession> LearningSessions { get; set; } = new List<LearningSession>();

    /// <summary>
    /// Gets or sets the student responses for this student.
    /// </summary>
    public virtual ICollection<StudentResponse> Responses { get; set; } = new List<StudentResponse>();

    /// <summary>
    /// Gets or sets the conversation threads for this student.
    /// </summary>
    public virtual ICollection<ConversationThread> ConversationThreads { get; set; } = new List<ConversationThread>();
}

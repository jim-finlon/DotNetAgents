using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a family account in the system.
/// </summary>
[Table("families")]
public class Family
{
    /// <summary>
    /// Gets or sets the unique identifier for the family.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the family name.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription tier.
    /// </summary>
    [Column("subscription_tier")]
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;

    /// <summary>
    /// Gets or sets when the subscription expires.
    /// </summary>
    [Column("subscription_expires_at")]
    public DateTimeOffset? SubscriptionExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets family settings as JSON.
    /// </summary>
    [Column("settings", TypeName = "jsonb")]
    public Dictionary<string, object> Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets when the family was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the family was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the family was deleted (soft delete for COPPA compliance).
    /// </summary>
    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the guardians associated with this family.
    /// </summary>
    public virtual ICollection<Guardian> Guardians { get; set; } = new List<Guardian>();

    /// <summary>
    /// Gets or sets the students associated with this family.
    /// </summary>
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}

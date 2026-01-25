using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents a guardian (parent/teacher) associated with a family.
/// </summary>
[Table("guardians")]
public class Guardian
{
    /// <summary>
    /// Gets or sets the unique identifier for the guardian.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the family ID this guardian belongs to.
    /// </summary>
    [Required]
    [Column("family_id")]
    public Guid FamilyId { get; set; }

    /// <summary>
    /// Gets or sets the authentication provider ID (from OAuth provider).
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("auth_provider_id")]
    public string AuthProviderId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the guardian's email address.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the email has been verified.
    /// </summary>
    [Column("email_verified")]
    public bool EmailVerified { get; set; } = false;

    /// <summary>
    /// Gets or sets the guardian's name.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the guardian's role within the family.
    /// </summary>
    [Column("role")]
    public GuardianRole Role { get; set; } = GuardianRole.Primary;

    /// <summary>
    /// Gets or sets permissions as JSON.
    /// </summary>
    [Column("permissions", TypeName = "jsonb")]
    public Dictionary<string, object> Permissions { get; set; } = new()
    {
        ["can_view_progress"] = true,
        ["can_modify_settings"] = true
    };

    /// <summary>
    /// Gets or sets when the guardian last logged in.
    /// </summary>
    [Column("last_login_at")]
    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>
    /// Gets or sets when the guardian was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the guardian was last updated.
    /// </summary>
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the family this guardian belongs to.
    /// </summary>
    [ForeignKey(nameof(FamilyId))]
    public virtual Family Family { get; set; } = null!;
}

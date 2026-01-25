using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace TeachingAssistant.Data.Entities;

/// <summary>
/// Represents an audit log entry for compliance and security tracking.
/// </summary>
[Table("audit_log")]
public class AuditLogEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit log entry.
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the type of actor (student, guardian, system, etc.).
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("actor_type")]
    public string ActorType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the actor ID.
    /// </summary>
    [Column("actor_id")]
    public Guid? ActorId { get; set; }

    /// <summary>
    /// Gets or sets the action performed.
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource type.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("resource_type")]
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource ID.
    /// </summary>
    [Column("resource_id")]
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the old values as JSON.
    /// </summary>
    [Column("old_values", TypeName = "jsonb")]
    public Dictionary<string, object>? OldValues { get; set; }

    /// <summary>
    /// Gets or sets the new values as JSON.
    /// </summary>
    [Column("new_values", TypeName = "jsonb")]
    public Dictionary<string, object>? NewValues { get; set; }

    /// <summary>
    /// Gets or sets the IP address.
    /// </summary>
    [Column("ip_address")]
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent.
    /// </summary>
    [Column("user_agent", TypeName = "text")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets when the audit entry was created.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

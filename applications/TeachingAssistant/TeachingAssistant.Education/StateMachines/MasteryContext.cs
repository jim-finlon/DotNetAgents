using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.StateMachines;

/// <summary>
/// Context object for mastery state machine operations.
/// </summary>
public class MasteryContext
{
    /// <summary>
    /// Gets or sets the student identifier.
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the concept identifier.
    /// </summary>
    public ConceptId? ConceptId { get; set; }

    /// <summary>
    /// Gets or sets the current mastery score (0-100).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Gets or sets the mastery level.
    /// </summary>
    public MasteryLevel Level { get; set; }

    /// <summary>
    /// Gets or sets when the mastery was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the number of assessments completed.
    /// </summary>
    public int AssessmentCount { get; set; }

    /// <summary>
    /// Gets or sets whether prerequisites are met.
    /// </summary>
    public bool PrerequisitesMet { get; set; }

    /// <summary>
    /// Gets or sets the number of errors encountered.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the last error message.
    /// </summary>
    public string? LastErrorMessage { get; set; }
}

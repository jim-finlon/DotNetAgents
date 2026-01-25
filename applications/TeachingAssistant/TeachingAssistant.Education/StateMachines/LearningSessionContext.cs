using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.StateMachines;

/// <summary>
/// Context object for learning session state machine operations.
/// </summary>
public class LearningSessionContext
{
    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    public string SessionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the student identifier.
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the concept being learned.
    /// </summary>
    public ConceptId? ConceptId { get; set; }

    /// <summary>
    /// Gets or sets the current step in the session.
    /// </summary>
    public int CurrentStep { get; set; }

    /// <summary>
    /// Gets or sets the state before pausing (for resume functionality).
    /// </summary>
    public string? PreviousStateBeforePause { get; set; }

    /// <summary>
    /// Gets or sets when the session was initialized.
    /// </summary>
    public DateTimeOffset? InitializedAt { get; set; }

    /// <summary>
    /// Gets or sets when learning started.
    /// </summary>
    public DateTimeOffset? LearningStartedAt { get; set; }

    /// <summary>
    /// Gets or sets when assessment started.
    /// </summary>
    public DateTimeOffset? AssessmentStartedAt { get; set; }

    /// <summary>
    /// Gets or sets when review started.
    /// </summary>
    public DateTimeOffset? ReviewStartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the session was paused.
    /// </summary>
    public DateTimeOffset? PausedAt { get; set; }

    /// <summary>
    /// Gets or sets when the session was completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of errors encountered.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the last error message.
    /// </summary>
    public string? LastErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets whether the session is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether mastery has been achieved.
    /// </summary>
    public bool MasteryAchieved { get; set; }

    /// <summary>
    /// Gets or sets the current mastery score (0-100).
    /// </summary>
    public double? CurrentMasteryScore { get; set; }
}

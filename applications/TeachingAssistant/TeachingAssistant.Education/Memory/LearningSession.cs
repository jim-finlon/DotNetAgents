using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Memory;

/// <summary>
/// Represents a learning session state.
/// </summary>
public record LearningSession
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public string SessionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the student identifier.
    /// </summary>
    public string StudentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the concept being learned in this session.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the current step or checkpoint in the session.
    /// </summary>
    public int CurrentStep { get; init; }

    /// <summary>
    /// Gets the session state data (serializable).
    /// </summary>
    public IDictionary<string, object>? StateData { get; init; }

    /// <summary>
    /// Gets when the session was started.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets when the session was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets when the session was completed (null if still active).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Gets whether the session is currently active.
    /// </summary>
    public bool IsActive => CompletedAt == null;
}

using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Memory;

/// <summary>
/// Represents a student's mastery level for a specific concept.
/// </summary>
public record ConceptMastery
{
    /// <summary>
    /// Gets the concept identifier.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the mastery level.
    /// </summary>
    public MasteryLevel Level { get; init; }

    /// <summary>
    /// Gets the score percentage (0-100).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Gets when the mastery was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; init; } = DateTimeOffset.UtcNow;
}

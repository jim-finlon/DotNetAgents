using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Pedagogy;

/// <summary>
/// Represents an assessment result used for mastery calculation.
/// </summary>
public record AssessmentResult
{
    /// <summary>
    /// Gets the score as a percentage (0-100).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Gets the timestamp when the assessment was completed.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the assessment identifier.
    /// </summary>
    public string AssessmentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets additional metadata about the assessment.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Interface for calculating student mastery levels from assessment history.
/// </summary>
public interface IMasteryCalculator
{
    /// <summary>
    /// Calculates the mastery level for a concept based on assessment history.
    /// </summary>
    /// <param name="concept">The concept identifier.</param>
    /// <param name="history">The assessment history for this concept.</param>
    /// <returns>The calculated mastery level.</returns>
    MasteryLevel CalculateMastery(ConceptId concept, IReadOnlyList<AssessmentResult> history);

    /// <summary>
    /// Checks whether the student meets the prerequisites for a target concept.
    /// </summary>
    /// <param name="targetConcept">The target concept.</param>
    /// <param name="studentMastery">A dictionary mapping concept IDs to mastery levels.</param>
    /// <returns>True if prerequisites are met, false otherwise.</returns>
    bool MeetsPrerequisites(
        ConceptId targetConcept,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery);

    /// <summary>
    /// Gets a list of concepts that are ready for learning based on prerequisite mastery.
    /// </summary>
    /// <param name="availableConcepts">The list of available concepts.</param>
    /// <param name="studentMastery">A dictionary mapping concept IDs to mastery levels.</param>
    /// <returns>A list of concept IDs that are ready for learning.</returns>
    IReadOnlyList<ConceptId> GetReadyConcepts(
        IReadOnlyList<ConceptId> availableConcepts,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery);
}

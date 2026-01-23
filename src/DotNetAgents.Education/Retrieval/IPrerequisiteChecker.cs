using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Retrieval;

/// <summary>
/// Interface for checking concept prerequisites.
/// </summary>
public interface IPrerequisiteChecker
{
    /// <summary>
    /// Checks if a student meets the prerequisites for a concept.
    /// </summary>
    /// <param name="conceptId">The concept to check prerequisites for.</param>
    /// <param name="studentMastery">The student's current mastery levels.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if prerequisites are met, false otherwise.</returns>
    Task<bool> CheckPrerequisitesAsync(
        ConceptId conceptId,
        IReadOnlyDictionary<ConceptId, ConceptMastery> studentMastery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the missing prerequisites for a concept.
    /// </summary>
    /// <param name="conceptId">The concept to check.</param>
    /// <param name="studentMastery">The student's current mastery levels.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of concept IDs that are missing prerequisites.</returns>
    Task<IReadOnlyList<ConceptId>> GetMissingPrerequisitesAsync(
        ConceptId conceptId,
        IReadOnlyDictionary<ConceptId, ConceptMastery> studentMastery,
        CancellationToken cancellationToken = default);
}

using DotNetAgents.Core.Retrieval;
using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Retrieval;

/// <summary>
/// Interface for curriculum-aware retrieval that filters content based on student mastery and prerequisites.
/// </summary>
public interface ICurriculumAwareRetriever
{
    /// <summary>
    /// Retrieves relevant content for a student, filtered by their mastery level and prerequisites.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="conceptId">Optional concept to focus on.</param>
    /// <param name="topK">The number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of relevant content filtered by curriculum requirements.</returns>
    Task<IReadOnlyList<RetrievedContent>> RetrieveAsync(
        string query,
        string studentId,
        ConceptId? conceptId = null,
        int topK = 5,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents content retrieved for educational purposes.
/// </summary>
public record RetrievedContent
{
    /// <summary>
    /// Gets the content identifier.
    /// </summary>
    public string ContentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the content text.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the concept this content relates to.
    /// </summary>
    public ConceptId? ConceptId { get; init; }

    /// <summary>
    /// Gets the similarity score.
    /// </summary>
    public float SimilarityScore { get; init; }

    /// <summary>
    /// Gets the grade level appropriateness.
    /// </summary>
    public GradeLevel? GradeLevel { get; init; }

    /// <summary>
    /// Gets additional metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

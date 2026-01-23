using DotNetAgents.Core.Models;
using DotNetAgents.Core.Retrieval;
using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Retrieval;

/// <summary>
/// Implementation of curriculum-aware retrieval that filters content based on student mastery.
/// </summary>
public class CurriculumAwareRetriever : ICurriculumAwareRetriever
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingModel _embeddingModel;
    private readonly MasteryStateMemory _masteryMemory;
    private readonly IMasteryCalculator _masteryCalculator;
    private readonly IPrerequisiteChecker _prerequisiteChecker;
    private readonly ILogger<CurriculumAwareRetriever> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurriculumAwareRetriever"/> class.
    /// </summary>
    /// <param name="vectorStore">The vector store to search.</param>
    /// <param name="embeddingModel">The embedding model for query embedding.</param>
    /// <param name="masteryMemory">The mastery state memory.</param>
    /// <param name="masteryCalculator">The mastery calculator.</param>
    /// <param name="prerequisiteChecker">The prerequisite checker.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public CurriculumAwareRetriever(
        IVectorStore vectorStore,
        IEmbeddingModel embeddingModel,
        MasteryStateMemory masteryMemory,
        IMasteryCalculator masteryCalculator,
        IPrerequisiteChecker prerequisiteChecker,
        ILogger<CurriculumAwareRetriever>? logger = null)
    {
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _embeddingModel = embeddingModel ?? throw new ArgumentNullException(nameof(embeddingModel));
        _masteryMemory = masteryMemory ?? throw new ArgumentNullException(nameof(masteryMemory));
        _masteryCalculator = masteryCalculator ?? throw new ArgumentNullException(nameof(masteryCalculator));
        _prerequisiteChecker = prerequisiteChecker ?? throw new ArgumentNullException(nameof(prerequisiteChecker));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<CurriculumAwareRetriever>.Instance;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RetrievedContent>> RetrieveAsync(
        string query,
        string studentId,
        ConceptId? conceptId = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (topK <= 0)
            throw new ArgumentException("TopK must be positive.", nameof(topK));

        _logger.LogDebug(
            "Retrieving content for student {StudentId}, query: {Query}, concept: {ConceptId}",
            studentId,
            query,
            conceptId?.Value ?? "any");

        // Get student mastery levels
        var studentMastery = await _masteryMemory.GetAllMasteryAsync(studentId, cancellationToken)
            .ConfigureAwait(false);

        // Generate query embedding
        var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken).ConfigureAwait(false);

        // Build metadata filter based on concept and prerequisites
        var filter = BuildFilter(studentId, conceptId, studentMastery);

        // Search vector store with filter
        var searchResults = await _vectorStore.SearchAsync(
            queryEmbedding,
            topK: topK * 2, // Retrieve more to filter down
            filter: filter,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        // Filter results based on prerequisites and mastery
        var filteredResults = await FilterByPrerequisitesAsync(
            searchResults,
            studentMastery,
            cancellationToken).ConfigureAwait(false);

        // Convert to RetrievedContent and take topK
        var retrievedContent = filteredResults
            .Take(topK)
            .Select(ConvertToRetrievedContent)
            .ToList();

        _logger.LogInformation(
            "Retrieved {Count} content items for student {StudentId}",
            retrievedContent.Count,
            studentId);

        return retrievedContent;
    }

    private IDictionary<string, object>? BuildFilter(
        string studentId,
        ConceptId? conceptId,
        IReadOnlyDictionary<ConceptId, ConceptMastery> studentMastery)
    {
        var filter = new Dictionary<string, object>();

        if (conceptId != null)
        {
            filter["concept_id"] = conceptId.Value;
        }

        // Filter out concepts the student has already mastered (optional - can be configurable)
        // For now, we'll allow review of mastered concepts

        return filter.Count > 0 ? filter : null;
    }

    private async Task<IReadOnlyList<VectorSearchResult>> FilterByPrerequisitesAsync(
        IReadOnlyList<VectorSearchResult> results,
        IReadOnlyDictionary<ConceptId, ConceptMastery> studentMastery,
        CancellationToken cancellationToken)
    {
        var filtered = new List<VectorSearchResult>();

        foreach (var result in results)
        {
            // Extract concept ID from metadata
            var conceptId = ExtractConceptId(result.Metadata);
            if (conceptId != null)
            {
                // Check prerequisites
                var meetsPrerequisites = await _prerequisiteChecker.CheckPrerequisitesAsync(
                    conceptId,
                    studentMastery,
                    cancellationToken).ConfigureAwait(false);

                if (meetsPrerequisites)
                {
                    filtered.Add(result);
                }
                else
                {
                    _logger.LogDebug(
                        "Filtered out content {ContentId} - prerequisites not met",
                        result.Id);
                }
            }
            else
            {
                // No concept ID - include it (might be general content)
                filtered.Add(result);
            }
        }

        return filtered;
    }

    private static ConceptId? ExtractConceptId(IDictionary<string, object>? metadata)
    {
        if (metadata == null)
            return null;

        if (!metadata.TryGetValue("concept_id", out var conceptIdObj) || conceptIdObj is not string conceptIdStr)
            return null;

        var subject = metadata.TryGetValue("subject", out var subjectObj) &&
            Enum.TryParse<SubjectArea>(subjectObj?.ToString(), out var parsedSubject)
            ? parsedSubject
            : SubjectArea.Other;

        var gradeLevel = metadata.TryGetValue("grade_level", out var gradeObj) &&
            Enum.TryParse<GradeLevel>(gradeObj?.ToString(), out var parsedGrade)
            ? parsedGrade
            : GradeLevel.G3_5; // Default

        return new ConceptId(conceptIdStr, subject, gradeLevel);
    }

    private static RetrievedContent ConvertToRetrievedContent(VectorSearchResult result)
    {
        var conceptId = ExtractConceptId(result.Metadata);

        var gradeLevel = result.Metadata?.TryGetValue("grade_level", out var gradeObj) == true &&
            Enum.TryParse<GradeLevel>(gradeObj?.ToString(), out var grade)
            ? (GradeLevel?)grade
            : null;

        var content = result.Metadata?.TryGetValue("content", out var contentObj) == true
            ? contentObj?.ToString() ?? string.Empty
            : string.Empty;

        return new RetrievedContent
        {
            ContentId = result.Id,
            Content = content,
            ConceptId = conceptId,
            SimilarityScore = result.Score,
            GradeLevel = gradeLevel,
            Metadata = result.Metadata
        };
    }
}

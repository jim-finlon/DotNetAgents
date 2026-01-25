using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.Retrieval;
using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.Retrieval;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeachingAssistant.Data;
using TeachingAssistant.Data.Entities;

namespace DotNetAgents.Education.RAG;

/// <summary>
/// Enhanced RAG service for curriculum-aware content retrieval with query expansion and reranking.
/// </summary>
public class CurriculumRagService
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingModel _embeddingModel;
    private readonly TeachingAssistantDbContext _dbContext;
    private readonly MasteryStateMemory _masteryMemory;
    private readonly IPrerequisiteChecker _prerequisiteChecker;
    private readonly ILogger<CurriculumRagService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurriculumRagService"/> class.
    /// </summary>
    public CurriculumRagService(
        IVectorStore vectorStore,
        IEmbeddingModel embeddingModel,
        TeachingAssistantDbContext dbContext,
        MasteryStateMemory masteryMemory,
        IPrerequisiteChecker prerequisiteChecker,
        ILogger<CurriculumRagService> logger)
    {
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _embeddingModel = embeddingModel ?? throw new ArgumentNullException(nameof(embeddingModel));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _masteryMemory = masteryMemory ?? throw new ArgumentNullException(nameof(masteryMemory));
        _prerequisiteChecker = prerequisiteChecker ?? throw new ArgumentNullException(nameof(prerequisiteChecker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves curriculum content using RAG with student context.
    /// </summary>
    /// <param name="query">The student's query.</param>
    /// <param name="studentId">The student ID.</param>
    /// <param name="subject">Optional subject filter.</param>
    /// <param name="gradeBand">Optional grade band filter.</param>
    /// <param name="topK">Number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Retrieved curriculum content chunks.</returns>
    public async Task<RagContext> RetrieveContextAsync(
        string query,
        string studentId,
        Subject? subject = null,
        GradeBand? gradeBand = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        _logger.LogDebug(
            "Retrieving curriculum content for student {StudentId}, query: {Query}",
            studentId,
            query);

        // 1. Query expansion
        var expandedQuery = await ExpandQueryAsync(query, studentId, subject, gradeBand, cancellationToken);

        // 2. Generate query embedding
        var queryEmbedding = await _embeddingModel.EmbedAsync(expandedQuery, cancellationToken);

        // 3. Build metadata filter
        var filter = BuildMetadataFilter(studentId, subject, gradeBand);

        // 4. Vector search
        var searchResults = await _vectorStore.SearchAsync(
            queryEmbedding,
            topK: topK * 3, // Retrieve more for filtering and reranking
            filter: filter,
            cancellationToken: cancellationToken);

        // 5. Filter by grade band and prerequisites
        var filteredResults = await FilterResultsAsync(
            searchResults,
            studentId,
            gradeBand,
            cancellationToken);

        // 6. Rerank results (if reranking service available)
        var rerankedResults = await RerankAsync(query, filteredResults, cancellationToken);

        // 7. Take top chunks
        var topChunks = rerankedResults
            .Take(topK)
            .Select(r => new ContentChunk
            {
                ContentUnitId = ExtractContentUnitId(r.Id),
                ChunkIndex = ExtractChunkIndex(r.Id),
                Text = ExtractChunkText(r.Metadata),
                RelevanceScore = r.Score,
                Title = ExtractTitle(r.Metadata),
                TopicPath = ExtractTopicPath(r.Metadata)
            })
            .ToList();

        // 8. Load full content units from database
        var contentUnitIds = topChunks.Select(c => c.ContentUnitId).Distinct().ToList();
        var contentUnits = await _dbContext.ContentUnits
            .Where(cu => contentUnitIds.Contains(cu.Id))
            .Include(cu => cu.LearningObjectives)
            .ToListAsync(cancellationToken);

        // 9. Assemble context with pedagogical framing
        var assembledContext = AssembleContext(topChunks, contentUnits, studentId);

        return new RagContext
        {
            RetrievedChunks = topChunks,
            AssembledContext = assembledContext,
            SourceContentIds = contentUnitIds
        };
    }

    private async Task<string> ExpandQueryAsync(
        string query,
        string studentId,
        Subject? subject,
        GradeBand? gradeBand,
        CancellationToken cancellationToken)
    {
        // Simple query expansion - in production, use LLM for better expansion
        var expanded = query;

        // Add subject-specific terms if subject is known
        if (subject.HasValue)
        {
            var subjectTerms = GetSubjectTerms(subject.Value);
            expanded = $"{query} {string.Join(" ", subjectTerms)}";
        }

        return expanded;
    }

    private static string[] GetSubjectTerms(Subject subject)
    {
        return subject switch
        {
            Subject.Biology => new[] { "living", "organism", "cell", "life" },
            Subject.Chemistry => new[] { "matter", "atom", "molecule", "reaction" },
            Subject.Physics => new[] { "force", "energy", "motion", "wave" },
            Subject.EarthScience => new[] { "earth", "rock", "weather", "climate" },
            Subject.Astronomy => new[] { "space", "planet", "star", "galaxy" },
            Subject.EnvironmentalScience => new[] { "ecosystem", "environment", "conservation" },
            Subject.Mathematics => new[] { "number", "equation", "calculation", "problem" },
            _ => Array.Empty<string>()
        };
    }

    private IDictionary<string, object>? BuildMetadataFilter(
        string studentId,
        Subject? subject,
        GradeBand? gradeBand)
    {
        var filter = new Dictionary<string, object>();

        if (subject.HasValue)
        {
            filter["subject"] = subject.Value.ToString();
        }

        if (gradeBand.HasValue)
        {
            filter["grade_band"] = gradeBand.Value.ToString();
        }

        return filter.Count > 0 ? filter : null;
    }

    private async Task<IReadOnlyList<VectorSearchResult>> FilterResultsAsync(
        IReadOnlyList<VectorSearchResult> results,
        string studentId,
        GradeBand? gradeBand,
        CancellationToken cancellationToken)
    {
        var filtered = new List<VectorSearchResult>();

        // Get student mastery
        var studentMastery = await _masteryMemory.GetAllMasteryAsync(studentId, cancellationToken);

        foreach (var result in results)
        {
            var contentUnitId = ExtractContentUnitId(result.Id);
            if (contentUnitId == Guid.Empty)
            {
                continue;
            }

            // Check grade band compatibility
            if (gradeBand.HasValue)
            {
                var resultGradeBand = ExtractGradeBand(result.Metadata);
                if (resultGradeBand.HasValue && !IsGradeCompatible(gradeBand.Value, resultGradeBand.Value))
                {
                    continue;
                }
            }

            // Check prerequisites (if content unit exists in DB)
            var contentUnit = await _dbContext.ContentUnits
                .Include(cu => cu.Prerequisites)
                .FirstOrDefaultAsync(cu => cu.Id == contentUnitId, cancellationToken);

            if (contentUnit != null)
            {
                var meetsPrerequisites = await CheckPrerequisitesAsync(
                    contentUnit,
                    studentMastery,
                    cancellationToken);

                if (!meetsPrerequisites)
                {
                    _logger.LogDebug(
                        "Filtered out content unit {ContentUnitId} - prerequisites not met",
                        contentUnitId);
                    continue;
                }
            }

            filtered.Add(result);
        }

        return filtered;
    }

    private async Task<bool> CheckPrerequisitesAsync(
        ContentUnit contentUnit,
        IReadOnlyDictionary<ConceptId, ConceptMastery> studentMastery,
        CancellationToken cancellationToken)
    {
        if (!contentUnit.Prerequisites.Any())
        {
            return true; // No prerequisites
        }

        foreach (var prereq in contentUnit.Prerequisites)
        {
            var prereqUnit = await _dbContext.ContentUnits
                .FirstOrDefaultAsync(cu => cu.Id == prereq.PrerequisiteUnitId, cancellationToken);

            if (prereqUnit == null)
            {
                continue;
            }

            // Convert to ConceptId for mastery check
            var conceptId = new ConceptId(
                prereqUnit.Id.ToString(),
                MapSubject(prereqUnit.Subject),
                MapGradeBand(prereqUnit.GradeBand));

            if (!studentMastery.TryGetValue(conceptId, out var mastery) ||
                mastery.Level < MasteryLevel.Proficient)
            {
                return false; // Prerequisite not met
            }
        }

        return true;
    }

    private static SubjectArea MapSubject(Subject subject)
    {
        return subject switch
        {
            Subject.Biology => SubjectArea.Science,
            Subject.Chemistry => SubjectArea.Science,
            Subject.Physics => SubjectArea.Science,
            Subject.EarthScience => SubjectArea.Science,
            Subject.Astronomy => SubjectArea.Science,
            Subject.EnvironmentalScience => SubjectArea.Science,
            Subject.Mathematics => SubjectArea.Mathematics,
            _ => SubjectArea.Other
        };
    }

    private static GradeLevel MapGradeBand(GradeBand gradeBand)
    {
        return gradeBand switch
        {
            GradeBand.K2 => GradeLevel.K2,
            GradeBand.G3_5 => GradeLevel.G3_5,
            GradeBand.G6_8 => GradeLevel.G6_8,
            GradeBand.G9_10 => GradeLevel.G9_10,
            GradeBand.G11_12 => GradeLevel.G11_12,
            _ => GradeLevel.G6_8
        };
    }

    private static bool IsGradeCompatible(GradeBand studentGrade, GradeBand contentGrade)
    {
        // Allow content from same or one level below
        var gradeOrder = new[] { GradeBand.K2, GradeBand.G3_5, GradeBand.G6_8, GradeBand.G9_10, GradeBand.G11_12 };
        var studentIdx = Array.IndexOf(gradeOrder, studentGrade);
        var contentIdx = Array.IndexOf(gradeOrder, contentGrade);

        return contentIdx <= studentIdx && contentIdx >= studentIdx - 1;
    }

    private async Task<IReadOnlyList<VectorSearchResult>> RerankAsync(
        string query,
        IReadOnlyList<VectorSearchResult> candidates,
        CancellationToken cancellationToken)
    {
        // TODO: Implement reranking using Python service or cross-encoder model
        // For now, return candidates as-is (sorted by similarity score)
        return candidates.OrderByDescending(r => r.Score).ToList();
    }

    private string AssembleContext(
        List<ContentChunk> chunks,
        List<ContentUnit> contentUnits,
        string studentId)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## Relevant Curriculum Content");
        sb.AppendLine();

        var contentUnitLookup = contentUnits.ToDictionary(cu => cu.Id);

        foreach (var chunk in chunks)
        {
            if (contentUnitLookup.TryGetValue(chunk.ContentUnitId, out var contentUnit))
            {
                sb.AppendLine($"### {contentUnit.Title}");
                sb.AppendLine($"**Subject:** {contentUnit.Subject}");
                sb.AppendLine($"**Grade Band:** {contentUnit.GradeBand}");
                if (contentUnit.LearningObjectives.Any())
                {
                    sb.AppendLine("**Learning Objectives:**");
                    foreach (var lo in contentUnit.LearningObjectives.OrderBy(lo => lo.SequenceOrder))
                    {
                        sb.AppendLine($"- {lo.Description}");
                    }
                }
                sb.AppendLine();
                sb.AppendLine(chunk.Text);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    // Helper methods for extracting data from vector search results
    private static Guid ExtractContentUnitId(string vectorId)
    {
        // Vector ID format: "{contentUnitId}_{chunkIndex}"
        var parts = vectorId.Split('_');
        if (parts.Length >= 1 && Guid.TryParse(parts[0], out var guid))
        {
            return guid;
        }
        return Guid.Empty;
    }

    private static int ExtractChunkIndex(string vectorId)
    {
        var parts = vectorId.Split('_');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var index))
        {
            return index;
        }
        return 0;
    }

    private static string ExtractChunkText(IDictionary<string, object>? metadata)
    {
        return metadata?.TryGetValue("chunk_text", out var text) == true
            ? text?.ToString() ?? string.Empty
            : string.Empty;
    }

    private static string ExtractTitle(IDictionary<string, object>? metadata)
    {
        return metadata?.TryGetValue("title", out var title) == true
            ? title?.ToString() ?? string.Empty
            : string.Empty;
    }

    private static List<string> ExtractTopicPath(IDictionary<string, object>? metadata)
    {
        if (metadata?.TryGetValue("topic_path", out var path) == true)
        {
            if (path is string pathStr)
            {
                return pathStr.Split('/').ToList();
            }
        }
        return new List<string>();
    }

    private static GradeBand? ExtractGradeBand(IDictionary<string, object>? metadata)
    {
        if (metadata?.TryGetValue("grade_band", out var gradeBand) == true &&
            Enum.TryParse<GradeBand>(gradeBand?.ToString(), out var parsed))
        {
            return parsed;
        }
        return null;
    }
}

/// <summary>
/// Represents retrieved curriculum content chunk.
/// </summary>
public class ContentChunk
{
    public Guid ContentUnitId { get; set; }
    public int ChunkIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> TopicPath { get; set; } = new();
}

/// <summary>
/// Represents the assembled RAG context.
/// </summary>
public class RagContext
{
    public List<ContentChunk> RetrievedChunks { get; set; } = new();
    public string AssembledContext { get; set; } = string.Empty;
    public List<Guid> SourceContentIds { get; set; } = new();
}

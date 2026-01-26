using DotNetAgents.Knowledge;
using DotNetAgents.Knowledge.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary.HiveMind;

/// <summary>
/// Evaluates novelty by comparing knowledge items to existing knowledge base.
/// </summary>
public sealed class NoveltyEvaluator : INoveltyEvaluator
{
    private readonly IKnowledgeRepository _knowledgeRepository;
    private readonly ILogger<NoveltyEvaluator>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoveltyEvaluator"/> class.
    /// </summary>
    /// <param name="knowledgeRepository">The knowledge repository.</param>
    /// <param name="logger">Optional logger instance.</param>
    public NoveltyEvaluator(
        IKnowledgeRepository knowledgeRepository,
        ILogger<NoveltyEvaluator>? logger = null)
    {
        _knowledgeRepository = knowledgeRepository ?? throw new ArgumentNullException(nameof(knowledgeRepository));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<double> EvaluateNoveltyAsync(
        KnowledgeItem knowledge,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);

        // Check for exact duplicates first
        var duplicate = await _knowledgeRepository.FindDuplicateAsync(
            knowledge.Title,
            knowledge.Description,
            cancellationToken).ConfigureAwait(false);

        if (duplicate != null)
        {
            return 0.0; // Not novel at all
        }

        // Search for similar knowledge
        var searchText = $"{knowledge.Title} {knowledge.Description}";
        var similarKnowledge = await _knowledgeRepository.SearchKnowledgeAsync(
            searchText,
            sessionId: null,
            includeGlobal: true,
            cancellationToken).ConfigureAwait(false);

        if (similarKnowledge.Count == 0)
        {
            return 1.0; // Completely novel
        }

        // Calculate similarity-based novelty
        // More similar items = lower novelty
        // This is a simplified approach - full implementation would use embeddings
        var similarityScore = CalculateSimilarity(knowledge, similarKnowledge);
        var noveltyScore = 1.0 - similarityScore;

        _logger?.LogDebug(
            "Novelty evaluation: Score={Novelty}, SimilarItems={Count}",
            noveltyScore,
            similarKnowledge.Count);

        return Math.Max(0.0, Math.Min(1.0, noveltyScore));
    }

    private static double CalculateSimilarity(KnowledgeItem knowledge, IReadOnlyList<KnowledgeItem> similarItems)
    {
        if (similarItems.Count == 0)
            return 0.0;

        // Simple keyword-based similarity
        var knowledgeText = $"{knowledge.Title} {knowledge.Description}".ToUpperInvariant();
        var knowledgeWords = knowledgeText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var maxSimilarity = 0.0;

        foreach (var similar in similarItems)
        {
            var similarText = $"{similar.Title} {similar.Description}".ToUpperInvariant();
            var similarWords = similarText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet();

            var intersection = knowledgeWords.Intersect(similarWords).Count();
            var union = knowledgeWords.Union(similarWords).Count();

            if (union > 0)
            {
                var jaccardSimilarity = (double)intersection / union;
                maxSimilarity = Math.Max(maxSimilarity, jaccardSimilarity);
            }
        }

        return maxSimilarity;
    }
}

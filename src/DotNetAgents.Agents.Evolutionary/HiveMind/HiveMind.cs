using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Knowledge;
using DotNetAgents.Knowledge.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary.HiveMind;

/// <summary>
/// Enhanced hive mind implementation with novelty detection and knowledge extraction.
/// </summary>
public sealed class HiveMind : IHiveMind
{
    private readonly IKnowledgeRepository _knowledgeRepository;
    private readonly INoveltyEvaluator _noveltyEvaluator;
    private readonly IKnowledgeExtractor _knowledgeExtractor;
    private readonly ILogger<HiveMind>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HiveMind"/> class.
    /// </summary>
    /// <param name="knowledgeRepository">The knowledge repository.</param>
    /// <param name="noveltyEvaluator">The novelty evaluator.</param>
    /// <param name="knowledgeExtractor">The knowledge extractor.</param>
    /// <param name="logger">Optional logger instance.</param>
    public HiveMind(
        IKnowledgeRepository knowledgeRepository,
        INoveltyEvaluator noveltyEvaluator,
        IKnowledgeExtractor knowledgeExtractor,
        ILogger<HiveMind>? logger = null)
    {
        _knowledgeRepository = knowledgeRepository ?? throw new ArgumentNullException(nameof(knowledgeRepository));
        _noveltyEvaluator = noveltyEvaluator ?? throw new ArgumentNullException(nameof(noveltyEvaluator));
        _knowledgeExtractor = knowledgeExtractor ?? throw new ArgumentNullException(nameof(knowledgeExtractor));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<double> EvaluateNoveltyAsync(
        KnowledgeItem knowledge,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);

        return await _noveltyEvaluator.EvaluateNoveltyAsync(knowledge, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<KnowledgeItem>> ExtractKnowledgeAsync(
        Guid chromosomeId,
        int generation,
        string taskInput,
        string taskResult,
        FitnessResult fitnessResult,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskInput);
        ArgumentNullException.ThrowIfNull(fitnessResult);

        return await _knowledgeExtractor.ExtractKnowledgeAsync(
            chromosomeId,
            generation,
            taskInput,
            taskResult,
            fitnessResult,
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> StoreIfNovelAsync(
        KnowledgeItem knowledge,
        Guid chromosomeId,
        int generation,
        double noveltyThreshold = 0.8,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);

        // Evaluate novelty
        var noveltyScore = await EvaluateNoveltyAsync(knowledge, cancellationToken).ConfigureAwait(false);

        if (noveltyScore < noveltyThreshold)
        {
            _logger?.LogDebug(
                "Knowledge not novel enough: Score={Score}, Threshold={Threshold}",
                noveltyScore,
                noveltyThreshold);
            return false;
        }

        // Check for duplicates
        var duplicate = await _knowledgeRepository.FindDuplicateAsync(
            knowledge.Title,
            knowledge.Description,
            cancellationToken).ConfigureAwait(false);

        if (duplicate != null)
        {
            _logger?.LogDebug("Duplicate knowledge found, not storing");
            return false;
        }

        // Add provenance metadata
        var knowledgeWithProvenance = knowledge with
        {
            Metadata = new Dictionary<string, string>(knowledge.Metadata)
            {
                ["ChromosomeId"] = chromosomeId.ToString(),
                ["Generation"] = generation.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["NoveltyScore"] = noveltyScore.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)
            }
        };

        // Store in knowledge repository
        await _knowledgeRepository.AddKnowledgeAsync(knowledgeWithProvenance, cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation(
            "Stored novel knowledge: Title={Title}, Novelty={Novelty}, Chromosome={ChromosomeId}, Generation={Generation}",
            knowledge.Title,
            noveltyScore,
            chromosomeId,
            generation);

        return true;
    }

    /// <inheritdoc/>
    public async Task<List<KnowledgeItem>> GetRelevantKnowledgeAsync(
        AgentChromosome chromosome,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chromosome);

        // Extract tech stack and tags from chromosome
        var techStack = new List<string>();
        var tags = new List<string> { "evolution", "genetic-algorithm" };

        if (chromosome.Model != null)
        {
            techStack.Add(chromosome.Model.Provider);
            tags.Add(chromosome.Model.ModelIdentifier);
        }

        // Get relevant knowledge
        var relevantKnowledge = await _knowledgeRepository.GetRelevantKnowledgeAsync(
            techStack,
            tags,
            maxResults,
            cancellationToken).ConfigureAwait(false);

        return relevantKnowledge.ToList();
    }
}

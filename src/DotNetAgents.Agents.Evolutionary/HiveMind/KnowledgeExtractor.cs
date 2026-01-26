using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Knowledge.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary.HiveMind;

/// <summary>
/// Extracts knowledge from agent execution results.
/// </summary>
public sealed class KnowledgeExtractor : IKnowledgeExtractor
{
    private readonly ILogger<KnowledgeExtractor>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="KnowledgeExtractor"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    public KnowledgeExtractor(ILogger<KnowledgeExtractor>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<List<KnowledgeItem>> ExtractKnowledgeAsync(
        Guid chromosomeId,
        int generation,
        string taskInput,
        string taskResult,
        FitnessResult fitnessResult,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskInput);
        ArgumentNullException.ThrowIfNull(fitnessResult);

        var knowledgeItems = new List<KnowledgeItem>();

        // Extract solution pattern if task was successful
        if (fitnessResult.CompletionRate > 0.8 && fitnessResult.QualityScore > 0.7)
        {
            var solutionKnowledge = new KnowledgeItem
            {
                Title = $"Successful solution pattern (Generation {generation})",
                Description = $"Task: {taskInput}\nSolution: {taskResult}",
                Context = taskInput,
                Solution = taskResult,
                Category = KnowledgeCategory.Solution,
                Severity = KnowledgeSeverity.Info,
                Tags = new[] { "evolution", "solution-pattern", $"generation-{generation}" },
                Metadata = new Dictionary<string, string>
                {
                    ["ChromosomeId"] = chromosomeId.ToString(),
                    ["Generation"] = generation.ToString(),
                    ["Fitness"] = fitnessResult.OverallFitness.ToString("F3"),
                    ["CompletionRate"] = fitnessResult.CompletionRate.ToString("F3"),
                    ["QualityScore"] = fitnessResult.QualityScore.ToString("F3")
                }
            };

            knowledgeItems.Add(solutionKnowledge);
        }

        // Extract failure pattern if task failed
        if (fitnessResult.CompletionRate < 0.5)
        {
            var failureKnowledge = new KnowledgeItem
            {
                Title = $"Failure pattern (Generation {generation})",
                Description = $"Task: {taskInput}\nResult: {taskResult}",
                Context = taskInput,
                ErrorMessage = $"Low completion rate: {fitnessResult.CompletionRate:F3}",
                Category = KnowledgeCategory.Error,
                Severity = KnowledgeSeverity.Warning,
                Tags = new[] { "evolution", "failure-pattern", $"generation-{generation}" },
                Metadata = new Dictionary<string, string>
                {
                    ["ChromosomeId"] = chromosomeId.ToString(),
                    ["Generation"] = generation.ToString(),
                    ["Fitness"] = fitnessResult.OverallFitness.ToString("F3")
                }
            };

            knowledgeItems.Add(failureKnowledge);
        }

        // Extract efficiency insights
        if (fitnessResult.EfficiencyScore < 0.5)
        {
            var efficiencyKnowledge = new KnowledgeItem
            {
                Title = $"Efficiency issue (Generation {generation})",
                Description = $"Task took longer than expected. Efficiency score: {fitnessResult.EfficiencyScore:F3}",
                Category = KnowledgeCategory.Performance,
                Severity = KnowledgeSeverity.Warning,
                Tags = new[] { "evolution", "efficiency", "performance", $"generation-{generation}" },
                Metadata = new Dictionary<string, string>
                {
                    ["ChromosomeId"] = chromosomeId.ToString(),
                    ["Generation"] = generation.ToString(),
                    ["EfficiencyScore"] = fitnessResult.EfficiencyScore.ToString("F3")
                }
            };

            knowledgeItems.Add(efficiencyKnowledge);
        }

        _logger?.LogDebug(
            "Extracted {Count} knowledge items from chromosome {ChromosomeId}, generation {Generation}",
            knowledgeItems.Count,
            chromosomeId,
            generation);

        return Task.FromResult(knowledgeItems);
    }
}

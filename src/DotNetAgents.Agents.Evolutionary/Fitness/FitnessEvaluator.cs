using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Agents.Evolutionary.Integration;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.Tools;
using DotNetAgents.Core.Agents;
using DotNetAgents.Core.Prompts;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary.Fitness;

/// <summary>
/// Evaluates agent fitness by executing agents on evaluation tasks and measuring performance.
/// </summary>
public sealed class FitnessEvaluator : IFitnessEvaluator
{
    private readonly FitnessConfig _config;
    private readonly ChromosomeAdapter _adapter;
    private readonly ILLMModel<string, string> _llm;
    private readonly IToolRegistry _toolRegistry;
    private readonly ILogger<FitnessEvaluator>? _logger;
    private readonly List<EvaluationTask> _evaluationTasks;

    /// <summary>
    /// Initializes a new instance of the <see cref="FitnessEvaluator"/> class.
    /// </summary>
    /// <param name="config">Fitness evaluation configuration.</param>
    /// <param name="adapter">Chromosome adapter for creating agent executors.</param>
    /// <param name="llm">The LLM model to use for agent execution.</param>
    /// <param name="toolRegistry">The tool registry.</param>
    /// <param name="evaluationTasks">List of tasks to evaluate agents on.</param>
    /// <param name="logger">Optional logger instance.</param>
    public FitnessEvaluator(
        FitnessConfig config,
        ChromosomeAdapter adapter,
        ILLMModel<string, string> llm,
        IToolRegistry toolRegistry,
        List<EvaluationTask> evaluationTasks,
        ILogger<FitnessEvaluator>? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _llm = llm ?? throw new ArgumentNullException(nameof(llm));
        _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
        _evaluationTasks = evaluationTasks ?? throw new ArgumentNullException(nameof(evaluationTasks));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<FitnessResult> EvaluateAsync(
        AgentChromosome chromosome,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chromosome);

        var startTime = DateTimeOffset.UtcNow;
        var result = new FitnessResult();

        try
        {
            // Validate chromosome
            if (!ChromosomeAdapter.ValidateChromosome(chromosome))
            {
                _logger?.LogWarning("Chromosome {ChromosomeId} failed validation", chromosome.Id);
                return result; // Return zero fitness
            }

            // Create agent executor from chromosome
            var agentExecutor = _adapter.ToAgentExecutor(chromosome, _llm, _toolRegistry);

            // Select tasks to evaluate (random sample)
            var tasksToEvaluate = _evaluationTasks
                .OrderBy(_ => Random.Shared.Next())
                .Take(_config.TasksPerAgent)
                .ToList();

            if (tasksToEvaluate.Count == 0)
            {
                _logger?.LogWarning("No evaluation tasks available");
                return result;
            }

            var completedTasks = 0;
            var qualityScores = new List<double>();
            var executionTimes = new List<TimeSpan>();

            // Evaluate on each task
            foreach (var task in tasksToEvaluate)
            {
                try
                {
                    var taskStartTime = DateTimeOffset.UtcNow;
                    var taskResult = await agentExecutor.InvokeAsync(
                        task.Input,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    var taskDuration = DateTimeOffset.UtcNow - taskStartTime;
                    executionTimes.Add(taskDuration);

                    // Evaluate task result
                    var taskEvaluation = await EvaluateTaskResultAsync(
                        task,
                        taskResult,
                        cancellationToken).ConfigureAwait(false);

                    if (taskEvaluation.IsSuccess)
                    {
                        completedTasks++;
                        qualityScores.Add(taskEvaluation.QualityScore);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Task evaluation failed for chromosome {ChromosomeId}", chromosome.Id);
                }
            }

            // Calculate fitness metrics
            result.TasksEvaluated = tasksToEvaluate.Count;
            result.TasksCompleted = completedTasks;
            result.CompletionRate = tasksToEvaluate.Count > 0
                ? (double)completedTasks / tasksToEvaluate.Count
                : 0.0;
            result.QualityScore = qualityScores.Count > 0
                ? qualityScores.Average()
                : 0.0;

            // Calculate efficiency (inverse of average execution time, normalized)
            var avgExecutionTime = executionTimes.Count > 0
                ? executionTimes.Average(t => t.TotalSeconds)
                : double.MaxValue;
            result.EfficiencyScore = avgExecutionTime > 0
                ? Math.Min(1.0, 60.0 / avgExecutionTime) // Normalize: 60 seconds = 1.0, longer = lower
                : 0.0;

            // Novelty and contribution scores would be calculated by hive mind integration
            // For now, set to default values
            result.NoveltyScore = 0.5;
            result.ContributionScore = 0.0;
            result.ConsistencyScore = qualityScores.Count > 1
                ? 1.0 - (qualityScores.StandardDeviation() / qualityScores.Average())
                : 0.0;

            // Calculate overall fitness using weighted sum
            result.OverallFitness = CalculateOverallFitness(result);
            result.EvaluationTime = DateTimeOffset.UtcNow - startTime;

            _logger?.LogDebug(
                "Evaluated chromosome {ChromosomeId}: Fitness={Fitness}, Completion={Completion}, Quality={Quality}",
                chromosome.Id,
                result.OverallFitness,
                result.CompletionRate,
                result.QualityScore);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Fitness evaluation failed for chromosome {ChromosomeId}", chromosome.Id);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<Guid, FitnessResult>> EvaluateBatchAsync(
        IReadOnlyList<AgentChromosome> chromosomes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chromosomes);

        var results = new Dictionary<Guid, FitnessResult>();

        // Evaluate in parallel (with concurrency limit)
        using var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var tasks = chromosomes.Select(async chromosome =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await EvaluateAsync(chromosome, cancellationToken).ConfigureAwait(false);
                lock (results)
                {
                    results[chromosome.Id] = result;
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return results;
    }

    private double CalculateOverallFitness(FitnessResult result)
    {
        return _config.CompletionWeight * result.CompletionRate +
               _config.QualityWeight * result.QualityScore +
               _config.EfficiencyWeight * result.EfficiencyScore +
               _config.NoveltyWeight * result.NoveltyScore +
               _config.ContributionWeight * result.ContributionScore +
               _config.ConsistencyWeight * result.ConsistencyScore;
    }

    private async Task<TaskEvaluation> EvaluateTaskResultAsync(
        EvaluationTask task,
        string result,
        CancellationToken cancellationToken)
    {
        // Simple evaluation: check if result contains expected keywords or patterns
        // In a full implementation, this would use LLM-based evaluation or human evaluation
        var isSuccess = false;
        var qualityScore = 0.0;

        if (!string.IsNullOrWhiteSpace(result))
        {
            // Check if result contains expected output keywords
            if (task.ExpectedKeywords != null && task.ExpectedKeywords.Count > 0)
            {
                var matches = task.ExpectedKeywords.Count(keyword =>
                    result.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                qualityScore = (double)matches / task.ExpectedKeywords.Count;
                isSuccess = qualityScore >= 0.5; // At least 50% keyword match
            }
            else
            {
                // If no expected keywords, consider non-empty result as success
                isSuccess = result.Length > 10; // Minimum length threshold
                qualityScore = isSuccess ? 0.7 : 0.0;
            }
        }

        return new TaskEvaluation
        {
            IsSuccess = isSuccess,
            QualityScore = qualityScore
        };
    }
}

/// <summary>
/// Represents an evaluation task for fitness evaluation.
/// </summary>
public sealed class EvaluationTask
{
    /// <summary>
    /// Gets or sets the task identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task input/prompt.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets expected keywords in the result (for automated evaluation).
    /// </summary>
    public List<string>? ExpectedKeywords { get; set; }

    /// <summary>
    /// Gets or sets the expected output (for exact matching).
    /// </summary>
    public string? ExpectedOutput { get; set; }

    /// <summary>
    /// Gets or sets task metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the result of evaluating a single task.
/// </summary>
internal sealed class TaskEvaluation
{
    public bool IsSuccess { get; set; }
    public double QualityScore { get; set; }
}

/// <summary>
/// Extension methods for calculating standard deviation.
/// </summary>
internal static class StatisticsExtensions
{
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        var valueList = values.ToList();
        if (valueList.Count == 0)
            return 0.0;

        var avg = valueList.Average();
        var sumOfSquares = valueList.Sum(v => Math.Pow(v - avg, 2));
        return Math.Sqrt(sumOfSquares / valueList.Count);
    }
}

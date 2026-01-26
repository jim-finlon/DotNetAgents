using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Agents.Messaging;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DotNetAgents.Agents.Evolutionary.Distribution;

/// <summary>
/// Distributed fitness evaluator that uses message buses for parallel evaluation.
/// </summary>
public sealed class DistributedEvaluator : IDistributedEvaluator
{
    private readonly IAgentMessageBus _messageBus;
    private readonly IFitnessEvaluator _localEvaluator;
    private readonly ILogger<DistributedEvaluator>? _logger;
    private readonly TimeSpan _evaluationTimeout;
    private readonly int _maxConcurrentEvaluations;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedEvaluator"/> class.
    /// </summary>
    /// <param name="messageBus">The message bus for distribution.</param>
    /// <param name="localEvaluator">The local fitness evaluator (fallback).</param>
    /// <param name="evaluationTimeout">The timeout for evaluations.</param>
    /// <param name="maxConcurrentEvaluations">Maximum concurrent evaluations.</param>
    /// <param name="logger">Optional logger instance.</param>
    public DistributedEvaluator(
        IAgentMessageBus messageBus,
        IFitnessEvaluator localEvaluator,
        TimeSpan evaluationTimeout,
        int maxConcurrentEvaluations = 50,
        ILogger<DistributedEvaluator>? logger = null)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _localEvaluator = localEvaluator ?? throw new ArgumentNullException(nameof(localEvaluator));
        _evaluationTimeout = evaluationTimeout;
        _maxConcurrentEvaluations = maxConcurrentEvaluations;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<Guid, FitnessResult>> EvaluateDistributedAsync(
        IReadOnlyList<AgentChromosome> chromosomes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chromosomes);

        if (chromosomes.Count == 0)
            return new Dictionary<Guid, FitnessResult>();

        // For now, fall back to local evaluation with concurrency limit
        // Full implementation would:
        // 1. Serialize chromosomes to messages
        // 2. Send to message bus
        // 3. Workers process and return results
        // 4. Collect results with timeout handling

        _logger?.LogInformation(
            "Evaluating {Count} chromosomes (distributed mode with local fallback)",
            chromosomes.Count);

        // Use semaphore to limit concurrency
        using var semaphore = new SemaphoreSlim(_maxConcurrentEvaluations);
        var results = new ConcurrentDictionary<Guid, FitnessResult>();

        var tasks = chromosomes.Select(async chromosome =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await _localEvaluator.EvaluateAsync(
                    chromosome,
                    cancellationToken).ConfigureAwait(false);

                results[chromosome.Id] = result;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Evaluation failed for chromosome {ChromosomeId}", chromosome.Id);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return new Dictionary<Guid, FitnessResult>(results);
    }
}

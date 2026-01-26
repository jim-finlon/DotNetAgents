using DotNetAgents.Agents.Evolutionary.Genetics;

namespace DotNetAgents.Agents.Evolutionary.Fitness;

/// <summary>
/// Interface for fitness evaluators that assess agent performance.
/// </summary>
public interface IFitnessEvaluator
{
    /// <summary>
    /// Evaluates the fitness of a single chromosome.
    /// </summary>
    /// <param name="chromosome">The chromosome to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fitness evaluation result.</returns>
    Task<FitnessResult> EvaluateAsync(
        AgentChromosome chromosome,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates the fitness of multiple chromosomes in parallel.
    /// </summary>
    /// <param name="chromosomes">The chromosomes to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary mapping chromosome IDs to fitness results.</returns>
    Task<Dictionary<Guid, FitnessResult>> EvaluateBatchAsync(
        IReadOnlyList<AgentChromosome> chromosomes,
        CancellationToken cancellationToken = default);
}

using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Agents.Evolutionary.Genetics;

namespace DotNetAgents.Agents.Evolutionary.Distribution;

/// <summary>
/// Interface for distributed fitness evaluation using message buses.
/// </summary>
public interface IDistributedEvaluator
{
    /// <summary>
    /// Evaluates chromosomes in parallel using distributed workers.
    /// </summary>
    /// <param name="chromosomes">The chromosomes to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary mapping chromosome IDs to fitness results.</returns>
    Task<Dictionary<Guid, FitnessResult>> EvaluateDistributedAsync(
        IReadOnlyList<AgentChromosome> chromosomes,
        CancellationToken cancellationToken = default);
}

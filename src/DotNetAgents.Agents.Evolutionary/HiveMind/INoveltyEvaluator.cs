using DotNetAgents.Knowledge.Models;

namespace DotNetAgents.Agents.Evolutionary.HiveMind;

/// <summary>
/// Evaluates the novelty of knowledge items using vector similarity.
/// </summary>
public interface INoveltyEvaluator
{
    /// <summary>
    /// Evaluates the novelty of a knowledge item.
    /// </summary>
    /// <param name="knowledge">The knowledge item to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The novelty score (0.0 to 1.0, where 1.0 is completely novel).</returns>
    Task<double> EvaluateNoveltyAsync(
        KnowledgeItem knowledge,
        CancellationToken cancellationToken = default);
}

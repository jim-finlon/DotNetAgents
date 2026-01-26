using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Knowledge.Models;

namespace DotNetAgents.Agents.Evolutionary.HiveMind;

/// <summary>
/// Extracts knowledge from agent execution results.
/// </summary>
public interface IKnowledgeExtractor
{
    /// <summary>
    /// Extracts knowledge items from an agent execution.
    /// </summary>
    /// <param name="chromosomeId">The chromosome ID.</param>
    /// <param name="generation">The generation number.</param>
    /// <param name="taskInput">The task input.</param>
    /// <param name="taskResult">The task result.</param>
    /// <param name="fitnessResult">The fitness evaluation result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Extracted knowledge items.</returns>
    Task<List<KnowledgeItem>> ExtractKnowledgeAsync(
        Guid chromosomeId,
        int generation,
        string taskInput,
        string taskResult,
        FitnessResult fitnessResult,
        CancellationToken cancellationToken = default);
}

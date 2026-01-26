using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Knowledge.Models;

namespace DotNetAgents.Agents.Evolutionary.HiveMind;

/// <summary>
/// Enhanced hive mind that extends knowledge repository with evolutionary-specific features.
/// </summary>
public interface IHiveMind
{
    /// <summary>
    /// Evaluates the novelty of knowledge extracted from an agent execution.
    /// </summary>
    /// <param name="knowledge">The knowledge to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The novelty score (0.0 to 1.0, where 1.0 is completely novel).</returns>
    Task<double> EvaluateNoveltyAsync(
        KnowledgeItem knowledge,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts knowledge from an agent execution result.
    /// </summary>
    /// <param name="chromosomeId">The chromosome ID that produced the result.</param>
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
        Fitness.FitnessResult fitnessResult,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores knowledge in the hive mind if it's novel enough.
    /// </summary>
    /// <param name="knowledge">The knowledge to store.</param>
    /// <param name="chromosomeId">The chromosome that contributed this knowledge.</param>
    /// <param name="generation">The generation number.</param>
    /// <param name="noveltyThreshold">The minimum novelty score to store (default: 0.8).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if stored; false if not novel enough.</returns>
    Task<bool> StoreIfNovelAsync(
        KnowledgeItem knowledge,
        Guid chromosomeId,
        int generation,
        double noveltyThreshold = 0.8,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves relevant knowledge for a chromosome based on its configuration.
    /// </summary>
    /// <param name="chromosome">The chromosome to get knowledge for.</param>
    /// <param name="maxResults">Maximum number of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Relevant knowledge items.</returns>
    Task<List<KnowledgeItem>> GetRelevantKnowledgeAsync(
        AgentChromosome chromosome,
        int maxResults = 10,
        CancellationToken cancellationToken = default);
}

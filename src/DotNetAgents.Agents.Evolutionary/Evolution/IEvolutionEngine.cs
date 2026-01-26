namespace DotNetAgents.Agents.Evolutionary.Evolution;

/// <summary>
/// Interface for the evolution engine that orchestrates evolutionary cycles.
/// </summary>
public interface IEvolutionEngine
{
    /// <summary>
    /// Runs evolution with the given configuration.
    /// </summary>
    /// <param name="config">Evolution configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The evolution result.</returns>
    Task<EvolutionResult> EvolveAsync(
        EvolutionConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses the current evolution run.
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// Resumes a paused evolution run.
    /// </summary>
    Task ResumeAsync();

    /// <summary>
    /// Stops the current evolution run.
    /// </summary>
    Task StopAsync();
}

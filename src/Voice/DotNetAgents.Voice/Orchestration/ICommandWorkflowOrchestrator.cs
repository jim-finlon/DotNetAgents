namespace DotNetAgents.Voice.Orchestration;

/// <summary>
/// Interface for orchestrating voice command workflows.
/// </summary>
public interface ICommandWorkflowOrchestrator
{
    /// <summary>
    /// Executes a command workflow starting from the given state.
    /// </summary>
    /// <param name="state">The initial command state.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The final command state after execution.</returns>
    Task<CommandState> ExecuteAsync(
        CommandState state,
        CancellationToken cancellationToken = default);
}

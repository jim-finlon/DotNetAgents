namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// A node in the agent graph.
/// </summary>
/// <typeparam name="TState">Type of agent state.</typeparam>
public interface IGraphNode<TState> where TState : AgentState
{
    /// <summary>
    /// Gets the unique node name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the node logic.
    /// </summary>
    /// <param name="state">Current agent state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated agent state.</returns>
    Task<TState> ExecuteAsync(
        TState state,
        CancellationToken cancellationToken = default);
}


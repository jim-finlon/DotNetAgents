namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// Conditional edge in the graph.
/// </summary>
/// <typeparam name="TState">Type of agent state.</typeparam>
public interface IConditionalEdge<TState> where TState : AgentState
{
    /// <summary>
    /// Determines the next node based on state.
    /// </summary>
    /// <param name="state">Current agent state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Edge decision indicating next node.</returns>
    Task<EdgeDecision> DecideAsync(
        TState state,
        CancellationToken cancellationToken = default);
}


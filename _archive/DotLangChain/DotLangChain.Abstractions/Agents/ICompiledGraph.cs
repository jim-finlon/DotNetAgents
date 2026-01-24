namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// Compiled executable graph.
/// </summary>
/// <typeparam name="TState">Type of agent state.</typeparam>
public interface ICompiledGraph<TState> where TState : AgentState
{
    /// <summary>
    /// Executes the graph to completion.
    /// </summary>
    /// <param name="initialState">Initial agent state.</param>
    /// <param name="options">Execution options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Final agent state.</returns>
    Task<TState> InvokeAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams execution state after each node.
    /// </summary>
    /// <param name="initialState">Initial agent state.</param>
    /// <param name="options">Execution options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of graph events.</returns>
    IAsyncEnumerable<GraphEvent<TState>> StreamAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default);
}


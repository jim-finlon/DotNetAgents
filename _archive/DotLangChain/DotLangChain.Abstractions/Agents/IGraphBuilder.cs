namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// Graph builder for agent workflows.
/// </summary>
/// <typeparam name="TState">Type of agent state.</typeparam>
public interface IGraphBuilder<TState> where TState : AgentState, new()
{
    /// <summary>
    /// Adds a node with a function action.
    /// </summary>
    IGraphBuilder<TState> AddNode(string name, Func<TState, CancellationToken, Task<TState>> action);

    /// <summary>
    /// Adds a node implementation.
    /// </summary>
    IGraphBuilder<TState> AddNode(IGraphNode<TState> node);

    /// <summary>
    /// Adds an edge from one node to another.
    /// </summary>
    IGraphBuilder<TState> AddEdge(string from, string to);

    /// <summary>
    /// Adds a conditional edge with a synchronous condition function.
    /// </summary>
    IGraphBuilder<TState> AddConditionalEdge(string from, Func<TState, EdgeDecision> condition);

    /// <summary>
    /// Adds a conditional edge with an asynchronous condition function.
    /// </summary>
    IGraphBuilder<TState> AddConditionalEdge(string from, Func<TState, CancellationToken, Task<EdgeDecision>> asyncCondition);

    /// <summary>
    /// Adds a conditional edge implementation.
    /// </summary>
    IGraphBuilder<TState> AddConditionalEdge(string from, IConditionalEdge<TState> edge);

    /// <summary>
    /// Sets the entry point for the graph.
    /// </summary>
    IGraphBuilder<TState> SetEntryPoint(string nodeName);

    /// <summary>
    /// Compiles the graph into an executable form.
    /// </summary>
    ICompiledGraph<TState> Compile();
}


using DotLangChain.Abstractions.Agents;
using DotLangChain.Core.Exceptions;
using System.Collections.Frozen;

namespace DotLangChain.Core.Agents.Graph;

/// <summary>
/// Builder for agent graphs with fluent API.
/// </summary>
/// <typeparam name="TState">Type of agent state.</typeparam>
public sealed class GraphBuilder<TState> : IGraphBuilder<TState>
    where TState : AgentState, new()
{
    private readonly Dictionary<string, Func<TState, CancellationToken, Task<TState>>> _nodes = new();
    private readonly Dictionary<string, List<string>> _edges = new();
    private readonly Dictionary<string, Func<TState, CancellationToken, Task<EdgeDecision>>> _conditionalEdges = new();
    private string? _entryPoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphBuilder{TState}"/> class.
    /// </summary>
    public GraphBuilder()
    {
    }

    /// <inheritdoc/>
    public IGraphBuilder<TState> AddNode(
        string name,
        Func<TState, CancellationToken, Task<TState>> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);

        if (!_nodes.TryAdd(name, action))
        {
            throw new InvalidOperationException($"Node '{name}' already exists");
        }

        return this;
    }

    /// <inheritdoc/>
    public IGraphBuilder<TState> AddNode(IGraphNode<TState> node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return AddNode(node.Name, node.ExecuteAsync);
    }

    /// <inheritdoc/>
    public IGraphBuilder<TState> AddEdge(string from, string to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        ArgumentException.ThrowIfNullOrWhiteSpace(to);

        if (!_edges.TryGetValue(from, out var targets))
        {
            targets = new List<string>();
            _edges[from] = targets;
        }

        if (!targets.Contains(to))
        {
            targets.Add(to);
        }

        return this;
    }

    /// <inheritdoc/>
    public IGraphBuilder<TState> AddConditionalEdge(
        string from,
        Func<TState, EdgeDecision> condition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        ArgumentNullException.ThrowIfNull(condition);

        return AddConditionalEdge(from, (state, ct) => Task.FromResult(condition(state)));
    }

    /// <inheritdoc/>
    public IGraphBuilder<TState> AddConditionalEdge(
        string from,
        Func<TState, CancellationToken, Task<EdgeDecision>> asyncCondition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        ArgumentNullException.ThrowIfNull(asyncCondition);

        if (_conditionalEdges.ContainsKey(from))
        {
            throw new InvalidOperationException($"Conditional edge already exists for node '{from}'");
        }

        _conditionalEdges[from] = asyncCondition;
        return this;
    }

    /// <inheritdoc/>
    public IGraphBuilder<TState> AddConditionalEdge(
        string from,
        IConditionalEdge<TState> edge)
    {
        ArgumentNullException.ThrowIfNull(edge);
        return AddConditionalEdge(from, edge.DecideAsync);
    }

    /// <inheritdoc/>
    public IGraphBuilder<TState> SetEntryPoint(string nodeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _entryPoint = nodeName;
        return this;
    }

    /// <inheritdoc/>
    public ICompiledGraph<TState> Compile()
    {
        if (_entryPoint is null)
        {
            throw new InvalidOperationException("Entry point must be set before compilation");
        }

        if (!_nodes.ContainsKey(_entryPoint))
        {
            throw GraphException.NodeNotFound(_entryPoint);
        }

        // Convert to frozen collections for thread-safety and performance
        var frozenNodes = _nodes.ToFrozenDictionary();
        var frozenEdges = _edges.ToFrozenDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToFrozenSet());
        var frozenConditionalEdges = _conditionalEdges.ToFrozenDictionary();

        return new CompiledGraph<TState>(
            frozenNodes,
            frozenEdges,
            frozenConditionalEdges,
            _entryPoint);
    }
}


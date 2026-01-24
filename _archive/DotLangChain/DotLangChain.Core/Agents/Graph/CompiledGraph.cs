using DotLangChain.Abstractions.Agents;
using DotLangChain.Core.Exceptions;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DotLangChain.Core.Agents.Graph;

/// <summary>
/// Compiled executable graph implementation.
/// </summary>
/// <typeparam name="TState">Type of agent state.</typeparam>
internal sealed class CompiledGraph<TState> : ICompiledGraph<TState>
    where TState : AgentState
{
    private static readonly ActivitySource ActivitySource = new("DotLangChain.Graph");

    private readonly FrozenDictionary<string, Func<TState, CancellationToken, Task<TState>>> _nodes;
    private readonly FrozenDictionary<string, FrozenSet<string>> _edges;
    private readonly FrozenDictionary<string, Func<TState, CancellationToken, Task<EdgeDecision>>> _conditionalEdges;
    private readonly string _entryPoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledGraph{TState}"/> class.
    /// </summary>
    internal CompiledGraph(
        FrozenDictionary<string, Func<TState, CancellationToken, Task<TState>>> nodes,
        FrozenDictionary<string, FrozenSet<string>> edges,
        FrozenDictionary<string, Func<TState, CancellationToken, Task<EdgeDecision>>> conditionalEdges,
        string entryPoint)
    {
        _nodes = nodes;
        _edges = edges;
        _conditionalEdges = conditionalEdges;
        _entryPoint = entryPoint;
    }

    /// <inheritdoc/>
    public async Task<TState> InvokeAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(initialState);

        TState? finalState = null;

        await foreach (var evt in StreamAsync(initialState, options, cancellationToken))
        {
            if (evt.EventType == GraphEventType.GraphCompleted)
            {
                finalState = evt.State;
            }

            if (evt.EventType == GraphEventType.Error && evt.Error != null)
            {
                throw evt.Error;
            }
        }

        return finalState ?? throw new InvalidOperationException("Graph did not complete");
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<GraphEvent<TState>> StreamAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(initialState);

        options ??= new GraphExecutionOptions();
        var state = initialState;
        var currentNode = _entryPoint;
        var stepCount = 0;

        using var cts = options.Timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (cts != null && options.Timeout.HasValue)
        {
            cts.CancelAfter(options.Timeout.Value);
        }

        var token = cts?.Token ?? cancellationToken;

        while (currentNode != "__end__" && stepCount < options.MaxSteps)
        {
            token.ThrowIfCancellationRequested();

            using var activity = ActivitySource.StartActivity($"Node:{currentNode}");
            var stopwatch = Stopwatch.StartNew();

            yield return new GraphEvent<TState>
            {
                NodeName = currentNode,
                State = state,
                EventType = GraphEventType.NodeStarted
            };

            if (!_nodes.TryGetValue(currentNode, out var nodeFunc))
            {
                var exception = GraphException.NodeNotFound(currentNode);
                yield return new GraphEvent<TState>
                {
                    NodeName = currentNode,
                    State = state,
                    EventType = GraphEventType.Error,
                    Error = exception
                };
                throw exception;
            }

            try
            {
                state = await nodeFunc(state, token);
                state.CurrentNode = currentNode;
                state.StepCount = ++stepCount;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                yield return new GraphEvent<TState>
                {
                    NodeName = currentNode,
                    State = state,
                    EventType = GraphEventType.Error,
                    Duration = stopwatch.Elapsed,
                    Error = ex
                };
                throw;
            }

            stopwatch.Stop();

            yield return new GraphEvent<TState>
            {
                NodeName = currentNode,
                State = state,
                EventType = GraphEventType.NodeCompleted,
                Duration = stopwatch.Elapsed
            };

            var nextNode = await DetermineNextNodeAsync(currentNode, state, token);

            yield return new GraphEvent<TState>
            {
                NodeName = currentNode,
                State = state,
                EventType = GraphEventType.EdgeTraversed
            };

            currentNode = nextNode;
        }

        if (stepCount >= options.MaxSteps)
        {
            throw GraphException.MaxStepsExceeded(options.MaxSteps, stepCount);
        }

        yield return new GraphEvent<TState>
        {
            NodeName = currentNode,
            State = state,
            EventType = GraphEventType.GraphCompleted
        };
    }

    private async Task<string> DetermineNextNodeAsync(
        string currentNode,
        TState state,
        CancellationToken cancellationToken)
    {
        // Check for conditional edge first
        if (_conditionalEdges.TryGetValue(currentNode, out var condition))
        {
            var decision = await condition(state, cancellationToken);
            return decision.TargetNode;
        }

        // Check for regular edges
        if (_edges.TryGetValue(currentNode, out var targets) && targets.Count > 0)
        {
            // Take first edge (for now - could support parallel edges later)
            return targets.First();
        }

        // No edges - end graph
        return "__end__";
    }
}


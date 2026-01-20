using DotNetAgents.Core.Exceptions;
using DotNetAgents.Core.Execution;
using DotNetAgents.Workflow.Graph;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Workflow.Execution;

/// <summary>
/// Options for graph execution.
/// </summary>
public record GraphExecutionOptions
{
    /// <summary>
    /// Gets or sets the maximum number of iterations allowed.
    /// </summary>
    public int MaxIterations { get; init; } = 100;

    /// <summary>
    /// Gets or sets whether to validate the graph before execution.
    /// </summary>
    public bool ValidateGraph { get; init; } = true;

    /// <summary>
    /// Gets or sets an optional execution context for tracking.
    /// </summary>
    public Core.Execution.ExecutionContext? ExecutionContext { get; init; }
}

/// <summary>
/// Executes state graphs (workflows).
/// </summary>
/// <typeparam name="TState">The type of the workflow state.</typeparam>
public class GraphExecutor<TState> where TState : class
{
    private readonly StateGraph<TState> _graph;
    private readonly ILogger<GraphExecutor<TState>>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphExecutor{TState}"/> class.
    /// </summary>
    /// <param name="graph">The state graph to execute.</param>
    /// <param name="logger">Optional logger for execution tracking.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> is null.</exception>
    public GraphExecutor(StateGraph<TState> graph, ILogger<GraphExecutor<TState>>? logger = null)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _logger = logger;
    }

    /// <summary>
    /// Executes the graph starting from the entry point with the given initial state.
    /// </summary>
    /// <param name="initialState">The initial state.</param>
    /// <param name="options">Optional execution options.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The final state after execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialState"/> is null.</exception>
    /// <exception cref="AgentException">Thrown when execution fails or exceeds max iterations.</exception>
    public async Task<TState> ExecuteAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (initialState == null)
            throw new ArgumentNullException(nameof(initialState));

        options ??= new GraphExecutionOptions();

        if (options.ValidateGraph)
        {
            _graph.Validate();
        }

        if (_graph.EntryPoint == null)
        {
            throw new AgentException(
                "Graph has no entry point.",
                ErrorCategory.WorkflowError);
        }

        var currentState = initialState;
        var currentNode = _graph.EntryPoint;
        var iterationCount = 0;
        var executionPath = new List<string> { currentNode };

        _logger?.LogInformation(
            "Starting graph execution. Entry point: {EntryPoint}",
            currentNode);

        while (currentNode != null && iterationCount < options.MaxIterations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check if we've reached an exit point
            if (_graph.ExitPoints.Contains(currentNode))
            {
                _logger?.LogInformation(
                    "Reached exit point: {ExitPoint}. Total iterations: {Iterations}",
                    currentNode,
                    iterationCount);
                break;
            }

            // Execute the current node
            if (!_graph.Nodes.TryGetValue(currentNode, out var node))
            {
                throw new AgentException(
                    $"Node '{currentNode}' not found in graph.",
                    ErrorCategory.WorkflowError);
            }

            _logger?.LogDebug(
                "Executing node: {NodeName}. Iteration: {Iteration}",
                currentNode,
                iterationCount + 1);

            try
            {
                currentState = await node.ExecuteAsync(currentState, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error executing node '{NodeName}' at iteration {Iteration}",
                    currentNode,
                    iterationCount + 1);
                throw new AgentException(
                    $"Error executing node '{currentNode}': {ex.Message}",
                    ErrorCategory.WorkflowError,
                    ex);
            }

            // Find the next node(s) by evaluating outgoing edges
            var outgoingEdges = _graph.GetOutgoingEdges(currentNode, currentState);
            
            if (outgoingEdges.Count == 0)
            {
                // No outgoing edges - check if this is an exit point
                if (!_graph.ExitPoints.Contains(currentNode))
                {
                    throw new AgentException(
                        $"Node '{currentNode}' has no outgoing edges and is not an exit point.",
                        ErrorCategory.WorkflowError);
                }
                break;
            }

            if (outgoingEdges.Count > 1)
            {
                // Multiple edges - take the first one that matches
                // In the future, we could support parallel execution or explicit selection
                _logger?.LogWarning(
                    "Multiple edges available from node '{NodeName}'. Taking first matching edge.",
                    currentNode);
            }

            var nextEdge = outgoingEdges[0];
            currentNode = nextEdge.To;
            executionPath.Add(currentNode);
            iterationCount++;

            _logger?.LogDebug(
                "Transitioning from '{FromNode}' to '{ToNode}'",
                nextEdge.From,
                nextEdge.To);
        }

        if (iterationCount >= options.MaxIterations)
        {
            throw new AgentException(
                $"Graph execution exceeded maximum iterations ({options.MaxIterations}). Execution path: {string.Join(" -> ", executionPath)}",
                ErrorCategory.WorkflowError);
        }

        _logger?.LogInformation(
            "Graph execution completed. Total iterations: {Iterations}. Final node: {FinalNode}",
            iterationCount,
            currentNode);

        return currentState;
    }
}
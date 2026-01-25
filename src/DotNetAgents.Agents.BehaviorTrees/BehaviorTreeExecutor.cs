using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.BehaviorTrees;

/// <summary>
/// Executes behavior trees with observability and state management.
/// </summary>
/// <typeparam name="TContext">The type of the execution context.</typeparam>
public class BehaviorTreeExecutor<TContext> where TContext : class
{
    private readonly ILogger<BehaviorTreeExecutor<TContext>>? _logger;
    private readonly Meter? _meter;
    private readonly Counter<long>? _executionCounter;
    private readonly Histogram<double>? _executionDuration;
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Initializes a new instance of the <see cref="BehaviorTreeExecutor{TContext}"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="meter">Optional meter for metrics.</param>
    public BehaviorTreeExecutor(
        ILogger<BehaviorTreeExecutor<TContext>>? logger = null,
        Meter? meter = null)
    {
        _logger = logger;
        _meter = meter ?? new Meter("DotNetAgents.Agents.BehaviorTrees");
        _executionCounter = _meter.CreateCounter<long>(
            "behavior_tree_executions_total",
            "count",
            "Total number of behavior tree executions");
        _executionDuration = _meter.CreateHistogram<double>(
            "behavior_tree_execution_duration_seconds",
            "seconds",
            "Duration of behavior tree executions");
        _activitySource = new ActivitySource("DotNetAgents.Agents.BehaviorTrees");
    }

    /// <summary>
    /// Executes a behavior tree with observability.
    /// </summary>
    /// <param name="tree">The behavior tree to execute.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The execution result.</returns>
    public async Task<BehaviorTreeNodeResult<TContext>> ExecuteAsync(
        BehaviorTree<TContext> tree,
        TContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tree);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        using var activity = _activitySource.StartActivity($"BehaviorTree.Execute.{tree.Name}");
        activity?.SetTag("behavior_tree.name", tree.Name);

        var startTime = Stopwatch.GetTimestamp();
        _executionCounter?.Add(1, new KeyValuePair<string, object?>("tree_name", tree.Name));

        try
        {
            _logger?.LogInformation("Executing behavior tree '{TreeName}'", tree.Name);

            var status = await tree.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

            var duration = Stopwatch.GetElapsedTime(startTime);
            _executionDuration?.Record(duration.TotalSeconds, new KeyValuePair<string, object?>("tree_name", tree.Name));

            activity?.SetTag("status", status.ToString());
            activity?.SetStatus(status == BehaviorTreeNodeStatus.Success 
                ? ActivityStatusCode.Ok 
                : ActivityStatusCode.Error);

            _logger?.LogInformation(
                "Behavior tree '{TreeName}' completed with status '{Status}' in {Duration}ms",
                tree.Name, status, duration.TotalMilliseconds);

            return new BehaviorTreeNodeResult<TContext>(status, context, $"Tree '{tree.Name}' completed");
        }
        catch (Exception ex)
        {
            var duration = Stopwatch.GetElapsedTime(startTime);
            _executionDuration?.Record(duration.TotalSeconds, new KeyValuePair<string, object?>("tree_name", tree.Name));

            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);

            _logger?.LogError(ex, "Behavior tree '{TreeName}' execution failed", tree.Name);

            return new BehaviorTreeNodeResult<TContext>(
                BehaviorTreeNodeStatus.Failure,
                context,
                $"Tree '{tree.Name}' failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Activity source for behavior tree tracing.
/// </summary>
public static class BehaviorTreeActivitySource
{
    /// <summary>
    /// Gets the activity source name.
    /// </summary>
    public const string SourceName = "DotNetAgents.Agents.BehaviorTrees";

    /// <summary>
    /// Gets the activity source.
    /// </summary>
    public static readonly ActivitySource Source = new(SourceName);
}

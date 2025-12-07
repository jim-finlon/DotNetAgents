namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// Event emitted during graph execution.
/// </summary>
/// <typeparam name="TState">Type of agent state.</typeparam>
public sealed record GraphEvent<TState> where TState : AgentState
{
    public required string NodeName { get; init; }
    public required TState State { get; init; }
    public required GraphEventType EventType { get; init; }
    public TimeSpan Duration { get; init; }
    public Exception? Error { get; init; }
}

/// <summary>
/// Type of graph event.
/// </summary>
public enum GraphEventType
{
    NodeStarted,
    NodeCompleted,
    EdgeTraversed,
    GraphCompleted,
    Error,
    CheckpointSaved
}


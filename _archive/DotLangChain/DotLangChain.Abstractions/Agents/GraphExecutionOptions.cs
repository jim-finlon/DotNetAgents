namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// Options for graph execution.
/// </summary>
public sealed record GraphExecutionOptions
{
    /// <summary>
    /// Maximum number of steps before termination. Default: 100.
    /// </summary>
    public int MaxSteps { get; init; } = 100;

    /// <summary>
    /// Optional timeout for graph execution.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Whether to enable checkpointing for state persistence.
    /// </summary>
    public bool EnableCheckpointing { get; init; } = false;

    /// <summary>
    /// Optional checkpoint ID for resuming execution.
    /// </summary>
    public string? CheckpointId { get; init; }
}


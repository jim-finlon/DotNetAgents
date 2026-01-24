namespace DotNetAgents.Abstractions.Chains;

/// <summary>
/// Options for runnable execution.
/// </summary>
public record RunnableOptions
{
    /// <summary>
    /// Gets or sets whether to include execution metadata in the result.
    /// </summary>
    public bool IncludeMetadata { get; init; }

    /// <summary>
    /// Gets or sets tags for this execution.
    /// </summary>
    public IReadOnlyList<string>? Tags { get; init; }

    /// <summary>
    /// Gets or sets additional options specific to the runnable implementation.
    /// </summary>
    public IDictionary<string, object>? AdditionalOptions { get; init; }
}

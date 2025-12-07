namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// Edge routing decision.
/// </summary>
public sealed record EdgeDecision
{
    public required string TargetNode { get; init; }

    /// <summary>
    /// Decision to end graph execution.
    /// </summary>
    public static readonly EdgeDecision End = new() { TargetNode = "__end__" };

    /// <summary>
    /// Creates a decision to route to a specific node.
    /// </summary>
    public static EdgeDecision To(string nodeName) => new() { TargetNode = nodeName };
}


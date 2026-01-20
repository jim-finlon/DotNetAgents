using DotNetAgents.Core.Chains;
using DotNetAgents.Core.Tools;

namespace DotNetAgents.Core.Agents;

/// <summary>
/// Represents the result of an agent execution step.
/// </summary>
public record AgentStepResult
{
    /// <summary>
    /// Gets or sets the output from this step.
    /// </summary>
    public string Output { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the agent should continue executing.
    /// </summary>
    public bool ShouldContinue { get; init; } = true;

    /// <summary>
    /// Gets or sets the tool that was called, if any.
    /// </summary>
    public ITool? ToolCalled { get; init; }

    /// <summary>
    /// Gets or sets the result from the tool execution, if any.
    /// </summary>
    public ToolResult? ToolResult { get; init; }

    /// <summary>
    /// Gets or sets metadata about this step.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Interface for agents that can use tools and make decisions.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Executes a single step of the agent.
    /// </summary>
    /// <param name="input">The input for this step.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The result of this step.</returns>
    Task<AgentStepResult> ExecuteStepAsync(
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the tools available to this agent.
    /// </summary>
    IReadOnlyList<ITool> AvailableTools { get; }
}
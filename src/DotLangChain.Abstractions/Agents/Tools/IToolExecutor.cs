using DotLangChain.Abstractions.LLM;

namespace DotLangChain.Abstractions.Agents.Tools;

/// <summary>
/// Executes tools based on LLM tool calls.
/// </summary>
public interface IToolExecutor
{
    /// <summary>
    /// Gets available tool definitions for LLM function calling.
    /// </summary>
    /// <returns>List of tool definitions.</returns>
    IReadOnlyList<ToolDefinition> GetToolDefinitions();

    /// <summary>
    /// Executes a tool call.
    /// </summary>
    /// <param name="toolCall">Tool call from LLM.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tool execution result.</returns>
    Task<ToolResult> ExecuteAsync(
        ToolCall toolCall,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes multiple tool calls in parallel.
    /// </summary>
    /// <param name="toolCalls">Tool calls from LLM.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tool execution results.</returns>
    Task<IReadOnlyList<ToolResult>> ExecuteAllAsync(
        IReadOnlyList<ToolCall> toolCalls,
        CancellationToken cancellationToken = default);
}


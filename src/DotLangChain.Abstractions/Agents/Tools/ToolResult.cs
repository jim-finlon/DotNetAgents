namespace DotLangChain.Abstractions.Agents.Tools;

/// <summary>
/// Result from tool execution.
/// </summary>
public sealed record ToolResult
{
    public required string ToolCallId { get; init; }
    public required string Content { get; init; }
    public bool IsError { get; init; }
}


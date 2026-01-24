namespace DotNetAgents.Mcp.Models;

/// <summary>
/// Represents the response from an MCP tool call.
/// </summary>
public record McpToolCallResponse
{
    /// <summary>
    /// Gets a value indicating whether the tool call was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the result of the tool call (if successful).
    /// </summary>
    public object? Result { get; init; }

    /// <summary>
    /// Gets the error message (if the call failed).
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets the error code (if the call failed).
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets the duration of the tool call in milliseconds.
    /// </summary>
    public long DurationMs { get; init; }

    /// <summary>
    /// Gets additional metadata about the tool call.
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

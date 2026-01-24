namespace DotNetAgents.Mcp.Models;

/// <summary>
/// Represents a request to call an MCP tool.
/// </summary>
public record McpToolCallRequest
{
    /// <summary>
    /// Gets the name of the tool to call.
    /// </summary>
    public required string Tool { get; init; }

    /// <summary>
    /// Gets the arguments to pass to the tool.
    /// </summary>
    public Dictionary<string, object> Arguments { get; init; } = new();

    /// <summary>
    /// Gets the correlation ID for tracking this call.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the timeout in seconds for this call.
    /// </summary>
    public int? TimeoutSeconds { get; init; }
}

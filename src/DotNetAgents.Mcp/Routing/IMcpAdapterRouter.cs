namespace DotNetAgents.Mcp.Routing;

/// <summary>
/// Represents an intent for routing to MCP services.
/// </summary>
public record Intent
{
    /// <summary>
    /// Gets the domain of the intent.
    /// </summary>
    public required string Domain { get; init; }

    /// <summary>
    /// Gets the action of the intent.
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Gets the optional sub-type of the intent.
    /// </summary>
    public string? SubType { get; init; }

    /// <summary>
    /// Gets the parameters extracted from the command.
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();

    /// <summary>
    /// Gets the target MCP service name for this intent.
    /// </summary>
    public string? TargetService { get; init; }

    /// <summary>
    /// Gets the specific tool name to call (if applicable).
    /// </summary>
    public string? Tool { get; init; }

    /// <summary>
    /// Gets the full intent name (domain.action).
    /// </summary>
    public string FullName => string.IsNullOrEmpty(SubType)
        ? $"{Domain}.{Action}"
        : $"{Domain}.{Action}.{SubType}";
}

/// <summary>
/// Interface for routing intents to appropriate MCP service adapters.
/// </summary>
public interface IMcpAdapterRouter
{
    /// <summary>
    /// Executes an intent by routing it to the appropriate MCP service.
    /// </summary>
    /// <param name="intent">The intent to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The result of the execution, or null if no result.</returns>
    Task<object?> ExecuteIntentAsync(
        Intent intent,
        CancellationToken cancellationToken = default);
}

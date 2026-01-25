namespace DotNetAgents.Voice.IntentClassification;

/// <summary>
/// Represents a parsed intent from a voice command.
/// </summary>
public record Intent
{
    /// <summary>
    /// Gets the domain of the intent (e.g., "tasks", "calendar", "business").
    /// </summary>
    public required string Domain { get; init; }

    /// <summary>
    /// Gets the action of the intent (e.g., "create", "list", "update").
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Gets the optional sub-type of the intent (e.g., "personal", "team", "invoice").
    /// </summary>
    public string? SubType { get; init; }

    /// <summary>
    /// Gets the parameters extracted from the command.
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();

    /// <summary>
    /// Gets the list of missing required parameters.
    /// </summary>
    public List<string> MissingRequired { get; init; } = new();

    /// <summary>
    /// Gets the confidence score of the intent classification (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; }

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

    /// <summary>
    /// Gets a value indicating whether the intent is complete (no missing required parameters).
    /// </summary>
    public bool IsComplete => MissingRequired.Count == 0;

    /// <summary>
    /// Gets the raw command text that was parsed.
    /// </summary>
    public string? RawText { get; init; }
}

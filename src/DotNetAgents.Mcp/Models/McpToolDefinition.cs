namespace DotNetAgents.Mcp.Models;

/// <summary>
/// Represents an MCP tool definition.
/// </summary>
public record McpToolDefinition
{
    /// <summary>
    /// Gets the name of the tool.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the tool.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the input schema for the tool (JSON Schema format).
    /// </summary>
    public required McpToolInputSchema InputSchema { get; init; }

    /// <summary>
    /// Gets the name of the MCP service that provides this tool.
    /// </summary>
    public string? ServiceName { get; init; }
}

/// <summary>
/// Represents the input schema for an MCP tool (JSON Schema format).
/// </summary>
public record McpToolInputSchema
{
    /// <summary>
    /// Gets the type of the schema (typically "object").
    /// </summary>
    public string Type { get; init; } = "object";

    /// <summary>
    /// Gets the properties of the input schema.
    /// </summary>
    public Dictionary<string, McpProperty> Properties { get; init; } = new();

    /// <summary>
    /// Gets the list of required property names.
    /// </summary>
    public List<string> Required { get; init; } = new();
}

/// <summary>
/// Represents a property definition in a tool schema.
/// </summary>
public record McpProperty
{
    /// <summary>
    /// Gets the type of the property (e.g., "string", "number", "boolean", "array", "object").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the description of the property.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the enum values if the property is an enum.
    /// </summary>
    public List<string>? Enum { get; init; }

    /// <summary>
    /// Gets the default value for the property.
    /// </summary>
    public object? Default { get; init; }
}

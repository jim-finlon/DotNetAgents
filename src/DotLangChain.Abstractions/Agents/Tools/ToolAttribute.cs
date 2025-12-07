namespace DotLangChain.Abstractions.Agents.Tools;

/// <summary>
/// Attribute for marking tool methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the tool name (defaults to method name if not specified).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the tool description.
    /// </summary>
    public string? Description { get; set; }
}


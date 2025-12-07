namespace DotLangChain.Abstractions.Agents.Tools;

/// <summary>
/// Attribute for tool parameter descriptions.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ToolParameterAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the parameter description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the parameter is required. Default: true.
    /// </summary>
    public bool Required { get; set; } = true;
}


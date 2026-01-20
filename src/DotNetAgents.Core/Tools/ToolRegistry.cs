namespace DotNetAgents.Core.Tools;

/// <summary>
/// Registry for managing available tools.
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// Registers a tool in the registry.
    /// </summary>
    /// <param name="tool">The tool to register.</param>
    /// <exception cref="ArgumentException">Thrown when a tool with the same name is already registered.</exception>
    void Register(ITool tool);

    /// <summary>
    /// Gets a tool by name.
    /// </summary>
    /// <param name="name">The name of the tool.</param>
    /// <returns>The tool if found, otherwise null.</returns>
    ITool? GetTool(string name);

    /// <summary>
    /// Gets all registered tools.
    /// </summary>
    /// <returns>A read-only list of all registered tools.</returns>
    IReadOnlyList<ITool> GetAllTools();

    /// <summary>
    /// Unregisters a tool from the registry.
    /// </summary>
    /// <param name="name">The name of the tool to unregister.</param>
    /// <returns>True if the tool was removed, false if it was not found.</returns>
    bool Unregister(string name);

    /// <summary>
    /// Checks if a tool is registered.
    /// </summary>
    /// <param name="name">The name of the tool.</param>
    /// <returns>True if the tool is registered, otherwise false.</returns>
    bool IsRegistered(string name);
}

/// <summary>
/// Default implementation of <see cref="IToolRegistry"/>.
/// </summary>
public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public void Register(ITool tool)
    {
        if (tool == null)
            throw new ArgumentNullException(nameof(tool));

        if (_tools.ContainsKey(tool.Name))
        {
            throw new ArgumentException($"A tool with the name '{tool.Name}' is already registered.", nameof(tool));
        }

        _tools[tool.Name] = tool;
    }

    /// <inheritdoc/>
    public ITool? GetTool(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tool name cannot be null or whitespace.", nameof(name));

        return _tools.TryGetValue(name, out var tool) ? tool : null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ITool> GetAllTools() => _tools.Values.ToList().AsReadOnly();

    /// <inheritdoc/>
    public bool Unregister(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tool name cannot be null or whitespace.", nameof(name));

        return _tools.Remove(name);
    }

    /// <inheritdoc/>
    public bool IsRegistered(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return _tools.ContainsKey(name);
    }
}
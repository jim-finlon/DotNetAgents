namespace DotLangChain.Abstractions.Agents.Tools;

/// <summary>
/// Registry for tool implementations.
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// Registers a tool type (will be instantiated via DI if needed).
    /// </summary>
    /// <typeparam name="T">Tool class type with methods marked with [Tool] attribute.</typeparam>
    void Register<T>() where T : class;

    /// <summary>
    /// Registers a tool instance.
    /// </summary>
    /// <param name="toolInstance">Tool instance.</param>
    void Register(object toolInstance);

    /// <summary>
    /// Registers a tool with a delegate handler.
    /// </summary>
    /// <param name="name">Tool name.</param>
    /// <param name="handler">Tool execution handler.</param>
    /// <param name="description">Tool description.</param>
    void Register(string name, Delegate handler, string? description = null);

    /// <summary>
    /// Builds a tool executor from registered tools.
    /// </summary>
    /// <returns>Tool executor instance.</returns>
    IToolExecutor BuildExecutor();
}


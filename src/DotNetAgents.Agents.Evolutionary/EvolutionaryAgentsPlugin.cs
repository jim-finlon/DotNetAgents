using DotNetAgents.Ecosystem;

namespace DotNetAgents.Agents.Evolutionary;

/// <summary>
/// Plugin class for Evolutionary Agents functionality.
/// </summary>
public sealed class EvolutionaryAgentsPlugin : IPlugin
{
    /// <inheritdoc/>
    public string Id => "DotNetAgents.Agents.Evolutionary";

    /// <inheritdoc/>
    public string Name => "EvolutionaryAgents";

    /// <inheritdoc/>
    public string Version => "1.0.0";

    /// <inheritdoc/>
    public string Description => "Evolutionary Agent System that enables agents to evolve through genetic algorithms";

    /// <inheritdoc/>
    public string Author => "DotNetAgents Contributors";

    /// <inheritdoc/>
    public string License => "MIT";

    /// <inheritdoc/>
    public Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        // Plugin initialization is handled by ServiceCollectionExtensions
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        // Cleanup if needed
        return Task.CompletedTask;
    }
}

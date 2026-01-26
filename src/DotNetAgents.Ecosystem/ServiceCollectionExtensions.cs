using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetAgents.Ecosystem;

/// <summary>
/// Extension methods for registering ecosystem services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ecosystem support to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDotNetAgentsEcosystem(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register plugin registry
        services.TryAddSingleton<IPluginRegistry, PluginRegistry>();

        // Register integration marketplace
        services.TryAddSingleton<IIntegrationMarketplace, InMemoryIntegrationMarketplace>();

        return services;
    }
}

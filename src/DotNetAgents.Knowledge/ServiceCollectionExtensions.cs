using DotNetAgents.Knowledge.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetAgents.Knowledge;

/// <summary>
/// Extension methods for registering DotNetAgents.Knowledge services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DotNetAgents.Knowledge services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureStore">Optional action to configure the knowledge store. If not provided, uses InMemoryKnowledgeStore.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDotNetAgentsKnowledge(
        this IServiceCollection services,
        Action<IServiceCollection>? configureStore = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register store
        if (configureStore != null)
        {
            configureStore(services);
        }
        else
        {
            services.AddSingleton<IKnowledgeStore, InMemoryKnowledgeStore>();
        }

        // Register repository
        services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();

        return services;
    }
}

using DotNetAgents.Workflow.Checkpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Storage.PostgreSQL;

/// <summary>
/// Extension methods for registering PostgreSQL checkpoint store in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL checkpoint store to the service collection.
    /// </summary>
    /// <typeparam name="TState">The type of the workflow state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="tableName">Optional table name. Default: "workflow_checkpoints".</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPostgreSQLCheckpointStore<TState>(
        this IServiceCollection services,
        string connectionString,
        string tableName = "workflow_checkpoints")
        where TState : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddSingleton<ICheckpointStore<TState>>(sp =>
        {
            var logger = sp.GetService<ILogger<PostgreSQLCheckpointStore<TState>>>();
            return new PostgreSQLCheckpointStore<TState>(connectionString, tableName, null, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL checkpoint store to the service collection with a custom serializer.
    /// </summary>
    /// <typeparam name="TState">The type of the workflow state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="serializer">The state serializer to use.</param>
    /// <param name="tableName">Optional table name. Default: "workflow_checkpoints".</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPostgreSQLCheckpointStore<TState>(
        this IServiceCollection services,
        string connectionString,
        IStateSerializer<TState> serializer,
        string tableName = "workflow_checkpoints")
        where TState : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(serializer);

        services.AddSingleton<ICheckpointStore<TState>>(sp =>
        {
            var logger = sp.GetService<ILogger<PostgreSQLCheckpointStore<TState>>>();
            return new PostgreSQLCheckpointStore<TState>(connectionString, tableName, serializer, logger);
        });

        return services;
    }
}
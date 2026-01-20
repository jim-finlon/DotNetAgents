using DotNetAgents.Workflow.Checkpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Storage.SqlServer;

/// <summary>
/// Extension methods for registering SQL Server checkpoint store in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server checkpoint store to the service collection.
    /// </summary>
    /// <typeparam name="TState">The type of the workflow state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="tableName">Optional table name. Default: "WorkflowCheckpoints".</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddSqlServerCheckpointStore<TState>(
        this IServiceCollection services,
        string connectionString,
        string tableName = "WorkflowCheckpoints")
        where TState : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddSingleton<ICheckpointStore<TState>>(sp =>
        {
            var logger = sp.GetService<ILogger<SqlServerCheckpointStore<TState>>>();
            return new SqlServerCheckpointStore<TState>(connectionString, tableName, null, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds SQL Server checkpoint store to the service collection with a custom serializer.
    /// </summary>
    /// <typeparam name="TState">The type of the workflow state.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="serializer">The state serializer to use.</param>
    /// <param name="tableName">Optional table name. Default: "WorkflowCheckpoints".</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddSqlServerCheckpointStore<TState>(
        this IServiceCollection services,
        string connectionString,
        IStateSerializer<TState> serializer,
        string tableName = "WorkflowCheckpoints")
        where TState : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(serializer);

        services.AddSingleton<ICheckpointStore<TState>>(sp =>
        {
            var logger = sp.GetService<ILogger<SqlServerCheckpointStore<TState>>>();
            return new SqlServerCheckpointStore<TState>(connectionString, tableName, serializer, logger);
        });

        return services;
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetAgents.Voice.Dialog;

/// <summary>
/// Extension methods for registering dialog management services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds dialog management services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDialogManagement(
        this IServiceCollection services,
        Action<DialogManagementOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new DialogManagementOptions();
        configure?.Invoke(options);

        // Register dialog store (default to in-memory)
        if (options.DialogStoreFactory != null)
        {
            services.TryAddSingleton<IDialogStore>(options.DialogStoreFactory);
        }
        else
        {
            services.TryAddSingleton<IDialogStore, InMemoryDialogStore>();
        }

        // Register dialog manager
        services.TryAddScoped<IDialogManager, DialogManager>();

        return services;
    }

    /// <summary>
    /// Registers a dialog handler.
    /// </summary>
    /// <typeparam name="T">The dialog handler type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDialogHandler<T>(this IServiceCollection services)
        where T : class, IDialogHandler
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<T>();
        services.TryAddScoped<IDialogHandler, T>();
        return services;
    }
}

/// <summary>
/// Options for dialog management.
/// </summary>
public class DialogManagementOptions
{
    /// <summary>
    /// Gets or sets a factory function for creating the dialog store.
    /// </summary>
    public Func<IServiceProvider, IDialogStore>? DialogStoreFactory { get; set; }
}

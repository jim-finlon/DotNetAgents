using DotNetAgents.Abstractions.Models;
using DotNetAgents.Voice.IntentClassification;
using DotNetAgents.Voice.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Voice;

/// <summary>
/// Extension methods for registering voice command services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds voice command processing services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoiceCommands(
        this IServiceCollection services,
        Action<VoiceCommandOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new VoiceCommandOptions();
        configure?.Invoke(options);

        // Register taxonomy registry (singleton - shared across all classifiers)
        services.TryAddSingleton<IIntentTaxonomyRegistry, IntentTaxonomyRegistry>();

        // Register command template registry (singleton)
        services.TryAddSingleton<Commands.ICommandTemplateRegistry, Commands.CommandTemplateRegistry>();

        // Register intent classifier
        services.TryAddScoped<IIntentClassifier>(sp =>
        {
            var llmModel = sp.GetRequiredService<ILLMModel<ChatMessage[], ChatMessage>>();
            var logger = sp.GetRequiredService<ILogger<LLMIntentClassifier>>();
            var taxonomyRegistry = sp.GetRequiredService<IIntentTaxonomyRegistry>();
            return new LLMIntentClassifier(llmModel, logger, taxonomyRegistry);
        });

        // Register command parser
        services.TryAddScoped<ICommandParser, CommandParser>();

        return services;
    }

    /// <summary>
    /// Adds command workflow orchestration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCommandOrchestration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<Orchestration.ICommandWorkflowOrchestrator, Orchestration.CommandWorkflowOrchestrator>();

        return services;
    }

    /// <summary>
    /// Adds a custom intent classifier implementation.
    /// </summary>
    /// <typeparam name="T">The intent classifier implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntentClassifier<T>(this IServiceCollection services)
        where T : class, IIntentClassifier
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IIntentClassifier, T>();
        return services;
    }
}

/// <summary>
/// Options for voice command processing.
/// </summary>
public class VoiceCommandOptions
{
    /// <summary>
    /// Gets or sets whether to use structured output mode for intent classification.
    /// </summary>
    public bool UseStructuredOutput { get; set; } = true;

    /// <summary>
    /// Gets or sets the default confidence threshold for intent classification.
    /// </summary>
    public double ConfidenceThreshold { get; set; } = 0.7;
}

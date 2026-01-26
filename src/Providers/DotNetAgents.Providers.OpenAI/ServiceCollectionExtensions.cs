using DotNetAgents.Abstractions.Models;
using DotNetAgents.Ecosystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.OpenAI;

/// <summary>
/// Extension methods for registering OpenAI services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAI provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="modelName">The default model name to use (e.g., "gpt-3.5-turbo").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or apiKey is null.</exception>
    public static IServiceCollection AddOpenAI(
        this IServiceCollection services,
        string apiKey,
        string modelName = "gpt-3.5-turbo",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        // Register the OpenAI provider plugin
        services.AddPlugin(new OpenAIProviderPlugin());

        services.AddHttpClient<OpenAIModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(OpenAIModel));
            var logger = sp.GetService<ILogger<OpenAIModel>>();
            return new OpenAIModel(httpClient, apiKey, modelName, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds OpenAI provider services using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="modelName">The default model name to use.</param>
    /// <param name="configureOptions">Optional action to configure the OpenAI options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenAI(
        this IServiceCollection services,
        string apiKey,
        string modelName,
        Action<OpenAIOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Register the OpenAI provider plugin (idempotent)
        services.AddPlugin(new OpenAIProviderPlugin());

        var options = new OpenAIOptions
        {
            ApiKey = apiKey,
            ModelName = modelName
        };
        configureOptions(options);

        services.AddHttpClient<OpenAIModel>(client =>
        {
            options.ConfigureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(OpenAIModel));
            var logger = sp.GetService<ILogger<OpenAIModel>>();
            return new OpenAIModel(httpClient, options.ApiKey, options.ModelName, logger);
        });

        return services;
    }
}

/// <summary>
/// Configuration options for OpenAI provider.
/// </summary>
public class OpenAIOptions
{
    /// <summary>
    /// Gets or sets the OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default model name.
    /// </summary>
    public string ModelName { get; set; } = "gpt-3.5-turbo";

    /// <summary>
    /// Gets or sets an optional action to configure the HTTP client.
    /// </summary>
    public Action<HttpClient>? ConfigureHttpClient { get; set; }
}
using DotNetAgents.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Mistral;

/// <summary>
/// Extension methods for registering Mistral AI services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Mistral AI provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The Mistral AI API key.</param>
    /// <param name="modelName">The default model name to use (e.g., "mistral-small").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or apiKey is null.</exception>
    public static IServiceCollection AddMistral(
        this IServiceCollection services,
        string apiKey,
        string modelName = "mistral-small",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        services.AddHttpClient<MistralModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(MistralModel));
            var logger = sp.GetService<ILogger<MistralModel>>();
            return new MistralModel(httpClient, apiKey, modelName, logger);
        });

        return services;
    }
}
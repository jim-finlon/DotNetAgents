using DotNetAgents.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Google;

/// <summary>
/// Extension methods for registering Google Gemini services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Google Gemini provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The Google AI API key.</param>
    /// <param name="modelName">The default model name to use (e.g., "gemini-pro").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or apiKey is null.</exception>
    public static IServiceCollection AddGoogleGemini(
        this IServiceCollection services,
        string apiKey,
        string modelName = "gemini-pro",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        services.AddHttpClient<GeminiModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(GeminiModel));
            var logger = sp.GetService<ILogger<GeminiModel>>();
            return new GeminiModel(httpClient, apiKey, modelName, logger);
        });

        return services;
    }
}
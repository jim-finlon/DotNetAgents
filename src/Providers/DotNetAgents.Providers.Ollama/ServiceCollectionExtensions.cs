using DotNetAgents.Abstractions.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Ollama;

/// <summary>
/// Extension methods for registering Ollama services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Ollama provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modelName">The name of the model to use (e.g., "llama2", "mistral").</param>
    /// <param name="baseUrl">The base URL of the Ollama server (default: "http://localhost:11434").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddOllama(
        this IServiceCollection services,
        string modelName = "llama2",
        string baseUrl = "http://localhost:11434",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelName);

        services.AddHttpClient<OllamaModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(OllamaModel));
            var logger = sp.GetService<ILogger<OllamaModel>>();
            return new OllamaModel(httpClient, baseUrl, modelName, logger);
        });

        return services;
    }
}
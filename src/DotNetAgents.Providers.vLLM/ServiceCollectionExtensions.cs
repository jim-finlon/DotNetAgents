using DotNetAgents.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.vLLM;

/// <summary>
/// Extension methods for registering vLLM services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds vLLM provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modelName">The name of the model to use.</param>
    /// <param name="baseUrl">The base URL of the vLLM server (default: "http://localhost:8000").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddVLLM(
        this IServiceCollection services,
        string modelName = "local-model",
        string baseUrl = "http://localhost:8000",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelName);

        services.AddHttpClient<vLLMModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(vLLMModel));
            var logger = sp.GetService<ILogger<vLLMModel>>();
            return new vLLMModel(httpClient, baseUrl, modelName, logger);
        });

        return services;
    }
}
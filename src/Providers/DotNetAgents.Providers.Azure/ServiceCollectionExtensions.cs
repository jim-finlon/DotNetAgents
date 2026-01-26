using DotNetAgents.Abstractions.Models;
using DotNetAgents.Ecosystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Azure;

/// <summary>
/// Extension methods for registering Azure OpenAI services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure OpenAI provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="endpoint">The Azure OpenAI endpoint URL.</param>
    /// <param name="apiKey">The Azure OpenAI API key.</param>
    /// <param name="deploymentName">The deployment name of the model.</param>
    /// <param name="apiVersion">The API version to use (default: "2024-02-15-preview").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or required parameters are null.</exception>
    public static IServiceCollection AddAzureOpenAI(
        this IServiceCollection services,
        string endpoint,
        string apiKey,
        string deploymentName,
        string apiVersion = "2024-02-15-preview",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentName);

        services.AddPlugin(new AzureProviderPlugin());

        services.AddHttpClient<AzureOpenAIModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(AzureOpenAIModel));
            var logger = sp.GetService<ILogger<AzureOpenAIModel>>();
            return new AzureOpenAIModel(httpClient, endpoint, apiKey, deploymentName, apiVersion, logger);
        });

        return services;
    }
}
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Ecosystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.LMStudio;

/// <summary>
/// Extension methods for registering LM Studio services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds LM Studio provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modelName">The name of the model to use (optional, LM Studio uses the loaded model).</param>
    /// <param name="baseUrl">The base URL of the LM Studio server (default: "http://localhost:1234").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddLMStudio(
        this IServiceCollection services,
        string modelName = "local-model",
        string baseUrl = "http://localhost:1234",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPlugin(new LMStudioProviderPlugin());

        services.AddHttpClient<LMStudioModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(LMStudioModel));
            var logger = sp.GetService<ILogger<LMStudioModel>>();
            return new LMStudioModel(httpClient, baseUrl, modelName, logger);
        });

        return services;
    }
}
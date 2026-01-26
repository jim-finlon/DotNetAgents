using DotNetAgents.Abstractions.Models;
using DotNetAgents.Ecosystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Together;

/// <summary>
/// Extension methods for registering Together AI services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Together AI provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The Together AI API key.</param>
    /// <param name="modelName">The default model name to use (e.g., "meta-llama/Llama-2-70b-chat-hf").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or apiKey is null.</exception>
    public static IServiceCollection AddTogether(
        this IServiceCollection services,
        string apiKey,
        string modelName = "meta-llama/Llama-2-70b-chat-hf",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        services.AddPlugin(new TogetherProviderPlugin());

        services.AddHttpClient<TogetherModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(TogetherModel));
            var logger = sp.GetService<ILogger<TogetherModel>>();
            return new TogetherModel(httpClient, apiKey, modelName, logger);
        });

        return services;
    }
}
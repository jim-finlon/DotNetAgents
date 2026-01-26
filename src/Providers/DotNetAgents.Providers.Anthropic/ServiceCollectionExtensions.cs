using DotNetAgents.Abstractions.Models;
using DotNetAgents.Ecosystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Anthropic;

/// <summary>
/// Extension methods for registering Anthropic services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Anthropic provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The Anthropic API key.</param>
    /// <param name="modelName">The default model name to use (e.g., "claude-3-sonnet-20240229").</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or apiKey is null.</exception>
    public static IServiceCollection AddAnthropic(
        this IServiceCollection services,
        string apiKey,
        string modelName = "claude-3-sonnet-20240229",
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        services.AddPlugin(new AnthropicProviderPlugin());

        services.AddHttpClient<AnthropicModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(AnthropicModel));
            var logger = sp.GetService<ILogger<AnthropicModel>>();
            return new AnthropicModel(httpClient, apiKey, modelName, logger);
        });

        return services;
    }
}
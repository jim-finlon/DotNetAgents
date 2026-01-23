using DotNetAgents.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.AWS;

/// <summary>
/// Extension methods for registering AWS Bedrock services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AWS Bedrock provider services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="modelId">The Bedrock model ID (e.g., "anthropic.claude-3-sonnet-20240229-v1:0").</param>
    /// <param name="region">The AWS region (default: "us-east-1").</param>
    /// <param name="accessKeyId">Optional AWS access key ID (prefer IAM roles in production).</param>
    /// <param name="secretAccessKey">Optional AWS secret access key (prefer IAM roles in production).</param>
    /// <param name="configureHttpClient">Optional action to configure the HTTP client.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <remarks>
    /// In production, use AWS SDK credentials provider or IAM roles instead of passing credentials directly.
    /// </remarks>
    public static IServiceCollection AddAWSBedrock(
        this IServiceCollection services,
        string modelId,
        string region = "us-east-1",
        string? accessKeyId = null,
        string? secretAccessKey = null,
        Action<HttpClient>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        services.AddHttpClient<BedrockModel>(client =>
        {
            configureHttpClient?.Invoke(client);
        });

        services.AddSingleton<ILLMModel<string, string>>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(BedrockModel));
            var logger = sp.GetService<ILogger<BedrockModel>>();
            return new BedrockModel(httpClient, modelId, region, accessKeyId, secretAccessKey, logger);
        });

        return services;
    }
}
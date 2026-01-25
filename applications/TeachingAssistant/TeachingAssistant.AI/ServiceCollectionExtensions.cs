using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TeachingAssistant.AI;

/// <summary>
/// Extension methods for registering ModelRouter services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ModelRouter services to the service collection.
    /// </summary>
    public static IServiceCollection AddModelRouter(
        this IServiceCollection services,
        Action<ModelRouterOptions>? configure = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddHttpClient();
        services.AddOptions<ModelRouterOptions>();

        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            // Default configuration
            services.Configure<ModelRouterOptions>(options =>
            {
                options.Endpoints = new Dictionary<ModelTier, ModelEndpoint>
                {
                    [ModelTier.LocalFast] = new ModelEndpoint(
                        ModelTier.LocalFast,
                        "http://localhost:8002/v1",
                        "microsoft/Phi-3-medium-128k-instruct",
                        MaxTokens: 4096,
                        CostPer1kTokens: 0),

                    [ModelTier.LocalStandard] = new ModelEndpoint(
                        ModelTier.LocalStandard,
                        "http://localhost:8001/v1",
                        "mistral-7b-science-tutor",
                        MaxTokens: 4096,
                        CostPer1kTokens: 0),

                    [ModelTier.LocalHeavy] = new ModelEndpoint(
                        ModelTier.LocalHeavy,
                        "http://localhost:8000/v1",
                        "meta-llama/Meta-Llama-3.1-70B-Instruct",
                        MaxTokens: 8192,
                        CostPer1kTokens: 0),

                    [ModelTier.CloudClaude] = new ModelEndpoint(
                        ModelTier.CloudClaude,
                        "https://api.anthropic.com/v1",
                        "claude-sonnet-4-20250514",
                        MaxTokens: 8192,
                        CostPer1kTokens: 0.015m,
                        RequiresApiKey: true,
                        ApiKeyEnvironmentVariable: "ANTHROPIC_API_KEY"),

                    [ModelTier.CloudOpenAI] = new ModelEndpoint(
                        ModelTier.CloudOpenAI,
                        "https://api.openai.com/v1",
                        "gpt-4o",
                        MaxTokens: 8192,
                        CostPer1kTokens: 0.005m,
                        RequiresApiKey: true,
                        ApiKeyEnvironmentVariable: "OPENAI_API_KEY")
                };
            });
        }

        services.AddSingleton<IModelRouter, ModelRouter>();
        services.AddHostedService<ModelHealthMonitor>();

        return services;
    }
}

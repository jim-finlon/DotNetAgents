using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Providers.Anthropic;
using DotNetAgents.Providers.OpenAI;
using DotNetAgents.Providers.vLLM;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TeachingAssistant.AI;

/// <summary>
/// Task complexity levels for routing decisions.
/// </summary>
public enum TaskComplexity
{
    /// <summary>
    /// Simple tasks: classification, short responses, quick lookups.
    /// </summary>
    Simple,

    /// <summary>
    /// Standard tasks: normal tutoring dialogue, content generation.
    /// </summary>
    Standard,

    /// <summary>
    /// Complex tasks: multi-step reasoning, assessment generation, complex content.
    /// </summary>
    Complex,

    /// <summary>
    /// Critical tasks: safety-sensitive content, requires cloud backup.
    /// </summary>
    Critical
}

/// <summary>
/// Model tier for routing.
/// </summary>
public enum ModelTier
{
    /// <summary>
    /// Local fast model (Phi-3, 16GB GPU).
    /// </summary>
    LocalFast,

    /// <summary>
    /// Local standard model (Mistral fine-tuned, 16GB GPU).
    /// </summary>
    LocalStandard,

    /// <summary>
    /// Local heavy model (Llama 70B, RTX 6000 Pro).
    /// </summary>
    LocalHeavy,

    /// <summary>
    /// Cloud Claude API.
    /// </summary>
    CloudClaude,

    /// <summary>
    /// Cloud OpenAI API.
    /// </summary>
    CloudOpenAI
}

/// <summary>
/// Model endpoint configuration.
/// </summary>
public record ModelEndpoint(
    ModelTier Tier,
    string Url,
    string ModelName,
    int MaxTokens,
    decimal CostPer1kTokens,
    bool RequiresApiKey = false,
    string? ApiKeyEnvironmentVariable = null);

/// <summary>
/// Model router options.
/// </summary>
public class ModelRouterOptions
{
    /// <summary>
    /// Gets or sets the model endpoints configuration.
    /// </summary>
    public Dictionary<ModelTier, ModelEndpoint> Endpoints { get; set; } = new();

    /// <summary>
    /// Gets or sets the health check interval.
    /// </summary>
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the health check timeout.
    /// </summary>
    public TimeSpan HealthCheckTimeout { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// Intelligent model router that selects optimal LLM based on task complexity and availability.
/// </summary>
public interface IModelRouter
{
    /// <summary>
    /// Gets an LLM model instance for the specified complexity level.
    /// </summary>
    Task<ILLMModel<string, string>> GetModelAsync(
        TaskComplexity complexity,
        bool requiresSafetyCheck = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects the optimal model endpoint for the given complexity.
    /// </summary>
    Task<ModelEndpoint> SelectModelAsync(
        TaskComplexity complexity,
        bool requiresSafetyCheck = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs health checks on all model endpoints.
    /// </summary>
    Task HealthCheckAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of intelligent model router.
/// </summary>
public class ModelRouter : IModelRouter
{
    private readonly ModelRouterOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ModelRouter> _logger;
    private readonly Dictionary<ModelTier, bool> _healthStatus = new();
    private readonly Dictionary<ModelTier, ILLMModel<string, string>> _modelCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRouter"/> class.
    /// </summary>
    public ModelRouter(
        IOptions<ModelRouterOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<ModelRouter> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize health status
        foreach (var tier in Enum.GetValues<ModelTier>())
        {
            _healthStatus[tier] = true; // Assume healthy initially
        }
    }

    /// <inheritdoc />
    public async Task<ILLMModel<string, string>> GetModelAsync(
        TaskComplexity complexity,
        bool requiresSafetyCheck = false,
        CancellationToken cancellationToken = default)
    {
        var endpoint = await SelectModelAsync(complexity, requiresSafetyCheck, cancellationToken);

        // Check cache first
        if (_modelCache.TryGetValue(endpoint.Tier, out var cachedModel))
        {
            return cachedModel;
        }

        // Create model instance
        var model = CreateModelInstance(endpoint);
        _modelCache[endpoint.Tier] = model;

        return model;
    }

    /// <inheritdoc />
    public async Task<ModelEndpoint> SelectModelAsync(
        TaskComplexity complexity,
        bool requiresSafetyCheck = false,
        CancellationToken cancellationToken = default)
    {
        var endpoints = _options.Endpoints;

        // Safety-critical always goes to cloud
        if (requiresSafetyCheck)
        {
            if (endpoints.TryGetValue(ModelTier.CloudClaude, out var claudeEndpoint))
            {
                return claudeEndpoint;
            }
            if (endpoints.TryGetValue(ModelTier.CloudOpenAI, out var openAiEndpoint))
            {
                return openAiEndpoint;
            }
        }

        // Route by complexity
        var candidates = complexity switch
        {
            TaskComplexity.Simple => new[] { ModelTier.LocalFast, ModelTier.LocalStandard },
            TaskComplexity.Standard => new[] { ModelTier.LocalStandard, ModelTier.LocalHeavy },
            TaskComplexity.Complex => new[] { ModelTier.LocalHeavy, ModelTier.CloudClaude },
            TaskComplexity.Critical => new[] { ModelTier.CloudClaude },
            _ => new[] { ModelTier.CloudClaude }
        };

        // Check health and select first available
        foreach (var tier in candidates)
        {
            if (_healthStatus.GetValueOrDefault(tier, false) && endpoints.ContainsKey(tier))
            {
                return endpoints[tier];
            }
        }

        // Fallback to cloud
        if (endpoints.TryGetValue(ModelTier.CloudClaude, out var fallbackClaude))
        {
            return fallbackClaude;
        }
        if (endpoints.TryGetValue(ModelTier.CloudOpenAI, out var fallbackOpenAI))
        {
            return fallbackOpenAI;
        }

        throw new InvalidOperationException("No available model endpoints configured");
    }

    /// <inheritdoc />
    public async Task HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = _options.HealthCheckTimeout;

        foreach (var (tier, endpoint) in _options.Endpoints)
        {
            if (tier.ToString().StartsWith("Local"))
            {
                try
                {
                    var healthUrl = endpoint.Url.TrimEnd('/') + "/health";
                    var response = await client.GetAsync(healthUrl, cancellationToken);
                    _healthStatus[tier] = response.IsSuccessStatusCode;

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Health check failed for {Tier} at {Url}", tier, endpoint.Url);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Health check failed for {Tier}", tier);
                    _healthStatus[tier] = false;
                }
            }
            else
            {
                // Cloud endpoints are assumed healthy (will fail on actual use if not)
                _healthStatus[tier] = true;
            }
        }
    }

    private ILLMModel<string, string> CreateModelInstance(ModelEndpoint endpoint)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(endpoint.Url);

        return endpoint.Tier switch
        {
            ModelTier.LocalFast or ModelTier.LocalStandard or ModelTier.LocalHeavy => new vLLMModel(
                httpClient,
                endpoint.Url,
                endpoint.ModelName,
                null), // Logger type mismatch - pass null for now

            ModelTier.CloudClaude => new AnthropicModel(
                httpClient,
                Environment.GetEnvironmentVariable(endpoint.ApiKeyEnvironmentVariable ?? "ANTHROPIC_API_KEY") ?? throw new InvalidOperationException("Anthropic API key not found"),
                endpoint.ModelName,
                null), // Logger type mismatch - pass null for now

            ModelTier.CloudOpenAI => new OpenAIModel(
                httpClient,
                Environment.GetEnvironmentVariable(endpoint.ApiKeyEnvironmentVariable ?? "OPENAI_API_KEY") ?? throw new InvalidOperationException("OpenAI API key not found"),
                endpoint.ModelName,
                null), // Logger type mismatch - pass null for now

            _ => throw new NotSupportedException($"Model tier {endpoint.Tier} is not supported")
        };
    }
}

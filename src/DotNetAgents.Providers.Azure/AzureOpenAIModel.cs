using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Core.Exceptions;
using DotNetAgents.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Azure;

/// <summary>
/// Azure OpenAI implementation of <see cref="ILLMModel{TInput, TOutput}"/> for chat completions.
/// </summary>
public class AzureOpenAIModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _deploymentName;
    private readonly string _apiVersion;
    private readonly ILogger<AzureOpenAIModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureOpenAIModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="endpoint">The Azure OpenAI endpoint URL (e.g., "https://your-resource.openai.azure.com").</param>
    /// <param name="apiKey">The Azure OpenAI API key.</param>
    /// <param name="deploymentName">The deployment name of the model.</param>
    /// <param name="apiVersion">The API version to use (default: "2024-02-15-preview").</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null or empty.</exception>
    public AzureOpenAIModel(
        HttpClient httpClient,
        string endpoint,
        string apiKey,
        string deploymentName,
        string apiVersion = "2024-02-15-preview",
        ILogger<AzureOpenAIModel>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        _apiKey = string.IsNullOrWhiteSpace(apiKey) ? throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey)) : apiKey;
        _deploymentName = string.IsNullOrWhiteSpace(deploymentName) ? throw new ArgumentException("Deployment name cannot be null or empty.", nameof(deploymentName)) : deploymentName;
        _apiVersion = string.IsNullOrWhiteSpace(apiVersion) ? throw new ArgumentException("API version cannot be null or empty.", nameof(apiVersion)) : apiVersion;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Configure HTTP client for Azure OpenAI
        var baseUri = endpoint.TrimEnd('/');
        _httpClient.BaseAddress = new Uri($"{baseUri}/openai/deployments/{_deploymentName}/");
        _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
    }

    /// <inheritdoc/>
    public string ModelName => _deploymentName;

    /// <inheritdoc/>
    public int MaxTokens => 8192; // Azure OpenAI typically supports up to 8192 tokens

    /// <inheritdoc/>
    public async Task<string> GenerateAsync(
        string input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = new ChatCompletionRequest
        {
            Messages = new[]
            {
                new ChatCompletionMessage
                {
                    Role = "user",
                    Content = input
                }
            },
            Temperature = options?.Temperature,
            MaxTokens = options?.MaxTokens,
            TopP = options?.TopP,
            FrequencyPenalty = options?.FrequencyPenalty,
            PresencePenalty = options?.PresencePenalty
        };

        try
        {
            _logger?.LogDebug("Calling Azure OpenAI API with deployment {DeploymentName}", _deploymentName);

            var url = $"chat/completions?api-version={_apiVersion}";
            var response = await _httpClient.PostAsJsonAsync(
                url,
                request,
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var completionResponse = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            if (completionResponse == null)
            {
                throw new AgentException("Failed to deserialize Azure OpenAI response.", ErrorCategory.LLMError);
            }

            if (completionResponse.Choices == null || completionResponse.Choices.Length == 0)
            {
                throw new AgentException("Azure OpenAI response contained no choices.", ErrorCategory.LLMError);
            }

            var content = completionResponse.Choices[0].Message?.Content;
            if (content == null)
            {
                throw new AgentException("Azure OpenAI response contained no content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("Azure OpenAI API call completed successfully. Tokens used: {Usage}", completionResponse.Usage?.TotalTokens ?? 0);

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling Azure OpenAI API");
            throw new AgentException($"Azure OpenAI API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Azure OpenAI API");
            throw new AgentException("Azure OpenAI API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for Azure OpenAI response");
            throw new AgentException("Failed to parse Azure OpenAI response.", ErrorCategory.LLMError, ex);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string input,
        LLMOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = new ChatCompletionRequest
        {
            Messages = new[]
            {
                new ChatCompletionMessage
                {
                    Role = "user",
                    Content = input
                }
            },
            Stream = true,
            Temperature = options?.Temperature,
            MaxTokens = options?.MaxTokens,
            TopP = options?.TopP,
            FrequencyPenalty = options?.FrequencyPenalty,
            PresencePenalty = options?.PresencePenalty
        };

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            var url = $"chat/completions?api-version={_apiVersion}";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(request, options: _jsonOptions)
            };

            response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            reader = new StreamReader(stream);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling Azure OpenAI streaming API");
            throw new AgentException($"Azure OpenAI streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Azure OpenAI streaming API");
            throw new AgentException("Azure OpenAI streaming API call timed out.", ErrorCategory.LLMError, ex);
        }

        if (stream == null || reader == null)
        {
            yield break;
        }

        try
        {
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ", StringComparison.Ordinal))
                {
                    continue;
                }

                var jsonData = line.Substring(6); // Remove "data: " prefix

                if (jsonData == "[DONE]")
                {
                    yield break;
                }

                var delta = ParseStreamChunk(jsonData);
                if (!string.IsNullOrEmpty(delta))
                {
                    yield return delta;
                }
            }
        }
        finally
        {
            reader?.Dispose();
            stream?.Dispose();
            response?.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GenerateBatchAsync(
        IEnumerable<string> inputs,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var inputList = inputs.ToList();
        if (inputList.Count == 0)
        {
            return Array.Empty<string>();
        }

        var tasks = inputList.Select(input => GenerateAsync(input, options, cancellationToken));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results;
    }

    private string? ParseStreamChunk(string jsonData)
    {
        try
        {
            var streamResponse = JsonSerializer.Deserialize<ChatCompletionStreamResponse>(jsonData, _jsonOptions);
            if (streamResponse?.Choices != null && streamResponse.Choices.Length > 0)
            {
                return streamResponse.Choices[0].Delta?.Content;
            }
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Failed to parse streaming chunk: {JsonData}", jsonData);
        }
        return null;
    }

    // Request/Response models for Azure OpenAI API (same structure as OpenAI)
    private record ChatCompletionRequest
    {
        [JsonPropertyName("messages")]
        public ChatCompletionMessage[] Messages { get; init; } = Array.Empty<ChatCompletionMessage>();

        [JsonPropertyName("temperature")]
        public double? Temperature { get; init; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; init; }

        [JsonPropertyName("top_p")]
        public double? TopP { get; init; }

        [JsonPropertyName("frequency_penalty")]
        public double? FrequencyPenalty { get; init; }

        [JsonPropertyName("presence_penalty")]
        public double? PresencePenalty { get; init; }

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }
    }

    private record ChatCompletionMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; init; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;
    }

    private record ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public ChatCompletionChoice[]? Choices { get; init; }

        [JsonPropertyName("usage")]
        public TokenUsage? Usage { get; init; }
    }

    private record ChatCompletionChoice
    {
        [JsonPropertyName("message")]
        public ChatCompletionMessage? Message { get; init; }
    }

    private record ChatCompletionStreamResponse
    {
        [JsonPropertyName("choices")]
        public ChatCompletionStreamChoice[]? Choices { get; init; }
    }

    private record ChatCompletionStreamChoice
    {
        [JsonPropertyName("delta")]
        public ChatCompletionMessage? Delta { get; init; }
    }

    private record TokenUsage
    {
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; init; }
    }
}
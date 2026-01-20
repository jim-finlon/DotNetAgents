using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Core.Exceptions;
using DotNetAgents.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.AWS;

/// <summary>
/// AWS Bedrock implementation of <see cref="ILLMModel{TInput, TOutput}"/> for chat completions.
/// AWS Bedrock provides access to multiple LLM providers through a unified API.
/// </summary>
public class BedrockModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _modelId;
    private readonly string _region;
    private readonly ILogger<BedrockModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="BedrockModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="modelId">The Bedrock model ID (e.g., "anthropic.claude-3-sonnet-20240229-v1:0", "amazon.titan-text-lite-v1").</param>
    /// <param name="region">The AWS region (e.g., "us-east-1").</param>
    /// <param name="accessKeyId">The AWS access key ID.</param>
    /// <param name="secretAccessKey">The AWS secret access key.</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null or empty.</exception>
    /// <remarks>
    /// Note: In production, use AWS SDK credentials provider or IAM roles instead of passing credentials directly.
    /// This implementation uses basic HTTP authentication for simplicity.
    /// </remarks>
    public BedrockModel(
        HttpClient httpClient,
        string modelId,
        string region = "us-east-1",
        string? accessKeyId = null,
        string? secretAccessKey = null,
        ILogger<BedrockModel>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _modelId = string.IsNullOrWhiteSpace(modelId) ? throw new ArgumentException("Model ID cannot be null or empty.", nameof(modelId)) : modelId;
        _region = string.IsNullOrWhiteSpace(region) ? throw new ArgumentException("Region cannot be null or empty.", nameof(region)) : region;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Configure HTTP client for AWS Bedrock
        _httpClient.BaseAddress = new Uri($"https://bedrock-runtime.{_region}.amazonaws.com/");
        
        // Note: AWS Bedrock uses AWS Signature Version 4 for authentication
        // In production, use AWS SDK or implement proper SigV4 signing
        // For now, this is a placeholder - actual implementation would use AWS SDK
        if (!string.IsNullOrEmpty(accessKeyId) && !string.IsNullOrEmpty(secretAccessKey))
        {
            _logger?.LogWarning("Direct credential passing is not recommended. Use AWS SDK credentials provider or IAM roles.");
        }
    }

    /// <inheritdoc/>
    public string ModelName => _modelId;

    /// <inheritdoc/>
    public int MaxTokens => GetMaxTokensForModel(_modelId);

    /// <inheritdoc/>
    public async Task<string> GenerateAsync(
        string input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        // Determine model provider and format request accordingly
        var request = CreateRequest(input, options, false);

        try
        {
            _logger?.LogDebug("Calling AWS Bedrock API with model {ModelId}", _modelId);

            var url = $"model/{_modelId}/invoke";
            var response = await _httpClient.PostAsJsonAsync(
                url,
                request,
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var content = ParseResponse(responseContent);

            if (string.IsNullOrEmpty(content))
            {
                throw new AgentException("AWS Bedrock response contained no content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("AWS Bedrock API call completed successfully");

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling AWS Bedrock API");
            throw new AgentException($"AWS Bedrock API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling AWS Bedrock API");
            throw new AgentException("AWS Bedrock API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for AWS Bedrock response");
            throw new AgentException("Failed to parse AWS Bedrock response.", ErrorCategory.LLMError, ex);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string input,
        LLMOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = CreateRequest(input, options, true);

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            var url = $"model/{_modelId}/invoke-with-response-stream";
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
            _logger?.LogError(ex, "HTTP error calling AWS Bedrock streaming API");
            throw new AgentException($"AWS Bedrock streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling AWS Bedrock streaming API");
            throw new AgentException("AWS Bedrock streaming API call timed out.", ErrorCategory.LLMError, ex);
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

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var delta = ParseStreamChunk(line);
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

    private object CreateRequest(string input, LLMOptions? options, bool stream)
    {
        // Determine provider based on model ID
        if (_modelId.StartsWith("anthropic.", StringComparison.OrdinalIgnoreCase))
        {
            return new AnthropicRequest
            {
                AnthropicVersion = "bedrock-2023-05-31",
                MaxTokens = options?.MaxTokens ?? 1024,
                Messages = new[]
                {
                    new AnthropicMessage
                    {
                        Role = "user",
                        Content = input
                    }
                },
                Temperature = options?.Temperature,
                TopP = options?.TopP,
                Stream = stream
            };
        }
        else if (_modelId.StartsWith("amazon.", StringComparison.OrdinalIgnoreCase))
        {
            return new AmazonRequest
            {
                InputText = input,
                TextGenerationConfig = new AmazonTextGenerationConfig
                {
                    Temperature = options?.Temperature ?? 0.7,
                    MaxTokenCount = options?.MaxTokens ?? 512,
                    TopP = options?.TopP ?? 0.9
                }
            };
        }
        else
        {
            // Default to Anthropic format for other models
            return new AnthropicRequest
            {
                AnthropicVersion = "bedrock-2023-05-31",
                MaxTokens = options?.MaxTokens ?? 1024,
                Messages = new[]
                {
                    new AnthropicMessage
                    {
                        Role = "user",
                        Content = input
                    }
                },
                Temperature = options?.Temperature,
                TopP = options?.TopP,
                Stream = stream
            };
        }
    }

    private string ParseResponse(string responseContent)
    {
        try
        {
            // Try Anthropic format first
            var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseContent, _jsonOptions);
            if (anthropicResponse?.Content != null && anthropicResponse.Content.Length > 0)
            {
                return anthropicResponse.Content[0].Text ?? string.Empty;
            }

            // Try Amazon format
            var amazonResponse = JsonSerializer.Deserialize<AmazonResponse>(responseContent, _jsonOptions);
            if (amazonResponse?.Results != null && amazonResponse.Results.Length > 0)
            {
                return amazonResponse.Results[0].OutputText ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            // Fall through to return empty string
        }

        return string.Empty;
    }

    private string? ParseStreamChunk(string jsonData)
    {
        try
        {
            var streamResponse = JsonSerializer.Deserialize<AnthropicStreamResponse>(jsonData, _jsonOptions);
            if (streamResponse?.Type == "content_block_delta" && streamResponse.Delta != null)
            {
                return streamResponse.Delta.Text;
            }
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Failed to parse streaming chunk: {JsonData}", jsonData);
        }
        return null;
    }

    private static int GetMaxTokensForModel(string modelId)
    {
        return modelId.ToUpperInvariant() switch
        {
            var m when m.Contains("CLAUDE-3", StringComparison.OrdinalIgnoreCase) => 4096,
            var m when m.Contains("TITAN", StringComparison.OrdinalIgnoreCase) => 4096,
            _ => 4096 // Default
        };
    }

    // Request/Response models for AWS Bedrock (supporting multiple providers)
    private record AnthropicRequest
    {
        [JsonPropertyName("anthropic_version")]
        public string AnthropicVersion { get; init; } = string.Empty;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; init; }

        [JsonPropertyName("messages")]
        public AnthropicMessage[] Messages { get; init; } = Array.Empty<AnthropicMessage>();

        [JsonPropertyName("temperature")]
        public double? Temperature { get; init; }

        [JsonPropertyName("top_p")]
        public double? TopP { get; init; }

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }
    }

    private record AnthropicMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; init; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;
    }

    private record AnthropicResponse
    {
        [JsonPropertyName("content")]
        public AnthropicContentBlock[]? Content { get; init; }
    }

    private record AnthropicContentBlock
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }

    private record AnthropicStreamResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("delta")]
        public AnthropicDelta? Delta { get; init; }
    }

    private record AnthropicDelta
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }

    private record AmazonRequest
    {
        [JsonPropertyName("inputText")]
        public string InputText { get; init; } = string.Empty;

        [JsonPropertyName("textGenerationConfig")]
        public AmazonTextGenerationConfig? TextGenerationConfig { get; init; }
    }

    private record AmazonTextGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; init; }

        [JsonPropertyName("maxTokenCount")]
        public int MaxTokenCount { get; init; }

        [JsonPropertyName("topP")]
        public double TopP { get; init; }
    }

    private record AmazonResponse
    {
        [JsonPropertyName("results")]
        public AmazonResult[]? Results { get; init; }
    }

    private record AmazonResult
    {
        [JsonPropertyName("outputText")]
        public string? OutputText { get; init; }
    }
}
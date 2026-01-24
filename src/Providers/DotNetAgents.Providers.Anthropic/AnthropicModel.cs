using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Abstractions.Exceptions;
using DotNetAgents.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Anthropic;

/// <summary>
/// Anthropic Claude implementation of <see cref="ILLMModel{TInput, TOutput}"/> for chat completions.
/// </summary>
public class AnthropicModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly ILogger<AnthropicModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnthropicModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="apiKey">The Anthropic API key.</param>
    /// <param name="modelName">The name of the model to use (e.g., "claude-3-opus-20240229", "claude-3-sonnet-20240229").</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient, apiKey, or modelName is null or empty.</exception>
    public AnthropicModel(
        HttpClient httpClient,
        string apiKey,
        string modelName = "claude-3-sonnet-20240229",
        ILogger<AnthropicModel>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = string.IsNullOrWhiteSpace(apiKey) ? throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey)) : apiKey;
        _modelName = string.IsNullOrWhiteSpace(modelName) ? throw new ArgumentException("Model name cannot be null or empty.", nameof(modelName)) : modelName;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Configure HTTP client for Anthropic API
        _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    /// <inheritdoc/>
    public string ModelName => _modelName;

    /// <inheritdoc/>
    public int MaxTokens => GetMaxTokensForModel(_modelName);

    /// <inheritdoc/>
    public async Task<string> GenerateAsync(
        string input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = new MessageRequest
        {
            Model = _modelName,
            MaxTokens = options?.MaxTokens ?? 1024,
            Messages = new[]
            {
                new Message
                {
                    Role = "user",
                    Content = input
                }
            },
            Temperature = options?.Temperature,
            TopP = options?.TopP
        };

        try
        {
            _logger?.LogDebug("Calling Anthropic API with model {ModelName}", _modelName);

            var response = await _httpClient.PostAsJsonAsync(
                "messages",
                request,
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var completionResponse = await response.Content.ReadFromJsonAsync<MessageResponse>(
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            if (completionResponse == null)
            {
                throw new AgentException("Failed to deserialize Anthropic response.", ErrorCategory.LLMError);
            }

            if (completionResponse.Content == null || completionResponse.Content.Length == 0)
            {
                throw new AgentException("Anthropic response contained no content.", ErrorCategory.LLMError);
            }

            var content = completionResponse.Content[0].Text;
            if (string.IsNullOrEmpty(content))
            {
                throw new AgentException("Anthropic response contained no text content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("Anthropic API call completed successfully. Tokens used: {Usage}", completionResponse.Usage?.InputTokens + completionResponse.Usage?.OutputTokens ?? 0);

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling Anthropic API");
            throw new AgentException($"Anthropic API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Anthropic API");
            throw new AgentException("Anthropic API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for Anthropic response");
            throw new AgentException("Failed to parse Anthropic response.", ErrorCategory.LLMError, ex);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string input,
        LLMOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = new MessageRequest
        {
            Model = _modelName,
            MaxTokens = options?.MaxTokens ?? 1024,
            Messages = new[]
            {
                new Message
                {
                    Role = "user",
                    Content = input
                }
            },
            Stream = true,
            Temperature = options?.Temperature,
            TopP = options?.TopP
        };

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "messages")
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
            _logger?.LogError(ex, "HTTP error calling Anthropic streaming API");
            throw new AgentException($"Anthropic streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Anthropic streaming API");
            throw new AgentException("Anthropic streaming API call timed out.", ErrorCategory.LLMError, ex);
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
            var streamResponse = JsonSerializer.Deserialize<StreamResponse>(jsonData, _jsonOptions);
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

    private static int GetMaxTokensForModel(string modelName)
    {
        return modelName.ToUpperInvariant() switch
        {
            var m when m.Contains("OPUS", StringComparison.OrdinalIgnoreCase) => 4096,
            var m when m.Contains("SONNET", StringComparison.OrdinalIgnoreCase) => 4096,
            var m when m.Contains("HAIKU", StringComparison.OrdinalIgnoreCase) => 4096,
            _ => 4096 // Default
        };
    }

    // Request/Response models for Anthropic API
    private record MessageRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; init; }

        [JsonPropertyName("messages")]
        public Message[] Messages { get; init; } = Array.Empty<Message>();

        [JsonPropertyName("temperature")]
        public double? Temperature { get; init; }

        [JsonPropertyName("top_p")]
        public double? TopP { get; init; }

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }
    }

    private record Message
    {
        [JsonPropertyName("role")]
        public string Role { get; init; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;
    }

    private record MessageResponse
    {
        [JsonPropertyName("content")]
        public ContentBlock[]? Content { get; init; }

        [JsonPropertyName("usage")]
        public TokenUsage? Usage { get; init; }
    }

    private record ContentBlock
    {
        [JsonPropertyName("text")]
        public string Text { get; init; } = string.Empty;
    }

    private record StreamResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("delta")]
        public StreamDelta? Delta { get; init; }
    }

    private record StreamDelta
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }

    private record TokenUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; init; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; init; }
    }
}
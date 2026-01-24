using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Abstractions.Exceptions;
using DotNetAgents.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Mistral;

/// <summary>
/// Mistral AI implementation of <see cref="ILLMModel{TInput, TOutput}"/> for chat completions.
/// Mistral AI provides high-quality open models with competitive performance.
/// </summary>
public class MistralModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly ILogger<MistralModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MistralModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="apiKey">The Mistral AI API key.</param>
    /// <param name="modelName">The name of the model to use (e.g., "mistral-tiny", "mistral-small", "mistral-medium", "mistral-large-latest").</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient, apiKey, or modelName is null or empty.</exception>
    public MistralModel(
        HttpClient httpClient,
        string apiKey,
        string modelName = "mistral-small",
        ILogger<MistralModel>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = string.IsNullOrWhiteSpace(apiKey) ? throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey)) : apiKey;
        _modelName = string.IsNullOrWhiteSpace(modelName) ? throw new ArgumentException("Model name cannot be null or empty.", nameof(modelName)) : modelName;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        // Configure HTTP client for Mistral AI API
        _httpClient.BaseAddress = new Uri("https://api.mistral.ai/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    /// <inheritdoc/>
    public string ModelName => _modelName;

    /// <inheritdoc/>
    public int MaxTokens => 8192; // Mistral models typically support up to 8192 tokens

    /// <inheritdoc/>
    public async Task<string> GenerateAsync(
        string input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = new ChatCompletionRequest
        {
            Model = _modelName,
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
            TopP = options?.TopP
        };

        try
        {
            _logger?.LogDebug("Calling Mistral AI API with model {ModelName}", _modelName);

            var response = await _httpClient.PostAsJsonAsync(
                "chat/completions",
                request,
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var completionResponse = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            if (completionResponse == null)
            {
                throw new AgentException("Failed to deserialize Mistral AI response.", ErrorCategory.LLMError);
            }

            if (completionResponse.Choices == null || completionResponse.Choices.Length == 0)
            {
                throw new AgentException("Mistral AI response contained no choices.", ErrorCategory.LLMError);
            }

            var content = completionResponse.Choices[0].Message?.Content;
            if (content == null)
            {
                throw new AgentException("Mistral AI response contained no content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("Mistral AI API call completed successfully");

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling Mistral AI API");
            throw new AgentException($"Mistral AI API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Mistral AI API");
            throw new AgentException("Mistral AI API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for Mistral AI response");
            throw new AgentException("Failed to parse Mistral AI response.", ErrorCategory.LLMError, ex);
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
            Model = _modelName,
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
            TopP = options?.TopP
        };

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
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
            _logger?.LogError(ex, "HTTP error calling Mistral AI streaming API");
            throw new AgentException($"Mistral AI streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Mistral AI streaming API");
            throw new AgentException("Mistral AI streaming API call timed out.", ErrorCategory.LLMError, ex);
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

    // Request/Response models for Mistral AI API (OpenAI-compatible)
    private record ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("messages")]
        public ChatCompletionMessage[] Messages { get; init; } = Array.Empty<ChatCompletionMessage>();

        [JsonPropertyName("temperature")]
        public double? Temperature { get; init; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; init; }

        [JsonPropertyName("top_p")]
        public double? TopP { get; init; }

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
}
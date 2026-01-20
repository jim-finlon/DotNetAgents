using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Core.Exceptions;
using DotNetAgents.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Together;

/// <summary>
/// Together AI implementation of <see cref="ILLMModel{TInput, TOutput}"/> for chat completions.
/// Together AI provides access to open-source LLMs with competitive pricing.
/// </summary>
public class TogetherModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly ILogger<TogetherModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="TogetherModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="apiKey">The Together AI API key.</param>
    /// <param name="modelName">The name of the model to use (e.g., "meta-llama/Llama-2-70b-chat-hf", "mistralai/Mixtral-8x7B-Instruct-v0.1").</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient, apiKey, or modelName is null or empty.</exception>
    public TogetherModel(
        HttpClient httpClient,
        string apiKey,
        string modelName = "meta-llama/Llama-2-70b-chat-hf",
        ILogger<TogetherModel>? logger = null)
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

        // Configure HTTP client for Together AI API (OpenAI-compatible)
        _httpClient.BaseAddress = new Uri("https://api.together.xyz/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    /// <inheritdoc/>
    public string ModelName => _modelName;

    /// <inheritdoc/>
    public int MaxTokens => 4096; // Together AI models typically support up to 4096 tokens

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
            _logger?.LogDebug("Calling Together AI API with model {ModelName}", _modelName);

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
                throw new AgentException("Failed to deserialize Together AI response.", ErrorCategory.LLMError);
            }

            if (completionResponse.Choices == null || completionResponse.Choices.Length == 0)
            {
                throw new AgentException("Together AI response contained no choices.", ErrorCategory.LLMError);
            }

            var content = completionResponse.Choices[0].Message?.Content;
            if (content == null)
            {
                throw new AgentException("Together AI response contained no content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("Together AI API call completed successfully");

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling Together AI API");
            throw new AgentException($"Together AI API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Together AI API");
            throw new AgentException("Together AI API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for Together AI response");
            throw new AgentException("Failed to parse Together AI response.", ErrorCategory.LLMError, ex);
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
            _logger?.LogError(ex, "HTTP error calling Together AI streaming API");
            throw new AgentException($"Together AI streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Together AI streaming API");
            throw new AgentException("Together AI streaming API call timed out.", ErrorCategory.LLMError, ex);
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

    // Request/Response models for Together AI API (OpenAI-compatible)
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
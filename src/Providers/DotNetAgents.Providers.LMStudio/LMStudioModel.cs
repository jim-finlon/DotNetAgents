using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Abstractions.Exceptions;
using DotNetAgents.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.LMStudio;

/// <summary>
/// LM Studio implementation of <see cref="ILLMModel{TInput, TOutput}"/> for local LLM chat completions.
/// LM Studio provides an OpenAI-compatible API for running local LLMs.
/// </summary>
public class LMStudioModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private readonly ILogger<LMStudioModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="LMStudioModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="baseUrl">The base URL of the LM Studio server (default: "http://localhost:1234").</param>
    /// <param name="modelName">The name of the model to use (optional, LM Studio uses the loaded model).</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient is null.</exception>
    public LMStudioModel(
        HttpClient httpClient,
        string baseUrl = "http://localhost:1234",
        string modelName = "local-model",
        ILogger<LMStudioModel>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _modelName = modelName;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Configure HTTP client for LM Studio API (OpenAI-compatible)
        var baseUri = baseUrl.TrimEnd('/');
        _httpClient.BaseAddress = new Uri($"{baseUri}/v1/");
    }

    /// <inheritdoc/>
    public string ModelName => _modelName;

    /// <inheritdoc/>
    public int MaxTokens => 4096; // LM Studio models typically support up to 4096 tokens

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
            TopP = options?.TopP,
            FrequencyPenalty = options?.FrequencyPenalty,
            PresencePenalty = options?.PresencePenalty
        };

        try
        {
            _logger?.LogDebug("Calling LM Studio API with model {ModelName}", _modelName);

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
                throw new AgentException("Failed to deserialize LM Studio response.", ErrorCategory.LLMError);
            }

            if (completionResponse.Choices == null || completionResponse.Choices.Length == 0)
            {
                throw new AgentException("LM Studio response contained no choices.", ErrorCategory.LLMError);
            }

            var content = completionResponse.Choices[0].Message?.Content;
            if (content == null)
            {
                throw new AgentException("LM Studio response contained no content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("LM Studio API call completed successfully");

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling LM Studio API");
            throw new AgentException($"LM Studio API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling LM Studio API");
            throw new AgentException("LM Studio API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for LM Studio response");
            throw new AgentException("Failed to parse LM Studio response.", ErrorCategory.LLMError, ex);
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
            TopP = options?.TopP,
            FrequencyPenalty = options?.FrequencyPenalty,
            PresencePenalty = options?.PresencePenalty
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
            _logger?.LogError(ex, "HTTP error calling LM Studio streaming API");
            throw new AgentException($"LM Studio streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling LM Studio streaming API");
            throw new AgentException("LM Studio streaming API call timed out.", ErrorCategory.LLMError, ex);
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

    // Request/Response models for LM Studio API (OpenAI-compatible)
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
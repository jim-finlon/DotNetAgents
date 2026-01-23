using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Core.Exceptions;
using DotNetAgents.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Cohere;

/// <summary>
/// Cohere implementation of <see cref="ILLMModel{TInput, TOutput}"/> for chat completions.
/// Cohere provides strong text generation and embedding capabilities.
/// </summary>
public class CohereModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly ILogger<CohereModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CohereModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="apiKey">The Cohere API key.</param>
    /// <param name="modelName">The name of the model to use (e.g., "command", "command-light", "command-nightly").</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient, apiKey, or modelName is null or empty.</exception>
    public CohereModel(
        HttpClient httpClient,
        string apiKey,
        string modelName = "command",
        ILogger<CohereModel>? logger = null)
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

        // Configure HTTP client for Cohere API
        _httpClient.BaseAddress = new Uri("https://api.cohere.ai/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
    }

    /// <inheritdoc/>
    public string ModelName => _modelName;

    /// <inheritdoc/>
    public int MaxTokens => 4096; // Cohere models typically support up to 4096 tokens

    /// <inheritdoc/>
    public async Task<string> GenerateAsync(
        string input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = new GenerateRequest
        {
            Model = _modelName,
            Prompt = input,
            Temperature = options?.Temperature,
            MaxTokens = options?.MaxTokens,
            TopP = options?.TopP
        };

        try
        {
            _logger?.LogDebug("Calling Cohere API with model {ModelName}", _modelName);

            var response = await _httpClient.PostAsJsonAsync(
                "generate",
                request,
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var completionResponse = await response.Content.ReadFromJsonAsync<GenerateResponse>(
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            if (completionResponse == null)
            {
                throw new AgentException("Failed to deserialize Cohere response.", ErrorCategory.LLMError);
            }

            if (completionResponse.Generations == null || completionResponse.Generations.Length == 0)
            {
                throw new AgentException("Cohere response contained no generations.", ErrorCategory.LLMError);
            }

            var content = completionResponse.Generations[0].Text;
            if (string.IsNullOrEmpty(content))
            {
                throw new AgentException("Cohere response contained no text content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("Cohere API call completed successfully");

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling Cohere API");
            throw new AgentException($"Cohere API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Cohere API");
            throw new AgentException("Cohere API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for Cohere response");
            throw new AgentException("Failed to parse Cohere response.", ErrorCategory.LLMError, ex);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string input,
        LLMOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = new GenerateRequest
        {
            Model = _modelName,
            Prompt = input,
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
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "generate")
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
            _logger?.LogError(ex, "HTTP error calling Cohere streaming API");
            throw new AgentException($"Cohere streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Cohere streaming API");
            throw new AgentException("Cohere streaming API call timed out.", ErrorCategory.LLMError, ex);
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

    private string? ParseStreamChunk(string jsonData)
    {
        try
        {
            var streamResponse = JsonSerializer.Deserialize<GenerateStreamResponse>(jsonData, _jsonOptions);
            if (streamResponse?.IsFinished == false && streamResponse.Text != null)
            {
                return streamResponse.Text;
            }
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Failed to parse streaming chunk: {JsonData}", jsonData);
        }
        return null;
    }

    // Request/Response models for Cohere API
    private record GenerateRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; init; } = string.Empty;

        [JsonPropertyName("temperature")]
        public double? Temperature { get; init; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; init; }

        [JsonPropertyName("p")]
        public double? TopP { get; init; }

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }
    }

    private record GenerateResponse
    {
        [JsonPropertyName("generations")]
        public Generation[]? Generations { get; init; }
    }

    private record Generation
    {
        [JsonPropertyName("text")]
        public string Text { get; init; } = string.Empty;
    }

    private record GenerateStreamResponse
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }

        [JsonPropertyName("is_finished")]
        public bool IsFinished { get; init; }
    }
}
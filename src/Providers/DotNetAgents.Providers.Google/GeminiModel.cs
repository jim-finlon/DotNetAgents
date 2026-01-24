using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Abstractions.Exceptions;
using DotNetAgents.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Google;

/// <summary>
/// Google Gemini implementation of <see cref="ILLMModel{TInput, TOutput}"/> for chat completions.
/// Supports both Google AI Studio API and Vertex AI.
/// </summary>
public class GeminiModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly bool _useVertexAI;
    private readonly ILogger<GeminiModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiModel"/> class for Google AI Studio API.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="apiKey">The Google AI API key.</param>
    /// <param name="modelName">The name of the model to use (e.g., "gemini-pro", "gemini-pro-vision").</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient, apiKey, or modelName is null or empty.</exception>
    public GeminiModel(
        HttpClient httpClient,
        string apiKey,
        string modelName = "gemini-pro",
        ILogger<GeminiModel>? logger = null)
        : this(httpClient, apiKey, modelName, false, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="apiKey">The API key or access token.</param>
    /// <param name="modelName">The name of the model to use.</param>
    /// <param name="useVertexAI">Whether to use Vertex AI endpoint (requires different authentication).</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    private GeminiModel(
        HttpClient httpClient,
        string apiKey,
        string modelName,
        bool useVertexAI,
        ILogger<GeminiModel>? logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = string.IsNullOrWhiteSpace(apiKey) ? throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey)) : apiKey;
        _modelName = string.IsNullOrWhiteSpace(modelName) ? throw new ArgumentException("Model name cannot be null or empty.", nameof(modelName)) : modelName;
        _useVertexAI = useVertexAI;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Configure HTTP client for Google AI Studio API
        if (!_useVertexAI)
        {
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
        }
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

        var request = new GenerateContentRequest
        {
            Contents = new[]
            {
                new Content
                {
                    Parts = new[]
                    {
                        new Part
                        {
                            Text = input
                        }
                    }
                }
            },
            GenerationConfig = new GenerationConfig
            {
                Temperature = options?.Temperature,
                MaxOutputTokens = options?.MaxTokens,
                TopP = options?.TopP
            }
        };

        try
        {
            _logger?.LogDebug("Calling Google Gemini API with model {ModelName}", _modelName);

            var url = $"models/{_modelName}:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsJsonAsync(
                url,
                request,
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var completionResponse = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(
                _jsonOptions,
                cancellationToken).ConfigureAwait(false);

            if (completionResponse == null)
            {
                throw new AgentException("Failed to deserialize Google Gemini response.", ErrorCategory.LLMError);
            }

            if (completionResponse.Candidates == null || completionResponse.Candidates.Length == 0)
            {
                throw new AgentException("Google Gemini response contained no candidates.", ErrorCategory.LLMError);
            }

            var content = completionResponse.Candidates[0].Content?.Parts?[0]?.Text;
            if (string.IsNullOrEmpty(content))
            {
                throw new AgentException("Google Gemini response contained no text content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("Google Gemini API call completed successfully");

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling Google Gemini API");
            throw new AgentException($"Google Gemini API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Google Gemini API");
            throw new AgentException("Google Gemini API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for Google Gemini response");
            throw new AgentException("Failed to parse Google Gemini response.", ErrorCategory.LLMError, ex);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string input,
        LLMOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var request = new GenerateContentRequest
        {
            Contents = new[]
            {
                new Content
                {
                    Parts = new[]
                    {
                        new Part
                        {
                            Text = input
                        }
                    }
                }
            },
            GenerationConfig = new GenerationConfig
            {
                Temperature = options?.Temperature,
                MaxOutputTokens = options?.MaxTokens,
                TopP = options?.TopP
            }
        };

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            var url = $"models/{_modelName}:streamGenerateContent?key={_apiKey}";
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
            _logger?.LogError(ex, "HTTP error calling Google Gemini streaming API");
            throw new AgentException($"Google Gemini streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Google Gemini streaming API");
            throw new AgentException("Google Gemini streaming API call timed out.", ErrorCategory.LLMError, ex);
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
            var streamResponse = JsonSerializer.Deserialize<GenerateContentResponse>(jsonData, _jsonOptions);
            if (streamResponse?.Candidates != null && streamResponse.Candidates.Length > 0)
            {
                return streamResponse.Candidates[0].Content?.Parts?[0]?.Text;
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
            var m when m.Contains("GEMINI-PRO", StringComparison.OrdinalIgnoreCase) => 8192,
            var m when m.Contains("GEMINI-ULTRA", StringComparison.OrdinalIgnoreCase) => 8192,
            _ => 4096 // Default
        };
    }

    // Request/Response models for Google Gemini API
    private record GenerateContentRequest
    {
        [JsonPropertyName("contents")]
        public Content[] Contents { get; init; } = Array.Empty<Content>();

        [JsonPropertyName("generationConfig")]
        public GenerationConfig? GenerationConfig { get; init; }
    }

    private record Content
    {
        [JsonPropertyName("parts")]
        public Part[]? Parts { get; init; }
    }

    private record Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }

    private record GenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double? Temperature { get; init; }

        [JsonPropertyName("maxOutputTokens")]
        public int? MaxOutputTokens { get; init; }

        [JsonPropertyName("topP")]
        public double? TopP { get; init; }
    }

    private record GenerateContentResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; init; }
    }

    private record Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; init; }
    }
}
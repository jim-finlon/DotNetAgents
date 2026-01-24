using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetAgents.Abstractions.Exceptions;
using DotNetAgents.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Ollama;

/// <summary>
/// Ollama implementation of <see cref="ILLMModel{TInput, TOutput}"/> for local LLM chat completions.
/// Ollama provides an OpenAI-compatible API for running local LLMs.
/// </summary>
public class OllamaModel : ILLMModel<string, string>
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private readonly ILogger<OllamaModel>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaModel"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    /// <param name="baseUrl">The base URL of the Ollama server (default: "http://localhost:11434").</param>
    /// <param name="modelName">The name of the model to use (e.g., "llama2", "mistral", "codellama").</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpClient or modelName is null or empty.</exception>
    public OllamaModel(
        HttpClient httpClient,
        string baseUrl = "http://localhost:11434",
        string modelName = "llama2",
        ILogger<OllamaModel>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _modelName = string.IsNullOrWhiteSpace(modelName) ? throw new ArgumentException("Model name cannot be null or empty.", nameof(modelName)) : modelName;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        // Configure HTTP client for Ollama API
        var baseUri = baseUrl.TrimEnd('/');
        _httpClient.BaseAddress = new Uri($"{baseUri}/api/");
    }

    /// <inheritdoc/>
    public string ModelName => _modelName;

    /// <inheritdoc/>
    public int MaxTokens => 4096; // Ollama models typically support up to 4096 tokens

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
            Stream = false,
            Options = new GenerateOptions
            {
                Temperature = options?.Temperature,
                TopP = options?.TopP,
                NumPredict = options?.MaxTokens
            }
        };

        try
        {
            _logger?.LogDebug("Calling Ollama API with model {ModelName}", _modelName);

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
                throw new AgentException("Failed to deserialize Ollama response.", ErrorCategory.LLMError);
            }

            if (string.IsNullOrEmpty(completionResponse.Response))
            {
                throw new AgentException("Ollama response contained no content.", ErrorCategory.LLMError);
            }

            _logger?.LogDebug("Ollama API call completed successfully");

            return completionResponse.Response;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP error calling Ollama API");
            throw new AgentException($"Ollama API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Ollama API");
            throw new AgentException("Ollama API call timed out.", ErrorCategory.LLMError, ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "JSON deserialization error for Ollama response");
            throw new AgentException("Failed to parse Ollama response.", ErrorCategory.LLMError, ex);
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
            Options = new GenerateOptions
            {
                Temperature = options?.Temperature,
                TopP = options?.TopP,
                NumPredict = options?.MaxTokens
            }
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
            _logger?.LogError(ex, "HTTP error calling Ollama streaming API");
            throw new AgentException($"Ollama streaming API call failed: {ex.Message}", ErrorCategory.LLMError, ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogError(ex, "Timeout calling Ollama streaming API");
            throw new AgentException("Ollama streaming API call timed out.", ErrorCategory.LLMError, ex);
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
            return streamResponse?.Response;
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Failed to parse streaming chunk: {JsonData}", jsonData);
        }
        return null;
    }

    // Request/Response models for Ollama API
    private record GenerateRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; init; } = string.Empty;

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }

        [JsonPropertyName("options")]
        public GenerateOptions? Options { get; init; }
    }

    private record GenerateOptions
    {
        [JsonPropertyName("temperature")]
        public double? Temperature { get; init; }

        [JsonPropertyName("top_p")]
        public double? TopP { get; init; }

        [JsonPropertyName("num_predict")]
        public int? NumPredict { get; init; }
    }

    private record GenerateResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; init; } = string.Empty;
    }

    private record GenerateStreamResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; init; }

        [JsonPropertyName("done")]
        public bool Done { get; init; }
    }
}
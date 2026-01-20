namespace DotNetAgents.Core.Models;

/// <summary>
/// Represents options for LLM generation requests.
/// </summary>
public record LLMOptions
{
    /// <summary>
    /// Gets or sets the temperature for generation (0.0 to 2.0).
    /// Higher values make output more random.
    /// </summary>
    public double? Temperature { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Gets or sets the top-p (nucleus) sampling parameter.
    /// </summary>
    public double? TopP { get; init; }

    /// <summary>
    /// Gets or sets the frequency penalty (-2.0 to 2.0).
    /// </summary>
    public double? FrequencyPenalty { get; init; }

    /// <summary>
    /// Gets or sets the presence penalty (-2.0 to 2.0).
    /// </summary>
    public double? PresencePenalty { get; init; }

    /// <summary>
    /// Gets or sets additional provider-specific options.
    /// </summary>
    public IDictionary<string, object>? AdditionalOptions { get; init; }
}

/// <summary>
/// Interface for Large Language Model providers.
/// </summary>
/// <typeparam name="TInput">The type of input expected by the model.</typeparam>
/// <typeparam name="TOutput">The type of output produced by the model.</typeparam>
public interface ILLMModel<TInput, TOutput>
{
    /// <summary>
    /// Generates a response using the LLM model.
    /// </summary>
    /// <param name="input">The input to process.</param>
    /// <param name="options">Optional configuration for the generation.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The generated output.</returns>
    Task<TOutput> GenerateAsync(
        TInput input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a streaming response using the LLM model.
    /// </summary>
    /// <param name="input">The input to process.</param>
    /// <param name="options">Optional configuration for the generation.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An async enumerable of output chunks.</returns>
    IAsyncEnumerable<TOutput> GenerateStreamAsync(
        TInput input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates responses for multiple inputs in batch.
    /// </summary>
    /// <param name="inputs">The inputs to process.</param>
    /// <param name="options">Optional configuration for the generation.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of generated outputs.</returns>
    Task<IReadOnlyList<TOutput>> GenerateBatchAsync(
        IEnumerable<TInput> inputs,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the name of the model.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Gets the maximum number of tokens supported by the model.
    /// </summary>
    int MaxTokens { get; }
}
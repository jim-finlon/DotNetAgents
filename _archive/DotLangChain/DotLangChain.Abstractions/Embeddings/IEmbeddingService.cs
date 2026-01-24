using DotLangChain.Abstractions.Documents;

namespace DotLangChain.Abstractions.Embeddings;

/// <summary>
/// Generates embeddings for text content.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Gets the provider name (e.g., "OpenAI", "Ollama").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the embedding dimension for this provider.
    /// </summary>
    int Dimensions { get; }

    /// <summary>
    /// Gets the maximum tokens per request.
    /// </summary>
    int MaxTokens { get; }

    /// <summary>
    /// Gets the maximum number of texts in a batch.
    /// </summary>
    int MaxBatchSize { get; }

    /// <summary>
    /// Generates an embedding for a single text.
    /// </summary>
    /// <param name="text">Text to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Embedding result.</returns>
    Task<EmbeddingResult> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for multiple texts.
    /// </summary>
    /// <param name="texts">Texts to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Batch embedding result.</returns>
    Task<BatchEmbeddingResult> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for document chunks (populates Embedding property).
    /// </summary>
    /// <param name="chunks">Chunks to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Chunks with embeddings populated.</returns>
    Task<IReadOnlyList<DocumentChunk>> EmbedChunksAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default);
}


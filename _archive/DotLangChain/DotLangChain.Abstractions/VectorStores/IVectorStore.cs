using DotLangChain.Abstractions.Embeddings;

namespace DotLangChain.Abstractions.VectorStores;

/// <summary>
/// Abstract vector store operations.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Gets the collection/namespace name.
    /// </summary>
    string CollectionName { get; }

    /// <summary>
    /// Creates the collection if it doesn't exist.
    /// </summary>
    /// <param name="dimensions">Vector dimensions.</param>
    /// <param name="metric">Distance metric. Default: Cosine.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task EnsureCollectionAsync(
        int dimensions,
        DistanceMetric metric = DistanceMetric.Cosine,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts vectors into the store.
    /// </summary>
    /// <param name="records">Records to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpsertAsync(
        IReadOnlyList<VectorRecord> records,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar vectors.
    /// </summary>
    /// <param name="queryVector">Query embedding vector.</param>
    /// <param name="options">Search options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results with similarity scores.</returns>
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryVector,
        VectorSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar vectors by text (generates embedding automatically).
    /// </summary>
    /// <param name="query">Query text.</param>
    /// <param name="embeddingService">Embedding service to generate query vector.</param>
    /// <param name="options">Search options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results with similarity scores.</returns>
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string query,
        IEmbeddingService embeddingService,
        VectorSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes vectors by ID.
    /// </summary>
    /// <param name="ids">IDs to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vectors by ID.
    /// </summary>
    /// <param name="ids">IDs to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Vector records.</returns>
    Task<IReadOnlyList<VectorRecord>> GetAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of vectors in the collection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of vectors.</returns>
    Task<long> CountAsync(
        CancellationToken cancellationToken = default);
}


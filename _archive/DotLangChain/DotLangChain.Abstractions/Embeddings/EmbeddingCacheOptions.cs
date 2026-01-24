namespace DotLangChain.Abstractions.Embeddings;

/// <summary>
/// Caching decorator options for embedding services.
/// </summary>
public sealed record EmbeddingCacheOptions
{
    /// <summary>
    /// Gets or sets the absolute expiration time.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; init; }

    /// <summary>
    /// Gets or sets the sliding expiration time.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; init; }

    /// <summary>
    /// Gets or sets whether caching is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}


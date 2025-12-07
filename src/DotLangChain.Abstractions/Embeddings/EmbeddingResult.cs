namespace DotLangChain.Abstractions.Embeddings;

/// <summary>
/// Result from a single embedding operation.
/// </summary>
/// <param name="Embedding">The embedding vector.</param>
/// <param name="TokenCount">Tokens consumed.</param>
/// <param name="Model">Model used.</param>
/// <param name="Duration">Operation duration.</param>
public sealed record EmbeddingResult
{
    public required float[] Embedding { get; init; }
    public int TokenCount { get; init; }
    public string? Model { get; init; }
    public TimeSpan Duration { get; init; }
}


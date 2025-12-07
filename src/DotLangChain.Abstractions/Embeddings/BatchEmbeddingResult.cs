namespace DotLangChain.Abstractions.Embeddings;

/// <summary>
/// Result from batch embedding operation.
/// </summary>
public sealed record BatchEmbeddingResult
{
    public required IReadOnlyList<EmbeddingResult> Embeddings { get; init; }
    public int TotalTokens { get; init; }
    public string? Model { get; init; }
    public TimeSpan Duration { get; init; }
}


namespace DotLangChain.Abstractions.VectorStores;

/// <summary>
/// Search result with similarity score.
/// </summary>
public sealed record VectorSearchResult
{
    public required VectorRecord Record { get; init; }
    public required float Score { get; init; }
}


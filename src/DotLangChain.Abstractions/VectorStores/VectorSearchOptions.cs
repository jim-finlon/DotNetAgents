namespace DotLangChain.Abstractions.VectorStores;

/// <summary>
/// Search options for vector queries.
/// </summary>
public sealed record VectorSearchOptions
{
    /// <summary>
    /// Number of results to return. Default: 10.
    /// </summary>
    public int TopK { get; init; } = 10;

    /// <summary>
    /// Minimum similarity score threshold.
    /// </summary>
    public float? MinScore { get; init; }

    /// <summary>
    /// Metadata filter conditions.
    /// </summary>
    public Dictionary<string, object?>? Filter { get; init; }

    /// <summary>
    /// Whether to return vectors in results. Default: false.
    /// </summary>
    public bool IncludeVectors { get; init; } = false;

    /// <summary>
    /// Whether to return metadata in results. Default: true.
    /// </summary>
    public bool IncludeMetadata { get; init; } = true;
}


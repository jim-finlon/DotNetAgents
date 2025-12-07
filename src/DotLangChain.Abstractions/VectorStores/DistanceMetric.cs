namespace DotLangChain.Abstractions.VectorStores;

/// <summary>
/// Distance metric for similarity calculations.
/// </summary>
public enum DistanceMetric
{
    /// <summary>
    /// Cosine similarity (default, normalized).
    /// </summary>
    Cosine,

    /// <summary>
    /// Euclidean distance (L2).
    /// </summary>
    Euclidean,

    /// <summary>
    /// Dot product (for non-normalized vectors).
    /// </summary>
    DotProduct
}


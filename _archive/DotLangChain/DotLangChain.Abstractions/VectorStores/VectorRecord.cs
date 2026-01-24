namespace DotLangChain.Abstractions.VectorStores;

/// <summary>
/// A vector with associated metadata.
/// </summary>
public sealed record VectorRecord
{
    public required string Id { get; init; }
    public required float[] Vector { get; init; }
    public string? Content { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
}


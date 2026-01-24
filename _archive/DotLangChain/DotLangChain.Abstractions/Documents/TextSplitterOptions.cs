namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Configuration for text splitting operations.
/// </summary>
public sealed record TextSplitterOptions
{
    /// <summary>
    /// Maximum size per chunk. Default: 1000.
    /// </summary>
    public int ChunkSize { get; init; } = 1000;

    /// <summary>
    /// Overlap between adjacent chunks. Default: 200.
    /// </summary>
    public int ChunkOverlap { get; init; } = 200;

    /// <summary>
    /// Whether to include separator in chunk. Default: true.
    /// </summary>
    public bool KeepSeparator { get; init; } = true;

    /// <summary>
    /// Whether to trim whitespace from chunks. Default: true.
    /// </summary>
    public bool StripWhitespace { get; init; } = true;

    /// <summary>
    /// How to measure length. Default: Characters.
    /// </summary>
    public LengthFunction LengthFunction { get; init; } = LengthFunction.Characters;
}

/// <summary>
/// Function used to measure text length.
/// </summary>
public enum LengthFunction
{
    /// <summary>
    /// Count characters.
    /// </summary>
    Characters,

    /// <summary>
    /// Count tokens (requires tokenizer).
    /// </summary>
    Tokens,

    /// <summary>
    /// Count words.
    /// </summary>
    Words
}


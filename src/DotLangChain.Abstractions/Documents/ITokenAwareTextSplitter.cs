namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Marker interface for token-aware splitters.
/// </summary>
public interface ITokenAwareTextSplitter : ITextSplitter
{
    /// <summary>
    /// Counts tokens in the given text.
    /// </summary>
    /// <param name="text">Text to count tokens in.</param>
    /// <returns>Number of tokens.</returns>
    int CountTokens(string text);

    /// <summary>
    /// Gets the maximum tokens supported.
    /// </summary>
    int MaxTokens { get; }

    /// <summary>
    /// Gets the tokenizer model name (e.g., "cl100k_base").
    /// </summary>
    string TokenizerModel { get; }
}


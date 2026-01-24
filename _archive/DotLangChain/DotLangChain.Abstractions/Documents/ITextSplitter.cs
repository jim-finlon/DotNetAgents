namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Splits documents into smaller chunks.
/// </summary>
public interface ITextSplitter
{
    /// <summary>
    /// Splits a document into chunks with full lineage tracking.
    /// </summary>
    /// <param name="document">Document to split.</param>
    /// <param name="options">Splitting options (null = defaults).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of document chunks.</returns>
    IAsyncEnumerable<DocumentChunk> SplitAsync(
        Document document,
        TextSplitterOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Splits raw text into string chunks.
    /// </summary>
    /// <param name="text">Text to split.</param>
    /// <param name="options">Splitting options (null = defaults).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of text chunks.</returns>
    IAsyncEnumerable<string> SplitTextAsync(
        string text,
        TextSplitterOptions? options = null,
        CancellationToken cancellationToken = default);
}


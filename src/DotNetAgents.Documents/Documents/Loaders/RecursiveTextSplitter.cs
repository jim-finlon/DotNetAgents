using DotNetAgents.Abstractions.Documents;

using DotNetAgents.Abstractions.Documents;

namespace DotNetAgents.Documents.Loaders;

/// <summary>
/// Recursive text splitter that attempts to split text by multiple separators in order of preference.
/// This is useful for splitting structured text like code, markdown, or documents with multiple formatting levels.
/// </summary>
public class RecursiveTextSplitter : ITextSplitter
{
    private readonly int _chunkSize;
    private readonly int _chunkOverlap;
    private readonly string[] _separators;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveTextSplitter"/> class.
    /// </summary>
    /// <param name="chunkSize">The target size of each chunk in characters.</param>
    /// <param name="chunkOverlap">The number of characters to overlap between chunks.</param>
    /// <param name="separators">Optional list of separators to try in order. Defaults to common text separators.</param>
    public RecursiveTextSplitter(
        int chunkSize = 1000,
        int chunkOverlap = 200,
        string[]? separators = null)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be positive.", nameof(chunkSize));
        if (chunkOverlap < 0)
            throw new ArgumentException("Chunk overlap must be non-negative.", nameof(chunkOverlap));
        if (chunkOverlap >= chunkSize)
            throw new ArgumentException("Chunk overlap must be less than chunk size.", nameof(chunkOverlap));

        _chunkSize = chunkSize;
        _chunkOverlap = chunkOverlap;
        _separators = separators ?? new[]
        {
            "\n\n",      // Paragraphs
            "\n",        // Lines
            ". ",        // Sentences
            " ",         // Words
            ""           // Characters (fallback)
        };
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Document>> SplitDocumentsAsync(
        IEnumerable<Document> documents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var result = new List<Document>();
        foreach (var document in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chunks = SplitText(document.Content, document.Metadata);
            result.AddRange(chunks);
        }

        return Task.FromResult<IReadOnlyList<Document>>(result);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> SplitTextAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);

        var chunks = SplitText(text, new Dictionary<string, object>());
        var texts = chunks.Select(c => c.Content).ToList();
        return Task.FromResult<IReadOnlyList<string>>(texts);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Document>> SplitAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var chunks = SplitText(document.Content, document.Metadata);
        return Task.FromResult<IReadOnlyList<Document>>(chunks);
    }

    private List<Document> SplitText(string text, IDictionary<string, object> baseMetadata)
    {
        if (string.IsNullOrEmpty(text))
            return new List<Document>();

        // If text is smaller than chunk size, return as-is
        if (text.Length <= _chunkSize)
        {
            return new List<Document>
            {
                new Document
                {
                    Content = text,
                    Metadata = new Dictionary<string, object>(baseMetadata)
                }
            };
        }

        var chunks = new List<Document>();
        var remainingText = text;
        var chunkIndex = 0;

        while (!string.IsNullOrEmpty(remainingText))
        {
            // Try to find a good split point using separators in order
            var splitIndex = FindSplitIndex(remainingText, _chunkSize);

            if (splitIndex == -1)
            {
                // No good split point found, take the whole remaining text
                splitIndex = Math.Min(remainingText.Length, _chunkSize);
            }

            var chunk = remainingText.Substring(0, splitIndex).Trim();
            if (!string.IsNullOrEmpty(chunk))
            {
                var metadata = new Dictionary<string, object>(baseMetadata)
                {
                    ["chunk_index"] = chunkIndex,
                    ["chunk_size"] = chunk.Length
                };
                chunks.Add(new Document
                {
                    Content = chunk,
                    Metadata = metadata
                });
                chunkIndex++;
            }

            // Move forward, accounting for overlap
            var nextStart = Math.Max(0, splitIndex - _chunkOverlap);
            remainingText = remainingText.Substring(nextStart);
        }

        return chunks;
    }

    private int FindSplitIndex(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text.Length;

        // Try each separator in order
        foreach (var separator in _separators)
        {
            if (string.IsNullOrEmpty(separator))
            {
                // Character-level split (fallback)
                return maxLength;
            }

            // Find the last occurrence of the separator within the max length
            var searchText = text.Substring(0, Math.Min(maxLength, text.Length));
            var lastIndex = searchText.LastIndexOf(separator, StringComparison.Ordinal);

            if (lastIndex > 0)
            {
                // Found a good split point
                return lastIndex + separator.Length;
            }
        }

        // No good split point found
        return -1;
    }
}
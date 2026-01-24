using DotLangChain.Abstractions.Documents;
using DotLangChain.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DotLangChain.Core.Documents.Splitters;

/// <summary>
/// Simple character-based text splitter (fallback implementation).
/// </summary>
public sealed class CharacterTextSplitter : ITextSplitter
{
    private readonly ILogger<CharacterTextSplitter>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterTextSplitter"/> class.
    /// </summary>
    public CharacterTextSplitter(ILogger<CharacterTextSplitter>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DocumentChunk> SplitAsync(
        Document document,
        TextSplitterOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        options ??= new TextSplitterOptions();
        var chunkIndex = 0;
        var charOffset = 0;

        await foreach (var chunkText in SplitTextAsync(document.Content, options, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = new DocumentChunk
            {
                Id = $"{document.Id}_chunk_{chunkIndex}",
                Content = chunkText,
                ParentDocumentId = document.Id,
                ChunkIndex = chunkIndex,
                StartCharOffset = charOffset,
                EndCharOffset = charOffset + chunkText.Length,
                Metadata = new DocumentMetadata(document.Metadata)
                {
                    ChunkIndex = chunkIndex,
                    ParentDocumentId = document.Id
                }
            };

            yield return chunk;

            charOffset += chunkText.Length - (options.ChunkOverlap > 0 ? options.ChunkOverlap : 0);
            chunkIndex++;
        }

        _logger?.LogDebug("Split document {DocumentId} into {ChunkCount} chunks", document.Id, chunkIndex);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> SplitTextAsync(
        string text,
        TextSplitterOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);

        options ??= new TextSplitterOptions();

        if (text.Length <= options.ChunkSize)
        {
            yield return options.StripWhitespace ? text.Trim() : text;
            await Task.CompletedTask;
            yield break;
        }

        var overlap = options.ChunkOverlap;
        var start = 0;

        while (start < text.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var end = Math.Min(start + options.ChunkSize, text.Length);
            var chunk = text.Substring(start, end - start);

            if (options.StripWhitespace)
            {
                chunk = chunk.Trim();
            }

            if (!string.IsNullOrEmpty(chunk))
            {
                yield return chunk;
            }

            // Move start position forward, accounting for overlap
            start = end - overlap;
            if (start >= end)
            {
                start = end;
            }
        }

        await Task.CompletedTask;
    }
}


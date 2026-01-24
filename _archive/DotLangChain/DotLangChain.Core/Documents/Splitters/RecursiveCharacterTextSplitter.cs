using DotLangChain.Abstractions.Documents;
using DotLangChain.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DotLangChain.Core.Documents.Splitters;

/// <summary>
/// Recursively splits text by trying to keep chunks as large as possible while respecting chunk size limits.
/// Uses a hierarchical list of separators to split on.
/// </summary>
public sealed class RecursiveCharacterTextSplitter : ITextSplitter
{
    private static readonly string[] DefaultSeparators = ["\n\n", "\n", ". ", " ", ""];

    private readonly string[] _separators;
    private readonly ILogger<RecursiveCharacterTextSplitter>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveCharacterTextSplitter"/> class.
    /// </summary>
    /// <param name="logger">Optional logger.</param>
    /// <param name="separators">Custom separators. Default: paragraph, newline, sentence, word, character.</param>
    public RecursiveCharacterTextSplitter(
        ILogger<RecursiveCharacterTextSplitter>? logger = null,
        string[]? separators = null)
    {
        _logger = logger;
        _separators = separators ?? DefaultSeparators;
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

        foreach (var chunk in SplitTextRecursive(text, _separators, options))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = options.StripWhitespace ? chunk.Trim() : chunk;
            if (!string.IsNullOrEmpty(result))
            {
                yield return result;
            }
        }

        await Task.CompletedTask;
    }

    private IEnumerable<string> SplitTextRecursive(
        string text,
        ReadOnlySpan<string> separators,
        TextSplitterOptions options)
    {
        if (text.Length <= options.ChunkSize)
        {
            yield return text;
            yield break;
        }

        var separator = separators.IsEmpty ? "" : separators[0];
        var nextSeparators = separators.Length > 1 ? separators[1..] : ReadOnlySpan<string>.Empty;

        var splits = separator == ""
            ? text.Select(c => c.ToString()).ToList()
            : text.Split(separator, StringSplitOptions.None).ToList();

        var currentChunk = new StringBuilder();

        foreach (var split in splits)
        {
            var piece = options.KeepSeparator && separator != ""
                ? separator + split
                : split;

            if (currentChunk.Length + piece.Length > options.ChunkSize)
            {
                if (currentChunk.Length > 0)
                {
                    var chunkText = currentChunk.ToString();
                    if (chunkText.Length > options.ChunkSize && !nextSeparators.IsEmpty)
                    {
                        foreach (var subChunk in SplitTextRecursive(chunkText, nextSeparators, options))
                        {
                            yield return subChunk;
                        }
                    }
                    else
                    {
                        yield return chunkText;
                    }

                    currentChunk.Clear();

                    // Add overlap
                    if (options.ChunkOverlap > 0)
                    {
                        var overlap = chunkText.Length > options.ChunkOverlap
                            ? chunkText[^options.ChunkOverlap..]
                            : chunkText;
                        currentChunk.Append(overlap);
                    }
                }
            }

            currentChunk.Append(piece);
        }

        if (currentChunk.Length > 0)
        {
            yield return currentChunk.ToString();
        }
    }
}


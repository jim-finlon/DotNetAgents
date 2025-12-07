using DotLangChain.Abstractions.Documents;
using DotLangChain.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DotLangChain.Core.Documents.Loaders;

/// <summary>
/// Loads plain text documents from various text-based formats.
/// </summary>
public sealed class TextDocumentLoader : IDocumentLoader
{
    private static readonly HashSet<string> SupportedExtensionsSet = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".text", ".csv", ".json", ".xml", ".log"
    };

    private readonly ILogger<TextDocumentLoader>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextDocumentLoader"/> class.
    /// </summary>
    public TextDocumentLoader(ILogger<TextDocumentLoader>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlySet<string> SupportedExtensions => SupportedExtensionsSet;

    /// <inheritdoc/>
    public async Task<Document> LoadAsync(
        Stream stream,
        string fileName,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        try
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            var content = await reader.ReadToEndAsync(cancellationToken);

            var document = new Document
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                Metadata = metadata ?? new DocumentMetadata(),
                SourceUri = fileName
            };

            document.Metadata.Source ??= fileName;
            document.Metadata.ContentType ??= GetContentType(fileName);

            _logger?.LogDebug("Loaded text document: {FileName}, {Length} characters", fileName, content.Length);

            return document;
        }
        catch (Exception ex) when (ex is not DocumentException)
        {
            throw DocumentException.LoadFailed(fileName, ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Document> LoadAsync(
        string filePath,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw DocumentException.LoadFailed(filePath, new FileNotFoundException("File not found", filePath));
        }

        await using var stream = File.OpenRead(filePath);
        return await LoadAsync(stream, filePath, metadata, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Document> LoadAsync(
        Uri uri,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);

        if (uri.Scheme == "file")
        {
            var localPath = uri.LocalPath;
            return await LoadAsync(localPath, metadata, cancellationToken);
        }

        if (uri.Scheme == "http" || uri.Scheme == "https")
        {
            // TODO: Implement HTTP loading with validation
            throw new NotImplementedException("HTTP/HTTPS loading not yet implemented");
        }

        throw DocumentException.LoadFailed(uri.ToString(), new NotSupportedException($"URI scheme '{uri.Scheme}' not supported"));
    }

    private static string? GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".log" => "text/plain",
            _ => "text/plain"
        };
    }
}


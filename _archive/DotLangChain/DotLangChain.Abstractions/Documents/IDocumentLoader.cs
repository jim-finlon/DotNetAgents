namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Loads documents from various sources.
/// </summary>
public interface IDocumentLoader
{
    /// <summary>
    /// Gets the supported file extensions (e.g., ".pdf", ".docx").
    /// </summary>
    IReadOnlySet<string> SupportedExtensions { get; }

    /// <summary>
    /// Loads a document from a stream.
    /// </summary>
    /// <param name="stream">Input stream containing document data.</param>
    /// <param name="fileName">Original filename (used for extension detection).</param>
    /// <param name="metadata">Optional metadata to attach.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded document.</returns>
    Task<Document> LoadAsync(
        Stream stream,
        string fileName,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a document from a file path.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="metadata">Optional metadata to attach.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded document.</returns>
    Task<Document> LoadAsync(
        string filePath,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a document from a URI.
    /// </summary>
    /// <param name="uri">URI to the document.</param>
    /// <param name="metadata">Optional metadata to attach.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded document.</returns>
    Task<Document> LoadAsync(
        Uri uri,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);
}


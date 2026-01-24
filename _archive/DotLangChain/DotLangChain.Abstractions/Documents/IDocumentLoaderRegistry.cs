namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Registry for managing document loaders by file extension.
/// </summary>
public interface IDocumentLoaderRegistry
{
    /// <summary>
    /// Gets loader for the specified extension (include dot, e.g., ".pdf").
    /// </summary>
    /// <param name="extension">File extension with dot prefix.</param>
    /// <returns>The document loader, or null if not found.</returns>
    IDocumentLoader? GetLoader(string extension);

    /// <summary>
    /// Registers a loader for its supported extensions.
    /// </summary>
    /// <param name="loader">The document loader to register.</param>
    void Register(IDocumentLoader loader);

    /// <summary>
    /// Gets all registered file extensions.
    /// </summary>
    IReadOnlySet<string> SupportedExtensions { get; }
}


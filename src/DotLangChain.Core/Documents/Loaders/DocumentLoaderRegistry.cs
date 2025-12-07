using DotLangChain.Abstractions.Documents;
using DotLangChain.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DotLangChain.Core.Documents.Loaders;

/// <summary>
/// Registry for managing document loaders by file extension.
/// </summary>
public sealed class DocumentLoaderRegistry : IDocumentLoaderRegistry
{
    private readonly Dictionary<string, IDocumentLoader> _loaders = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<DocumentLoaderRegistry>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentLoaderRegistry"/> class.
    /// </summary>
    public DocumentLoaderRegistry(ILogger<DocumentLoaderRegistry>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public IReadOnlySet<string> SupportedExtensions
    {
        get
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var loader in _loaders.Values)
            {
                foreach (var ext in loader.SupportedExtensions)
                {
                    extensions.Add(ext);
                }
            }
            return extensions;
        }
    }

    /// <inheritdoc/>
    public IDocumentLoader? GetLoader(string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);

        // Ensure extension starts with dot
        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        return _loaders.TryGetValue(extension, out var loader) ? loader : null;
    }

    /// <inheritdoc/>
    public void Register(IDocumentLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        foreach (var extension in loader.SupportedExtensions)
        {
            if (_loaders.ContainsKey(extension))
            {
                _logger?.LogWarning("Loader for extension '{Extension}' already registered, overwriting", extension);
            }

            _loaders[extension] = loader;
            _logger?.LogDebug("Registered document loader for extension: {Extension}", extension);
        }
    }
}


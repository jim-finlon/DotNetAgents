using DotLangChain.Abstractions.LLM;

namespace DotLangChain.Abstractions.Memory;

/// <summary>
/// Entity extraction and storage interface.
/// </summary>
public interface IEntityMemory
{
    /// <summary>
    /// Extracts entities from a message and stores them.
    /// </summary>
    /// <param name="message">Message to extract entities from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExtractAndStoreAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stored entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of entity names to values.</returns>
    Task<IReadOnlyDictionary<string, string>> GetEntitiesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific entity by name.
    /// </summary>
    /// <param name="entityName">Name of the entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entity value, or null if not found.</returns>
    Task<string?> GetEntityAsync(
        string entityName,
        CancellationToken cancellationToken = default);
}


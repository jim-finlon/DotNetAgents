namespace DotLangChain.Abstractions.VectorStores;

/// <summary>
/// Factory for creating vector store instances.
/// </summary>
public interface IVectorStoreFactory
{
    /// <summary>
    /// Creates a vector store instance for the specified collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection/namespace.</param>
    /// <returns>Vector store instance.</returns>
    IVectorStore Create(string collectionName);
}


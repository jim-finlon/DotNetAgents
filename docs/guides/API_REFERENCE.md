# DotNetAgents API Reference

**Version:** 2.0.0  
**Date:** January 2025  
**Last Updated:** January 2025

---

## Table of Contents

1. [Document Ingestion](#1-document-ingestion)
2. [Text Splitting](#2-text-splitting)
3. [Embedding Services](#3-embedding-services)
4. [Vector Stores](#4-vector-stores)
5. [LLM Services](#5-llm-services)
6. [Agent Orchestration](#6-agent-orchestration)
7. [Tool System](#7-tool-system)
8. [Memory](#8-memory)
9. [Chains & Pipelines](#9-chains--pipelines)
10. [Security](#10-security)
11. [Configuration & DI](#11-configuration--dependency-injection)
12. [Observability](#12-observability)
13. [State Machines](#13-state-machines)
14. [Behavior Trees](#14-behavior-trees)

---

## 1. Document Ingestion

### 1.1 Document

Represents a loaded document with content and metadata.

**Namespace:** `DotLangChain.Abstractions.Documents`

```csharp
public sealed record Document
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public DocumentMetadata Metadata { get; init; }
    public string? SourceUri { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
```

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` | Yes | Unique identifier for the document |
| `Content` | `string` | Yes | Extracted text content |
| `Metadata` | `DocumentMetadata` | No | Key-value metadata dictionary (default: empty) |
| `SourceUri` | `string?` | No | Original source location (file path, URL) |
| `CreatedAt` | `DateTimeOffset` | No | Timestamp when document was loaded (default: UtcNow) |

**Example:**

```csharp
var document = new Document
{
    Id = Guid.NewGuid().ToString(),
    Content = "This is the document content...",
    Metadata = new DocumentMetadata { Title = "My Document" },
    SourceUri = "/path/to/file.pdf"
};
```

---

### 1.2 DocumentMetadata

Strongly-typed metadata container extending `Dictionary<string, object?>`.

**Namespace:** `DotLangChain.Abstractions.Documents`

```csharp
public sealed class DocumentMetadata : Dictionary<string, object?>
```

**Built-in Properties:**

| Property | Type | Key | Description |
|----------|------|-----|-------------|
| `Title` | `string?` | `"title"` | Document title |
| `Source` | `string?` | `"source"` | Source identifier |
| `Author` | `string?` | `"author"` | Document author |
| `PageNumber` | `int?` | `"page_number"` | Page number (for chunked docs) |
| `ChunkIndex` | `int?` | `"chunk_index"` | Chunk index in parent document |
| `ParentDocumentId` | `string?` | `"parent_document_id"` | Reference to parent document |
| `ContentType` | `string?` | `"content_type"` | MIME type |
| `FileSize` | `long?` | `"file_size"` | File size in bytes |
| `CreatedDate` | `DateTimeOffset?` | `"created_date"` | Original creation date |
| `ModifiedDate` | `DateTimeOffset?` | `"modified_date"` | Last modified date |

**Example:**

```csharp
var metadata = new DocumentMetadata
{
    Title = "Q4 Financial Report",
    Author = "Finance Team",
    Source = "/documents/reports/q4-2025.pdf",
    ["department"] = "Finance",           // Custom property
    ["fiscal_year"] = 2025,               // Custom property
    ["confidential"] = true               // Custom property
};

// Access properties
Console.WriteLine(metadata.Title);        // "Q4 Financial Report"
Console.WriteLine(metadata["department"]); // "Finance"
```

---

### 1.3 IDocumentLoader

Interface for loading documents from various sources.

**Namespace:** `DotLangChain.Abstractions.Documents`

```csharp
public interface IDocumentLoader
{
    IReadOnlySet<string> SupportedExtensions { get; }
    
    Task<Document> LoadAsync(
        Stream stream,
        string fileName,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);
    
    Task<Document> LoadAsync(
        string filePath,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);
    
    Task<Document> LoadAsync(
        Uri uri,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);
}
```

**Methods:**

#### LoadAsync (Stream)

Loads a document from a stream.

| Parameter | Type | Description |
|-----------|------|-------------|
| `stream` | `Stream` | Input stream containing document data |
| `fileName` | `string` | Original filename (used for extension detection) |
| `metadata` | `DocumentMetadata?` | Optional metadata to attach |
| `cancellationToken` | `CancellationToken` | Cancellation token |

**Returns:** `Task<Document>`

#### LoadAsync (File Path)

Loads a document from a file path.

| Parameter | Type | Description |
|-----------|------|-------------|
| `filePath` | `string` | Path to the file |
| `metadata` | `DocumentMetadata?` | Optional metadata to attach |
| `cancellationToken` | `CancellationToken` | Cancellation token |

**Returns:** `Task<Document>`

#### LoadAsync (URI)

Loads a document from a URI (file:// or http://).

| Parameter | Type | Description |
|-----------|------|-------------|
| `uri` | `Uri` | URI to the document |
| `metadata` | `DocumentMetadata?` | Optional metadata to attach |
| `cancellationToken` | `CancellationToken` | Cancellation token |

**Returns:** `Task<Document>`

**Implementations:**

| Class | Package | Extensions | Description |
|-------|---------|------------|-------------|
| `PdfDocumentLoader` | `DotLangChain.Core` | `.pdf` | Extracts text from PDF using PdfPig |
| `DocxDocumentLoader` | `DotLangChain.Core` | `.docx` | Parses Word documents via OpenXml |
| `XlsxDocumentLoader` | `DotLangChain.Core` | `.xlsx`, `.xls` | Extracts spreadsheet data |
| `MarkdownDocumentLoader` | `DotLangChain.Core` | `.md`, `.markdown` | Parses markdown with frontmatter |
| `HtmlDocumentLoader` | `DotLangChain.Core` | `.html`, `.htm` | Extracts text from HTML |
| `TextDocumentLoader` | `DotLangChain.Core` | `.txt`, `.csv`, `.json`, `.xml` | Plain text loading |
| `EmailDocumentLoader` | `DotLangChain.Core` | `.eml`, `.msg` | Parses email messages |

**Example:**

```csharp
public class DocumentService
{
    private readonly IDocumentLoaderRegistry _registry;

    public DocumentService(IDocumentLoaderRegistry registry)
    {
        _registry = registry;
    }

    public async Task<Document> LoadDocumentAsync(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        var loader = _registry.GetLoader(extension) 
            ?? throw new NotSupportedException($"No loader for {extension}");

        return await loader.LoadAsync(filePath, new DocumentMetadata
        {
            Source = filePath
        });
    }
}
```

---

### 1.4 IDocumentLoaderRegistry

Registry for managing document loaders by file extension.

**Namespace:** `DotLangChain.Abstractions.Documents`

```csharp
public interface IDocumentLoaderRegistry
{
    IDocumentLoader? GetLoader(string extension);
    void Register(IDocumentLoader loader);
    IReadOnlySet<string> SupportedExtensions { get; }
}
```

**Methods:**

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `GetLoader` | `string extension` | `IDocumentLoader?` | Gets loader for extension (include dot) |
| `Register` | `IDocumentLoader loader` | `void` | Registers loader for its supported extensions |

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `SupportedExtensions` | `IReadOnlySet<string>` | All registered file extensions |

**Example:**

```csharp
// Registration during startup
services.AddDotLangChain(builder =>
{
    builder.AddDocumentLoaders(docs =>
    {
        docs.AddPdf();
        docs.AddDocx();
        docs.AddMarkdown();
        docs.AddCustomLoader<MyCustomLoader>(); // Custom loader
    });
});

// Usage
var registry = serviceProvider.GetRequiredService<IDocumentLoaderRegistry>();

// Check supported extensions
Console.WriteLine(string.Join(", ", registry.SupportedExtensions));
// Output: .pdf, .docx, .md, .markdown

// Get specific loader
var pdfLoader = registry.GetLoader(".pdf");
```

---

## 2. Text Splitting

### 2.1 DocumentChunk

Represents a text chunk with lineage tracking.

**Namespace:** `DotLangChain.Abstractions.Documents`

```csharp
public sealed record DocumentChunk
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public required string ParentDocumentId { get; init; }
    public required int ChunkIndex { get; init; }
    public int StartCharOffset { get; init; }
    public int EndCharOffset { get; init; }
    public float[]? Embedding { get; init; }
    public DocumentMetadata Metadata { get; init; }
}
```

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` | Yes | Unique chunk ID (typically `{docId}_chunk_{index}`) |
| `Content` | `string` | Yes | The chunk text content |
| `ParentDocumentId` | `string` | Yes | ID of source document |
| `ChunkIndex` | `int` | Yes | Zero-based position in document |
| `StartCharOffset` | `int` | No | Starting character position in original |
| `EndCharOffset` | `int` | No | Ending character position in original |
| `Embedding` | `float[]?` | No | Pre-computed embedding vector |
| `Metadata` | `DocumentMetadata` | No | Inherited and chunk-specific metadata |

---

### 2.2 TextSplitterOptions

Configuration for text splitting operations.

**Namespace:** `DotLangChain.Abstractions.Documents`

```csharp
public sealed record TextSplitterOptions
{
    public int ChunkSize { get; init; } = 1000;
    public int ChunkOverlap { get; init; } = 200;
    public bool KeepSeparator { get; init; } = true;
    public bool StripWhitespace { get; init; } = true;
    public LengthFunction LengthFunction { get; init; } = LengthFunction.Characters;
}

public enum LengthFunction
{
    Characters,
    Tokens,
    Words
}
```

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ChunkSize` | `int` | 1000 | Maximum size per chunk |
| `ChunkOverlap` | `int` | 200 | Overlap between adjacent chunks |
| `KeepSeparator` | `bool` | true | Include separator in chunk |
| `StripWhitespace` | `bool` | true | Trim whitespace from chunks |
| `LengthFunction` | `LengthFunction` | Characters | How to measure length |

**Example:**

```csharp
var options = new TextSplitterOptions
{
    ChunkSize = 512,
    ChunkOverlap = 50,
    LengthFunction = LengthFunction.Tokens
};
```

---

### 2.3 ITextSplitter

Interface for splitting documents into chunks.

**Namespace:** `DotLangChain.Abstractions.Documents`

```csharp
public interface ITextSplitter
{
    IAsyncEnumerable<DocumentChunk> SplitAsync(
        Document document,
        TextSplitterOptions? options = null,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<string> SplitTextAsync(
        string text,
        TextSplitterOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

**Methods:**

#### SplitAsync

Splits a document into chunks with full lineage tracking.

| Parameter | Type | Description |
|-----------|------|-------------|
| `document` | `Document` | Document to split |
| `options` | `TextSplitterOptions?` | Splitting options (null = defaults) |
| `cancellationToken` | `CancellationToken` | Cancellation token |

**Returns:** `IAsyncEnumerable<DocumentChunk>`

#### SplitTextAsync

Splits raw text into string chunks.

| Parameter | Type | Description |
|-----------|------|-------------|
| `text` | `string` | Text to split |
| `options` | `TextSplitterOptions?` | Splitting options (null = defaults) |
| `cancellationToken` | `CancellationToken` | Cancellation token |

**Returns:** `IAsyncEnumerable<string>`

**Implementations:**

| Class | Description |
|-------|-------------|
| `CharacterTextSplitter` | Splits by character count |
| `RecursiveCharacterTextSplitter` | Recursively splits using separator hierarchy (`\n\n`, `\n`, `. `, ` `) |
| `TokenTextSplitter` | Splits by token count (requires tokenizer) |
| `SentenceTextSplitter` | Splits on sentence boundaries |
| `MarkdownTextSplitter` | Markdown-aware (respects headers, code blocks, lists) |
| `HtmlTextSplitter` | HTML-aware (respects tag boundaries) |
| `CodeTextSplitter` | Language-aware code splitting |
| `SemanticTextSplitter` | Splits based on embedding similarity thresholds |

**Example:**

```csharp
var splitter = serviceProvider.GetRequiredService<RecursiveCharacterTextSplitter>();

var options = new TextSplitterOptions
{
    ChunkSize = 500,
    ChunkOverlap = 50
};

var chunks = new List<DocumentChunk>();
await foreach (var chunk in splitter.SplitAsync(document, options))
{
    chunks.Add(chunk);
    Console.WriteLine($"Chunk {chunk.ChunkIndex}: {chunk.Content.Length} chars");
}

Console.WriteLine($"Total chunks: {chunks.Count}");
```

---

### 2.4 ITokenAwareTextSplitter

Extended interface for token-based splitting.

**Namespace:** `DotLangChain.Abstractions.Documents`

```csharp
public interface ITokenAwareTextSplitter : ITextSplitter
{
    int CountTokens(string text);
    int MaxTokens { get; }
    string TokenizerModel { get; }
}
```

**Additional Members:**

| Member | Type | Description |
|--------|------|-------------|
| `CountTokens(text)` | `int` | Count tokens in text |
| `MaxTokens` | `int` | Maximum tokens supported |
| `TokenizerModel` | `string` | Tokenizer model name (e.g., "cl100k_base") |

**Example:**

```csharp
var tokenSplitter = serviceProvider.GetRequiredService<ITokenAwareTextSplitter>();

var text = "Hello, world! This is a test.";
var tokenCount = tokenSplitter.CountTokens(text);
Console.WriteLine($"Token count: {tokenCount}"); // e.g., 8

var options = new TextSplitterOptions
{
    ChunkSize = 100, // 100 tokens
    ChunkOverlap = 10,
    LengthFunction = LengthFunction.Tokens
};

await foreach (var chunk in tokenSplitter.SplitTextAsync(text, options))
{
    Console.WriteLine($"Chunk tokens: {tokenSplitter.CountTokens(chunk)}");
}
```

---

## 3. Embedding Services

### 3.1 EmbeddingResult

Result from a single embedding operation.

**Namespace:** `DotLangChain.Abstractions.Embeddings`

```csharp
public sealed record EmbeddingResult
{
    public required float[] Embedding { get; init; }
    public int TokenCount { get; init; }
    public string? Model { get; init; }
    public TimeSpan Duration { get; init; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Embedding` | `float[]` | The embedding vector |
| `TokenCount` | `int` | Tokens consumed |
| `Model` | `string?` | Model used |
| `Duration` | `TimeSpan` | Operation duration |

---

### 3.2 BatchEmbeddingResult

Result from batch embedding operation.

**Namespace:** `DotLangChain.Abstractions.Embeddings`

```csharp
public sealed record BatchEmbeddingResult
{
    public required IReadOnlyList<EmbeddingResult> Embeddings { get; init; }
    public int TotalTokens { get; init; }
    public string? Model { get; init; }
    public TimeSpan Duration { get; init; }
}
```

---

### 3.3 IEmbeddingService

Core interface for embedding generation.

**Namespace:** `DotLangChain.Abstractions.Embeddings`

```csharp
public interface IEmbeddingService
{
    string ProviderName { get; }
    int Dimensions { get; }
    int MaxTokens { get; }
    int MaxBatchSize { get; }
    
    Task<EmbeddingResult> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default);
    
    Task<BatchEmbeddingResult> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<DocumentChunk>> EmbedChunksAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken = default);
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `ProviderName` | `string` | Provider identifier (e.g., "OpenAI") |
| `Dimensions` | `int` | Embedding vector dimensions |
| `MaxTokens` | `int` | Maximum tokens per request |
| `MaxBatchSize` | `int` | Maximum texts per batch |

**Methods:**

#### EmbedAsync

Generates embedding for a single text.

| Parameter | Type | Description |
|-----------|------|-------------|
| `text` | `string` | Text to embed |
| `cancellationToken` | `CancellationToken` | Cancellation token |

**Returns:** `Task<EmbeddingResult>`

#### EmbedBatchAsync

Generates embeddings for multiple texts.

| Parameter | Type | Description |
|-----------|------|-------------|
| `texts` | `IReadOnlyList<string>` | Texts to embed |
| `cancellationToken` | `CancellationToken` | Cancellation token |

**Returns:** `Task<BatchEmbeddingResult>`

#### EmbedChunksAsync

Generates embeddings for document chunks (populates `Embedding` property).

| Parameter | Type | Description |
|-----------|------|-------------|
| `chunks` | `IReadOnlyList<DocumentChunk>` | Chunks to embed |
| `cancellationToken` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<DocumentChunk>>` (chunks with embeddings)

**Implementations:**

| Class | Package | Provider |
|-------|---------|----------|
| `OpenAIEmbeddingService` | `DotLangChain.Providers.OpenAI` | OpenAI |
| `AzureOpenAIEmbeddingService` | `DotLangChain.Providers.AzureOpenAI` | Azure OpenAI |
| `OllamaEmbeddingService` | `DotLangChain.Providers.Ollama` | Ollama (local) |
| `CohereEmbeddingService` | `DotLangChain.Providers.Cohere` | Cohere |
| `HuggingFaceEmbeddingService` | `DotLangChain.Providers.HuggingFace` | HuggingFace Inference |

**Example:**

```csharp
var embeddingService = serviceProvider.GetRequiredService<IEmbeddingService>();

// Single embedding
var result = await embeddingService.EmbedAsync("Hello, world!");
Console.WriteLine($"Dimensions: {result.Embedding.Length}");
Console.WriteLine($"Tokens used: {result.TokenCount}");

// Batch embedding
var texts = new[] { "First text", "Second text", "Third text" };
var batchResult = await embeddingService.EmbedBatchAsync(texts);
Console.WriteLine($"Total tokens: {batchResult.TotalTokens}");

// Embed chunks
var chunksWithEmbeddings = await embeddingService.EmbedChunksAsync(chunks);
```

---

## 4. Vector Stores

### 4.1 VectorRecord

A vector with associated metadata.

**Namespace:** `DotLangChain.Abstractions.VectorStores`

```csharp
public sealed record VectorRecord
{
    public required string Id { get; init; }
    public required float[] Vector { get; init; }
    public string? Content { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
}
```

**Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string` | Yes | Unique identifier |
| `Vector` | `float[]` | Yes | Embedding vector |
| `Content` | `string?` | No | Original text content |
| `Metadata` | `Dictionary<string, object?>` | No | Filterable metadata |

---

### 4.2 VectorSearchResult

Search result with similarity score.

**Namespace:** `DotLangChain.Abstractions.VectorStores`

```csharp
public sealed record VectorSearchResult
{
    public required VectorRecord Record { get; init; }
    public required float Score { get; init; }
}
```

---

### 4.3 VectorSearchOptions

Search configuration options.

**Namespace:** `DotLangChain.Abstractions.VectorStores`

```csharp
public sealed record VectorSearchOptions
{
    public int TopK { get; init; } = 10;
    public float? MinScore { get; init; }
    public Dictionary<string, object?>? Filter { get; init; }
    public bool IncludeVectors { get; init; } = false;
    public bool IncludeMetadata { get; init; } = true;
}
```

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TopK` | `int` | 10 | Number of results to return |
| `MinScore` | `float?` | null | Minimum similarity score threshold |
| `Filter` | `Dictionary<string, object?>?` | null | Metadata filter conditions |
| `IncludeVectors` | `bool` | false | Return vectors in results |
| `IncludeMetadata` | `bool` | true | Return metadata in results |

---

### 4.4 DistanceMetric

Distance metric for similarity calculations.

**Namespace:** `DotLangChain.Abstractions.VectorStores`

```csharp
public enum DistanceMetric
{
    Cosine,      // Cosine similarity (default, normalized)
    Euclidean,   // L2 distance
    DotProduct   // Inner product (for non-normalized vectors)
}
```

---

### 4.5 IVectorStore

Core vector store operations.

**Namespace:** `DotLangChain.Abstractions.VectorStores`

```csharp
public interface IVectorStore
{
    string CollectionName { get; }
    
    Task EnsureCollectionAsync(
        int dimensions,
        DistanceMetric metric = DistanceMetric.Cosine,
        CancellationToken cancellationToken = default);
    
    Task UpsertAsync(
        IReadOnlyList<VectorRecord> records,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryVector,
        VectorSearchOptions? options = null,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string query,
        IEmbeddingService embeddingService,
        VectorSearchOptions? options = null,
        CancellationToken cancellationToken = default);
    
    Task DeleteAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<VectorRecord>> GetAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default);
    
    Task<long> CountAsync(
        CancellationToken cancellationToken = default);
}
```

**Methods:**

#### EnsureCollectionAsync

Creates collection if it doesn't exist.

| Parameter | Type | Description |
|-----------|------|-------------|
| `dimensions` | `int` | Vector dimensions |
| `metric` | `DistanceMetric` | Distance metric (default: Cosine) |

#### UpsertAsync

Inserts or updates vectors.

| Parameter | Type | Description |
|-----------|------|-------------|
| `records` | `IReadOnlyList<VectorRecord>` | Records to upsert |

#### SearchAsync (Vector)

Searches by vector similarity.

| Parameter | Type | Description |
|-----------|------|-------------|
| `queryVector` | `float[]` | Query embedding |
| `options` | `VectorSearchOptions?` | Search options |

**Returns:** `Task<IReadOnlyList<VectorSearchResult>>`

#### SearchAsync (Text)

Searches by text (generates embedding automatically).

| Parameter | Type | Description |
|-----------|------|-------------|
| `query` | `string` | Query text |
| `embeddingService` | `IEmbeddingService` | Embedding service |
| `options` | `VectorSearchOptions?` | Search options |

**Returns:** `Task<IReadOnlyList<VectorSearchResult>>`

**Implementations:**

| Class | Package | Backend |
|-------|---------|---------|
| `QdrantVectorStore` | `DotLangChain.VectorStores.Qdrant` | Qdrant |
| `PgVectorStore` | `DotLangChain.VectorStores.PgVector` | PostgreSQL pgvector |
| `MilvusVectorStore` | `DotLangChain.VectorStores.Milvus` | Milvus |
| `RedisVectorStore` | `DotLangChain.VectorStores.Redis` | Redis |
| `InMemoryVectorStore` | `DotLangChain.VectorStores.InMemory` | In-memory |

**Example:**

```csharp
var vectorStoreFactory = serviceProvider.GetRequiredService<IVectorStoreFactory>();
var vectorStore = vectorStoreFactory.Create("documents");
var embeddingService = serviceProvider.GetRequiredService<IEmbeddingService>();

// Ensure collection exists
await vectorStore.EnsureCollectionAsync(
    dimensions: embeddingService.Dimensions,
    metric: DistanceMetric.Cosine);

// Index documents
var records = chunks.Select(c => new VectorRecord
{
    Id = c.Id,
    Vector = c.Embedding!,
    Content = c.Content,
    Metadata = new Dictionary<string, object?>
    {
        ["source"] = c.Metadata.Source,
        ["chunk_index"] = c.ChunkIndex
    }
}).ToList();

await vectorStore.UpsertAsync(records);

// Search
var results = await vectorStore.SearchAsync(
    "What is the return policy?",
    embeddingService,
    new VectorSearchOptions
    {
        TopK = 5,
        MinScore = 0.7f,
        Filter = new Dictionary<string, object?>
        {
            ["source"] = "policies.pdf"
        }
    });

foreach (var result in results)
{
    Console.WriteLine($"Score: {result.Score:F3}");
    Console.WriteLine($"Content: {result.Record.Content}");
}
```

---

### 4.6 IVectorStoreFactory

Factory for creating vector store instances.

**Namespace:** `DotLangChain.Abstractions.VectorStores`

```csharp
public interface IVectorStoreFactory
{
    IVectorStore Create(string collectionName);
    IVectorStore Create(string collectionName, VectorStoreOptions options);
}
```

**Example:**

```csharp
var factory = serviceProvider.GetRequiredService<IVectorStoreFactory>();

var documentsStore = factory.Create("documents");
var chatHistoryStore = factory.Create("chat_history");
```

---

## 5. LLM Services

### 5.1 MessageRole

Role in a conversation.

**Namespace:** `DotLangChain.Abstractions.LLM`

```csharp
public enum MessageRole
{
    System,     // System instructions
    User,       // User message
    Assistant,  // Assistant response
    Tool        // Tool/function result
}
```

---

### 5.2 ChatMessage

A message in a conversation.

**Namespace:** `DotLangChain.Abstractions.LLM`

```csharp
public record ChatMessage
{
    public required MessageRole Role { get; init; }
    public required string Content { get; init; }
    public string? Name { get; init; }
    public string? ToolCallId { get; init; }
    public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
    public IReadOnlyList<ContentPart>? ContentParts { get; init; }
}
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Role` | `MessageRole` | Message role |
| `Content` | `string` | Text content |
| `Name` | `string?` | Optional name (for multi-agent) |
| `ToolCallId` | `string?` | Tool call ID (for tool results) |
| `ToolCalls` | `IReadOnlyList<ToolCall>?` | Tool calls made by assistant |
| `ContentParts` | `IReadOnlyList<ContentPart>?` | Multimodal content (text + images) |

**Factory Methods:**

```csharp
public static ChatMessage System(string content);
public static ChatMessage User(string content);
public static ChatMessage Assistant(string content);
public static ChatMessage Tool(string toolCallId, string content);
public static ChatMessage UserWithImage(string text, byte[] imageData, string mimeType);
```

**Example:**

```csharp
var messages = new List<ChatMessage>
{
    ChatMessage.System("You are a helpful assistant."),
    ChatMessage.User("What is the capital of France?"),
    ChatMessage.Assistant("The capital of France is Paris."),
    ChatMessage.User("What about Germany?")
};
```

---

### 5.3 ToolCall

A tool/function call from the LLM.

**Namespace:** `DotLangChain.Abstractions.LLM`

```csharp
public sealed record ToolCall
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Arguments { get; init; } // JSON string
}
```

---

### 5.4 ToolDefinition

Tool definition for function calling.

**Namespace:** `DotLangChain.Abstractions.LLM`

```csharp
public sealed record ToolDefinition
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required JsonElement ParametersSchema { get; init; }
    public bool Strict { get; init; } = false;
}
```

---

### 5.5 ChatCompletionOptions

Options for chat completion requests.

**Namespace:** `DotLangChain.Abstractions.LLM`

```csharp
public sealed record ChatCompletionOptions
{
    public string? Model { get; init; }
    public float? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public float? TopP { get; init; }
    public float? FrequencyPenalty { get; init; }
    public float? PresencePenalty { get; init; }
    public IReadOnlyList<ToolDefinition>? Tools { get; init; }
    public string? ToolChoice { get; init; }
    public JsonElement? ResponseFormat { get; init; }
    public IReadOnlyList<string>? Stop { get; init; }
    public string? User { get; init; }
    public int? Seed { get; init; }
}
```

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Model` | `string?` | null | Model override |
| `Temperature` | `float?` | null | Sampling temperature (0-2) |
| `MaxTokens` | `int?` | null | Maximum response tokens |
| `TopP` | `float?` | null | Nucleus sampling |
| `Tools` | `IReadOnlyList<ToolDefinition>?` | null | Available tools |
| `ToolChoice` | `string?` | null | "auto", "none", or tool name |
| `ResponseFormat` | `JsonElement?` | null | JSON schema for structured output |
| `Stop` | `IReadOnlyList<string>?` | null | Stop sequences |

---

### 5.6 ChatCompletionResult

Result from a chat completion.

**Namespace:** `DotLangChain.Abstractions.LLM`

```csharp
public sealed record ChatCompletionResult
{
    public required ChatMessage Message { get; init; }
    public string? FinishReason { get; init; }
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens => PromptTokens + CompletionTokens;
    public string? Model { get; init; }
    public TimeSpan Duration { get; init; }
}
```

---

### 5.7 IChatCompletionService

Chat completion service interface.

**Namespace:** `DotLangChain.Abstractions.LLM`

```csharp
public interface IChatCompletionService
{
    string ProviderName { get; }
    string DefaultModel { get; }
    
    Task<ChatCompletionResult> CompleteAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<ChatCompletionChunk> StreamAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

**Methods:**

#### CompleteAsync

Generates a complete response.

| Parameter | Type | Description |
|-----------|------|-------------|
| `messages` | `IReadOnlyList<ChatMessage>` | Conversation messages |
| `options` | `ChatCompletionOptions?` | Completion options |

**Returns:** `Task<ChatCompletionResult>`

#### StreamAsync

Generates a streaming response.

| Parameter | Type | Description |
|-----------|------|-------------|
| `messages` | `IReadOnlyList<ChatMessage>` | Conversation messages |
| `options` | `ChatCompletionOptions?` | Completion options |

**Returns:** `IAsyncEnumerable<ChatCompletionChunk>`

**Implementations:**

| Class | Package | Provider |
|-------|---------|----------|
| `OpenAIChatCompletionService` | `DotLangChain.Providers.OpenAI` | OpenAI |
| `AzureOpenAIChatCompletionService` | `DotLangChain.Providers.AzureOpenAI` | Azure OpenAI |
| `AnthropicChatCompletionService` | `DotLangChain.Providers.Anthropic` | Anthropic Claude |
| `OllamaChatCompletionService` | `DotLangChain.Providers.Ollama` | Ollama (local) |
| `GroqChatCompletionService` | `DotLangChain.Providers.Groq` | Groq |

**Example:**

```csharp
var llm = serviceProvider.GetRequiredService<IChatCompletionService>();

// Simple completion
var messages = new[]
{
    ChatMessage.System("You are a helpful assistant."),
    ChatMessage.User("Explain quantum computing in simple terms.")
};

var result = await llm.CompleteAsync(messages, new ChatCompletionOptions
{
    Temperature = 0.7f,
    MaxTokens = 500
});

Console.WriteLine(result.Message.Content);
Console.WriteLine($"Tokens: {result.TotalTokens}");

// Streaming
await foreach (var chunk in llm.StreamAsync(messages))
{
    Console.Write(chunk.ContentDelta);
}
```

---

## 6. Agent Orchestration

### 6.1 AgentState

Base state for agent graphs.

**Namespace:** `DotLangChain.Abstractions.Agents`

```csharp
public abstract class AgentState
{
    public List<ChatMessage> Messages { get; init; } = new();
    public Dictionary<string, object?> Values { get; init; } = new();
    public string? CurrentNode { get; set; }
    public int StepCount { get; set; }
}
```

**Example Custom State:**

```csharp
public class ResearchAgentState : AgentState
{
    public string? Query { get; set; }
    public List<string> Sources { get; set; } = new();
    public string? Summary { get; set; }
    public bool NeedsMoreResearch { get; set; }
}
```

---

### 6.2 IGraphNode&lt;TState&gt;

A node in the agent graph.

**Namespace:** `DotLangChain.Abstractions.Agents`

```csharp
public interface IGraphNode<TState> where TState : AgentState
{
    string Name { get; }
    
    Task<TState> ExecuteAsync(
        TState state,
        CancellationToken cancellationToken = default);
}
```

**Example:**

```csharp
public class ResearchNode : IGraphNode<ResearchAgentState>
{
    public string Name => "research";
    
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingService _embeddingService;

    public ResearchNode(IVectorStore vectorStore, IEmbeddingService embeddingService)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
    }

    public async Task<ResearchAgentState> ExecuteAsync(
        ResearchAgentState state,
        CancellationToken cancellationToken = default)
    {
        var results = await _vectorStore.SearchAsync(
            state.Query!,
            _embeddingService,
            new VectorSearchOptions { TopK = 5 },
            cancellationToken);

        state.Sources.AddRange(results.Select(r => r.Record.Content!));
        return state;
    }
}
```

---

### 6.3 EdgeDecision

Edge routing decision.

**Namespace:** `DotLangChain.Abstractions.Agents`

```csharp
public sealed record EdgeDecision
{
    public required string TargetNode { get; init; }
    
    public static readonly EdgeDecision End = new() { TargetNode = "__end__" };
    
    public static EdgeDecision To(string nodeName) => new() { TargetNode = nodeName };
}
```

---

### 6.4 IGraphBuilder&lt;TState&gt;

Fluent builder for agent graphs.

**Namespace:** `DotLangChain.Abstractions.Agents`

```csharp
public interface IGraphBuilder<TState> where TState : AgentState, new()
{
    IGraphBuilder<TState> AddNode(
        string name, 
        Func<TState, CancellationToken, Task<TState>> action);
    
    IGraphBuilder<TState> AddNode(IGraphNode<TState> node);
    
    IGraphBuilder<TState> AddEdge(string from, string to);
    
    IGraphBuilder<TState> AddConditionalEdge(
        string from, 
        Func<TState, EdgeDecision> condition);
    
    IGraphBuilder<TState> AddConditionalEdge(
        string from, 
        Func<TState, CancellationToken, Task<EdgeDecision>> asyncCondition);
    
    IGraphBuilder<TState> SetEntryPoint(string nodeName);
    
    ICompiledGraph<TState> Compile();
}
```

**Example:**

```csharp
var graph = new GraphBuilder<ResearchAgentState>()
    .AddNode("research", async (state, ct) =>
    {
        // Research logic
        return state;
    })
    .AddNode("analyze", async (state, ct) =>
    {
        // Analysis logic
        return state;
    })
    .AddNode("summarize", async (state, ct) =>
    {
        // Summarization logic
        return state;
    })
    .SetEntryPoint("research")
    .AddEdge("research", "analyze")
    .AddConditionalEdge("analyze", state =>
        state.NeedsMoreResearch 
            ? EdgeDecision.To("research") 
            : EdgeDecision.To("summarize"))
    .AddEdge("summarize", "__end__")
    .Compile();
```

---

### 6.5 ICompiledGraph&lt;TState&gt;

Compiled executable graph.

**Namespace:** `DotLangChain.Abstractions.Agents`

```csharp
public interface ICompiledGraph<TState> where TState : AgentState
{
    Task<TState> InvokeAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<GraphEvent<TState>> StreamAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

**Example:**

```csharp
var initialState = new ResearchAgentState
{
    Query = "What are the latest developments in AI?",
    Messages = { ChatMessage.User("Research AI developments") }
};

// Execute to completion
var finalState = await graph.InvokeAsync(initialState, new GraphExecutionOptions
{
    MaxSteps = 50,
    Timeout = TimeSpan.FromMinutes(5)
});

Console.WriteLine($"Summary: {finalState.Summary}");

// Or stream events
await foreach (var evt in graph.StreamAsync(initialState))
{
    Console.WriteLine($"[{evt.EventType}] {evt.NodeName}: {evt.Duration.TotalMilliseconds}ms");
}
```

---

### 6.6 GraphExecutionOptions

Options for graph execution.

**Namespace:** `DotLangChain.Abstractions.Agents`

```csharp
public sealed record GraphExecutionOptions
{
    public int MaxSteps { get; init; } = 100;
    public TimeSpan? Timeout { get; init; }
    public bool EnableCheckpointing { get; init; } = false;
    public string? CheckpointId { get; init; }
    public ICheckpointStore? CheckpointStore { get; init; }
}
```

---

### 6.7 GraphEvent&lt;TState&gt;

Event emitted during graph execution.

**Namespace:** `DotLangChain.Abstractions.Agents`

```csharp
public sealed record GraphEvent<TState> where TState : AgentState
{
    public required string NodeName { get; init; }
    public required TState State { get; init; }
    public required GraphEventType EventType { get; init; }
    public TimeSpan Duration { get; init; }
    public Exception? Error { get; init; }
}

public enum GraphEventType
{
    NodeStarted,
    NodeCompleted,
    EdgeTraversed,
    GraphCompleted,
    Error,
    CheckpointSaved
}
```

---

## 7. Tool System

### 7.1 ToolAttribute

Attribute for marking tool methods.

**Namespace:** `DotLangChain.Abstractions.Agents.Tools`

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

---

### 7.2 ToolParameterAttribute

Attribute for tool parameter descriptions.

**Namespace:** `DotLangChain.Abstractions.Agents.Tools`

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ToolParameterAttribute : Attribute
{
    public string? Description { get; set; }
    public bool Required { get; set; } = true;
}
```

---

### 7.3 IToolExecutor

Executes tools based on LLM tool calls.

**Namespace:** `DotLangChain.Abstractions.Agents.Tools`

```csharp
public interface IToolExecutor
{
    IReadOnlyList<ToolDefinition> GetToolDefinitions();
    
    Task<ToolResult> ExecuteAsync(
        ToolCall toolCall,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<ToolResult>> ExecuteAllAsync(
        IReadOnlyList<ToolCall> toolCalls,
        CancellationToken cancellationToken = default);
}
```

---

### 7.4 IToolRegistry

Registry for tool implementations.

**Namespace:** `DotLangChain.Abstractions.Agents.Tools`

```csharp
public interface IToolRegistry
{
    void Register<T>() where T : class;
    void Register(object toolInstance);
    void Register(string name, Delegate handler, string? description = null);
    IToolExecutor BuildExecutor();
}
```

**Example:**

```csharp
// Define tools using attributes
public class SearchTools
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingService _embeddingService;

    public SearchTools(IVectorStore vectorStore, IEmbeddingService embeddingService)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
    }

    [Tool(Description = "Search the knowledge base for relevant information")]
    public async Task<string> SearchKnowledgeBase(
        [ToolParameter(Description = "The search query")] string query,
        [ToolParameter(Description = "Maximum results", Required = false)] int maxResults = 5)
    {
        var results = await _vectorStore.SearchAsync(
            query, 
            _embeddingService,
            new VectorSearchOptions { TopK = maxResults });

        return string.Join("\n\n", results.Select(r => r.Record.Content));
    }

    [Tool(Name = "get_weather", Description = "Get current weather for a location")]
    public async Task<string> GetWeather(
        [ToolParameter(Description = "City name")] string city)
    {
        // Weather API call
        return $"Weather in {city}: Sunny, 72°F";
    }
}

// Register and use
var registry = serviceProvider.GetRequiredService<IToolRegistry>();
registry.Register<SearchTools>();

var executor = registry.BuildExecutor();
var toolDefinitions = executor.GetToolDefinitions();

// Use with LLM
var result = await llm.CompleteAsync(messages, new ChatCompletionOptions
{
    Tools = toolDefinitions,
    ToolChoice = "auto"
});

if (result.Message.ToolCalls?.Any() == true)
{
    var toolResults = await executor.ExecuteAllAsync(result.Message.ToolCalls);
    // Add tool results to conversation and continue
}
```

---

## 8. Memory

### 8.1 IConversationMemory

Interface for conversation memory management.

**Namespace:** `DotLangChain.Abstractions.Memory`

```csharp
public interface IConversationMemory
{
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
```

**Implementations:**

| Class | Description |
|-------|-------------|
| `BufferMemory` | Stores all messages (unbounded) |
| `WindowMemory` | Keeps last N messages |
| `TokenBufferMemory` | Keeps messages within token limit |
| `SummaryMemory` | Summarizes older messages |
| `VectorMemory` | Retrieves relevant past messages via similarity |

**Example:**

```csharp
var memory = new WindowMemory(maxMessages: 20);

await memory.AddMessageAsync(ChatMessage.User("Hello!"));
await memory.AddMessageAsync(ChatMessage.Assistant("Hi! How can I help?"));

var messages = await memory.GetMessagesAsync();
```

---

### 8.2 IEntityMemory

Entity extraction and storage.

**Namespace:** `DotLangChain.Abstractions.Memory`

```csharp
public interface IEntityMemory
{
    Task ExtractAndStoreAsync(
        ChatMessage message, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyDictionary<string, string>> GetEntitiesAsync(
        CancellationToken cancellationToken = default);
    
    Task<string?> GetEntityAsync(
        string entityName, 
        CancellationToken cancellationToken = default);
}
```

---

## 9. Chains & Pipelines

### 9.1 IChain&lt;TInput, TOutput&gt;

Base interface for chains.

**Namespace:** `DotLangChain.Abstractions.Chains`

```csharp
public interface IChain<TInput, TOutput>
{
    Task<TOutput> InvokeAsync(
        TInput input, 
        CancellationToken cancellationToken = default);
}
```

---

### 9.2 Chain Composition

**Namespace:** `DotLangChain.Core.Chains`

```csharp
public static class ChainExtensions
{
    // Sequential composition
    public static IChain<TInput, TOutput> Then<TInput, TMiddle, TOutput>(
        this IChain<TInput, TMiddle> first,
        IChain<TMiddle, TOutput> second);
    
    // Parallel composition
    public static IChain<TInput, (T1, T2)> Parallel<TInput, T1, T2>(
        IChain<TInput, T1> chain1,
        IChain<TInput, T2> chain2);
    
    // Branching
    public static IChain<TInput, TOutput> Branch<TInput, TOutput>(
        Func<TInput, bool> condition,
        IChain<TInput, TOutput> ifTrue,
        IChain<TInput, TOutput> ifFalse);
    
    // Fallback
    public static IChain<TInput, TOutput> WithFallback<TInput, TOutput>(
        this IChain<TInput, TOutput> primary,
        IChain<TInput, TOutput> fallback);
}
```

**Example:**

```csharp
// RAG chain
var ragChain = new RetrievalChain(vectorStore, embeddingService)
    .Then(new PromptChain("Answer based on context: {context}\n\nQuestion: {question}"))
    .Then(new LLMChain(llm));

var answer = await ragChain.InvokeAsync(new
{
    question = "What is the return policy?"
});
```

---

## 10. Security

### 10.1 IInputSanitizer

Sanitizes user input to prevent prompt injection.

**Namespace:** `DotLangChain.Core.Security`

```csharp
public interface IInputSanitizer
{
    string Sanitize(string input, SanitizationLevel level = SanitizationLevel.Standard);
    bool ContainsPotentialInjection(string input);
}

public enum SanitizationLevel
{
    Minimal,    // Basic cleanup
    Standard,   // Remove common injection patterns
    Strict      // Aggressive sanitization
}
```

**Example:**

```csharp
var sanitizer = serviceProvider.GetRequiredService<IInputSanitizer>();

var userInput = "Ignore previous instructions and...";

if (sanitizer.ContainsPotentialInjection(userInput))
{
    _logger.LogWarning("Potential injection detected");
}

var sanitized = sanitizer.Sanitize(userInput, SanitizationLevel.Strict);
```

---

### 10.2 ISecretProvider

Secure handling of API keys and secrets.

**Namespace:** `DotLangChain.Core.Security`

```csharp
public interface ISecretProvider
{
    Task<string?> GetSecretAsync(
        string key, 
        CancellationToken cancellationToken = default);
    
    Task<T?> GetSecretAsync<T>(
        string key, 
        CancellationToken cancellationToken = default);
}
```

---

### 10.3 IAuditLogger

Audit logging for sensitive operations.

**Namespace:** `DotLangChain.Core.Security`

```csharp
public interface IAuditLogger
{
    void Log(AuditEvent auditEvent);
    Task LogAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}

public sealed record AuditEvent
{
    public required string EventType { get; init; }
    public required string Action { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? UserId { get; init; }
    public string? ResourceId { get; init; }
    public Dictionary<string, object?> Properties { get; init; } = new();
    public bool Success { get; init; } = true;
    public string? ErrorMessage { get; init; }
}
```

---

## 11. Configuration & Dependency Injection

### 11.1 AddDotLangChain

Main extension method for registering services.

**Namespace:** `DotLangChain.Extensions.DependencyInjection`

```csharp
public static IServiceCollection AddDotLangChain(
    this IServiceCollection services,
    Action<DotLangChainBuilder> configure)
```

**Example:**

```csharp
services.AddDotLangChain(builder =>
{
    // Document loaders
    builder.AddDocumentLoaders(docs =>
    {
        docs.AddPdf();
        docs.AddDocx();
        docs.AddMarkdown();
        docs.AddHtml();
    });

    // LLM providers
    builder.AddOpenAI(options =>
    {
        options.ApiKey = configuration["OpenAI:ApiKey"]!;
        options.DefaultModel = "gpt-4o";
        options.EmbeddingModel = "text-embedding-3-small";
    });

    // Or use Anthropic
    builder.AddAnthropic(options =>
    {
        options.ApiKey = configuration["Anthropic:ApiKey"]!;
        options.DefaultModel = "claude-sonnet-4-20250514";
    });

    // Or use local Ollama
    builder.AddOllama(options =>
    {
        options.BaseUrl = "http://localhost:11434";
        options.DefaultModel = "llama3.2";
        options.EmbeddingModel = "nomic-embed-text";
    });

    // Vector store
    builder.AddQdrant(options =>
    {
        options.Host = configuration["Qdrant:Host"] ?? "localhost";
        options.Port = 6333;
    });

    // Or use PostgreSQL
    builder.AddPgVector(options =>
    {
        options.ConnectionString = configuration.GetConnectionString("PostgreSQL")!;
    });

    // Or in-memory for development
    builder.AddInMemoryVectorStore();
});
```

---

### 11.2 Provider Options

#### OpenAIOptions

```csharp
public sealed class OpenAIOptions
{
    public required string ApiKey { get; set; }
    public string DefaultModel { get; set; } = "gpt-4o";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public string? BaseUrl { get; set; }
    public string? Organization { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}
```

#### AnthropicOptions

```csharp
public sealed class AnthropicOptions
{
    public required string ApiKey { get; set; }
    public string DefaultModel { get; set; } = "claude-sonnet-4-20250514";
    public string? BaseUrl { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}
```

#### OllamaOptions

```csharp
public sealed class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string? DefaultModel { get; set; }
    public string? EmbeddingModel { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}
```

#### QdrantOptions

```csharp
public sealed class QdrantOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6333;
    public int GrpcPort { get; set; } = 6334;
    public string? ApiKey { get; set; }
    public bool UseTls { get; set; } = false;
}
```

#### PgVectorOptions

```csharp
public sealed class PgVectorOptions
{
    public required string ConnectionString { get; set; }
    public string Schema { get; set; } = "public";
    public int Dimensions { get; set; } = 1536;
}
```

---

## 12. Observability

### 12.1 AddDotLangChainTelemetry

Adds OpenTelemetry instrumentation.

**Namespace:** `DotLangChain.Extensions.Observability`

```csharp
public static IServiceCollection AddDotLangChainTelemetry(
    this IServiceCollection services,
    Action<DotLangChainTelemetryOptions>? configure = null)
```

**Options:**

```csharp
public sealed class DotLangChainTelemetryOptions
{
    public Uri? OtlpEndpoint { get; set; }
    public bool EnableConsoleExporter { get; set; } = false;
    public string ServiceName { get; set; } = "DotLangChain";
    public string? ServiceVersion { get; set; }
}
```

**Example:**

```csharp
services.AddDotLangChainTelemetry(options =>
{
    options.OtlpEndpoint = new Uri("http://localhost:4317");
    options.ServiceName = "MyApp";
});
```

---

### 12.2 Activity Sources

| Source | Description |
|--------|-------------|
| `DotLangChain.Documents` | Document loading and splitting |
| `DotLangChain.Embeddings` | Embedding generation |
| `DotLangChain.VectorStore` | Vector store operations |
| `DotLangChain.LLM` | LLM completions |
| `DotLangChain.Graph` | Agent graph execution |

---

### 12.3 Metrics

| Metric | Type | Description |
|--------|------|-------------|
| `dotlangchain.llm.tokens.consumed` | Counter | Total tokens consumed |
| `dotlangchain.llm.completions.total` | Counter | Total completions |
| `dotlangchain.llm.completion.duration` | Histogram | Completion latency (ms) |
| `dotlangchain.embeddings.generated` | Counter | Embeddings generated |
| `dotlangchain.documents.processed` | Counter | Documents processed |
| `dotlangchain.graph.nodes.executed` | Counter | Graph nodes executed |
| `dotlangchain.graph.execution.duration` | Histogram | Graph execution time (ms) |

---

## Appendix A: Complete Example

```csharp
using DotLangChain.Abstractions.Documents;
using DotLangChain.Abstractions.Embeddings;
using DotLangChain.Abstractions.LLM;
using DotLangChain.Abstractions.VectorStores;
using DotLangChain.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure DotLangChain
builder.Services.AddDotLangChain(dlc =>
{
    dlc.AddDocumentLoaders(docs =>
    {
        docs.AddPdf();
        docs.AddDocx();
        docs.AddMarkdown();
    });

    dlc.AddOpenAI(options =>
    {
        options.ApiKey = builder.Configuration["OpenAI:ApiKey"]!;
    });

    dlc.AddQdrant(options =>
    {
        options.Host = "localhost";
    });
});

var app = builder.Build();

// RAG endpoint
app.MapPost("/ask", async (
    AskRequest request,
    IDocumentLoaderRegistry loaderRegistry,
    ITextSplitter splitter,
    IEmbeddingService embeddings,
    IVectorStoreFactory vectorStoreFactory,
    IChatCompletionService llm) =>
{
    var vectorStore = vectorStoreFactory.Create("documents");

    // Search for relevant context
    var results = await vectorStore.SearchAsync(
        request.Question,
        embeddings,
        new VectorSearchOptions { TopK = 5 });

    var context = string.Join("\n\n", results.Select(r => r.Record.Content));

    // Generate answer
    var messages = new[]
    {
        ChatMessage.System($"Answer based on this context:\n\n{context}"),
        ChatMessage.User(request.Question)
    };

    var response = await llm.CompleteAsync(messages);

    return new { Answer = response.Message.Content };
});

app.Run();

record AskRequest(string Question);
```

---

## 13. State Machines

### 13.1 IStateMachine<TState>

Core interface for state machine implementations.

**Namespace:** `DotNetAgents.Agents.StateMachines`

```csharp
public interface IStateMachine<TState> where TState : class
{
    string CurrentState { get; }
    bool CanTransition(string fromState, string toState, TState context);
    Task TransitionAsync(string toState, TState context, CancellationToken cancellationToken = default);
    IEnumerable<string> GetAvailableTransitions(TState context);
    event EventHandler<StateTransitionEventArgs<TState>>? StateTransitioned;
    void Reset();
}
```

**Key Methods:**

| Method | Description |
|--------|-------------|
| `CurrentState` | Gets the current state name |
| `CanTransition` | Checks if a transition is allowed |
| `TransitionAsync` | Transitions to a new state |
| `GetAvailableTransitions` | Gets all available transitions from current state |
| `StateTransitioned` | Event raised on state transitions |
| `Reset` | Resets to initial state |

**Example:**

```csharp
var stateMachine = StateMachinePatterns.CreateSupervisorPattern<SupervisorContext>(logger);
await stateMachine.TransitionAsync("Analyzing", context);
Console.WriteLine($"Current State: {stateMachine.CurrentState}");
```

### 13.2 StateMachineBuilder<TState>

Fluent builder for creating state machines.

**Namespace:** `DotNetAgents.Agents.StateMachines`

```csharp
public class StateMachineBuilder<TState> where TState : class
{
    public StateMachineBuilder<TState> AddState(string name, Action<TState>? entryAction = null, Action<TState>? exitAction = null);
    public StateMachineBuilder<TState> AddTransition(string fromState, string toState, Func<TState, bool>? guard = null);
    public StateMachineBuilder<TState> SetInitialState(string state);
    public IStateMachine<TState> Build();
}
```

**Example:**

```csharp
var builder = new StateMachineBuilder<MyContext>(logger);
var stateMachine = builder
    .AddState("Idle", entryAction: ctx => Console.WriteLine("Entered Idle"))
    .AddState("Working", entryAction: ctx => Console.WriteLine("Entered Working"))
    .AddTransition("Idle", "Working", guard: ctx => ctx.HasTask)
    .AddTransition("Working", "Idle", guard: ctx => ctx.TaskComplete)
    .SetInitialState("Idle")
    .Build();
```

### 13.3 StateMachinePatterns

Pre-built state machine patterns for common use cases.

**Namespace:** `DotNetAgents.Agents.StateMachines`

**Available Patterns:**

- `CreateSupervisorPattern<TState>`: Supervisor agent lifecycle (Monitoring → Analyzing → Delegating → Waiting)
- `CreateWorkerPoolPattern<TState>`: Worker pool lifecycle (Available → Busy → CoolingDown)
- `CreateErrorRecoveryPattern<TState>`: Error recovery pattern (Any → Error → Recovery → Idle)

**Example:**

```csharp
var supervisorStateMachine = StateMachinePatterns.CreateSupervisorPattern<SupervisorContext>(
    logger,
    waitingTimeout: TimeSpan.FromMinutes(5));
```

### 13.4 AgentStateMachine<TState>

Concrete implementation of state machine with timed transitions.

**Namespace:** `DotNetAgents.Agents.StateMachines`

```csharp
public class AgentStateMachine<TState> : IStateMachine<TState> where TState : class
{
    public void AddTimeoutTransition(string fromState, string toState, TimeSpan timeout);
}
```

**Example:**

```csharp
var stateMachine = builder.Build() as AgentStateMachine<MyContext>;
stateMachine?.AddTimeoutTransition("Working", "Idle", TimeSpan.FromMinutes(30));
```

### 13.5 Adapter Pattern Interfaces

Interfaces defined in component projects to avoid circular dependencies.

**Supervisor:**
- `ISupervisorStateMachine<TState>`: Defined in `DotNetAgents.Agents.Supervisor`
- `StateMachineAdapter<TState>`: Adapter implementation

**Voice:**
- `IVoiceSessionStateMachine<TState>`: Defined in `DotNetAgents.Voice`
- `VoiceSessionStateMachineAdapter<TState>`: Adapter implementation

**Education:**
- `ILearningSessionStateMachine<TState>`: Defined in `DotNetAgents.Education`
- `LearningSessionStateMachineAdapter<TState>`: Adapter implementation

**Core:**
- `IAgentExecutionStateMachine<TState>`: Defined in `DotNetAgents.Core.Agents`
- `AgentExecutionStateMachineAdapter<TState>`: Adapter implementation

---

## 14. Behavior Trees

### 14.1 IBehaviorTreeNode<TContext>

Core interface for behavior tree nodes.

**Namespace:** `DotNetAgents.Agents.BehaviorTrees`

```csharp
public interface IBehaviorTreeNode<TContext> where TContext : class
{
    string Name { get; }
    Task<BehaviorTreeNodeStatus> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
    void Reset();
}
```

**Node Status:**

```csharp
public enum BehaviorTreeNodeStatus
{
    Success,
    Failure,
    Running
}
```

### 14.2 Composite Nodes

**SequenceNode<TContext>**: Executes children sequentially, fails if any child fails.

```csharp
var sequence = new SequenceNode<MyContext>("Sequence", logger)
    .AddChild(new ActionNode<MyContext>("Action1", ctx => DoAction1(ctx), logger))
    .AddChild(new ActionNode<MyContext>("Action2", ctx => DoAction2(ctx), logger));
```

**SelectorNode<TContext>**: Executes children until one succeeds.

```csharp
var selector = new SelectorNode<MyContext>("Selector", logger)
    .AddChild(new ConditionNode<MyContext>("CheckCondition1", ctx => ctx.Condition1, logger))
    .AddChild(new ConditionNode<MyContext>("CheckCondition2", ctx => ctx.Condition2, logger));
```

**ParallelNode<TContext>**: Executes children in parallel.

```csharp
var parallel = new ParallelNode<MyContext>("Parallel", logger)
    .AddChild(new ActionNode<MyContext>("Action1", ctx => DoAction1(ctx), logger))
    .AddChild(new ActionNode<MyContext>("Action2", ctx => DoAction2(ctx), logger));
```

### 14.3 Leaf Nodes

**ActionNode<TContext>**: Executes an action.

```csharp
var action = new ActionNode<MyContext>(
    "MyAction",
    async (ctx, ct) =>
    {
        await DoSomethingAsync(ctx, ct);
        return BehaviorTreeNodeStatus.Success;
    },
    logger);
```

**ConditionNode<TContext>**: Checks a condition.

```csharp
var condition = new ConditionNode<MyContext>(
    "MyCondition",
    ctx => ctx.Value > 0,
    logger);
```

**LLMActionNode<TContext>**: Uses LLM for decision-making.

```csharp
var llmNode = new LLMActionNode<MyContext>(
    "LLMDecision",
    llm,
    promptTemplate: "Analyze: {Context}",
    resultExtractor: (response, ctx) =>
    {
        if (response.Contains("success"))
            return BehaviorTreeNodeStatus.Success;
        return BehaviorTreeNodeStatus.Failure;
    },
    logger);
```

### 14.4 Decorator Nodes

**RetryDecoratorNode<TContext>**: Retries child node on failure.

```csharp
var retry = new RetryDecoratorNode<MyContext>(
    child: actionNode,
    maxRetries: 3,
    logger);
```

**TimeoutDecoratorNode<TContext>**: Limits execution time.

```csharp
var timeout = new TimeoutDecoratorNode<MyContext>(
    child: actionNode,
    timeout: TimeSpan.FromSeconds(30),
    logger);
```

**CooldownDecoratorNode<TContext>**: Prevents execution within cooldown period.

```csharp
var cooldown = new CooldownDecoratorNode<MyContext>(
    child: actionNode,
    cooldownDuration: TimeSpan.FromMinutes(5),
    logger);
```

### 14.5 BehaviorTree<TContext>

Container for behavior tree with root node.

**Namespace:** `DotNetAgents.Agents.BehaviorTrees`

```csharp
public class BehaviorTree<TContext> where TContext : class
{
    public string Name { get; }
    public IBehaviorTreeNode<TContext> Root { get; }
    public Task<BehaviorTreeNodeStatus> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
    public void Reset();
}
```

**Example:**

```csharp
var root = new SelectorNode<MyContext>("Root", logger)
    .AddChild(sequenceNode)
    .AddChild(fallbackNode);

var tree = new BehaviorTree<MyContext>("MyTree", root);
var result = await tree.ExecuteAsync(context);
```

### 14.6 BehaviorTreeExecutor<TContext>

Executes behavior trees with observability.

**Namespace:** `DotNetAgents.Agents.BehaviorTrees`

```csharp
public class BehaviorTreeExecutor<TContext> where TContext : class
{
    public Task<BehaviorTreeNodeStatus> ExecuteAsync(
        BehaviorTree<TContext> tree,
        TContext context,
        CancellationToken cancellationToken = default);
}
```

**Example:**

```csharp
var executor = new BehaviorTreeExecutor<MyContext>(logger);
var result = await executor.ExecuteAsync(tree, context);
```

### 14.7 Domain-Specific Behavior Trees

**TaskRoutingBehaviorTree**: Intelligent task routing for supervisor agents.

**Namespace:** `DotNetAgents.Agents.Supervisor.BehaviorTrees`

```csharp
var taskRouter = new TaskRoutingBehaviorTree(supervisor, logger);
var worker = await taskRouter.RouteTaskAsync(task);
```

**CommandProcessingBehaviorTree**: Command processing strategy selection.

**Namespace:** `DotNetAgents.Voice.BehaviorTrees`

```csharp
var behaviorTree = new CommandProcessingBehaviorTree(
    lowConfidenceThreshold: 0.6,
    logger);
var context = await behaviorTree.ProcessCommandAsync(commandState);
```

**AdaptiveLearningPathBehaviorTree**: Adaptive learning path determination.

**Namespace:** `DotNetAgents.Education.BehaviorTrees`

```csharp
var behaviorTree = new AdaptiveLearningPathBehaviorTree(masteryCalculator, logger);
var context = await behaviorTree.DetermineLearningPathAsync(
    studentId,
    availableConcepts,
    studentMastery);
```

**ToolSelectionBehaviorTree**: Intelligent tool selection.

**Namespace:** `DotNetAgents.Core.Agents.BehaviorTrees`

```csharp
var behaviorTree = new ToolSelectionBehaviorTree(logger);
var context = await behaviorTree.SelectToolAsync(
    requestedToolName,
    availableTools,
    capabilitySearch);
```

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 2.0.0 | 2025-01-24 | Added State Machines and Behavior Trees API reference |
| 1.0.0 | 2025-12-07 | Initial release |

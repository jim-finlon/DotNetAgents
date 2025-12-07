# DotLangChain Technical Specifications

**Project Name:** DotLangChain  
**Version:** 1.0.0  
**Date:** December 7, 2025  
**Status:** Draft  

---

## 1. Introduction

### 1.1 Purpose

This document provides the technical specifications for implementing DotLangChain, a .NET 9 library for document ingestion, embedding generation, vector storage, and agent orchestration. It details the architecture, component design, interfaces, and implementation patterns.

### 1.2 Scope

This specification covers:

- Solution structure and project organization
- Core abstractions and interfaces
- Component implementations
- Integration patterns
- Security implementation
- Performance optimization strategies

### 1.3 Reference Documents

- DotLangChain Requirements Document v1.0.0
- Microsoft .NET 9 Documentation
- OWASP Application Security Verification Standard (ASVS) 4.0
- OpenTelemetry .NET SDK Documentation

### 1.4 Supporting Documentation

This specification should be read in conjunction with:

- **BUILD_AND_CICD.md**: Build configuration, CI/CD pipelines, and package versioning
- **TESTING_STRATEGY.md**: Testing approach, coverage requirements, and test organization
- **PERFORMANCE_BENCHMARKS.md**: Performance targets, benchmark scenarios, and measurement methodology
- **ERROR_HANDLING.md**: Exception hierarchy, error codes, and retry policies
- **VERSIONING_AND_MIGRATION.md**: Versioning strategy, breaking changes policy, and migration guides
- **PACKAGE_METADATA.md**: Package organization, NuGet metadata, and distribution guidelines

---

## 2. Architecture Overview

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Application Layer                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │   Chains    │  │   Agents    │  │  Patterns   │  │   Pre-built Flows   │ │
│  │  Pipeline   │  │   (Graph)   │  │   (ReAct)   │  │   (RAG, QA, etc.)   │ │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └──────────┬──────────┘ │
│         │                │                │                     │            │
├─────────┴────────────────┴────────────────┴─────────────────────┴────────────┤
│                           Orchestration Layer                                 │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │                         Graph Execution Engine                          │ │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────────────────┐ │ │
│  │  │   Nodes   │  │   Edges   │  │   State   │  │   Checkpoint Store    │ │ │
│  │  └───────────┘  └───────────┘  └───────────┘  └───────────────────────┘ │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
├──────────────────────────────────────────────────────────────────────────────┤
│                             Core Services Layer                              │
│                                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐ │
│  │   Document   │  │  Embedding   │  │    Vector    │  │       LLM        │ │
│  │   Ingestion  │  │   Service    │  │    Store     │  │     Service      │ │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘  └────────┬─────────┘ │
│         │                 │                 │                    │           │
│  ┌──────┴───────┐  ┌──────┴───────┐  ┌──────┴───────┐  ┌────────┴─────────┐ │
│  │   Loaders    │  │  Providers   │  │  Providers   │  │    Providers     │ │
│  │  Splitters   │  │   (OpenAI,   │  │  (Qdrant,    │  │   (OpenAI,       │ │
│  │  Extractors  │  │   Ollama)    │  │   pgvector)  │  │   Claude, etc.)  │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────────┘ │
│                                                                              │
├──────────────────────────────────────────────────────────────────────────────┤
│                            Infrastructure Layer                              │
│                                                                              │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐ │
│  │  Caching   │ │ Resilience │ │Observability│ │  Security  │ │   Config   │ │
│  │ (IDistrib  │ │  (Polly)   │ │  (OTEL)    │ │ (Sanitize) │ │ (IConfig)  │ │
│  │  Cache)    │ │            │ │            │ │            │ │            │ │
│  └────────────┘ └────────────┘ └────────────┘ └────────────┘ └────────────┘ │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Design Principles

| Principle | Implementation |
|-----------|----------------|
| Dependency Inversion | All components depend on abstractions (interfaces) |
| Single Responsibility | Each class has one reason to change |
| Open/Closed | Extensible via new implementations, not modification |
| Interface Segregation | Small, focused interfaces |
| Composition over Inheritance | Behaviors composed via DI, not class hierarchies |
| Fail Fast | Validate inputs early; throw meaningful exceptions |
| Async by Default | All I/O operations are async |

---

## 3. Solution Structure

### 3.1 Project Organization

```
DotLangChain/
├── src/
│   ├── DotLangChain.Abstractions/          # Core interfaces and contracts
│   │   ├── Documents/
│   │   ├── Embeddings/
│   │   ├── VectorStores/
│   │   ├── LLM/
│   │   ├── Agents/
│   │   ├── Memory/
│   │   └── Common/
│   │
│   ├── DotLangChain.Core/                  # Core implementations
│   │   ├── Documents/
│   │   │   ├── Loaders/
│   │   │   ├── Splitters/
│   │   │   └── Extractors/
│   │   ├── Embeddings/
│   │   ├── Agents/
│   │   │   ├── Graph/
│   │   │   ├── State/
│   │   │   └── Tools/
│   │   ├── Memory/
│   │   ├── Chains/
│   │   └── Patterns/
│   │
│   ├── DotLangChain.Providers.OpenAI/      # OpenAI provider
│   ├── DotLangChain.Providers.Anthropic/   # Anthropic Claude provider
│   ├── DotLangChain.Providers.Ollama/      # Ollama local provider
│   ├── DotLangChain.Providers.AzureOpenAI/ # Azure OpenAI provider
│   │
│   ├── DotLangChain.VectorStores.Qdrant/   # Qdrant integration
│   ├── DotLangChain.VectorStores.PgVector/ # PostgreSQL pgvector
│   ├── DotLangChain.VectorStores.InMemory/ # In-memory (dev/test)
│   │
│   ├── DotLangChain.StateStores.Redis/     # Redis state persistence
│   ├── DotLangChain.StateStores.PostgreSQL/# PostgreSQL state persistence
│   │
│   └── DotLangChain.Extensions/            # Extension methods, utilities
│       ├── DependencyInjection/
│       └── Observability/
│
├── tests/
│   ├── DotLangChain.Tests.Unit/
│   ├── DotLangChain.Tests.Integration/
│   └── DotLangChain.Tests.Benchmarks/
│
├── samples/
│   ├── DotLangChain.Samples.RAG/
│   ├── DotLangChain.Samples.Agent/
│   └── DotLangChain.Samples.MultiAgent/
│
└── docs/
    ├── api/
    └── guides/
```

### 3.2 Package Dependencies

#### DotLangChain.Abstractions

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
  <PackageReference Include="System.Text.Json" Version="9.0.0" />
</ItemGroup>
```

#### DotLangChain.Core

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.Options" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
  <PackageReference Include="Polly.Extensions" Version="8.5.0" />
  <PackageReference Include="OpenTelemetry.Api" Version="1.10.0" />
  <PackageReference Include="System.Threading.Channels" Version="9.0.0" />
</ItemGroup>
```

---

## 4. Core Abstractions

### 4.1 Document Types

```csharp
namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Represents a document with content and metadata.
/// </summary>
public sealed record Document
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public DocumentMetadata Metadata { get; init; } = new();
    public string? SourceUri { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Strongly-typed metadata container with extension support.
/// </summary>
public sealed class DocumentMetadata : Dictionary<string, object?>
{
    public string? Title
    {
        get => TryGetValue("title", out var v) ? v?.ToString() : null;
        set => this["title"] = value;
    }

    public string? Source
    {
        get => TryGetValue("source", out var v) ? v?.ToString() : null;
        set => this["source"] = value;
    }

    public int? PageNumber
    {
        get => TryGetValue("page_number", out var v) && v is int i ? i : null;
        set => this["page_number"] = value;
    }

    public int? ChunkIndex
    {
        get => TryGetValue("chunk_index", out var v) && v is int i ? i : null;
        set => this["chunk_index"] = value;
    }

    public string? ParentDocumentId
    {
        get => TryGetValue("parent_document_id", out var v) ? v?.ToString() : null;
        set => this["parent_document_id"] = value;
    }
}

/// <summary>
/// Represents a text chunk with embedding and lineage.
/// </summary>
public sealed record DocumentChunk
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public required string ParentDocumentId { get; init; }
    public required int ChunkIndex { get; init; }
    public int StartCharOffset { get; init; }
    public int EndCharOffset { get; init; }
    public float[]? Embedding { get; init; }
    public DocumentMetadata Metadata { get; init; } = new();
}
```

### 4.2 Document Loader Interface

```csharp
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
    Task<Document> LoadAsync(
        Stream stream,
        string fileName,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a document from a file path.
    /// </summary>
    Task<Document> LoadAsync(
        string filePath,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a document from a URI.
    /// </summary>
    Task<Document> LoadAsync(
        Uri uri,
        DocumentMetadata? metadata = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Registry for document loaders by file type.
/// </summary>
public interface IDocumentLoaderRegistry
{
    IDocumentLoader? GetLoader(string extension);
    void Register(IDocumentLoader loader);
}
```

### 4.3 Text Splitter Interface

```csharp
namespace DotLangChain.Abstractions.Documents;

/// <summary>
/// Configuration for text splitting operations.
/// </summary>
public sealed record TextSplitterOptions
{
    public int ChunkSize { get; init; } = 1000;
    public int ChunkOverlap { get; init; } = 200;
    public bool KeepSeparator { get; init; } = true;
    public bool StripWhitespace { get; init; } = true;
    public int? LengthFunction { get; init; } // null = character count
}

/// <summary>
/// Splits documents into smaller chunks.
/// </summary>
public interface ITextSplitter
{
    /// <summary>
    /// Splits a document into chunks.
    /// </summary>
    IAsyncEnumerable<DocumentChunk> SplitAsync(
        Document document,
        TextSplitterOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Splits raw text into chunks.
    /// </summary>
    IAsyncEnumerable<string> SplitTextAsync(
        string text,
        TextSplitterOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker interface for token-aware splitters.
/// </summary>
public interface ITokenAwareTextSplitter : ITextSplitter
{
    /// <summary>
    /// Counts tokens in the given text.
    /// </summary>
    int CountTokens(string text);
}
```

### 4.4 Embedding Service Interface

```csharp
namespace DotLangChain.Abstractions.Embeddings;

/// <summary>
/// Embedding generation result.
/// </summary>
public sealed record EmbeddingResult
{
    public required float[] Embedding { get; init; }
    public int TokenCount { get; init; }
    public string? Model { get; init; }
}

/// <summary>
/// Batch embedding result.
/// </summary>
public sealed record BatchEmbeddingResult
{
    public required IReadOnlyList<EmbeddingResult> Embeddings { get; init; }
    public int TotalTokens { get; init; }
    public string? Model { get; init; }
}

/// <summary>
/// Generates embeddings for text content.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Gets the embedding dimension for this provider.
    /// </summary>
    int Dimensions { get; }

    /// <summary>
    /// Gets the maximum tokens per request.
    /// </summary>
    int MaxTokens { get; }

    /// <summary>
    /// Generates an embedding for a single text.
    /// </summary>
    Task<EmbeddingResult> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for multiple texts.
    /// </summary>
    Task<BatchEmbeddingResult> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Caching decorator options.
/// </summary>
public sealed record EmbeddingCacheOptions
{
    public TimeSpan? AbsoluteExpiration { get; init; }
    public TimeSpan? SlidingExpiration { get; init; }
    public bool Enabled { get; init; } = true;
}
```

### 4.5 Vector Store Interface

```csharp
namespace DotLangChain.Abstractions.VectorStores;

/// <summary>
/// A vector with associated metadata.
/// </summary>
public sealed record VectorRecord
{
    public required string Id { get; init; }
    public required float[] Vector { get; init; }
    public string? Content { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
}

/// <summary>
/// Search result with similarity score.
/// </summary>
public sealed record VectorSearchResult
{
    public required VectorRecord Record { get; init; }
    public required float Score { get; init; }
}

/// <summary>
/// Search options for vector queries.
/// </summary>
public sealed record VectorSearchOptions
{
    public int TopK { get; init; } = 10;
    public float? MinScore { get; init; }
    public Dictionary<string, object?>? Filter { get; init; }
    public bool IncludeVectors { get; init; } = false;
    public bool IncludeMetadata { get; init; } = true;
}

/// <summary>
/// Distance metric for similarity calculations.
/// </summary>
public enum DistanceMetric
{
    Cosine,
    Euclidean,
    DotProduct
}

/// <summary>
/// Abstract vector store operations.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Gets the collection/namespace name.
    /// </summary>
    string CollectionName { get; }

    /// <summary>
    /// Creates the collection if it doesn't exist.
    /// </summary>
    Task EnsureCollectionAsync(
        int dimensions,
        DistanceMetric metric = DistanceMetric.Cosine,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts vectors into the store.
    /// </summary>
    Task UpsertAsync(
        IReadOnlyList<VectorRecord> records,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar vectors.
    /// </summary>
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryVector,
        VectorSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes vectors by ID.
    /// </summary>
    Task DeleteAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets vectors by ID.
    /// </summary>
    Task<IReadOnlyList<VectorRecord>> GetAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Factory for creating vector store instances.
/// </summary>
public interface IVectorStoreFactory
{
    IVectorStore Create(string collectionName);
}
```

### 4.6 LLM Service Interface

```csharp
namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// Role in a conversation.
/// </summary>
public enum MessageRole
{
    System,
    User,
    Assistant,
    Tool
}

/// <summary>
/// A message in a conversation.
/// </summary>
public record ChatMessage
{
    public required MessageRole Role { get; init; }
    public required string Content { get; init; }
    public string? Name { get; init; }
    public string? ToolCallId { get; init; }
    public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
}

/// <summary>
/// A tool/function call from the LLM.
/// </summary>
public sealed record ToolCall
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Arguments { get; init; } // JSON string
}

/// <summary>
/// Tool definition for function calling.
/// </summary>
public sealed record ToolDefinition
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required JsonElement ParametersSchema { get; init; }
    public bool Strict { get; init; } = false;
}

/// <summary>
/// Options for chat completion requests.
/// </summary>
public sealed record ChatCompletionOptions
{
    public string? Model { get; init; }
    public float? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public float? TopP { get; init; }
    public IReadOnlyList<ToolDefinition>? Tools { get; init; }
    public string? ToolChoice { get; init; } // "auto", "none", or specific tool name
    public JsonElement? ResponseFormat { get; init; }
    public IReadOnlyList<string>? Stop { get; init; }
}

/// <summary>
/// Result from a chat completion.
/// </summary>
public sealed record ChatCompletionResult
{
    public required ChatMessage Message { get; init; }
    public string? FinishReason { get; init; }
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public string? Model { get; init; }
}

/// <summary>
/// Streaming chunk from completion.
/// </summary>
public sealed record ChatCompletionChunk
{
    public string? ContentDelta { get; init; }
    public ToolCall? ToolCallDelta { get; init; }
    public string? FinishReason { get; init; }
}

/// <summary>
/// Chat completion service interface.
/// </summary>
public interface IChatCompletionService
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Generates a chat completion.
    /// </summary>
    Task<ChatCompletionResult> CompleteAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a streaming chat completion.
    /// </summary>
    IAsyncEnumerable<ChatCompletionChunk> StreamAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

### 4.7 Agent/Graph Abstractions

```csharp
namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// Base state for agent graphs.
/// </summary>
public abstract class AgentState
{
    public List<ChatMessage> Messages { get; init; } = new();
    public Dictionary<string, object?> Values { get; init; } = new();
    public string? CurrentNode { get; set; }
    public int StepCount { get; set; }
}

/// <summary>
/// A node in the agent graph.
/// </summary>
public interface IGraphNode<TState> where TState : AgentState
{
    /// <summary>
    /// Gets the unique node name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the node logic.
    /// </summary>
    Task<TState> ExecuteAsync(
        TState state,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Edge routing decision.
/// </summary>
public sealed record EdgeDecision
{
    public required string TargetNode { get; init; }
    public static readonly EdgeDecision End = new() { TargetNode = "__end__" };
}

/// <summary>
/// Conditional edge in the graph.
/// </summary>
public interface IConditionalEdge<TState> where TState : AgentState
{
    /// <summary>
    /// Determines the next node based on state.
    /// </summary>
    Task<EdgeDecision> DecideAsync(
        TState state,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Graph builder for agent workflows.
/// </summary>
public interface IGraphBuilder<TState> where TState : AgentState, new()
{
    IGraphBuilder<TState> AddNode(string name, Func<TState, CancellationToken, Task<TState>> action);
    IGraphBuilder<TState> AddNode(IGraphNode<TState> node);
    IGraphBuilder<TState> AddEdge(string from, string to);
    IGraphBuilder<TState> AddConditionalEdge(string from, Func<TState, EdgeDecision> condition);
    IGraphBuilder<TState> AddConditionalEdge(string from, IConditionalEdge<TState> edge);
    IGraphBuilder<TState> SetEntryPoint(string nodeName);
    ICompiledGraph<TState> Compile();
}

/// <summary>
/// Compiled executable graph.
/// </summary>
public interface ICompiledGraph<TState> where TState : AgentState
{
    /// <summary>
    /// Executes the graph to completion.
    /// </summary>
    Task<TState> InvokeAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams execution state after each node.
    /// </summary>
    IAsyncEnumerable<GraphEvent<TState>> StreamAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Event emitted during graph execution.
/// </summary>
public sealed record GraphEvent<TState> where TState : AgentState
{
    public required string NodeName { get; init; }
    public required TState State { get; init; }
    public required GraphEventType EventType { get; init; }
    public TimeSpan Duration { get; init; }
}

public enum GraphEventType
{
    NodeStarted,
    NodeCompleted,
    EdgeTraversed,
    GraphCompleted,
    Error
}

/// <summary>
/// Options for graph execution.
/// </summary>
public sealed record GraphExecutionOptions
{
    public int MaxSteps { get; init; } = 100;
    public TimeSpan? Timeout { get; init; }
    public bool EnableCheckpointing { get; init; } = false;
    public string? CheckpointId { get; init; }
}
```

### 4.8 Tool System

```csharp
namespace DotLangChain.Abstractions.Agents.Tools;

/// <summary>
/// Attribute for marking tool methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ToolAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Attribute for tool parameter descriptions.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ToolParameterAttribute : Attribute
{
    public string? Description { get; set; }
    public bool Required { get; set; } = true;
}

/// <summary>
/// Result from tool execution.
/// </summary>
public sealed record ToolResult
{
    public required string ToolCallId { get; init; }
    public required string Content { get; init; }
    public bool IsError { get; init; }
}

/// <summary>
/// Executes tools based on LLM tool calls.
/// </summary>
public interface IToolExecutor
{
    /// <summary>
    /// Gets available tool definitions.
    /// </summary>
    IReadOnlyList<ToolDefinition> GetToolDefinitions();

    /// <summary>
    /// Executes a tool call.
    /// </summary>
    Task<ToolResult> ExecuteAsync(
        ToolCall toolCall,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Registry for tool implementations.
/// </summary>
public interface IToolRegistry
{
    void Register<T>() where T : class;
    void Register(object toolInstance);
    IToolExecutor BuildExecutor();
}
```

---

## 5. Core Implementations

### 5.1 Recursive Text Splitter

```csharp
namespace DotLangChain.Core.Documents.Splitters;

public sealed class RecursiveCharacterTextSplitter : ITextSplitter
{
    private readonly string[] _separators;
    private readonly ILogger<RecursiveCharacterTextSplitter> _logger;

    private static readonly string[] DefaultSeparators = ["\n\n", "\n", ". ", " ", ""];

    public RecursiveCharacterTextSplitter(
        ILogger<RecursiveCharacterTextSplitter> logger,
        string[]? separators = null)
    {
        _logger = logger;
        _separators = separators ?? DefaultSeparators;
    }

    public async IAsyncEnumerable<DocumentChunk> SplitAsync(
        Document document,
        TextSplitterOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new TextSplitterOptions();
        var chunkIndex = 0;
        var charOffset = 0;

        await foreach (var chunkText in SplitTextAsync(document.Content, options, cancellationToken))
        {
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

            charOffset += chunkText.Length - options.ChunkOverlap;
            chunkIndex++;
        }
    }

    public async IAsyncEnumerable<string> SplitTextAsync(
        string text,
        TextSplitterOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new TextSplitterOptions();

        foreach (var chunk in SplitTextRecursive(text, _separators, options))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return options.StripWhitespace ? chunk.Trim() : chunk;
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
            : text.Split(separator).ToList();

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
```

### 5.2 Graph Execution Engine

```csharp
namespace DotLangChain.Core.Agents.Graph;

public sealed class GraphBuilder<TState> : IGraphBuilder<TState>
    where TState : AgentState, new()
{
    private readonly Dictionary<string, Func<TState, CancellationToken, Task<TState>>> _nodes = new();
    private readonly Dictionary<string, List<string>> _edges = new();
    private readonly Dictionary<string, Func<TState, EdgeDecision>> _conditionalEdges = new();
    private string? _entryPoint;

    public IGraphBuilder<TState> AddNode(
        string name,
        Func<TState, CancellationToken, Task<TState>> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);

        if (!_nodes.TryAdd(name, action))
            throw new InvalidOperationException($"Node '{name}' already exists");

        return this;
    }

    public IGraphBuilder<TState> AddNode(IGraphNode<TState> node)
    {
        return AddNode(node.Name, node.ExecuteAsync);
    }

    public IGraphBuilder<TState> AddEdge(string from, string to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        ArgumentException.ThrowIfNullOrWhiteSpace(to);

        if (!_edges.TryGetValue(from, out var targets))
        {
            targets = new List<string>();
            _edges[from] = targets;
        }
        targets.Add(to);

        return this;
    }

    public IGraphBuilder<TState> AddConditionalEdge(
        string from,
        Func<TState, EdgeDecision> condition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        ArgumentNullException.ThrowIfNull(condition);

        _conditionalEdges[from] = condition;
        return this;
    }

    public IGraphBuilder<TState> AddConditionalEdge(
        string from,
        IConditionalEdge<TState> edge)
    {
        return AddConditionalEdge(from, state =>
        {
            return edge.DecideAsync(state, CancellationToken.None).GetAwaiter().GetResult();
        });
    }

    public IGraphBuilder<TState> SetEntryPoint(string nodeName)
    {
        _entryPoint = nodeName;
        return this;
    }

    public ICompiledGraph<TState> Compile()
    {
        if (_entryPoint is null)
            throw new InvalidOperationException("Entry point not set");

        if (!_nodes.ContainsKey(_entryPoint))
            throw new InvalidOperationException($"Entry point '{_entryPoint}' not found in nodes");

        return new CompiledGraph<TState>(
            _nodes.ToFrozenDictionary(),
            _edges.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value.ToFrozenSet()),
            _conditionalEdges.ToFrozenDictionary(),
            _entryPoint);
    }
}

internal sealed class CompiledGraph<TState> : ICompiledGraph<TState>
    where TState : AgentState
{
    private readonly FrozenDictionary<string, Func<TState, CancellationToken, Task<TState>>> _nodes;
    private readonly FrozenDictionary<string, FrozenSet<string>> _edges;
    private readonly FrozenDictionary<string, Func<TState, EdgeDecision>> _conditionalEdges;
    private readonly string _entryPoint;
    private static readonly ActivitySource ActivitySource = new("DotLangChain.Graph");

    public CompiledGraph(
        FrozenDictionary<string, Func<TState, CancellationToken, Task<TState>>> nodes,
        FrozenDictionary<string, FrozenSet<string>> edges,
        FrozenDictionary<string, Func<TState, EdgeDecision>> conditionalEdges,
        string entryPoint)
    {
        _nodes = nodes;
        _edges = edges;
        _conditionalEdges = conditionalEdges;
        _entryPoint = entryPoint;
    }

    public async Task<TState> InvokeAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        TState? finalState = null;

        await foreach (var evt in StreamAsync(initialState, options, cancellationToken))
        {
            if (evt.EventType == GraphEventType.GraphCompleted)
                finalState = evt.State;
        }

        return finalState ?? throw new InvalidOperationException("Graph did not complete");
    }

    public async IAsyncEnumerable<GraphEvent<TState>> StreamAsync(
        TState initialState,
        GraphExecutionOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new GraphExecutionOptions();
        var state = initialState;
        var currentNode = _entryPoint;
        var stepCount = 0;

        using var cts = options.Timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (cts is not null)
            cts.CancelAfter(options.Timeout!.Value);

        var token = cts?.Token ?? cancellationToken;

        while (currentNode != "__end__" && stepCount < options.MaxSteps)
        {
            token.ThrowIfCancellationRequested();

            using var activity = ActivitySource.StartActivity($"Node:{currentNode}");
            var stopwatch = Stopwatch.StartNew();

            yield return new GraphEvent<TState>
            {
                NodeName = currentNode,
                State = state,
                EventType = GraphEventType.NodeStarted
            };

            if (!_nodes.TryGetValue(currentNode, out var nodeFunc))
                throw new InvalidOperationException($"Node '{currentNode}' not found");

            state = await nodeFunc(state, token);
            state.CurrentNode = currentNode;
            state.StepCount = ++stepCount;

            stopwatch.Stop();

            yield return new GraphEvent<TState>
            {
                NodeName = currentNode,
                State = state,
                EventType = GraphEventType.NodeCompleted,
                Duration = stopwatch.Elapsed
            };

            var nextNode = DetermineNextNode(currentNode, state);

            yield return new GraphEvent<TState>
            {
                NodeName = currentNode,
                State = state,
                EventType = GraphEventType.EdgeTraversed
            };

            currentNode = nextNode;
        }

        yield return new GraphEvent<TState>
        {
            NodeName = currentNode,
            State = state,
            EventType = GraphEventType.GraphCompleted
        };
    }

    private string DetermineNextNode(string currentNode, TState state)
    {
        if (_conditionalEdges.TryGetValue(currentNode, out var condition))
        {
            var decision = condition(state);
            return decision.TargetNode;
        }

        if (_edges.TryGetValue(currentNode, out var targets) && targets.Count > 0)
        {
            return targets.First();
        }

        return "__end__";
    }
}
```

---

## 6. Security Implementation

### 6.1 Input Sanitization

```csharp
namespace DotLangChain.Core.Security;

/// <summary>
/// Sanitizes user input to prevent prompt injection.
/// </summary>
public interface IInputSanitizer
{
    string Sanitize(string input, SanitizationLevel level = SanitizationLevel.Standard);
    bool ContainsPotentialInjection(string input);
}

public enum SanitizationLevel
{
    Minimal,
    Standard,
    Strict
}

public sealed class DefaultInputSanitizer : IInputSanitizer
{
    private static readonly Regex InjectionPatterns = new(
        @"(ignore\s+(previous|all|above)\s+instructions)|" +
        @"(system\s*:)|" +
        @"(you\s+are\s+now)|" +
        @"(pretend\s+to\s+be)|" +
        @"(disregard\s+(your|all))|" +
        @"(\[INST\])|(\[/INST\])|" +
        @"(<\|im_start\|>)|(<\|im_end\|>)|" +
        @"(###\s*(System|User|Assistant))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ControlCharacters = new(
        @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]",
        RegexOptions.Compiled);

    public string Sanitize(string input, SanitizationLevel level = SanitizationLevel.Standard)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = input;

        result = ControlCharacters.Replace(result, "");

        if (level >= SanitizationLevel.Standard)
        {
            result = NormalizeUnicode(result);
            result = EscapeInjectionMarkers(result);
        }

        if (level == SanitizationLevel.Strict)
        {
            result = InjectionPatterns.Replace(result, "[FILTERED]");
        }

        return result;
    }

    public bool ContainsPotentialInjection(string input)
    {
        return InjectionPatterns.IsMatch(input);
    }

    private static string NormalizeUnicode(string input)
    {
        return input.Normalize(NormalizationForm.FormKC);
    }

    private static string EscapeInjectionMarkers(string input)
    {
        return input
            .Replace("```", "` ` `")
            .Replace("---", "- - -")
            .Replace("###", "# # #");
    }
}
```

### 6.2 Secrets Management

```csharp
namespace DotLangChain.Core.Security;

/// <summary>
/// Secure handling of API keys and secrets.
/// </summary>
public interface ISecretProvider
{
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);
    Task<T?> GetSecretAsync<T>(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration-based secret provider with validation.
/// </summary>
public sealed class ConfigurationSecretProvider : ISecretProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationSecretProvider> _logger;

    public ConfigurationSecretProvider(
        IConfiguration configuration,
        ILogger<ConfigurationSecretProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = _configuration[key];

        if (string.IsNullOrEmpty(value))
        {
            _logger.LogWarning("Secret not found: {Key}", key);
            return Task.FromResult<string?>(null);
        }

        _logger.LogDebug("Secret retrieved: {Key}", key);

        return Task.FromResult<string?>(value);
    }

    public Task<T?> GetSecretAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection(key);

        if (!section.Exists())
        {
            return Task.FromResult<T?>(default);
        }

        return Task.FromResult(section.Get<T>());
    }
}
```

### 6.3 Audit Logging

```csharp
namespace DotLangChain.Core.Security;

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

public interface IAuditLogger
{
    void Log(AuditEvent auditEvent);
    Task LogAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}

public sealed class StructuredAuditLogger : IAuditLogger
{
    private readonly ILogger<StructuredAuditLogger> _logger;
    private readonly IRedactor _redactor;

    public StructuredAuditLogger(
        ILogger<StructuredAuditLogger> logger,
        IRedactor redactor)
    {
        _logger = logger;
        _redactor = redactor;
    }

    public void Log(AuditEvent auditEvent)
    {
        var redactedProperties = _redactor.RedactProperties(auditEvent.Properties);

        _logger.LogInformation(
            "Audit: {EventType} | {Action} | User: {UserId} | Resource: {ResourceId} | Success: {Success} | Properties: {Properties}",
            auditEvent.EventType,
            auditEvent.Action,
            auditEvent.UserId ?? "anonymous",
            auditEvent.ResourceId ?? "none",
            auditEvent.Success,
            JsonSerializer.Serialize(redactedProperties));
    }

    public Task LogAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        Log(auditEvent);
        return Task.CompletedTask;
    }
}

public interface IRedactor
{
    Dictionary<string, object?> RedactProperties(Dictionary<string, object?> properties);
    string Redact(string value, string pattern);
}
```

---

## 7. Resilience Patterns

### 7.1 Retry Policies

```csharp
namespace DotLangChain.Core.Resilience;

public static class ResiliencePolicies
{
    public static ResiliencePipeline<T> CreateStandardPipeline<T>(
        string pipelineName,
        ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .Handle<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry {AttemptNumber} for {Pipeline} after {Delay}ms due to {Exception}",
                        args.AttemptNumber,
                        pipelineName,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "unknown");
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    logger.LogError(
                        "Circuit breaker opened for {Pipeline} for {Duration}s",
                        pipelineName,
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Circuit breaker closed for {Pipeline}", pipelineName);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(60))
            .Build();
    }

    public static ResiliencePipeline<HttpResponseMessage> CreateHttpPipeline(
        ILogger logger,
        string serviceName)
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response =>
                        response.StatusCode == HttpStatusCode.TooManyRequests ||
                        response.StatusCode >= HttpStatusCode.InternalServerError),
                DelayGenerator = args =>
                {
                    if (args.Outcome.Result?.Headers.RetryAfter?.Delta is { } delta)
                    {
                        return ValueTask.FromResult<TimeSpan?>(delta);
                    }
                    return ValueTask.FromResult<TimeSpan?>(null);
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();
    }
}
```

---

## 8. Observability

### 8.1 OpenTelemetry Integration

```csharp
namespace DotLangChain.Extensions.Observability;

public static class OpenTelemetryExtensions
{
    public const string ServiceName = "DotLangChain";
    public const string ServiceVersion = "1.0.0";

    public static IServiceCollection AddDotLangChainTelemetry(
        this IServiceCollection services,
        Action<DotLangChainTelemetryOptions>? configure = null)
    {
        var options = new DotLangChainTelemetryOptions();
        configure?.Invoke(options);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(ServiceName, serviceVersion: ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("DotLangChain.Documents")
                    .AddSource("DotLangChain.Embeddings")
                    .AddSource("DotLangChain.VectorStore")
                    .AddSource("DotLangChain.LLM")
                    .AddSource("DotLangChain.Graph")
                    .AddHttpClientInstrumentation();

                if (options.EnableConsoleExporter)
                    tracing.AddConsoleExporter();

                if (options.OtlpEndpoint is not null)
                    tracing.AddOtlpExporter(o => o.Endpoint = options.OtlpEndpoint);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter("DotLangChain.Documents")
                    .AddMeter("DotLangChain.Embeddings")
                    .AddMeter("DotLangChain.VectorStore")
                    .AddMeter("DotLangChain.LLM")
                    .AddMeter("DotLangChain.Graph")
                    .AddHttpClientInstrumentation();

                if (options.EnableConsoleExporter)
                    metrics.AddConsoleExporter();

                if (options.OtlpEndpoint is not null)
                    metrics.AddOtlpExporter(o => o.Endpoint = options.OtlpEndpoint);
            });

        return services;
    }
}

public sealed class DotLangChainTelemetryOptions
{
    public Uri? OtlpEndpoint { get; set; }
    public bool EnableConsoleExporter { get; set; }
}
```

### 8.2 Metrics Instrumentation

```csharp
namespace DotLangChain.Core.Observability;

public static class DotLangChainMetrics
{
    private static readonly Meter Meter = new("DotLangChain.LLM", "1.0.0");

    public static readonly Counter<long> TokensConsumed = Meter.CreateCounter<long>(
        "dotlangchain.llm.tokens.consumed",
        "tokens",
        "Total tokens consumed by LLM calls");

    public static readonly Counter<long> CompletionsTotal = Meter.CreateCounter<long>(
        "dotlangchain.llm.completions.total",
        "completions",
        "Total number of LLM completions");

    public static readonly Histogram<double> CompletionDuration = Meter.CreateHistogram<double>(
        "dotlangchain.llm.completion.duration",
        "ms",
        "Duration of LLM completion requests");

    public static readonly Counter<long> EmbeddingsGenerated = Meter.CreateCounter<long>(
        "dotlangchain.embeddings.generated",
        "embeddings",
        "Total embeddings generated");

    public static readonly Counter<long> DocumentsProcessed = Meter.CreateCounter<long>(
        "dotlangchain.documents.processed",
        "documents",
        "Total documents processed");

    public static readonly Counter<long> GraphNodesExecuted = Meter.CreateCounter<long>(
        "dotlangchain.graph.nodes.executed",
        "nodes",
        "Total graph nodes executed");

    public static readonly Histogram<double> GraphExecutionDuration = Meter.CreateHistogram<double>(
        "dotlangchain.graph.execution.duration",
        "ms",
        "Duration of complete graph executions");
}
```

---

## 9. Dependency Injection

### 9.1 Service Registration

```csharp
namespace DotLangChain.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDotLangChain(
        this IServiceCollection services,
        Action<DotLangChainBuilder> configure)
    {
        var builder = new DotLangChainBuilder(services);
        configure(builder);
        return services;
    }
}

public sealed class DotLangChainBuilder
{
    private readonly IServiceCollection _services;

    public DotLangChainBuilder(IServiceCollection services)
    {
        _services = services;

        services.AddSingleton<IInputSanitizer, DefaultInputSanitizer>();
        services.AddSingleton<IDocumentLoaderRegistry, DocumentLoaderRegistry>();
        services.AddSingleton(typeof(IGraphBuilder<>), typeof(GraphBuilder<>));
    }

    public DotLangChainBuilder AddDocumentLoaders(Action<DocumentLoaderBuilder>? configure = null)
    {
        var builder = new DocumentLoaderBuilder(_services);
        configure?.Invoke(builder);
        return this;
    }

    public DotLangChainBuilder AddOpenAI(Action<OpenAIOptions> configure)
    {
        _services.Configure(configure);
        _services.AddSingleton<IChatCompletionService, OpenAIChatCompletionService>();
        _services.AddSingleton<IEmbeddingService, OpenAIEmbeddingService>();
        return this;
    }

    public DotLangChainBuilder AddOllama(Action<OllamaOptions> configure)
    {
        _services.Configure(configure);
        _services.AddSingleton<IChatCompletionService, OllamaChatCompletionService>();
        _services.AddSingleton<IEmbeddingService, OllamaEmbeddingService>();
        return this;
    }

    public DotLangChainBuilder AddAnthropic(Action<AnthropicOptions> configure)
    {
        _services.Configure(configure);
        _services.AddSingleton<IChatCompletionService, AnthropicChatCompletionService>();
        return this;
    }

    public DotLangChainBuilder AddQdrant(Action<QdrantOptions> configure)
    {
        _services.Configure(configure);
        _services.AddSingleton<IVectorStoreFactory, QdrantVectorStoreFactory>();
        return this;
    }

    public DotLangChainBuilder AddPgVector(Action<PgVectorOptions> configure)
    {
        _services.Configure(configure);
        _services.AddSingleton<IVectorStoreFactory, PgVectorStoreFactory>();
        return this;
    }

    public DotLangChainBuilder AddInMemoryVectorStore()
    {
        _services.AddSingleton<IVectorStoreFactory, InMemoryVectorStoreFactory>();
        return this;
    }
}
```

### 9.2 Usage Example

```csharp
// Program.cs or Startup.cs
services.AddDotLangChain(builder =>
{
    builder
        .AddDocumentLoaders(docs =>
        {
            docs.AddPdf();
            docs.AddDocx();
            docs.AddMarkdown();
        })
        .AddOpenAI(options =>
        {
            options.ApiKey = configuration["OpenAI:ApiKey"]!;
            options.DefaultModel = "gpt-4o";
            options.EmbeddingModel = "text-embedding-3-small";
        })
        .AddQdrant(options =>
        {
            options.Host = "localhost";
            options.Port = 6333;
            options.ApiKey = configuration["Qdrant:ApiKey"];
        });
});

services.AddDotLangChainTelemetry(options =>
{
    options.OtlpEndpoint = new Uri("http://localhost:4317");
});
```

---

## 10. Performance Optimization

### 10.1 Object Pooling

```csharp
namespace DotLangChain.Core.Performance;

/// <summary>
/// Pooled StringBuilder for reduced allocations.
/// </summary>
public static class StringBuilderPool
{
    private static readonly ObjectPool<StringBuilder> Pool =
        new DefaultObjectPoolProvider().CreateStringBuilderPool(
            initialCapacity: 256,
            maximumRetainedCapacity: 4096);

    public static StringBuilder Rent() => Pool.Get();

    public static void Return(StringBuilder sb)
    {
        sb.Clear();
        Pool.Return(sb);
    }
}

/// <summary>
/// Pooled buffers for embedding operations.
/// </summary>
public static class EmbeddingBufferPool
{
    private static readonly ArrayPool<float> Pool = ArrayPool<float>.Shared;

    public static float[] Rent(int dimensions) => Pool.Rent(dimensions);

    public static void Return(float[] buffer) => Pool.Return(buffer, clearArray: false);
}
```

### 10.2 Streaming Optimizations

```csharp
namespace DotLangChain.Core.Performance;

/// <summary>
/// High-performance streaming response accumulator.
/// </summary>
public sealed class StreamingAccumulator : IDisposable
{
    private readonly Channel<ChatCompletionChunk> _channel;
    private readonly StringBuilder _contentBuilder;
    private readonly List<ToolCall> _toolCalls;
    private bool _disposed;

    public StreamingAccumulator()
    {
        _channel = Channel.CreateUnbounded<ChatCompletionChunk>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });
        _contentBuilder = StringBuilderPool.Rent();
        _toolCalls = new List<ToolCall>();
    }

    public ChannelWriter<ChatCompletionChunk> Writer => _channel.Writer;

    public async IAsyncEnumerable<ChatCompletionChunk> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var chunk in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            if (chunk.ContentDelta is not null)
                _contentBuilder.Append(chunk.ContentDelta);

            if (chunk.ToolCallDelta is not null)
                AccumulateToolCall(chunk.ToolCallDelta);

            yield return chunk;
        }
    }

    public ChatMessage ToMessage()
    {
        return new ChatMessage
        {
            Role = MessageRole.Assistant,
            Content = _contentBuilder.ToString(),
            ToolCalls = _toolCalls.Count > 0 ? _toolCalls : null
        };
    }

    private void AccumulateToolCall(ToolCall delta)
    {
        var existing = _toolCalls.FirstOrDefault(t => t.Id == delta.Id);
        if (existing is null)
        {
            _toolCalls.Add(delta);
        }
        else
        {
            var index = _toolCalls.IndexOf(existing);
            _toolCalls[index] = existing with
            {
                Arguments = existing.Arguments + delta.Arguments
            };
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        StringBuilderPool.Return(_contentBuilder);
        _disposed = true;
    }
}
```

---

## 11. Testing Strategy

### 11.1 Test Categories

| Category | Purpose | Tools |
|----------|---------|-------|
| Unit Tests | Isolated component testing | xUnit, NSubstitute |
| Integration Tests | Provider/store integration | TestContainers |
| Contract Tests | API compatibility | Verify |
| Performance Tests | Benchmarks | BenchmarkDotNet |
| Chaos Tests | Failure scenarios | Polly, Simmy |

### 11.2 Test Infrastructure

```csharp
namespace DotLangChain.Tests.Infrastructure;

/// <summary>
/// Fixture for integration tests with real services.
/// </summary>
public sealed class DotLangChainFixture : IAsyncLifetime
{
    private readonly IContainer _qdrantContainer;
    private readonly IContainer _postgresContainer;

    public DotLangChainFixture()
    {
        _qdrantContainer = new ContainerBuilder()
            .WithImage("qdrant/qdrant:v1.12.0")
            .WithPortBinding(6333, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6333))
            .Build();

        _postgresContainer = new ContainerBuilder()
            .WithImage("pgvector/pgvector:pg16")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_PASSWORD", "test")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public string QdrantHost => _qdrantContainer.Hostname;
    public int QdrantPort => _qdrantContainer.GetMappedPublicPort(6333);

    public string PostgresConnectionString =>
        $"Host={_postgresContainer.Hostname};" +
        $"Port={_postgresContainer.GetMappedPublicPort(5432)};" +
        "Database=test;Username=postgres;Password=test";

    public async Task InitializeAsync()
    {
        await _qdrantContainer.StartAsync();
        await _postgresContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _qdrantContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}
```

---

## 12. Deployment Considerations

### 12.1 Docker Support

```dockerfile
# Multi-stage build for minimal image
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app .

# Non-root user
RUN adduser -D -u 1000 appuser
USER appuser

ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### 12.2 Native AOT Considerations

```xml
<!-- For AOT-compatible projects -->
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <TrimMode>full</TrimMode>
  <JsonSerializerIsReflectionEnabledByDefault>false</JsonSerializerIsReflectionEnabledByDefault>
</PropertyGroup>

<!-- Source generators for JSON -->
<ItemGroup>
  <PackageReference Include="System.Text.Json" Version="9.0.0" />
</ItemGroup>
```

```csharp
// JSON source generation for AOT
[JsonSerializable(typeof(ChatMessage))]
[JsonSerializable(typeof(ChatCompletionResult))]
[JsonSerializable(typeof(VectorRecord))]
internal partial class DotLangChainJsonContext : JsonSerializerContext { }
```

---

## 13. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-07 | - | Initial draft |

---

## 14. Appendices

### Appendix A: Error Codes

| Code | Category | Description |
|------|----------|-------------|
| DLC001 | Document | Failed to load document |
| DLC002 | Document | Unsupported format |
| DLC003 | Embedding | Provider unavailable |
| DLC004 | Embedding | Rate limit exceeded |
| DLC005 | VectorStore | Connection failed |
| DLC006 | VectorStore | Collection not found |
| DLC007 | LLM | Completion failed |
| DLC008 | LLM | Context length exceeded |
| DLC009 | Graph | Max steps exceeded |
| DLC010 | Graph | Node not found |
| DLC011 | Security | Potential injection detected |
| DLC012 | Security | Invalid credentials |

### Appendix B: Configuration Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "DotLangChain": {
      "type": "object",
      "properties": {
        "OpenAI": {
          "type": "object",
          "properties": {
            "ApiKey": { "type": "string" },
            "DefaultModel": { "type": "string", "default": "gpt-4o" },
            "EmbeddingModel": { "type": "string", "default": "text-embedding-3-small" },
            "BaseUrl": { "type": "string", "format": "uri" }
          },
          "required": ["ApiKey"]
        },
        "Anthropic": {
          "type": "object",
          "properties": {
            "ApiKey": { "type": "string" },
            "DefaultModel": { "type": "string", "default": "claude-sonnet-4-20250514" },
            "BaseUrl": { "type": "string", "format": "uri" }
          },
          "required": ["ApiKey"]
        },
        "Ollama": {
          "type": "object",
          "properties": {
            "BaseUrl": { "type": "string", "default": "http://localhost:11434" },
            "DefaultModel": { "type": "string" },
            "EmbeddingModel": { "type": "string" }
          }
        },
        "Qdrant": {
          "type": "object",
          "properties": {
            "Host": { "type": "string", "default": "localhost" },
            "Port": { "type": "integer", "default": 6333 },
            "ApiKey": { "type": "string" },
            "UseTls": { "type": "boolean", "default": false }
          }
        },
        "PgVector": {
          "type": "object",
          "properties": {
            "ConnectionString": { "type": "string" },
            "Schema": { "type": "string", "default": "public" }
          },
          "required": ["ConnectionString"]
        },
        "Telemetry": {
          "type": "object",
          "properties": {
            "OtlpEndpoint": { "type": "string", "format": "uri" },
            "EnableConsoleExporter": { "type": "boolean", "default": false }
          }
        }
      }
    }
  }
}
```

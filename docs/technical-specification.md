# DotNetAgents Library - Technical Specification

**Version:** 1.0  
**Date:** 2024  
**Status:** Active Development  
**Target Framework:** .NET 10 (LTS)

## 1. Introduction

### 1.1 Purpose
This document provides the technical specification for the DotNetAgents library, detailing the architecture, design patterns, implementation details, and technical decisions.

### 1.2 Scope
This specification covers the technical design of all components, interfaces, data structures, and integration points for the DotNetAgents library.

### 1.3 Architecture Overview

The library follows a layered, modular architecture:

```
┌─────────────────────────────────────────────────────────┐
│              Application Layer (User Code)               │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│         Workflow Engine (LangGraph-like)                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ StateGraph   │  │ Checkpoints  │  │ Human-in-Loop│  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│         Chain & Runnable Layer (LangChain-like)         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ IRunnable    │  │ ChainBuilder  │  │ Composition   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│              Core Abstractions Layer                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ ILLMModel    │  │ IPrompt      │  │ ITool        │  │
│  │ IEmbedding   │  │ IVectorStore │  │ IMemory      │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│            Integrations & Infrastructure                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ LLM Providers│  │ Vector DBs   │  │ Observability│  │
│  │ (OpenAI, etc)│  │ (Pinecone)   │  │ (OTel, Logs) │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## 2. Technology Stack

### 2.1 Core Technologies
- **.NET 8** - Target framework (LTS)
- **C# 12** - Latest language features
- **Nullable Reference Types** - Enabled throughout

### 2.2 Key Libraries
- **Microsoft.Extensions.DependencyInjection** - Dependency injection
- **Microsoft.Extensions.Logging** - Logging abstraction
- **Microsoft.Extensions.Http** - HTTP client factory
- **System.Text.Json** - JSON serialization
- **System.Threading.Channels** - Producer/consumer patterns for streaming
- **OpenTelemetry** - Distributed tracing and metrics

### 2.3 Testing Frameworks
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **Microsoft.NET.Test.Sdk** - Test SDK

## 3. Package Architecture (Hybrid Approach)

### 3.1 Package Strategy

The library uses a **hybrid modular approach**: code is structured as separate projects from day one, but initially published as a metapackage for simplicity. This allows:
- **Simple installation**: Users can install `DotNetAgents` (metapackage) for everything
- **Granular control**: Advanced users can reference individual packages
- **Easy splitting**: Can split into separate packages later without refactoring
- **Dependency isolation**: Core has zero dependencies on integrations

### 3.2 Package Structure

```
NuGet Packages:
├── DotNetAgents.Core (v1.0.0)
│   ├── Core abstractions only
│   ├── Zero dependencies on integrations
│   ├── ~500KB package size
│   └── Dependencies: Microsoft.Extensions.* (minimal)
│
├── DotNetAgents.Workflow (v1.0.0)
│   ├── Depends on: DotNetAgents.Core
│   └── ~200KB package size
│
├── DotNetAgents.Providers.OpenAI (v1.0.0)
│   ├── Depends on: DotNetAgents.Core
│   └── ~100KB package size
│
├── DotNetAgents.Providers.Azure (v1.0.0)
│   ├── Depends on: DotNetAgents.Core
│   └── ~100KB package size
│
├── DotNetAgents.Providers.Anthropic (v1.0.0)
│   ├── Depends on: DotNetAgents.Core
│   └── ~100KB package size
│
├── DotNetAgents.VectorStores.Pinecone (v1.0.0)
│   ├── Depends on: DotNetAgents.Core
│   └── ~150KB package size
│
├── DotNetAgents.VectorStores.InMemory (v1.0.0)
│   ├── Depends on: DotNetAgents.Core
│   └── ~50KB package size
│
├── DotNetAgents.Configuration (v1.0.0)
│   ├── Depends on: DotNetAgents.Core
│   └── Configuration management
│
├── DotNetAgents.Observability (v1.0.0)
│   ├── Depends on: DotNetAgents.Core
│   └── Logging, tracing, metrics
│
└── DotNetAgents (v1.0.0) [METAPACKAGE]
    ├── References all above packages
    ├── Convenience package for "install everything"
    └── ~50KB (metadata only)
```

### 3.3 Project Structure

```
DotNetAgents/
├── src/
│   ├── DotNetAgents.Core/              # Core abstractions (NO integration deps)
│   │   ├── Models/
│   │   │   ├── ILLMModel.cs
│   │   │   ├── IEmbeddingModel.cs
│   │   │   └── ModelOptions.cs
│   │   ├── Prompts/
│   │   │   ├── IPromptTemplate.cs
│   │   │   ├── PromptTemplate.cs
│   │   │   └── PromptBuilder.cs
│   │   ├── Tools/
│   │   │   ├── ITool.cs
│   │   │   ├── ToolRegistry.cs
│   │   │   └── ToolSchema.cs
│   │   ├── Memory/
│   │   │   ├── IMemory.cs
│   │   │   ├── IMemoryStore.cs
│   │   │   └── MemoryMessage.cs
│   │   ├── Documents/
│   │   │   ├── IDocument.cs
│   │   │   ├── IDocumentLoader.cs
│   │   │   └── DocumentChunker.cs
│   │   ├── Retrieval/
│   │   │   ├── IVectorStore.cs
│   │   │   ├── IRetriever.cs
│   │   │   └── RetrievalResult.cs
│   │   ├── Chains/
│   │   │   ├── IRunnable.cs
│   │   │   ├── Chain.cs
│   │   │   └── ChainBuilder.cs
│   │   ├── Configuration/
│   │   │   ├── IAgentConfiguration.cs
│   │   │   └── ConfigurationExtensions.cs
│   │   └── Execution/
│   │       ├── ExecutionContext.cs
│   │       └── IExecutionContext.cs
│   │
│   ├── DotNetAgents.Workflow/           # Workflow engine
│   │   ├── Graph/
│   │   │   ├── StateGraph.cs
│   │   │   ├── GraphNode.cs
│   │   │   └── GraphEdge.cs
│   │   ├── Execution/
│   │   │   ├── GraphExecutor.cs
│   │   │   ├── StateManager.cs
│   │   │   └── ExecutionContext.cs
│   │   ├── Checkpoints/
│   │   │   ├── ICheckpointStore.cs
│   │   │   ├── Checkpoint.cs
│   │   │   └── CheckpointManager.cs
│   │   └── HumanInLoop/
│   │       ├── HumanApprovalNode.cs
│   │       └── ApprovalCallback.cs
│   │
│   ├── DotNetAgents.Providers.OpenAI/    # OpenAI integration
│   │   ├── OpenAIModel.cs
│   │   ├── OpenAIClient.cs
│   │   ├── OpenAIStreamingHandler.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── DotNetAgents.Providers.Azure/     # Azure OpenAI integration
│   │   ├── AzureOpenAIModel.cs
│   │   ├── AzureOpenAIClient.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── DotNetAgents.Providers.Anthropic/ # Anthropic integration
│   │   ├── AnthropicModel.cs
│   │   ├── AnthropicClient.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── DotNetAgents.VectorStores.Pinecone/ # Pinecone integration
│   │   └── PineconeStore.cs
│   │
│   ├── DotNetAgents.VectorStores.InMemory/ # In-memory vector store
│   │   └── InMemoryVectorStore.cs
│   │
│   ├── DotNetAgents.Configuration/        # Configuration management
│   │   ├── AgentConfiguration.cs
│   │   ├── ConfigurationBuilder.cs
│   │   └── ConfigurationValidators.cs
│   │
│   ├── DotNetAgents.Observability/        # Observability
│   │   ├── Logging/
│   │   │   └── LoggerExtensions.cs
│   │   ├── Tracing/
│   │   │   └── TraceExtensions.cs
│   │   ├── Metrics/
│   │   │   ├── MetricsCollector.cs
│   │   │   └── CostTracker.cs
│   │   └── HealthChecks/
│   │       └── AgentHealthCheck.cs
│   │
│   └── DotNetAgents/                      # Metapackage (references all)
│       └── ServiceCollectionExtensions.cs
│
├── tests/
│   ├── DotNetAgents.Core.Tests/
│   ├── DotNetAgents.Workflow.Tests/
│   ├── DotNetAgents.Integrations.Tests/
│   └── DotNetAgents.Tests.Integration/
│
├── samples/
│   ├── SimpleChain/
│   ├── RAGExample/
│   ├── AgentExample/
│   └── WorkflowExample/
│
└── docs/
    ├── requirements.md
    ├── technical-specification.md
    └── architecture/
```

## 4. Core Interfaces and Abstractions

### 4.1 LLM Model Interface

```csharp
public interface ILLMModel<TInput, TOutput>
{
    Task<TOutput> GenerateAsync(
        TInput input, 
        LLMOptions? options = null, 
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<TOutput> GenerateStreamAsync(
        TInput input, 
        LLMOptions? options = null, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<TOutput>> GenerateBatchAsync(
        IEnumerable<TInput> inputs, 
        LLMOptions? options = null, 
        CancellationToken cancellationToken = default);
    
    string ModelName { get; }
    int MaxTokens { get; }
}
```

**Design Decisions:**
- Generic interface allows type-safe inputs/outputs
- Async/await for non-blocking operations
- `IAsyncEnumerable` for streaming support
- Batch processing for efficiency
- Cancellation token support throughout

### 4.2 Embedding Model Interface

```csharp
public interface IEmbeddingModel
{
    Task<float[]> EmbedAsync(
        string text, 
        CancellationToken cancellationToken = default);
    
    Task<float[][]> EmbedBatchAsync(
        IEnumerable<string> texts, 
        CancellationToken cancellationToken = default);
    
    int Dimension { get; }
    string ModelName { get; }
}
```

### 4.3 Prompt Template Interface

```csharp
public interface IPromptTemplate
{
    Task<string> FormatAsync(
        IDictionary<string, object> variables, 
        CancellationToken cancellationToken = default);
    
    IReadOnlySet<string> Variables { get; }
    string Template { get; }
}
```

**Implementation Details:**
- Variable substitution using `{variable}` syntax
- Support for nested templates
- Template compilation and caching
- Validation of required variables

### 4.4 Runnable Interface

```csharp
public interface IRunnable<TInput, TOutput>
{
    Task<TOutput> InvokeAsync(
        TInput input, 
        RunnableOptions? options = null, 
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<TOutput> StreamAsync(
        TInput input, 
        RunnableOptions? options = null, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<TOutput>> BatchAsync(
        IEnumerable<TInput> inputs, 
        RunnableOptions? options = null, 
        CancellationToken cancellationToken = default);
}
```

**Chain Composition:**
```csharp
public static class RunnableExtensions
{
    public static IRunnable<TInput, TFinalOutput> Pipe<TInput, TMiddle, TFinalOutput>(
        this IRunnable<TInput, TMiddle> first,
        IRunnable<TMiddle, TFinalOutput> second);
    
    public static IRunnable<TInput, TOutput> Map<TInput, TOutput>(
        this IRunnable<TInput, TOutput> runnable,
        Func<TOutput, TOutput> mapper);
}
```

### 4.5 Tool Interface

```csharp
public interface ITool
{
    string Name { get; }
    string Description { get; }
    JsonSchema InputSchema { get; }
    
    Task<ToolResult> ExecuteAsync(
        object input, 
        CancellationToken cancellationToken = default);
}
```

**Tool Registry:**
```csharp
public interface IToolRegistry
{
    void Register(ITool tool);
    ITool? GetTool(string name);
    IReadOnlyList<ITool> GetAllTools();
}
```

### 4.6 Vector Store Interface

```csharp
public interface IVectorStore
{
    Task<string> UpsertAsync(
        string id, 
        float[] vector, 
        IDictionary<string, object>? metadata = null, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryVector, 
        int topK = 10, 
        IDictionary<string, object>? filter = null, 
        CancellationToken cancellationToken = default);
    
    Task DeleteAsync(
        IEnumerable<string> ids, 
        CancellationToken cancellationToken = default);
}
```

### 4.7 Memory Interface

```csharp
public interface IMemory
{
    Task AddMessageAsync(
        MemoryMessage message, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MemoryMessage>> GetMessagesAsync(
        int count = 10, 
        CancellationToken cancellationToken = default);
    
    Task ClearAsync(CancellationToken cancellationToken = default);
}

public interface IMemoryStore : IMemory
{
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task LoadAsync(CancellationToken cancellationToken = default);
}
```

### 4.8 Execution Context Interface

```csharp
public class ExecutionContext
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public CancellationToken CancellationToken { get; set; }
    public ILogger? Logger { get; set; }
    public Activity? Activity { get; set; }
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
}

public interface IExecutionContextProvider
{
    ExecutionContext GetCurrent();
    void SetCurrent(ExecutionContext context);
}
```

**Purpose:**
- Correlation IDs for distributed tracing
- Context-aware logging
- Cancellation token propagation
- Performance tracking

### 4.9 Configuration Interface

```csharp
public interface IAgentConfiguration
{
    LLMConfiguration LLM { get; }
    VectorStoreConfiguration VectorStore { get; }
    WorkflowConfiguration Workflow { get; }
    ObservabilityConfiguration Observability { get; }
    CacheConfiguration Cache { get; }
    SecurityConfiguration Security { get; }
}

public interface IConfigurationBuilder
{
    IConfigurationBuilder WithLLM(Action<LLMConfigurationBuilder> configure);
    IConfigurationBuilder WithVectorStore(Action<VectorStoreConfigurationBuilder> configure);
    IConfigurationBuilder WithWorkflow(Action<WorkflowConfigurationBuilder> configure);
    IAgentConfiguration Build();
}
```

### 4.10 Cost Tracking Interface

```csharp
public interface ICostTracker
{
    Task RecordLLMCallAsync(
        string model, 
        int inputTokens, 
        int outputTokens, 
        CancellationToken cancellationToken = default);
    
    Task<CostSummary> GetCostSummaryAsync(
        TimeSpan period, 
        CancellationToken cancellationToken = default);
    
    decimal EstimateCost(string model, int estimatedTokens);
    
    Task<CostSummary> GetWorkflowCostAsync(
        string workflowId, 
        CancellationToken cancellationToken = default);
}

public record CostSummary
{
    public decimal TotalCost { get; init; }
    public int TotalInputTokens { get; init; }
    public int TotalOutputTokens { get; init; }
    public Dictionary<string, ModelCost> CostsByModel { get; init; } = new();
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
}
```

### 4.11 Factory Interfaces

```csharp
public interface ILLMModelFactory
{
    ILLMModel<TInput, TOutput> Create(string providerName, string modelName);
    ILLMModel<TInput, TOutput> CreateFromConfiguration(string configurationKey);
    IReadOnlyList<string> GetAvailableProviders();
}

public interface IVectorStoreFactory
{
    IVectorStore Create(string storeName);
    IVectorStore CreateFromConfiguration(string configurationKey);
    IReadOnlyList<string> GetAvailableStores();
}
```

## 5. Workflow Engine Design

### 5.1 State Graph

```csharp
public class StateGraph<TState> where TState : class
{
    private readonly Dictionary<string, GraphNode<TState>> _nodes;
    private readonly List<GraphEdge<TState>> _edges;
    
    public StateGraph<TState> AddNode(
        string name, 
        Func<TState, CancellationToken, Task<TState>> handler);
    
    public StateGraph<TState> AddEdge(
        string from, 
        string to, 
        Func<TState, bool>? condition = null);
    
    public StateGraph<TState> SetEntryPoint(string nodeName);
    public StateGraph<TState> SetExitPoint(string nodeName);
}
```

### 5.2 Graph Execution

```csharp
public class GraphExecutor<TState> where TState : class
{
    private readonly StateGraph<TState> _graph;
    private readonly ICheckpointStore? _checkpointStore;
    
    public async Task<TState> ExecuteAsync(
        TState initialState, 
        ExecutionOptions? options = null, 
        CancellationToken cancellationToken = default);
    
    public async Task<TState> ResumeAsync(
        string checkpointId, 
        ExecutionOptions? options = null, 
        CancellationToken cancellationToken = default);
}
```

**Execution Flow:**
1. Validate graph structure
2. Initialize state
3. Execute entry node
4. Evaluate edges
5. Execute next node(s)
6. Checkpoint state (if configured)
7. Repeat until exit node reached
8. Return final state

### 5.3 Checkpointing

```csharp
public interface ICheckpointStore
{
    Task<string> SaveAsync(
        string workflowId, 
        object state, 
        IDictionary<string, object>? metadata = null, 
        CancellationToken cancellationToken = default);
    
    Task<Checkpoint<TState>> LoadAsync<TState>(
        string checkpointId, 
        CancellationToken cancellationToken = default);
    
    Task DeleteAsync(
        string checkpointId, 
        CancellationToken cancellationToken = default);
}
```

**Checkpoint Strategy:**
- Save after each node execution (configurable)
- Support manual checkpointing
- Support checkpoint expiration
- Support checkpoint metadata

## 6. Provider Implementations

### 6.1 OpenAI Provider

```csharp
public class OpenAIModel : ILLMModel<ChatMessage[], ChatMessage>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    
    public OpenAIModel(
        HttpClient httpClient, 
        string apiKey, 
        string modelName = "gpt-4");
    
    // Implementation details...
}
```

**Features:**
- Chat completions API
- Streaming support
- Function calling support
- Rate limiting handling
- Retry logic with exponential backoff

### 6.2 Azure OpenAI Provider

```csharp
public class AzureOpenAIModel : ILLMModel<ChatMessage[], ChatMessage>
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _apiKey;
    private readonly string _deploymentName;
    
    // Similar to OpenAI but with Azure-specific configuration
}
```

### 6.3 Anthropic Provider

```csharp
public class AnthropicModel : ILLMModel<ChatMessage[], ChatMessage>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    
    // Claude-specific implementation
}
```

## 13. Source Generators and Compile-Time Features

### 13.1 Prompt Template Source Generator

```csharp
[PromptTemplate("You are a helpful assistant. User: {userMessage}")]
public partial class ChatPrompt
{
    public string UserMessage { get; set; }
    
    // Generated at compile time:
    // - Template validation
    // - Variable extraction
    // - Format method
}
```

### 13.2 Tool Schema Generator

```csharp
[Tool("calculator", "Performs mathematical calculations")]
public partial class CalculatorTool
{
    [ToolParameter("expression", "Mathematical expression to evaluate")]
    public string Expression { get; set; }
    
    // Generated at compile time:
    // - JSON schema
    // - Validation
    // - Serialization
}
```

### 13.3 State Type Generator

```csharp
[WorkflowState]
public partial class MyWorkflowState
{
    public string CurrentStep { get; set; }
    public Dictionary<string, object> Data { get; set; }
    
    // Generated at compile time:
    // - Serialization
    // - Validation
    // - Versioning support
}
```

## 14. Diagnostic Analyzers

### 14.1 Analyzer Rules

- **DOTNETAGENTS001**: Missing cancellation token in async method
- **DOTNETAGENTS002**: Improper async/await usage
- **DOTNETAGENTS003**: Missing error handling in chain
- **DOTNETAGENTS004**: Prompt template validation errors
- **DOTNETAGENTS005**: Tool schema validation errors
- **DOTNETAGENTS006**: Missing configuration validation

## 15. Caching Strategy

### 15.1 Multi-Level Caching

```csharp
public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default);
}

public class MultiLevelCache : ICache
{
    private readonly ICache _l1Cache; // In-memory
    private readonly ICache _l2Cache; // Distributed (Redis)
    private readonly ICache _l3Cache; // Persistent (Database)
    
    // Implements cache hierarchy with fallback
}
```

### 15.2 Cache Invalidation

```csharp
public interface ICacheInvalidator
{
    Task InvalidateAsync(string key, CancellationToken cancellationToken = default);
    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default);
}
```

## 8. Performance Optimizations

### 8.1 Caching Strategy

```csharp
public interface ICache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default);
}

// Embedding cache
public class EmbeddingCache
{
    private readonly ICache _cache;
    
    public async Task<float[]?> GetCachedEmbeddingAsync(string text);
    public async Task CacheEmbeddingAsync(string text, float[] embedding);
}
```

### 8.2 Connection Pooling

- Use `IHttpClientFactory` for HTTP clients
- Configure connection pooling
- Reuse connections across requests

### 8.3 Batch Processing

- Batch embedding generation
- Batch vector store operations
- Parallel tool execution where safe

## 9. Fluent API and Builder Patterns

### 9.1 Chain Builder

```csharp
public static class ChainBuilder
{
    public static ChainBuilder<TInput, TOutput> Create<TInput, TOutput>();
}

public class ChainBuilder<TInput, TOutput>
{
    public ChainBuilder<TInput, TOutput> WithLLM(ILLMModel<TInput, TOutput> model);
    public ChainBuilder<TInput, TOutput> WithPromptTemplate(IPromptTemplate template);
    public ChainBuilder<TInput, TOutput> WithMemory(IMemory memory);
    public ChainBuilder<TInput, TOutput> WithRetryPolicy(int maxRetries, TimeSpan? delay = null);
    public ChainBuilder<TInput, TOutput> WithCaching(bool enabled, TimeSpan? ttl = null);
    public ChainBuilder<TInput, TOutput> WithRateLimiting(IRateLimiter rateLimiter);
    public IRunnable<TInput, TOutput> Build();
}
```

### 9.2 Configuration Builder

```csharp
public class ConfigurationBuilder
{
    public ConfigurationBuilder WithLLM(Action<LLMConfigurationBuilder> configure);
    public ConfigurationBuilder WithVectorStore(Action<VectorStoreConfigurationBuilder> configure);
    public ConfigurationBuilder WithWorkflow(Action<WorkflowConfigurationBuilder> configure);
    public ConfigurationBuilder WithObservability(Action<ObservabilityConfigurationBuilder> configure);
    public ConfigurationBuilder FromAppSettings(IConfiguration configuration);
    public ConfigurationBuilder FromEnvironment();
    public IAgentConfiguration Build();
}
```

## 10. Observability

### 10.1 Logging

```csharp
public static class LoggerExtensions
{
    public static void LogLLMCall(
        this ILogger logger, 
        string modelName, 
        int tokensUsed, 
        TimeSpan duration);
    
    public static void LogToolExecution(
        this ILogger logger, 
        string toolName, 
        bool success, 
        TimeSpan duration);
    
    public static void LogWorkflowExecution(
        this ILogger logger, 
        string workflowId, 
        TimeSpan duration, 
        int nodesExecuted);
}
```

### 10.2 Tracing

```csharp
public static class TraceExtensions
{
    public static Activity StartLLMSpan(
        this ActivitySource source, 
        string modelName, 
        ExecutionContext? context = null);
    
    public static Activity StartWorkflowSpan(
        this ActivitySource source, 
        string workflowId, 
        ExecutionContext? context = null);
    
    public static Activity StartChainSpan(
        this ActivitySource source, 
        string chainName, 
        ExecutionContext? context = null);
}
```

### 10.3 Metrics

```csharp
public class MetricsCollector
{
    private readonly Meter _meter;
    
    public void RecordLLMCall(string modelName, int tokens, TimeSpan duration);
    public void RecordToolCall(string toolName, bool success, TimeSpan duration);
    public void RecordWorkflowExecution(string workflowId, TimeSpan duration);
    public void RecordCost(string model, decimal cost);
    public void RecordCacheHit(string cacheKey);
    public void RecordCacheMiss(string cacheKey);
}
```

### 10.4 Health Checks

```csharp
public class AgentHealthCheck : IHealthCheck
{
    private readonly ILLMModelFactory _llmFactory;
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly ICheckpointStore? _checkpointStore;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default);
}

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddAgentHealthChecks(
        this IHealthChecksBuilder builder);
}
```

## 11. Security

### 11.1 Rate Limiting

```csharp
public interface IRateLimiter
{
    Task<bool> TryAcquireAsync(
        string key, 
        int limit, 
        TimeSpan window, 
        CancellationToken cancellationToken = default);
    
    Task<RateLimitInfo> GetRateLimitInfoAsync(
        string key, 
        CancellationToken cancellationToken = default);
}

public class RateLimitInfo
{
    public int Remaining { get; init; }
    public int Limit { get; init; }
    public DateTimeOffset ResetTime { get; init; }
}
```

### 11.2 Input/Output Sanitization

```csharp
public interface ISanitizer
{
    string SanitizeInput(string input);
    string SanitizeOutput(string output);
    bool ContainsSensitiveData(string text);
    bool ContainsPromptInjection(string text);
}

public class DefaultSanitizer : ISanitizer
{
    // PII detection patterns
    // Prompt injection detection
    // Output filtering rules
}
```

### 11.3 Secrets Management

```csharp
public interface ISecretsProvider
{
    Task<string> GetSecretAsync(
        string secretName, 
        CancellationToken cancellationToken = default);
    
    Task<T> GetSecretAsync<T>(
        string secretName, 
        CancellationToken cancellationToken = default);
}

public class AzureKeyVaultSecretsProvider : ISecretsProvider { }
public class EnvironmentSecretsProvider : ISecretsProvider { }
public class AwsSecretsManagerProvider : ISecretsProvider { }
```

## 12. Error Handling

### 12.1 Retry Policy

```csharp
public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public double BackoffMultiplier { get; set; } = 2.0;
    public Func<Exception, bool>? ShouldRetry { get; set; }
    
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation, 
        CancellationToken cancellationToken = default);
}

public static class RetryPolicyExtensions
{
    public static RetryPolicy WithExponentialBackoff(
        this RetryPolicy policy, 
        TimeSpan initialDelay, 
        double multiplier);
    
    public static RetryPolicy WithMaxRetries(
        this RetryPolicy policy, 
        int maxRetries);
}
```

### 12.2 Circuit Breaker

```csharp
public class CircuitBreaker
{
    private int _failureCount;
    private DateTime? _openedAt;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation, 
        CancellationToken cancellationToken = default);
    
    public CircuitBreakerState State { get; }
}

public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}
```

### 12.3 Error Types

```csharp
public class AgentException : Exception
{
    public string? CorrelationId { get; }
    public ErrorCategory Category { get; }
}

public enum ErrorCategory
{
    LLMError,
    ToolError,
    WorkflowError,
    ConfigurationError,
    SecurityError
}
```

## 11. Testing Strategy

### 11.1 Unit Tests
- Test all core abstractions
- Mock external dependencies
- Test error scenarios
- Test edge cases

### 11.2 Integration Tests
- Test provider integrations (with test API keys)
- Test workflow execution
- Test checkpoint/resume
- Test end-to-end scenarios

### 11.3 Performance Tests
- Benchmark LLM call overhead
- Benchmark workflow execution
- Memory profiling
- Load testing

## 12. Deployment and Packaging

### 12.1 NuGet Package Structure
- Main package: `DotNetAgents`
- Optional packages: `DotNetAgents.Integrations.OpenAI`, etc.
- Package metadata and versioning
- XML documentation included

### 12.2 Versioning Strategy
- Semantic versioning (MAJOR.MINOR.PATCH)
- Breaking changes in MAJOR versions
- Backward compatibility in MINOR/PATCH

## 13. Future Enhancements

- Multi-agent coordination
- Advanced memory strategies (summarization, pruning)
- Graph visualization tools
- Cost tracking and optimization
- More vector store integrations
- Local model support (Ollama, etc.)

---

**Document Control:**
- **Author:** Architecture Team
- **Reviewers:** Development Team, Security Team
- **Approval:** Technical Lead
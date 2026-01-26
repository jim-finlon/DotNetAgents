# DotNetAgents Technical Specification

**Project Name:** DotNetAgents  
**Version:** 1.0  
**Date:** January 2026  
**Status:** Comprehensive Technical Specification  
**Target Framework:** .NET 10 (LTS)  
**Language:** C# 13

---

## 1. Introduction

### 1.1 Purpose

This document provides the comprehensive technical specification for implementing DotNetAgents, an enterprise-grade .NET 10 library for building AI agents, chains, and workflows. It details the architecture, design patterns, component specifications, interfaces, data structures, and implementation guidelines.

### 1.2 Scope

This specification covers:
- Solution structure and project organization
- Core abstractions and interfaces
- Component implementations
- Integration patterns
- Security implementation
- Performance optimization strategies
- Testing strategies
- Deployment architecture

### 1.3 Reference Documents

- **REQUIREMENTS_DOCUMENT.md**: Business and functional requirements
- **ARCHITECTURE_SUMMARY.md**: High-level architecture overview
- **COMPARISON.md**: Comparison with LangChain, LangGraph, Microsoft Agent Framework
- Microsoft .NET 10 Documentation
- OWASP Application Security Verification Standard (ASVS) 4.0
- OpenTelemetry .NET SDK Documentation

---

## 2. Architecture Overview

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Application Layer                                   │
│  (JARVIS, Teaching Assistant, Custom Applications, Sample Projects)        │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                  Autonomous Agent Capabilities Layer                         │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────────┐ │
│  │ State Machines    │  │ Behavior Trees   │  │ Multi-Agent Patterns    │ │
│  │ (Lifecycle Mgmt)  │  │ (Decision-Making)│  │ (Swarm, Hierarchical)   │ │
│  └──────────────────┘  └──────────────────┘  └──────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Workflow Engine Layer                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │ StateGraph   │  │ Checkpoints  │  │ Human-in-Loop│  │ Visualization   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                      Chain & Runnable Layer                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │ IRunnable    │  │ ChainBuilder │  │ Composition  │  │ Retry Policies  │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Core Abstractions Layer                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │ ILLMModel    │  │ IPrompt      │  │ ITool        │  │ IVectorStore    │ │
│  │ IEmbedding   │  │ IMemory      │  │ IDocument    │  │ IAgentRegistry  │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Integrations & Infrastructure Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │ LLM Providers│  │ Vector Stores│  │ Message Buses│  │ Observability   │ │
│  │ (12 types)   │  │ (5 types)    │  │ (5 types)    │  │ (OTel, Metrics) │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────────┘ │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │ Storage      │  │ Security     │  │ Configuration│  │ Plugin System   │ │
│  │ (PostgreSQL, │  │ (Secrets,    │  │ Management   │  │ (Discovery,     │ │
│  │  SQL Server) │  │  Validation) │  │              │  │  Lifecycle)     │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Design Principles

| Principle | Implementation |
|-----------|----------------|
| **Dependency Inversion** | All components depend on abstractions (interfaces) |
| **Single Responsibility** | Each class has one reason to change |
| **Open/Closed** | Extensible via new implementations, not modification |
| **Interface Segregation** | Small, focused interfaces |
| **Composition over Inheritance** | Behaviors composed via DI, not class hierarchies |
| **Fail Fast** | Validate inputs early; throw meaningful exceptions |
| **Async by Default** | All I/O operations are async |
| **Cancellation Support** | All async operations support CancellationToken |
| **Type Safety** | Strong typing throughout (C# 13) |
| **Nullable Reference Types** | Enabled throughout for null safety |

### 2.3 Package Architecture

The library uses a **hybrid modular approach**: code is structured as separate projects from day one, with a metapackage for convenience.

**Package Structure:**
```
DotNetAgents.Core (v1.0.0)
├── Core abstractions only
├── Zero dependencies on integrations
└── ~500KB package size

DotNetAgents.Workflow (v1.0.0)
├── Depends on: DotNetAgents.Core
└── ~200KB package size

DotNetAgents.Ecosystem (v1.0.0)
├── Plugin architecture
├── Depends on: DotNetAgents.Core
└── ~150KB package size

DotNetAgents.Agents.StateMachines (v1.0.0)
├── State machine implementation
├── Depends on: DotNetAgents.Core, DotNetAgents.Ecosystem
└── ~200KB package size

DotNetAgents.Agents.BehaviorTrees (v1.0.0)
├── Behavior tree implementation
├── Depends on: DotNetAgents.Core, DotNetAgents.Ecosystem
└── ~200KB package size

DotNetAgents.Providers.* (v1.0.0)
├── LLM provider implementations
├── Depends on: DotNetAgents.Core, DotNetAgents.Ecosystem
└── ~100KB each

DotNetAgents.VectorStores.* (v1.0.0)
├── Vector store implementations
├── Depends on: DotNetAgents.Core, DotNetAgents.Ecosystem
└── ~150KB each

DotNetAgents.Agents.Messaging.* (v1.0.0)
├── Message bus implementations
├── Depends on: DotNetAgents.Core, DotNetAgents.Ecosystem
└── ~100KB each

DotNetAgents (v1.0.0) [METAPACKAGE]
├── References all above packages
└── ~50KB (metadata only)
```

---

## 3. Solution Structure

### 3.1 Project Organization

```
DotNetAgents/
├── src/
│   ├── DotNetAgents.Abstractions/          # Core interfaces (ZERO deps on integrations)
│   │   ├── LLM/
│   │   ├── Embeddings/
│   │   ├── VectorStores/
│   │   ├── Documents/
│   │   ├── Agents/
│   │   ├── Memory/
│   │   ├── Tools/
│   │   └── Common/
│   │
│   ├── DotNetAgents.Core/                  # Core implementations
│   │   ├── Chains/
│   │   ├── Prompts/
│   │   ├── Tools/
│   │   ├── Memory/
│   │   ├── Documents/
│   │   ├── Retrieval/
│   │   └── Agents/
│   │
│   ├── DotNetAgents.Workflow/              # Workflow engine
│   │   ├── Graph/
│   │   ├── Execution/
│   │   ├── Checkpoints/
│   │   ├── Session/
│   │   └── HumanInLoop/
│   │
│   ├── DotNetAgents.Ecosystem/             # Plugin architecture
│   │   ├── IPlugin.cs
│   │   ├── PluginBase.cs
│   │   ├── IPluginRegistry.cs
│   │   ├── PluginDiscovery.cs
│   │   ├── PluginDependencyResolver.cs
│   │   ├── PluginLifecycleManager.cs
│   │   └── PluginDiscoveryService.cs
│   │
│   ├── DotNetAgents.Agents.StateMachines/  # State machines
│   │   ├── StateMachine.cs
│   │   ├── State.cs
│   │   ├── Transition.cs
│   │   └── Patterns/
│   │
│   ├── DotNetAgents.Agents.BehaviorTrees/  # Behavior trees
│   │   ├── BehaviorTree.cs
│   │   ├── Nodes/
│   │   └── Patterns/
│   │
│   ├── DotNetAgents.Agents.Registry/       # Agent registry
│   ├── DotNetAgents.Agents.WorkerPool/     # Worker pool
│   ├── DotNetAgents.Agents.Supervisor/     # Supervisor agent
│   ├── DotNetAgents.Agents.Messaging.*/    # Message buses
│   ├── DotNetAgents.Agents.Swarm/          # Swarm intelligence
│   ├── DotNetAgents.Agents.Hierarchical/   # Hierarchical agents
│   ├── DotNetAgents.Agents.Marketplace/    # Agent marketplace
│   │
│   ├── DotNetAgents.Providers.*/           # LLM providers (12)
│   ├── DotNetAgents.VectorStores.*/        # Vector stores (5)
│   ├── DotNetAgents.Storage.*/             # Storage implementations
│   ├── DotNetAgents.Documents/            # Document loaders (10)
│   ├── DotNetAgents.Tools.BuiltIn/        # Built-in tools (19)
│   ├── DotNetAgents.Tools.Development/    # AI dev tools
│   ├── DotNetAgents.Tasks/                # Task management
│   ├── DotNetAgents.Knowledge/            # Knowledge repository
│   ├── DotNetAgents.Education/            # Educational extensions
│   ├── DotNetAgents.Edge/                 # Edge computing
│   ├── DotNetAgents.Mcp/                  # MCP support
│   ├── DotNetAgents.Observability/        # Observability
│   ├── DotNetAgents.Security/             # Security
│   ├── DotNetAgents.Configuration/         # Configuration
│   │
│   └── DotNetAgents/                      # Metapackage
│
├── tests/
│   ├── DotNetAgents.Tests.Unit/
│   ├── DotNetAgents.Tests.Integration/
│   └── DotNetAgents.Tests.Benchmarks/
│
├── samples/
│   ├── DotNetAgents.Samples.BasicChain/
│   ├── DotNetAgents.Samples.AgentWithTools/
│   ├── DotNetAgents.Samples.Workflow/
│   ├── DotNetAgents.Samples.RAG/
│   ├── DotNetAgents.Samples.MultiAgent/
│   ├── DotNetAgents.Samples.StateMachines/
│   ├── DotNetAgents.Samples.Education/
│   ├── DotNetAgents.Samples.Tracing/
│   ├── DotNetAgents.Samples.TasksAndKnowledge/
│   └── DotNetAgents.Samples.JARVISVoice/
│
└── docs/
    ├── guides/
    ├── examples/
    ├── architecture/
    └── operations/
```

### 3.2 Package Dependencies

#### DotNetAgents.Abstractions
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
  <PackageReference Include="System.Text.Json" Version="10.0.0" />
</ItemGroup>
```

#### DotNetAgents.Core
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Options" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.0" />
  <PackageReference Include="Polly.Extensions" Version="8.5.0" />
  <PackageReference Include="OpenTelemetry.Api" Version="1.10.0" />
  <PackageReference Include="System.Threading.Channels" Version="10.0.0" />
</ItemGroup>
```

---

## 4. Core Abstractions

### 4.1 LLM Interface

```csharp
namespace DotNetAgents.Abstractions.LLM;

/// <summary>
/// Generic LLM model interface supporting different input/output types.
/// </summary>
public interface ILLMModel<TInput, TOutput>
{
    /// <summary>
    /// Generates a response from the input.
    /// </summary>
    Task<TOutput> GenerateAsync(
        TInput input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a response from the input.
    /// </summary>
    IAsyncEnumerable<TOutput> StreamAsync(
        TInput input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Chat message for conversation-based models.
/// </summary>
public sealed record ChatMessage
{
    public required MessageRole Role { get; init; }
    public required string Content { get; init; }
    public string? Name { get; init; }
    public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
    public string? ToolCallId { get; init; }
}

public enum MessageRole
{
    System,
    User,
    Assistant,
    Tool
}
```

### 4.2 Vector Store Interface

```csharp
namespace DotNetAgents.Abstractions.VectorStores;

/// <summary>
/// Vector store for similarity search operations.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Adds or updates vectors in the store.
    /// </summary>
    Task AddAsync(
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
}

public sealed record VectorRecord
{
    public required string Id { get; init; }
    public required float[] Vector { get; init; }
    public string? Content { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
}

public sealed record VectorSearchResult
{
    public required VectorRecord Record { get; init; }
    public required float Score { get; init; }
}

public enum DistanceMetric
{
    Cosine,
    Euclidean,
    DotProduct
}
```

### 4.3 Document Loader Interface

```csharp
namespace DotNetAgents.Abstractions.Documents;

/// <summary>
/// Loads documents from various sources.
/// </summary>
public interface IDocumentLoader
{
    /// <summary>
    /// Gets supported file extensions.
    /// </summary>
    IReadOnlySet<string> SupportedExtensions { get; }

    /// <summary>
    /// Loads a document from a file path.
    /// </summary>
    Task<IReadOnlyList<Document>> LoadAsync(
        string filePath,
        DocumentLoadOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a document from a stream.
    /// </summary>
    Task<IReadOnlyList<Document>> LoadAsync(
        Stream stream,
        string fileName,
        DocumentLoadOptions? options = null,
        CancellationToken cancellationToken = default);
}

public sealed record Document
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
    public string? Source { get; init; }
}
```

### 4.4 Tool Interface

```csharp
namespace DotNetAgents.Abstractions.Tools;

/// <summary>
/// A tool that can be called by an agent.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets the tool name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the tool description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the tool parameter schema (JSON Schema).
    /// </summary>
    JsonElement ParametersSchema { get; }

    /// <summary>
    /// Executes the tool with the given arguments.
    /// </summary>
    Task<ToolResult> ExecuteAsync(
        Dictionary<string, object?> arguments,
        CancellationToken cancellationToken = default);
}

public sealed record ToolResult
{
    public required string Content { get; init; }
    public bool IsError { get; init; }
    public string? ErrorMessage { get; init; }
}
```

### 4.5 Chain Interface

```csharp
namespace DotNetAgents.Abstractions.Chains;

/// <summary>
/// A runnable component that can be composed into chains.
/// </summary>
public interface IRunnable<TInput, TOutput>
{
    /// <summary>
    /// Invokes the runnable with the given input.
    /// </summary>
    Task<TOutput> InvokeAsync(
        TInput input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams the runnable execution.
    /// </summary>
    IAsyncEnumerable<TOutput> StreamAsync(
        TInput input,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Builder for creating chains.
/// </summary>
public static class ChainBuilder
{
    public static IChainBuilder<TInput, TOutput> Create<TInput, TOutput>()
    {
        return new ChainBuilder<TInput, TOutput>();
    }
}
```

### 4.6 Workflow Interface

```csharp
namespace DotNetAgents.Abstractions.Workflow;

/// <summary>
/// A stateful workflow graph.
/// </summary>
public interface IStateGraph<TState> where TState : class
{
    /// <summary>
    /// Adds a node to the graph.
    /// </summary>
    IStateGraph<TState> AddNode(
        string name,
        Func<TState, CancellationToken, Task<TState>> action);

    /// <summary>
    /// Adds an edge between nodes.
    /// </summary>
    IStateGraph<TState> AddEdge(string from, string to);

    /// <summary>
    /// Adds a conditional edge.
    /// </summary>
    IStateGraph<TState> AddConditionalEdge(
        string from,
        Func<TState, string> condition);

    /// <summary>
    /// Sets the entry point.
    /// </summary>
    IStateGraph<TState> SetEntryPoint(string nodeName);

    /// <summary>
    /// Compiles the graph for execution.
    /// </summary>
    ICompiledGraph<TState> Build();
}

/// <summary>
/// Compiled executable graph.
/// </summary>
public interface ICompiledGraph<TState> where TState : class
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
```

### 4.7 State Machine Interface

```csharp
namespace DotNetAgents.Abstractions.Agents.StateMachines;

/// <summary>
/// A state machine for managing agent lifecycle.
/// </summary>
public interface IStateMachine<TState> where TState : class
{
    /// <summary>
    /// Gets the current state.
    /// </summary>
    string CurrentState { get; }

    /// <summary>
    /// Transitions to a new state.
    /// </summary>
    Task<bool> TransitionToAsync(
        string targetState,
        TState? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a transition is valid.
    /// </summary>
    bool CanTransitionTo(string targetState);

    /// <summary>
    /// Gets available transitions from current state.
    /// </summary>
    IReadOnlyList<string> GetAvailableTransitions();
}
```

### 4.8 Behavior Tree Interface

```csharp
namespace DotNetAgents.Abstractions.Agents.BehaviorTrees;

/// <summary>
/// A behavior tree for hierarchical decision-making.
/// </summary>
public interface IBehaviorTree
{
    /// <summary>
    /// Executes the behavior tree.
    /// </summary>
    Task<BehaviorTreeStatus> ExecuteAsync(
        BehaviorTreeContext context,
        CancellationToken cancellationToken = default);
}

public enum BehaviorTreeStatus
{
    Success,
    Failure,
    Running
}

/// <summary>
/// A node in the behavior tree.
/// </summary>
public interface IBehaviorTreeNode
{
    /// <summary>
    /// Executes the node.
    /// </summary>
    Task<BehaviorTreeStatus> ExecuteAsync(
        BehaviorTreeContext context,
        CancellationToken cancellationToken = default);
}
```

### 4.9 Plugin Interface

```csharp
namespace DotNetAgents.Ecosystem;

/// <summary>
/// A plugin that extends DotNetAgents functionality.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the plugin ID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the plugin name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the plugin version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Initializes the plugin.
    /// </summary>
    Task InitializeAsync(
        IPluginContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuts down the plugin.
    /// </summary>
    Task ShutdownAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Plugin with metadata support.
/// </summary>
public interface IPluginWithMetadata : IPlugin
{
    /// <summary>
    /// Gets the plugin metadata.
    /// </summary>
    PluginMetadata Metadata { get; }
}
```

---

## 5. Component Specifications

### 5.1 Chain Implementation

**File:** `src/DotNetAgents.Core/Chains/ChainBuilder.cs`

**Responsibilities:**
- Build chains using fluent API
- Compose runnables into sequential/parallel chains
- Support retry policies and error handling

**Key Methods:**
```csharp
public class ChainBuilder<TInput, TOutput> : IChainBuilder<TInput, TOutput>
{
    public IChainBuilder<TInput, TOutput> WithLLM(ILLMModel<TInput, TOutput> llm);
    public IChainBuilder<TInput, TOutput> WithPromptTemplate(IPromptTemplate template);
    public IChainBuilder<TInput, TOutput> WithRetryPolicy(RetryPolicy policy);
    public IRunnable<TInput, TOutput> Build();
}
```

### 5.2 Workflow Implementation

**File:** `src/DotNetAgents.Workflow/Graph/StateGraph.cs`

**Responsibilities:**
- Build stateful workflows
- Manage state transitions
- Support checkpointing
- Handle human-in-the-loop nodes

**Key Methods:**
```csharp
public class StateGraph<TState> : IStateGraph<TState> where TState : class
{
    public IStateGraph<TState> AddNode(string name, Func<TState, CancellationToken, Task<TState>> action);
    public IStateGraph<TState> AddEdge(string from, string to);
    public IStateGraph<TState> AddConditionalEdge(string from, Func<TState, string> condition);
    public ICompiledGraph<TState> Build();
}
```

### 5.3 State Machine Implementation

**File:** `src/DotNetAgents.Agents.StateMachines/StateMachine.cs`

**Responsibilities:**
- Manage agent lifecycle states
- Handle state transitions
- Support hierarchical and parallel states
- Provide state persistence

**Key Methods:**
```csharp
public class StateMachine<TState> : IStateMachine<TState> where TState : class
{
    public string CurrentState { get; }
    public Task<bool> TransitionToAsync(string targetState, TState? context = null, CancellationToken cancellationToken = default);
    public bool CanTransitionTo(string targetState);
    public IReadOnlyList<string> GetAvailableTransitions();
}
```

### 5.4 Behavior Tree Implementation

**File:** `src/DotNetAgents.Agents.BehaviorTrees/BehaviorTree.cs`

**Responsibilities:**
- Execute hierarchical decision-making
- Support composite and decorator nodes
- Integrate with LLMs, workflows, and state machines

**Key Methods:**
```csharp
public class BehaviorTree : IBehaviorTree
{
    public IBehaviorTreeNode Root { get; set; }
    public Task<BehaviorTreeStatus> ExecuteAsync(BehaviorTreeContext context, CancellationToken cancellationToken = default);
}
```

### 5.5 Plugin System Implementation

**File:** `src/DotNetAgents.Ecosystem/PluginDiscovery.cs`

**Responsibilities:**
- Discover plugins from assemblies
- Resolve plugin dependencies
- Manage plugin lifecycle
- Register plugins in DI container

**Key Methods:**
```csharp
public class PluginDiscovery : IPluginDiscovery
{
    public IReadOnlyList<Type> DiscoverPluginTypes(IEnumerable<Assembly>? assemblies = null);
    public IReadOnlyList<IPlugin> CreatePluginInstances(IEnumerable<Type> pluginTypes, IServiceProvider serviceProvider);
}
```

---

## 6. Integration Patterns

### 6.1 Dependency Injection Pattern

**All components MUST be registered via Microsoft.Extensions.DependencyInjection.**

**Example:**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDotNetAgents(this IServiceCollection services)
    {
        services.AddSingleton<ILLMModelFactory, LLMModelFactory>();
        services.AddSingleton<IVectorStoreFactory, VectorStoreFactory>();
        return services;
    }
}
```

### 6.2 Provider Pattern

**All external integrations (LLMs, Vector Stores, Message Buses) MUST use the provider pattern.**

**Example:**
```csharp
// Abstractions define interfaces
public interface ILLMModel<TInput, TOutput> { }

// Providers implement interfaces
public class OpenAIModel : ILLMModel<ChatMessage[], ChatMessage> { }
public class AnthropicModel : ILLMModel<ChatMessage[], ChatMessage> { }
```

### 6.3 Plugin Pattern

**All optional features MUST be implemented as plugins.**

**Example:**
```csharp
public class StateMachinesPlugin : PluginBase
{
    public StateMachinesPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "statemachines",
            Name = "State Machines",
            Version = "1.0.0",
            Dependencies = []
        };
    }
}
```

### 6.4 Adapter Pattern

**Integration between components MUST use adapter pattern to avoid circular dependencies.**

**Example:**
```csharp
// State machine adapter for workflow integration
public class StateMachineWorkflowNode<TState> : IWorkflowNode<TState>
{
    private readonly IStateMachine<TState> _stateMachine;
    // Adapts state machine to workflow node interface
}
```

---

## 7. Security Implementation

### 7.1 Input Sanitization

**File:** `src/DotNetAgents.Security/Validation/ISanitizer.cs`

**Requirements:**
- Sanitize all user inputs before LLM calls
- Detect prompt injection attempts
- Support configurable sanitization levels

**Implementation:**
```csharp
public interface ISanitizer
{
    string Sanitize(string input, SanitizationLevel level = SanitizationLevel.Standard);
    bool ContainsPotentialInjection(string input);
}
```

### 7.2 Secrets Management

**File:** `src/DotNetAgents.Security/Secrets/ISecretsProvider.cs`

**Requirements:**
- Never hardcode API keys
- Support multiple secret sources (Environment, Azure Key Vault, AWS Secrets Manager)
- Validate secrets at startup

**Implementation:**
```csharp
public interface ISecretsProvider
{
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);
    Task<T?> GetSecretAsync<T>(string key, CancellationToken cancellationToken = default);
}
```

### 7.3 Audit Logging

**File:** `src/DotNetAgents.Security/Audit/IAuditLogger.cs`

**Requirements:**
- Log all sensitive operations
- Redact PII from logs
- Support structured logging

**Implementation:**
```csharp
public interface IAuditLogger
{
    void Log(AuditEvent auditEvent);
    Task LogAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}
```

---

## 8. Performance Optimization

### 8.1 Async/Await Patterns

**All I/O operations MUST be async.**

**Requirements:**
- Use `async`/`await` throughout
- Never use `.Result` or `.Wait()`
- Use `ConfigureAwait(false)` in library code
- Support `CancellationToken` in all async methods

### 8.2 Caching Strategy

**File:** `src/DotNetAgents.Core/Caching/`

**Requirements:**
- Cache embeddings with configurable TTL
- Cache LLM responses when appropriate
- Use multi-level caching (memory, distributed, persistent)

**Implementation:**
```csharp
public interface IEmbeddingCache
{
    Task<float[]?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, float[] embedding, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}
```

### 8.3 Connection Pooling

**All HTTP clients MUST use IHttpClientFactory.**

**Requirements:**
- Use `IHttpClientFactory` for all HTTP clients
- Configure connection pooling
- Support HTTP/3 where available

### 8.4 Object Pooling

**High-allocation scenarios MUST use object pooling.**

**Requirements:**
- Pool `StringBuilder` instances
- Pool embedding buffers
- Use `ArrayPool<T>` for temporary arrays

---

## 9. Observability

### 9.1 Distributed Tracing

**File:** `src/DotNetAgents.Observability/Tracing/`

**Requirements:**
- Use OpenTelemetry for tracing
- Create spans for all major operations
- Propagate trace context across async boundaries

**Implementation:**
```csharp
using System.Diagnostics;

private static readonly ActivitySource ActivitySource = new("DotNetAgents.LLM");

using var activity = ActivitySource.StartActivity("GenerateAsync");
activity?.SetTag("model", modelName);
activity?.SetTag("input_tokens", inputTokens);
```

### 9.2 Metrics

**File:** `src/DotNetAgents.Observability/Metrics/`

**Requirements:**
- Expose Prometheus-compatible metrics
- Track token usage, latency, error rates
- Support custom metrics

**Implementation:**
```csharp
private static readonly Meter Meter = new("DotNetAgents.LLM", "1.0.0");
private static readonly Counter<long> TokensConsumed = Meter.CreateCounter<long>("dotnetagents.llm.tokens.consumed");
```

### 9.3 Structured Logging

**All logging MUST use Microsoft.Extensions.Logging with structured parameters.**

**Requirements:**
- Use structured logging (parameters, not string interpolation)
- Include correlation IDs
- Use appropriate log levels

**Example:**
```csharp
_logger.LogInformation(
    "LLM call completed. Model: {ModelName}, Tokens: {TokenCount}, Duration: {Duration}ms",
    modelName,
    tokenCount,
    duration.TotalMilliseconds);
```

---

## 10. Testing Strategy

### 10.1 Unit Tests

**Requirements:**
- Test all public APIs
- Use xUnit, Moq, FluentAssertions
- Achieve ≥85% code coverage
- Test error paths and edge cases

### 10.2 Integration Tests

**Requirements:**
- Test with real providers (when possible)
- Use Testcontainers for external services
- Test end-to-end workflows

### 10.3 Performance Tests

**Requirements:**
- Use BenchmarkDotNet for benchmarks
- Track performance regressions
- Test under load

### 10.4 Chaos Tests

**Requirements:**
- Test failure scenarios
- Test circuit breakers
- Test retry policies
- Test graceful degradation

---

## 11. Deployment Architecture

### 11.1 Kubernetes Deployment

**Location:** `kubernetes/manifests/`

**Requirements:**
- Complete Kubernetes manifests
- Helm charts for easy deployment
- Support for horizontal scaling
- Health checks and readiness probes

### 11.2 Monitoring Stack

**Location:** `kubernetes/monitoring/`

**Requirements:**
- Prometheus for metrics
- Grafana for visualization
- Loki for log aggregation
- Prometheus alerts

### 11.3 Docker Support

**Location:** `docker/`

**Requirements:**
- Docker Compose for local development
- Optimized Dockerfiles
- Multi-stage builds
- Non-root user support

---

## 12. Error Handling

### 12.1 Exception Hierarchy

**All exceptions MUST inherit from `DotNetAgentsException`.**

```csharp
public abstract class DotNetAgentsException : Exception
{
    public string? CorrelationId { get; }
    public string ErrorCode { get; }
}

public class LLMException : DotNetAgentsException { }
public class VectorStoreException : DotNetAgentsException { }
public class DocumentException : DotNetAgentsException { }
public class AgentException : DotNetAgentsException { }
```

### 12.2 Error Codes

**All errors MUST have unique error codes.**

**Format:** `DNA-{Category}-{Number}`

**Examples:**
- `DNA-LLM-001`: LLM provider unavailable
- `DNA-VS-002`: Vector store connection failed
- `DNA-DOC-003`: Document format not supported

---

## 13. Configuration Management

### 13.1 Configuration Sources

**Support multiple configuration sources:**
- `appsettings.json`
- Environment variables
- Azure Key Vault
- AWS Secrets Manager
- Command-line arguments

### 13.2 Configuration Validation

**All configuration MUST be validated at startup.**

**Implementation:**
```csharp
public class OpenAIOptions
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;
    
    [Range(0, 2)]
    public double Temperature { get; set; } = 0.7;
}
```

---

## 14. Versioning Strategy

### 14.1 Semantic Versioning

**Follow Semantic Versioning (MAJOR.MINOR.PATCH):**
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### 14.2 Breaking Changes Policy

**Breaking changes MUST be:**
- Documented in CHANGELOG
- Deprecated before removal (at least one minor version)
- Clearly marked in migration guides

---

## 15. Documentation Requirements

### 15.1 XML Documentation

**All public APIs MUST have XML documentation.**

**Required Tags:**
- `<summary>`: Brief description
- `<param>`: Parameter descriptions
- `<returns>`: Return value description
- `<exception>`: Exception descriptions
- `<example>`: Code examples (for complex APIs)

### 15.2 README Files

**Each package MUST have a README.md with:**
- Purpose and overview
- Installation instructions
- Usage examples
- Key APIs
- Links to detailed documentation

### 15.3 Guides

**Comprehensive guides MUST be provided for:**
- Integration guide
- API reference
- Plugin development
- Multi-agent patterns
- Production deployment
- Troubleshooting

---

## 16. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | January 2026 | Technical Team | Initial comprehensive technical specification |

---

**Document Status:** ✅ Complete  
**Next Review Date:** Q2 2026  
**Approved By:** Architecture Review Board

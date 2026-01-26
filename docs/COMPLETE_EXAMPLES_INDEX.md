# Complete Examples Index

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This document provides a comprehensive index of all examples and sample projects available in DotNetAgents, organized by feature category.

## Sample Projects

All sample projects are located in the `samples/` directory. See [Samples README](../samples/README.md) for details.

### Core Features

| Sample | Location | Features | API Key Required |
|--------|----------|----------|------------------|
| **BasicChain** | `samples/DotNetAgents.Samples.BasicChain/` | Chain composition, prompt templates | ✅ Yes |
| **AgentWithTools** | `samples/DotNetAgents.Samples.AgentWithTools/` | Agent executor, tools, state machines, behavior trees | ✅ Yes |
| **Workflow** | `samples/DotNetAgents.Samples.Workflow/` | Stateful workflows, checkpointing | ✅ Yes |
| **RAG** | `samples/DotNetAgents.Samples.RAG/` | Document loading, embeddings, vector stores | ✅ Yes |

### Multi-Agent Features

| Sample | Location | Features | API Key Required |
|--------|----------|----------|------------------|
| **MultiAgent** | `samples/DotNetAgents.Samples.MultiAgent/` | Supervisor-worker, agent registry, worker pool | ❌ No |
| **StateMachines** | `samples/DotNetAgents.Samples.StateMachines/` | State machine patterns, lifecycle management | ❌ No |
| **TasksAndKnowledge** | `samples/DotNetAgents.Samples.TasksAndKnowledge/` | Task management, knowledge capture | ❌ No |

### Specialized Features

| Sample | Location | Features | API Key Required |
|--------|----------|----------|------------------|
| **Education** | `samples/DotNetAgents.Samples.Education/` | Educational extensions, pedagogy, assessment | ⚠️ Partial |
| **JARVISVoice** | `samples/DotNetAgents.Samples.JARVISVoice/` | Voice commands, state machines, behavior trees | ⚠️ Partial |
| **Tracing** | `samples/DotNetAgents.Samples.Tracing/` | Distributed tracing, OpenTelemetry | ❌ No |

## Documentation Examples

### Plugin Architecture

**Location:** `docs/examples/PLUGIN_ARCHITECTURE.md`

**Covers:**
- Creating plugins
- Plugin registration
- Plugin discovery
- Plugin dependencies
- Plugin lifecycle
- Complete working examples

**Code Examples:** ✅ Complete

### Behavior Trees

**Location:** `docs/guides/BEHAVIOR_TREES.md`

**Covers:**
- Node types (Action, Condition, Composite, Decorator)
- Creating behavior trees
- Integration with agents
- Tool selection patterns
- Task routing patterns
- Adaptive learning patterns

**Code Examples:** ✅ Complete

### Message Buses

**Location:** `docs/guides/MESSAGE_BUSES.md`

**Covers:**
- In-Memory message bus
- Kafka message bus
- RabbitMQ message bus
- Redis Pub/Sub message bus
- SignalR message bus
- Configuration and usage for each

**Code Examples:** ✅ Complete

### Vector Stores

**Location:** `docs/guides/VECTOR_STORES.md`

**Covers:**
- PostgreSQL (pgvector)
- Pinecone
- Weaviate
- Qdrant
- Chroma
- Comparison and migration

**Code Examples:** ✅ Complete

### Document Loaders

**Location:** `docs/guides/DOCUMENT_LOADERS.md`

**Covers:**
- PDF, CSV, Excel, EPUB, Markdown, Text, DOCX, HTML, JSON, XML loaders
- Text splitters (RecursiveCharacter, Character, Semantic)
- Loader-specific features
- Best practices

**Code Examples:** ✅ Complete

### LLM Providers

**Location:** `docs/guides/LLM_PROVIDERS.md`

**Covers:**
- All 12 providers (OpenAI, Azure, Anthropic, Google, AWS, Cohere, Groq, Mistral, Together, Ollama, LM Studio, vLLM)
- Cloud vs Local comparison
- Configuration examples
- Switching providers
- Best practices

**Code Examples:** ✅ Complete

### RAG (Retrieval-Augmented Generation)

**Location:** `docs/guides/RAG_GUIDE.md`

**Covers:**
- Complete RAG pipeline
- Document loading and splitting
- Embedding generation
- Vector storage
- Retrieval strategies
- Generation patterns
- Advanced patterns (multi-query, reranking, compression)

**Code Examples:** ✅ Complete

### Advanced Multi-Agent Patterns

**Location:** `docs/guides/ADVANCED_MULTI_AGENT_PATTERNS.md`

**Covers:**
- Swarm Intelligence (Particle Swarm, Ant Colony, Flocking, Consensus)
- Hierarchical Organizations
- Agent Marketplace
- Use cases and best practices

**Code Examples:** ✅ Complete

### Edge Computing

**Location:** `docs/guides/EDGE_COMPUTING.md`

**Covers:**
- Offline mode
- Edge models
- Network monitoring
- Mobile deployment
- Configuration examples

**Code Examples:** ✅ Complete

### MCP (Model Context Protocol)

**Location:** `src/DotNetAgents.Mcp/README.md`

**Covers:**
- MCP client setup
- Tool discovery
- Tool execution
- Service health
- Integration patterns

**Code Examples:** ✅ Complete

### Distributed Tracing

**Location:** `docs/examples/DISTRIBUTED_TRACING.md`

**Covers:**
- OpenTelemetry setup
- Multi-agent workflow tracing
- Chain and LLM call tracing
- Correlation ID propagation

**Code Examples:** ✅ Complete  
**Sample Project:** ✅ `samples/DotNetAgents.Samples.Tracing/`

### State Machine Integration

**Location:** `docs/examples/STATE_MACHINE_INTEGRATION.md`

**Covers:**
- State machine patterns
- Agent lifecycle management
- Integration examples

**Code Examples:** ✅ Complete  
**Sample Project:** ✅ `samples/DotNetAgents.Samples.StateMachines/`

### Behavior Tree Integration

**Location:** `docs/examples/BEHAVIOR_TREE_INTEGRATION.md`

**Covers:**
- Behavior tree patterns
- Tool selection
- Decision-making examples

**Code Examples:** ✅ Complete

## Feature Coverage Matrix

| Feature | Sample Project | Documentation | Code Examples |
|---------|---------------|---------------|---------------|
| **Basic Chains** | ✅ | ✅ | ✅ |
| **Workflows** | ✅ | ✅ | ✅ |
| **RAG** | ✅ | ✅ | ✅ |
| **Agents with Tools** | ✅ | ✅ | ✅ |
| **State Machines** | ✅ | ✅ | ✅ |
| **Behavior Trees** | ⚠️* | ✅ | ✅ |
| **Multi-Agent** | ✅ | ✅ | ✅ |
| **Plugin Architecture** | ❌ | ✅ | ✅ |
| **Swarm Intelligence** | ❌ | ✅ | ✅ |
| **Hierarchical Agents** | ❌ | ✅ | ✅ |
| **Agent Marketplace** | ❌ | ✅ | ✅ |
| **Message Buses** | ❌ | ✅ | ✅ |
| **Vector Stores** | ⚠️** | ✅ | ✅ |
| **Document Loaders** | ⚠️** | ✅ | ✅ |
| **LLM Providers** | ⚠️*** | ✅ | ✅ |
| **MCP** | ❌ | ✅ | ✅ |
| **Edge Computing** | ❌ | ✅ | ✅ |
| **Education** | ✅ | ✅ | ✅ |
| **Tracing** | ✅ | ✅ | ✅ |
| **Tasks & Knowledge** | ✅ | ✅ | ✅ |

*Behavior Trees are demonstrated in AgentWithTools, Education, JARVISVoice, and MultiAgent samples  
**Vector Stores and Document Loaders are demonstrated in RAG sample  
***LLM Providers are demonstrated in samples that use LLMs (BasicChain, Workflow, RAG, Education, etc.)

## Quick Start Examples

### Plugin Architecture

```csharp
// See: docs/examples/PLUGIN_ARCHITECTURE.md
services.AddDotNetAgentsEcosystem();
services.AddMyFeature(); // Your plugin
```

### Behavior Trees

```csharp
// See: docs/guides/BEHAVIOR_TREES.md
var tree = BehaviorTreeBuilder
    .Create()
    .Selector()
        .Sequence()
            .Condition(new HasEnemyCondition())
            .Action(new AttackAction())
        .End()
    .End()
    .Build();
```

### Message Bus (Kafka)

```csharp
// See: docs/guides/MESSAGE_BUSES.md
services.AddKafkaMessageBus(options =>
{
    options.BootstrapServers = "localhost:9092";
    options.GroupId = "dotnet-agents";
});
```

### Vector Store (PostgreSQL)

```csharp
// See: docs/guides/VECTOR_STORES.md
services.AddPostgreSQLVectorStore(options =>
{
    options.ConnectionString = connectionString;
    options.Dimension = 1536;
});
```

### RAG Pipeline

```csharp
// See: docs/guides/RAG_GUIDE.md
var documents = await pdfLoader.LoadAsync("doc.pdf");
var chunks = await splitter.SplitAsync(documents);
// ... generate embeddings, store, retrieve, generate
```

## Additional Resources

- [Samples README](../samples/README.md) - All sample projects
- [Integration Guide](guides/INTEGRATION_GUIDE.md) - Complete integration examples
- [API Reference](guides/API_REFERENCE.md) - API documentation
- [Plugin Architecture Examples](examples/PLUGIN_ARCHITECTURE.md) - Plugin examples

---

**Last Updated:** January 2026

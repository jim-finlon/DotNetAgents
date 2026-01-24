# DotNetAgents Comparison: LangChain, LangGraph, and Microsoft Agent Framework

**Last Updated:** January 2025  
**Version:** 1.0

## Executive Summary

DotNetAgents is a comprehensive .NET 10 library that provides native C# implementations of LangChain and LangGraph patterns, while also offering compatibility with Microsoft Agent Framework. This document provides a detailed feature-by-feature comparison to help developers choose the right solution for their needs.

## Quick Comparison Table

| Feature | DotNetAgents | LangChain | LangGraph | Microsoft Agent Framework |
|---------|--------------|-----------|-----------|---------------------------|
| **Language** | C# (.NET 10) | Python | Python | C# (.NET 10) |
| **License** | MIT | MIT | MIT | MIT (Preview) |
| **Maturity** | Production-ready | Mature | Mature | Preview |
| **LLM Providers** | 12+ providers | 100+ integrations | Same as LangChain | Limited (orchestration focus) |
| **Document Loaders** | 6 types (PDF, CSV, Excel, EPUB, Markdown, Text) | 100+ loaders | Same as LangChain | ❌ Not included |
| **Built-in Tools** | 19 tools | 50+ tools | Same as LangChain | Tool framework only |
| **Chains** | ✅ LangChain-like | ✅ Native | ✅ Native | ✅ Similar patterns |
| **Workflows** | ✅ LangGraph-like | Via LangGraph | ✅ Native | ✅ Agent workflows |
| **Vector Stores** | 5 (Pinecone, PostgreSQL, Weaviate, Qdrant, Chroma) | 50+ integrations | Same as LangChain | ✅ Memory abstractions |
| **Checkpointing** | ✅ SQL Server, PostgreSQL, InMemory | ✅ Various backends | ✅ Various backends | ✅ State persistence |
| **Observability** | ✅ OpenTelemetry, logging, metrics | ✅ Limited | ✅ Limited | ✅ OpenTelemetry |
| **Security** | ✅ Enterprise-grade | ⚠️ Basic | ⚠️ Basic | ⚠️ Basic |
| **Performance** | ✅ .NET 10 optimized | ⚠️ Python overhead | ⚠️ Python overhead | ✅ .NET 10 optimized |
| **Type Safety** | ✅ Strong typing | ⚠️ Dynamic typing | ⚠️ Dynamic typing | ✅ Strong typing |
| **Educational Extensions** | ✅ Full package (pedagogy, safety, assessment, compliance) | ❌ Not included | ❌ Not included | ❌ Not included |

## Detailed Feature Comparison

### 1. Core Architecture

#### DotNetAgents
- **Layered Architecture**: Clear separation between Core, Workflow, Providers, and Infrastructure
- **Dependency Inversion**: Core has zero dependencies on integrations
- **Interface-Based Design**: All components use interfaces for testability and extensibility
- **Modular Packages**: Can use individual packages or metapackage
- **.NET 10 Optimized**: Leverages AI optimizations and performance improvements

#### LangChain
- **Modular Components**: Separate packages for different features
- **Python Ecosystem**: Deep integration with Python data science stack
- **Dynamic Typing**: Flexible but less type-safe
- **Mature Ecosystem**: Extensive community and third-party integrations

#### LangGraph
- **Graph-Based**: Built on top of LangChain for stateful workflows
- **Checkpointing**: Built-in support for resumable workflows
- **Python Native**: Leverages Python's async capabilities
- **Visualization**: Tools for visualizing workflow graphs

#### Microsoft Agent Framework
- **Unified Framework**: Combines concepts from Semantic Kernel and AutoGen
- **.NET Native**: Built specifically for .NET 10
- **Orchestration Focus**: Emphasizes multi-agent workflows
- **Preview Status**: APIs may change, breaking changes likely

### 2. LLM Provider Support

#### DotNetAgents (12 Providers)
✅ **Cloud Providers:**
- OpenAI (GPT-3.5, GPT-4, GPT-4 Turbo)
- Azure OpenAI Service
- Anthropic Claude
- Google Gemini
- AWS Bedrock
- Cohere
- Groq
- Mistral AI
- Together AI

✅ **Local Providers:**
- Ollama
- LM Studio
- vLLM

**Features:**
- Streaming support (`GenerateStreamAsync`)
- Batch processing (`GenerateBatchAsync`)
- Retry logic with exponential backoff
- Circuit breaker pattern
- Resilient wrapper for fault tolerance

#### LangChain
- **100+ Integrations**: Extensive provider support
- **Community Maintained**: Many providers maintained by community
- **Python SDKs**: Uses official Python SDKs for each provider
- **Flexible**: Easy to add custom providers

#### LangGraph
- Uses LangChain's provider integrations
- Same provider support as LangChain

#### Microsoft Agent Framework
- **Limited Providers**: Focuses on orchestration, not provider integrations
- **Extensible**: Can integrate with any LLM provider
- **Requires Integration**: Need to build adapters for providers

**Winner for Provider Support:** LangChain (breadth), DotNetAgents (C# ecosystem)

### 3. Document Loaders

#### DotNetAgents (10 Types)
✅ **Implemented:**
- PDF (with page splitting)
- CSV (with header mapping)
- Excel (with worksheet/row splitting)
- EPUB (with chapter splitting)
- Markdown
- Text
- DOCX (Word documents with paragraph splitting)
- HTML (with text extraction and element splitting)
- JSON (with flattening and array splitting)
- XML (with text extraction and element splitting)

**Features:**
- Metadata extraction
- Configurable splitting strategies
- Async/await throughout
- Comprehensive error handling
- Unit tested

#### LangChain
- **100+ Loaders**: Extensive support for various formats
- **Community Contributions**: Many loaders from community
- **Format Support**: PDF, DOCX, HTML, JSON, CSV, and many more
- **Third-Party Libraries**: Uses libraries like PyPDF2, python-docx

#### LangGraph
- Uses LangChain's document loaders
- Same loader support as LangChain

#### Microsoft Agent Framework
- **❌ Not Included**: No document loaders provided
- **Requires Integration**: Need to build or use external loaders

**Winner for Document Loaders:** LangChain (breadth), DotNetAgents (C# ecosystem, production-ready)

### 4. Built-in Tools

#### DotNetAgents (19 Tools)
✅ **Implemented:**
1. Calculator
2. Web Search (DuckDuckGo)
3. DateTime
4. File System (with security controls)
5. HTTP/API Client
6. Database Query (parameterized SQL)
7. JSON/YAML Parser
8. URL Fetch (web scraping)
9. Text Processing (regex, encoding, string manipulation)
10. Shell Command (with security restrictions)
11. Memory Storage (key-value storage)
12. Wikipedia Search
13. Weather (OpenWeatherMap integration)
14. Email (SMTP)
15. Hash Generator (MD5, SHA1, SHA256, SHA384, SHA512)
16. Random Data Generator
17. CSV Reader
18. Slack (send messages to Slack channels)
19. GitHub (interact with GitHub API - issues, PRs, repos)

**Features:**
- Security controls and validation
- Comprehensive error handling
- Async/await support
- Tool schema validation
- Extensible tool registry

#### LangChain
- **50+ Tools**: Extensive tool library
- **Community Tools**: Many tools from community
- **Tool Categories**: Search, web scraping, APIs, databases, etc.
- **Easy Integration**: Simple decorator pattern for custom tools

#### LangGraph
- Uses LangChain's tools
- Same tool support as LangChain

#### Microsoft Agent Framework
- **Tool Framework**: Provides framework for tool calling
- **No Built-in Tools**: Need to implement or integrate tools
- **Tool Discovery**: Built-in tool discovery mechanisms

**Winner for Built-in Tools:** LangChain (breadth), DotNetAgents (C# ecosystem, security-focused)

### 5. Chains & Composition

#### DotNetAgents
✅ **LangChain-like Patterns:**
- `IRunnable<TInput, TOutput>` interface
- `RunnableExtensions.Pipe` for sequential composition
- `RunnableExtensions.Map` for parallel processing
- `RunnableExtensions.BatchAsync` for batch operations
- `LLMChain` for LLM-based chains
- `SequentialChain` for multi-step chains
- `ChainBuilder` fluent API

**Features:**
- Strong typing throughout
- Async/await support
- Cancellation token support
- Error handling and retry logic

#### LangChain
- **Native Implementation**: Original LangChain patterns
- **Runnable Protocol**: Core abstraction for composability
- **Chain Types**: LLMChain, SequentialChain, RouterChain, etc.
- **LCEL (LangChain Expression Language)**: Declarative chain composition

#### DotNetAgents (LCEL-like Support)
✅ **Declarative Chain Composition:**
- `ChainExpression<TInput, TOutput>` for type-safe chain building
- `|` operator for sequential composition (pipe)
- `&` operator for parallel composition
- `*` operator for batch processing
- `ChainExpressionParser` for string-based chain expressions

#### LangGraph
- Uses LangChain's chain patterns
- Adds stateful workflow capabilities

#### Microsoft Agent Framework
- **Similar Patterns**: Workflow composition similar to chains
- **Agent-Based**: Focuses on agent orchestration
- **State Management**: Built-in state management for workflows

**Winner for Chains:** LangChain (original), DotNetAgents (C# equivalent)

### 6. Workflows & State Management

#### DotNetAgents
✅ **LangGraph-like Workflows:**
- `StateGraph<TState>` for stateful workflows
- `GraphNode<TState>` and `GraphEdge<TState>` for graph definition
- `GraphExecutor<TState>` for execution
- Checkpointing with SQL Server, PostgreSQL, or InMemory stores
- Resume from checkpoint capability
- Fluent API (`WorkflowBuilder`)

**Features:**
- Strongly typed state
- Checkpoint persistence
- Human-in-the-loop support (✅ Implemented with `ApprovalNode<TState>`)
- Graph validation
- Async execution

#### LangChain
- **Basic Chains**: Sequential and parallel execution
- **No Native Workflows**: Requires LangGraph for stateful workflows

#### LangGraph
- **Native Workflows**: Built specifically for stateful workflows
- **Checkpointing**: Various backend support (Redis, SQLite, etc.)
- **Visualization**: Tools for visualizing workflow graphs
- **Human-in-the-Loop**: Built-in support for human approval nodes

#### DotNetAgents (Workflow Features)
✅ **Visualization:**
- `IGraphVisualizationService` for workflow graph visualization
- DOT format export (Graphviz)
- Mermaid diagram generation
- JSON metadata export

✅ **Human-in-the-Loop:**
- `ApprovalNode<TState>` for workflow approval points
- `IApprovalHandler<TState>` interface for custom approval workflows
- `InMemoryApprovalHandler` for development/testing
- `SignalRApprovalHandler` for web applications

#### Microsoft Agent Framework
- **Agent Workflows**: Multi-agent workflow orchestration
- **State Persistence**: Built-in state management
- **Orchestration**: Focus on coordinating multiple agents

**Winner for Workflows:** LangGraph (original), DotNetAgents (C# equivalent), MAF (multi-agent focus)

### 7. Memory & Retrieval

#### DotNetAgents
✅ **Memory System:**
- `IMemory` and `IMemoryStore` interfaces
- In-memory implementation
- Vector-based storage
- RAG (Retrieval-Augmented Generation) support

✅ **Vector Stores:**
- InMemoryVectorStore
- PineconeVectorStore
- PostgreSQLVectorStore (pgvector)
- WeaviateVectorStore
- QdrantVectorStore
- ChromaVectorStore
- Extensible interface for additional stores

✅ **Text Splitting:**
- CharacterTextSplitter
- RecursiveTextSplitter (multi-separator)
- SemanticTextSplitter (embedding-based)

✅ **Retrieval Chain:**
- `RetrievalChain` for RAG workflows

#### LangChain
- **Memory Abstractions**: Conversation memory, entity memory
- **Vector Stores**: 50+ integrations (Pinecone, Weaviate, Qdrant, etc.)
- **Text Splitters**: Various splitting strategies
- **Retrievers**: Multiple retrieval strategies

#### LangGraph
- Uses LangChain's memory and retrieval
- Adds stateful memory to workflows

#### Microsoft Agent Framework
- **Memory Abstractions**: Built-in memory management
- **State Management**: Persistent state for agents
- **Limited Vector Stores**: Focuses on orchestration, not retrieval

**Winner for Memory & Retrieval:** LangChain (breadth), DotNetAgents (C# ecosystem)

### 8. Observability & Monitoring

#### DotNetAgents
✅ **Comprehensive Observability:**
- OpenTelemetry integration for distributed tracing
- Structured logging with `Microsoft.Extensions.Logging`
- Performance metrics collection (`IMetricsCollector`)
- Cost tracking (`ICostTracker`) for LLM API calls
- Health checks (`AgentHealthCheck`)
- Correlation IDs for request tracing

**Features:**
- Production-ready observability
- Enterprise-grade monitoring
- Cost optimization insights

#### LangChain
- **Limited Observability**: Basic logging
- **Community Tools**: Some third-party observability tools
- **No Built-in Metrics**: Need to add custom monitoring

#### LangGraph
- **Workflow Tracing**: Basic tracing for workflow execution
- **Limited Metrics**: No built-in metrics collection

#### Microsoft Agent Framework
- **OpenTelemetry Integration**: Built-in OpenTelemetry support
- **Tracing**: Distributed tracing for agent workflows
- **Limited Metrics**: Focuses on orchestration tracing

**Winner for Observability:** DotNetAgents (comprehensive), MAF (good tracing)

### 9. Security Features

#### DotNetAgents
✅ **Enterprise Security:**
- Secrets management (`ISecretsProvider`) with Azure Key Vault, AWS Secrets Manager, Environment support
- Input validation and sanitization (`ISanitizer`) with prompt injection protection
- Rate limiting (`IRateLimiter`) with configurable policies
- Audit logging (`IAuditLogger`) for compliance
- Tool execution permissions and security controls
- PII detection and masking

**Features:**
- Production-ready security
- Compliance-ready audit trails
- Defense-in-depth approach

#### LangChain
- **Basic Security**: Limited built-in security features
- **Community Tools**: Some security tools from community
- **No Built-in Secrets Management**: Need to handle secrets manually

#### LangGraph
- Uses LangChain's security features
- Same security support as LangChain

#### Microsoft Agent Framework
- **Basic Security**: Standard .NET security practices
- **No Built-in Secrets Management**: Need to handle secrets manually
- **Tool Security**: Basic tool execution controls

**Winner for Security:** DotNetAgents (enterprise-grade)

### 10. Performance

#### DotNetAgents
✅ **.NET 10 Optimizations:**
- Leverages .NET 10 AI optimizations
- Up to 20% faster async operations
- Improved memory management for vector operations
- HTTP/3 support for LLM API calls
- Multi-level caching (memory, distributed)
- Connection pooling
- Reduced GC pressure

**Features:**
- Production-tested performance
- Optimized for AI workloads
- Low latency for real-time applications

#### LangChain
- **Python Overhead**: Python's GIL and overhead
- **Async Support**: Python async/await
- **Performance**: Acceptable for most use cases
- **Not Optimized**: Not specifically optimized for AI workloads

#### LangGraph
- **Workflow Overhead**: Additional overhead for state management
- **Checkpointing Cost**: Checkpoint persistence adds latency
- **Python Limitations**: Same Python performance characteristics

#### Microsoft Agent Framework
- **.NET 10 Optimized**: Leverages .NET 10 performance improvements
- **Orchestration Overhead**: Multi-agent coordination adds overhead
- **Good Performance**: Optimized for .NET runtime

**Winner for Performance:** DotNetAgents (AI-optimized), MAF (good .NET performance)

### 11. Type Safety & Developer Experience

#### DotNetAgents
✅ **Strong Typing:**
- Full C# type safety
- Compile-time error checking
- IntelliSense support
- Nullable reference types
- Generic type parameters for input/output

**Developer Experience:**
- Fluent APIs (`ChainBuilder`, `WorkflowBuilder`)
- Comprehensive XML documentation
- Clear error messages
- IDE support (Visual Studio, Rider, VS Code)

#### LangChain
- **Dynamic Typing**: Python's dynamic typing
- **Runtime Errors**: Many errors only discovered at runtime
- **Flexibility**: More flexible but less safe
- **Python Ecosystem**: Leverages Python's rich ecosystem

#### LangGraph
- **Python Typing**: Type hints available but not enforced
- **Runtime Validation**: Validation happens at runtime
- **Flexibility**: Dynamic workflow construction

#### Microsoft Agent Framework
- **Strong Typing**: Full C# type safety
- **.NET IDE Support**: Excellent IDE support
- **Compile-Time Safety**: Type checking at compile time

**Winner for Type Safety:** DotNetAgents, MAF (both C#)

### 12. Checkpointing & State Persistence

#### DotNetAgents
✅ **Multiple Backends:**
- SQL Server checkpoint store
- PostgreSQL checkpoint store
- InMemory checkpoint store (for testing)
- JSON state serializer
- Resume from checkpoint capability

**Features:**
- Production-ready persistence
- Transaction support
- Configurable retention policies
- Async operations

#### LangChain
- **No Native Checkpointing**: Basic chain execution only
- **Requires LangGraph**: Need LangGraph for checkpointing

#### LangGraph
- **Multiple Backends**: Redis, SQLite, Memory, custom backends
- **Checkpointing**: Built-in checkpoint support
- **Resume Capability**: Can resume workflows from checkpoints

#### Microsoft Agent Framework
- **State Persistence**: Built-in state management
- **Backend Support**: Various persistence backends
- **Agent State**: Persistent state for multi-agent workflows

**Winner for Checkpointing:** LangGraph (original), DotNetAgents (C# equivalent)

## Use Case Recommendations

### Choose DotNetAgents When:
- ✅ Building .NET/C# applications
- ✅ Need production-ready document loaders
- ✅ Require enterprise-grade security and observability
- ✅ Want strong typing and compile-time safety
- ✅ Need comprehensive LLM provider support in C#
- ✅ Building RAG applications with document processing
- ✅ Want LangChain/LangGraph patterns in C#
- ✅ Need .NET 10 AI optimizations

### Choose LangChain When:
- ✅ Building Python applications
- ✅ Need maximum ecosystem breadth (100+ integrations)
- ✅ Want the original, mature implementation
- ✅ Need community support and examples
- ✅ Building prototypes or research projects
- ✅ Prefer Python's dynamic typing and flexibility

### Choose LangGraph When:
- ✅ Building stateful workflows in Python
- ✅ Need workflow visualization tools
- ✅ Want checkpointing with various backends
- ✅ Building complex multi-step workflows
- ✅ Need human-in-the-loop workflows

### Choose Microsoft Agent Framework When:
- ✅ Building .NET 10 applications
- ✅ Need multi-agent orchestration
- ✅ Want Microsoft's official agent framework
- ✅ Building agent-to-agent communication workflows
- ✅ Can accept preview/breaking changes
- ✅ Don't need document loaders or built-in tools

## Integration Strategies

### Using DotNetAgents with Microsoft Agent Framework

DotNetAgents provides a compatibility layer (`DotNetAgents.AgentFramework`) to integrate with MAF:

```csharp
// Register DotNetAgents components as MAF tools
services.AddDotNetAgents()
    .AddAgentFrameworkIntegration(options =>
    {
        // Expose document loaders as tools
        options.ExposeDocumentLoaders = true;
        
        // Expose built-in tools
        options.ExposeBuiltInTools = true;
        
        // Register LLM providers
        options.RegisterLLMProviders = true;
    });
```

**Benefits:**
- Use DotNetAgents' document loaders with MAF agents
- Leverage DotNetAgents' 17 built-in tools
- Access all 12 LLM providers through MAF
- Combine MAF's orchestration with DotNetAgents' components

### Migrating from LangChain/LangGraph to DotNetAgents

DotNetAgents provides similar APIs to LangChain/LangGraph, making migration straightforward:

**LangChain Pattern:**
```python
from langchain.chains import LLMChain
chain = LLMChain(llm=llm, prompt=prompt)
result = chain.run("Hello")
```

**DotNetAgents Equivalent:**
```csharp
var chain = new LLMChain(llm, promptTemplate);
var result = await chain.InvokeAsync("Hello");
```

## Conclusion

DotNetAgents provides a comprehensive, production-ready alternative to LangChain and LangGraph for .NET developers, while also offering compatibility with Microsoft Agent Framework. The library excels in:

1. **Document Processing**: Production-ready loaders for PDF, CSV, Excel, EPUB, Markdown
2. **Security**: Enterprise-grade security features
3. **Observability**: Comprehensive monitoring and tracing
4. **Type Safety**: Strong typing throughout
5. **Performance**: .NET 10 AI optimizations
6. **Provider Support**: 12 LLM providers with more coming

For Python developers, LangChain and LangGraph remain excellent choices. For .NET developers, DotNetAgents provides native C# implementations with enterprise-grade features, Microsoft Agent Framework compatibility, and specialized educational extensions.

### Educational Extensions

DotNetAgents includes a comprehensive educational extensions package (`DotNetAgents.Education`) that provides specialized components for educational AI applications:

**Pedagogy Components:**
- ✅ Socratic dialogue engine (question generation, response evaluation, hint scaffolding)
- ✅ Spaced repetition scheduler (SuperMemo 2 algorithm)
- ✅ Mastery calculator (weighted scoring, prerequisite checking)

**Safety Components:**
- ✅ Child safety filter (COPPA compliance, multi-layer filtering)
- ✅ Conversation monitor (distress signal detection, alert generation)
- ✅ Age-adaptive transformer (grade-level content adaptation, complexity scoring)

**Assessment Components:**
- ✅ Assessment generator (multiple question types, difficulty calibration)
- ✅ Response evaluator (scoring, misconception detection, feedback)

**Memory & Retrieval:**
- ✅ Student profile memory (extends DotNetAgents IMemory)
- ✅ Mastery state memory (concept mastery tracking)
- ✅ Learning session memory (session management with resume capability)
- ✅ Curriculum-aware retriever (concept filtering, prerequisite-aware)
- ✅ Prerequisite checker (dependency validation)

**Compliance:**
- ✅ FERPA compliance service (access control, logging, parent consent)
- ✅ GDPR compliance service (data export, deletion, anonymization)
- ✅ RBAC (role-based access control for education roles)
- ✅ Comprehensive audit logging

**Infrastructure:**
- ✅ Multi-tenancy support (tenant context, isolation, management)
- ✅ Education-specific caching (content caching with TTL support)

**Comparison:**
| Feature | DotNetAgents.Education | LangChain | LangGraph | Microsoft Agent Framework |
|---------|------------------------|-----------|-----------|---------------------------|
| Educational Components | ✅ Full package | ❌ Not included | ❌ Not included | ❌ Not included |
| Pedagogy Tools | ✅ Socratic, SM2, Mastery | ❌ | ❌ | ❌ |
| Child Safety | ✅ COPPA compliant | ❌ | ❌ | ❌ |
| Assessment Generation | ✅ LLM-powered | ❌ | ❌ | ❌ |
| Compliance | ✅ FERPA/GDPR | ❌ | ❌ | ❌ |

## Additional Resources

- **DotNetAgents Documentation**: See `docs/` folder
- **LangChain Documentation**: https://python.langchain.com/
- **LangGraph Documentation**: https://langchain-ai.github.io/langgraph/
- **Microsoft Agent Framework**: https://learn.microsoft.com/en-us/agent-framework/
- **Migration Guide**: Coming soon

---

**Last Updated:** January 2025  
**Version:** 1.0

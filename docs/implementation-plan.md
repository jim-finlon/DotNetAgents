# DotNetAgents Library - Implementation Plan

**Version:** 1.0  
**Date:** 2024  
**Status:** Draft

## Overview

This plan outlines the development of a comprehensive .NET 10 library (`DotNetAgents`) that replicates and adapts LangChain and LangGraph functionality for C# developers. The library enables building AI agents, chains, and complex workflows with enterprise-grade quality, security, and performance. Built on .NET 10 (LTS) to leverage AI optimizations and Microsoft Agent Framework compatibility.

## Architecture Overview

The library follows a layered, modular architecture with clear separation of concerns:

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

## Package Architecture (Hybrid Approach)

The library uses a **hybrid modular approach**: code is structured as separate projects from day one, but initially published as a metapackage for simplicity. This allows:
- **Simple installation**: Users can install `DotNetAgents` (metapackage) for everything
- **Granular control**: Advanced users can reference individual packages
- **Easy splitting**: Can split into separate packages later without refactoring
- **Dependency isolation**: Core has zero dependencies on integrations

## Project Structure

```
DotNetAgents/
├── src/
│   ├── DotNetAgents.Core/              # Core abstractions (NO integration deps)
│   │   ├── Models/                      # ILLMModel, IEmbeddingModel
│   │   ├── Prompts/                     # IPromptTemplate, PromptBuilder
│   │   ├── Tools/                       # ITool, ToolRegistry
│   │   ├── Memory/                      # IMemory, IMemoryStore
│   │   ├── Documents/                   # IDocument, IDocumentLoader
│   │   ├── Retrieval/                   # IVectorStore, IRetriever
│   │   ├── Chains/                      # IRunnable, Chain composition
│   │   ├── Configuration/               # Configuration interfaces
│   │   └── Execution/                   # ExecutionContext
│   │
│   ├── DotNetAgents.Workflow/           # LangGraph-like workflow engine
│   │   ├── Graph/                       # StateGraph, Node, Edge
│   │   ├── Execution/                   # GraphExecutor, StateManager
│   │   ├── Checkpoints/                 # CheckpointStore, Resume logic
│   │   └── HumanInLoop/                 # Approval nodes, callbacks
│   │
│   ├── DotNetAgents.Providers.OpenAI/   # OpenAI provider (separate package)
│   │   ├── OpenAIModel.cs
│   │   ├── OpenAIClient.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── DotNetAgents.Providers.Azure/    # Azure OpenAI provider
│   │   ├── AzureOpenAIModel.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── DotNetAgents.Providers.Anthropic/ # Anthropic provider
│   │   ├── AnthropicModel.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   ├── DotNetAgents.VectorStores.Pinecone/ # Pinecone integration
│   │   └── PineconeStore.cs
│   │
│   ├── DotNetAgents.VectorStores.InMemory/ # In-memory vector store
│   │   └── InMemoryVectorStore.cs
│   │
│   ├── DotNetAgents.Configuration/       # Configuration management
│   │   ├── AgentConfiguration.cs
│   │   ├── ConfigurationBuilder.cs
│   │   └── ConfigurationValidators.cs
│   │
│   ├── DotNetAgents.Observability/       # Observability
│   │   ├── Logging/                     # Structured logging
│   │   ├── Tracing/                     # OpenTelemetry integration
│   │   ├── Metrics/                     # Performance metrics, cost tracking
│   │   └── HealthChecks/                 # Health check implementations
│   │
│   ├── DotNetAgents.Security/            # Security features
│   │   ├── RateLimiting/                # Rate limiting
│   │   ├── Sanitization/                # Input/output sanitization
│   │   └── Secrets/                     # Secrets management
│   │
│   ├── DotNetAgents.SourceGenerators/    # Source generators
│   │   ├── PromptTemplateGenerator/      # Prompt template source gen
│   │   ├── ToolSchemaGenerator/         # Tool schema generator
│   │   └── StateTypeGenerator/           # State type generator
│   │
│   ├── DotNetAgents.Analyzers/           # Diagnostic analyzers
│   │   └── AgentDiagnosticAnalyzer.cs
│   │
│   └── DotNetAgents/                     # Metapackage (references all)
│       └── ServiceCollectionExtensions.cs
│
├── tests/
│   ├── DotNetAgents.Core.Tests/
│   ├── DotNetAgents.Workflow.Tests/
│   ├── DotNetAgents.Providers.OpenAI.Tests/
│   ├── DotNetAgents.Providers.Azure.Tests/
│   ├── DotNetAgents.Providers.Anthropic.Tests/
│   ├── DotNetAgents.Configuration.Tests/
│   ├── DotNetAgents.Observability.Tests/
│   ├── DotNetAgents.Security.Tests/
│   └── DotNetAgents.Tests.Integration/  # End-to-end tests
│
├── samples/
│   ├── SimpleChain/                     # Basic chain example
│   ├── RAGExample/                      # RAG pipeline example
│   ├── AgentExample/                    # Simple agent example
│   ├── WorkflowExample/                 # LangGraph-like workflow
│   └── ConfigurationExample/           # Configuration examples
│
├── docs/
│   ├── requirements.md
│   ├── technical-specification.md
│   ├── implementation-plan.md
│   ├── recommendations.md
│   ├── package-strategy-analysis.md
│   ├── CONTRIBUTING.md                  # Contribution guidelines
│   ├── CODE_OF_CONDUCT.md               # Code of conduct
│   ├── LICENSE                          # Open source license
│   └── architecture/                    # Architecture docs
│
└── .github/
    ├── workflows/
    │   ├── ci.yml                       # CI pipeline
    │   ├── release.yml                  # Release pipeline
    │   └── benchmarks.yml               # Performance benchmarks
    ├── ISSUE_TEMPLATE/                  # Issue templates
    └── PULL_REQUEST_TEMPLATE.md         # PR template
```

## Core Components

### 1. Core Abstractions (`DotNetAgents.Core`)

**Key Interfaces:**

- `ILLMModel<TInput, TOutput>` - LLM provider abstraction with async/streaming support
- `IEmbeddingModel` - Embedding generation
- `IPromptTemplate` - Template engine with variable substitution
- `ITool` - Tool interface for external integrations
- `IRunnable<TInput, TOutput>` - Core chain/runnable abstraction
- `IVectorStore` - Vector database operations
- `IMemory` - Short-term and long-term memory
- `IDocumentLoader` - Document loading and parsing

**Design Principles:**

- All interfaces support async/await
- Streaming via `IAsyncEnumerable<T>`
- Dependency injection ready
- Immutable where possible

### 2. Workflow Engine (`DotNetAgents.Workflow`)

**Key Components:**

- `StateGraph<TState>` - Graph-based workflow definition
- `GraphNode` - Workflow nodes (LLM calls, tools, conditionals)
- `GraphEdge` - Conditional and unconditional edges
- `GraphExecutor` - Executes workflows with state management
- `CheckpointStore` - Persists workflow state for resumability
- `HumanApprovalNode` - Human-in-the-loop support

**State Management:**

- Serializable state objects
- Checkpointing at configurable intervals
- Resume from failure support
- State versioning

### 3. Integrations (`DotNetAgents.Integrations`)

**LLM Providers:**

- OpenAI (GPT-3.5, GPT-4, GPT-4 Turbo)
- Azure OpenAI Service
- Anthropic Claude
- Extensible for local models (Ollama, etc.)

**Vector Stores:**

- Pinecone
- Weaviate
- In-memory (for testing)
- Extensible interface for others

**Storage Providers:**

- SQL Server / PostgreSQL (for checkpoints)
- Redis (for caching)
- Azure Blob Storage (for large state)

### 4. Observability (`DotNetAgents.Observability`)

- Structured logging via `ILogger`
- OpenTelemetry tracing
- Performance metrics (latency, token counts, costs)
- Execution graph visualization data

## Implementation Phases

**Note:** Timeline is flexible and quality-focused. Each phase includes comprehensive testing, code reviews, and documentation.

### Phase 1: Foundation & Project Setup (Weeks 1-2)

**Deliverables:**

- Solution structure with modular projects
- CI/CD pipeline setup (GitHub Actions)
- Code analysis rules (StyleCop, analyzers)
- Coding standards document
- Project templates and scaffolding
- Basic README and contribution guidelines

**Key Files:**

- `.github/workflows/ci.yml`
- `Directory.Build.props` (shared properties)
- `CONTRIBUTING.md`
- `CODE_OF_CONDUCT.md`
- `LICENSE`

**Success Criteria:**
- All projects build successfully
- CI pipeline runs on every commit
- Code analysis passes
- Contribution guidelines published

### Phase 2: Core Abstractions (Weeks 3-5)

**Deliverables:**

- Core interfaces (`ILLMModel`, `IPromptTemplate`, `IRunnable`, `ITool`, `IVectorStore`, `IMemory`)
- Execution context implementation
- Basic prompt template engine
- Simple chain composition
- Factory interfaces
- Unit test framework setup
- Contract test base classes

**Key Files:**

- `src/DotNetAgents.Core/Models/ILLMModel.cs`
- `src/DotNetAgents.Core/Prompts/PromptTemplate.cs`
- `src/DotNetAgents.Core/Chains/Runnable.cs`
- `src/DotNetAgents.Core/Execution/ExecutionContext.cs`
- `tests/DotNetAgents.Core.Tests/`

**Success Criteria:**
- All core interfaces defined
- >80% test coverage
- All contract tests pass
- XML documentation complete

### Phase 3: Configuration Management (Weeks 6-7)

**Deliverables:**

- Configuration interfaces and builders
- AppSettings.json integration
- Environment variable support
- Configuration validation
- Fluent API for configuration

**Key Files:**

- `src/DotNetAgents.Configuration/`
- `src/DotNetAgents.Core/Configuration/`

**Success Criteria:**
- Configuration system fully functional
- Validation working
- Documentation complete
- Examples provided

### Phase 4: LLM Provider Integrations (Weeks 8-12)

**Deliverables:**

- OpenAI provider (full implementation)
- Azure OpenAI provider
- Anthropic provider
- Streaming support for all providers
- Error handling and retry logic
- Circuit breaker implementation
- Factory implementations
- Integration tests

**Key Files:**

- `src/DotNetAgents.Providers.OpenAI/`
- `src/DotNetAgents.Providers.Azure/`
- `src/DotNetAgents.Providers.Anthropic/`
- `tests/DotNetAgents.Providers.*.Tests/`

**Success Criteria:**
- All providers fully functional
- Streaming working correctly
- Error handling robust
- Integration tests passing
- Performance benchmarks meet targets

### Phase 5: Memory & Retrieval (Weeks 13-15)

**Deliverables:**

- Memory interfaces and implementations
- Document loaders (text, PDF, markdown)
- Vector store abstraction
- Pinecone integration
- In-memory vector store (for testing)
- Basic RAG chain example
- Chunking strategies

**Key Files:**

- `src/DotNetAgents.Core/Memory/`
- `src/DotNetAgents.Core/Retrieval/`
- `src/DotNetAgents.VectorStores.Pinecone/`
- `src/DotNetAgents.VectorStores.InMemory/`
- `samples/RAGExample/`

**Success Criteria:**
- Memory system functional
- Vector stores working
- RAG example complete
- Performance acceptable

### Phase 6: Tools & Agents (Weeks 16-18) ✅ COMPLETED

**Deliverables:**

- ✅ Tool interface and registry (`ITool`, `ToolRegistry`)
- ✅ Simple agent executor (ReAct pattern) (`AgentExecutor`)
- ✅ Built-in tools (17 total):
  - ✅ Calculator tool
  - ✅ Web search tool (DuckDuckGo)
  - ✅ DateTime tool
  - ✅ File system tool (with security controls)
  - ✅ HTTP/API Client tool
  - ✅ Database Query tool (parameterized SQL)
  - ✅ JSON/YAML Parser tool
  - ✅ URL Fetch tool (web scraping)
  - ✅ Text Processing tool (regex, encoding, string manipulation)
  - ✅ Shell Command tool (with security restrictions)
  - ✅ Memory Storage tool (key-value storage)
  - ✅ Wikipedia Search tool
  - ✅ Weather tool (OpenWeatherMap integration)
  - ✅ Email tool (SMTP)
  - ✅ Hash Generator tool (MD5, SHA1, SHA256, SHA384, SHA512)
  - ✅ Random Data Generator tool
  - ✅ CSV Reader tool
- ✅ Tool schema validation
- ✅ Tool execution permissions (via security controls)

**Key Files:**

- `src/DotNetAgents.Core/Tools/`
- `src/DotNetAgents.Core/Tools/BuiltIn/`
- `src/DotNetAgents.Core/Agents/`

**Status:**
- ✅ Tool system functional
- ✅ Agent executor working
- ✅ All built-in tools implemented and tested
- ✅ Sample examples:
  - ✅ Basic Chain example
  - ✅ Agent with Tools example
  - ✅ Workflow example
  - ✅ RAG (Retrieval-Augmented Generation) example

### Phase 7: Workflow Engine (Weeks 19-23) ✅ COMPLETED

**Deliverables:**

- ✅ StateGraph implementation
- ✅ Graph execution engine
- ✅ Node and edge definitions
- ✅ State management
- ✅ Graph validation
- ✅ Simple workflow example
- ✅ Fluent API for graph building (`WorkflowBuilder`)

**Key Files:**

- `src/DotNetAgents.Workflow/Graph/`
- `src/DotNetAgents.Workflow/Execution/`
- `samples/DotNetAgents.Samples.Workflow/`

**Status:**
- ✅ Workflow engine functional
- ✅ State management working
- ✅ Examples complete
- ✅ Performance acceptable

### Phase 8: State Persistence & Checkpoints (Weeks 24-26) ✅ COMPLETED

**Deliverables:**

- ✅ Checkpoint store interface
- ✅ SQL Server checkpoint implementation
- ✅ PostgreSQL checkpoint implementation
- ✅ Resume from checkpoint logic
- ✅ State serialization/deserialization
- ✅ Checkpoint cleanup/expiration
- ✅ State versioning

**Key Files:**

- `src/DotNetAgents.Workflow/Checkpoints/`
- `src/DotNetAgents.Storage.SqlServer/`
- `src/DotNetAgents.Storage.PostgreSQL/`

**Status:**
- ✅ Checkpointing working
- ✅ Multiple storage providers available
- ✅ Resume functionality implemented
- ✅ Performance acceptable
- ✅ Documentation complete

### Phase 9: Observability (Weeks 27-29)

**Deliverables:**

- OpenTelemetry integration
- Structured logging extensions
- Performance metrics collection
- Cost tracking implementation
- Health checks
- Execution graph visualization data export

**Key Files:**

- `src/DotNetAgents.Observability/`
- `src/DotNetAgents.Observability/Metrics/CostTracker.cs`

**Success Criteria:**
- Observability fully functional
- Cost tracking accurate
- Health checks working
- Metrics exported correctly

### Phase 10: Security Features (Weeks 30-32)

**Deliverables:**

- Secrets management (Azure Key Vault, AWS, Environment)
- Input validation and sanitization
- Rate limiting implementation
- Prompt injection detection
- PII detection and masking
- Audit logging
- Security documentation

**Key Files:**

- `src/DotNetAgents.Security/`
- `docs/security.md`

**Success Criteria:**
- Security features implemented
- Security audit passed
- Documentation complete
- Examples provided

### Phase 11: Performance & Caching (Weeks 33-34)

**Deliverables:**

- Multi-level caching implementation
- Embedding cache
- LLM response cache
- Prompt template cache
- Cache invalidation strategies
- Connection pooling optimization
- Performance benchmarks

**Key Files:**

- `src/DotNetAgents.Core/Caching/`
- `benchmarks/`

**Success Criteria:**
- Caching working correctly
- Performance targets met
- Benchmarks documented

### Phase 12: Source Generators & Analyzers (Weeks 35-37)

**Deliverables:**

- Prompt template source generator
- Tool schema generator
- State type generator
- Diagnostic analyzers
- Code fix providers
- Documentation

**Key Files:**

- `src/DotNetAgents.SourceGenerators/`
- `src/DotNetAgents.Analyzers/`

**Success Criteria:**
- Source generators working
- Analyzers functional
- Developer experience improved
- Documentation complete

### Phase 13: Fluent APIs & Developer Experience (Weeks 38-39)

**Deliverables:**

- Chain builder fluent API
- Configuration builder fluent API
- Workflow builder fluent API
- Extension methods for common patterns
- Improved IntelliSense support

**Key Files:**

- `src/DotNetAgents.Core/Chains/ChainBuilder.cs`
- `src/DotNetAgents.Configuration/ConfigurationBuilder.cs`

**Success Criteria:**
- Fluent APIs intuitive
- Developer experience excellent
- Examples updated

### Phase 14: Human-in-the-Loop (Weeks 40-41)

**Deliverables:**

- Human approval nodes
- Workflow pause/resume
- State inspection APIs
- Callback mechanisms
- Examples

**Key Files:**

- `src/DotNetAgents.Workflow/HumanInLoop/`

**Success Criteria:**
- Human-in-loop working
- Examples complete
- Documentation complete

### Phase 15: Testing & Quality Assurance (Weeks 42-44)

**Deliverables:**

- Comprehensive unit tests (>85% coverage)
- Integration tests for all providers
- Contract tests
- Performance tests
- Security tests
- Load tests
- Test infrastructure improvements

**Success Criteria:**
- >85% test coverage
- All tests passing
- Performance benchmarks documented
- Security tests passed

### Phase 16: Documentation & Samples (Weeks 45-48)

**Deliverables:**

- Complete API documentation (XML docs)
- Getting started guide
- Architecture documentation
- Migration guide from LangChain
- Troubleshooting guide
- Sample applications (all scenarios)
- Video tutorials (optional)
- Interactive documentation (optional)

**Key Files:**

- `docs/api/`
- `docs/guides/`
- `docs/architecture/`
- `samples/`

**Success Criteria:**
- Documentation complete
- All samples working
- Migration guide helpful
- User feedback positive

### Phase 17: Open Source Preparation (Weeks 49-50)

**Deliverables:**

- License file (MIT recommended)
- Contributing guidelines finalized
- Code of conduct
- Issue templates
- PR templates
- Release notes template
- Community guidelines
- GitHub repository setup

**Success Criteria:**
- Open source ready
- Community guidelines clear
- Contribution process smooth

### Phase 18: NuGet Packaging & Release (Weeks 51-52)

**Deliverables:**

- NuGet package configuration
- Package metadata
- Versioning strategy
- Release pipeline
- Pre-release testing
- Beta release
- Community feedback collection
- Final release (v1.0.0)

**Success Criteria:**
- NuGet packages published
- Release process documented
- Community feedback incorporated
- v1.0.0 released

**Total Timeline: ~52 weeks (1 year)**

## Technology Stack

- **.NET 8** - Target framework (LTS)
- **Microsoft.Extensions.DependencyInjection** - DI container
- **System.Text.Json** - JSON serialization
- **System.Net.Http** - HTTP client for API calls
- **OpenTelemetry** - Tracing and metrics
- **Microsoft.Extensions.Logging** - Logging abstraction
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Test assertions

## Security Considerations

1. **Secrets Management:**

   - Integration with Azure Key Vault, AWS Secrets Manager
   - No hardcoded API keys
   - Secure credential storage

2. **Input Validation:**

   - Prompt injection protection
   - Input sanitization
   - Output validation

3. **Data Protection:**

   - Encryption at rest for checkpoints
   - TLS for all external calls
   - PII handling guidelines

4. **Access Control:**

   - Tool execution permissions
   - State access controls
   - Audit logging

## Performance Optimizations

1. **Caching:**

   - Embedding cache (reduce API calls)
   - LLM response cache (configurable TTL)
   - Prompt template cache

2. **Concurrency:**

   - Parallel tool execution
   - Batch processing support
   - Async/await throughout

3. **Resource Management:**

   - Connection pooling for HTTP clients
   - Disposable pattern for resources
   - Memory-efficient streaming

## Testing Strategy

1. **Unit Tests:** Core logic, isolated components
2. **Integration Tests:** Provider integrations (with mocks)
3. **Contract Tests:** Interface compliance
4. **Performance Tests:** Latency, throughput benchmarks
5. **Security Tests:** Input validation, injection tests

## Documentation Requirements

1. **API Documentation:** XML comments for all public APIs
2. **Getting Started Guide:** Quick start tutorial
3. **Architecture Guide:** System design and patterns
4. **Sample Applications:** Working examples
5. **Migration Guide:** From Python LangChain (if applicable)

## Success Criteria

- MVP supports basic chains, LLM calls, and simple agents
- Multiple LLM providers integrated
- Workflow engine supports stateful, resumable workflows
- Comprehensive test coverage (>80%)
- Performance benchmarks meet targets
- Security audit passed
- Documentation complete
- NuGet package published

## Next Steps After MVP

- Advanced memory strategies (summarization, pruning)
- More vector store integrations
- Additional built-in tools
- Graph visualization tools
- Multi-agent coordination
- Cost tracking and optimization
- Advanced human-in-the-loop features
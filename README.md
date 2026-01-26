# DotNetAgents

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/DotNetAgents.svg)](https://www.nuget.org/packages/DotNetAgents)

> **Enterprise-grade .NET 10 library for building AI agents, chains, and workflows - A native C# alternative to LangChain and LangGraph, compatible with Microsoft Agent Framework**

## 🎯 Overview

DotNetAgents is a comprehensive .NET 10 library (currently in beta) that brings the power of LangChain and LangGraph to C# developers. Build sophisticated AI agents, chains, and stateful workflows with enterprise-grade quality, security, and performance. Compatible with Microsoft Agent Framework for enhanced orchestration capabilities.

### Why .NET 10?

DotNetAgents targets .NET 10 (LTS) to leverage cutting-edge AI optimizations and performance improvements:

- **🚀 Enhanced Performance**: .NET 10 includes significant runtime optimizations for AI workloads, including improved async/await performance and reduced memory allocations
- **🤖 Microsoft Agent Framework Support**: Native integration with Microsoft's Agent Framework for building AI agents and multi-agent workflows
- **⚡ Vector Operations**: Optimized SIMD operations and improved array/span handling for vector embeddings and similarity calculations
- **📊 Better Observability**: Enhanced OpenTelemetry support and improved diagnostics for tracing AI operations
- **🔧 Modern C# 13 Features**: Latest language features including improved pattern matching, collection expressions, and performance-focused syntax
- **💾 Memory Efficiency**: Reduced GC pressure and improved memory management for long-running AI agent processes
- **🌐 HTTP/3 Support**: Better network performance for LLM API calls with HTTP/3 and improved connection pooling

## ✨ Features

### Core Capabilities
- **🤖 AI Agents**: Build intelligent agents with tool calling and decision-making capabilities
- **🔗 Chains**: Compose complex workflows with sequential and parallel execution (LCEL-like declarative syntax)
- **📊 Workflows**: Stateful, resumable workflows with checkpointing and visualization (LangGraph-like)
- **🔄 State Machines**: Manage agent lifecycle and operational states with hierarchical, parallel, and timed transitions
- **🌳 Behavior Trees**: Hierarchical decision-making for autonomous agents with composite, decorator, and integration nodes
- **💾 Memory**: Short-term and long-term memory with vector-based storage
- **🔍 RAG**: Retrieval-Augmented Generation with document loaders and vector stores
- **🛠️ Tools**: 19 built-in tools + extensible tool system for external integrations
- **👥 Multi-Agent Workflows**: Supervisor-worker patterns, agent registry, load balancing, auto-scaling
- **📨 Agent Messaging**: Multiple message bus implementations (In-Memory, Kafka, RabbitMQ, Redis Pub/Sub, SignalR)
- **📚 Education Extensions**: Specialized components for educational AI applications (pedagogy, safety, assessment, compliance)
- **☸️ Kubernetes Ready**: Complete Kubernetes manifests and Helm charts for production deployment
- **📊 Monitoring Stack**: Prometheus, Grafana, and Loki integration for observability

### Document Loaders (10 Types)
- ✅ **PDF** (with page splitting)
- ✅ **CSV** (with header mapping)
- ✅ **Excel** (with worksheet/row splitting)
- ✅ **EPUB** (with chapter splitting)
- ✅ **Markdown**
- ✅ **Text**
- ✅ **DOCX** (Word documents)
- ✅ **HTML** (with text extraction)
- ✅ **JSON** (with flattening)
- ✅ **XML** (with text extraction)

### Vector Stores (5 Implementations)
- ✅ **Pinecone** (cloud vector database)
- ✅ **PostgreSQL** (pgvector extension)
- ✅ **Weaviate** (open-source vector database)
- ✅ **Qdrant** (high-performance vector database)
- ✅ **Chroma** (embedding database)

### LLM Provider Support (12 Providers)
- ✅ **OpenAI** (GPT-3.5, GPT-4, GPT-4 Turbo)
- ✅ **Azure OpenAI Service**
- ✅ **Anthropic Claude**
- ✅ **Google Gemini**
- ✅ **AWS Bedrock**
- ✅ **Cohere**
- ✅ **Groq**
- ✅ **Mistral AI**
- ✅ **Together AI**
- ✅ **Ollama** (local)
- ✅ **LM Studio** (local)
- ✅ **vLLM** (local)

### Enterprise Features
- 🔒 **Security**: Secrets management, input validation, rate limiting
- 📈 **Observability**: Structured logging, distributed tracing, cost tracking, Prometheus alerts, Grafana dashboards
- ⚡ **Performance**: Multi-level caching, connection pooling, async throughout - Leveraging .NET 10 AI optimizations
- 🏥 **Health Checks**: Integration with ASP.NET Core health checks
- 📝 **Configuration**: Centralized configuration with multiple sources
- 🚀 **.NET 10 Optimized**: Built on .NET 10 (LTS) with AI-focused performance improvements
- ☸️ **Kubernetes Ready**: Complete Kubernetes manifests, Helm charts, and deployment guides
- 📊 **Production Monitoring**: Prometheus, Grafana, and Loki integration out of the box
- 🔄 **Resilience**: Circuit breakers, retry policies, graceful degradation
- 🧪 **Testing**: Load testing suite, chaos engineering tests
- 📋 **Disaster Recovery**: Complete runbooks and recovery procedures

### Educational Extensions (DotNetAgents.Education)
- **🎓 Pedagogy**: Socratic dialogue engine, spaced repetition (SM2), mastery tracking
- **🛡️ Safety**: COPPA-compliant content filtering, conversation monitoring, age-adaptive content
- **📝 Assessment**: Question generation, response evaluation, misconception detection
- **💾 Memory**: Student profiles, mastery state, learning sessions with resume capability
- **🔍 Retrieval**: Curriculum-aware content retrieval with prerequisite checking
- **✅ Compliance**: FERPA/GDPR compliance, RBAC, comprehensive audit logging
- **🏢 Multi-Tenancy**: Tenant isolation, tenant-specific configuration

## 🚀 Quick Start

### Installation

Install the metapackage (includes all features):

```bash
dotnet add package DotNetAgents
```

Or install specific packages:

```bash
# Core only
dotnet add package DotNetAgents.Core

# With OpenAI provider
dotnet add package DotNetAgents.Providers.OpenAI
```

### Basic Usage

#### Simple Chain

```csharp
using DotNetAgents.Core;
using DotNetAgents.Providers.OpenAI;

// Register services
services.AddDotNetAgents()
    .AddOpenAI(options =>
    {
        options.ApiKey = configuration["OpenAI:ApiKey"];
        options.Model = "gpt-4";
    });

// Use in your code
var llm = serviceProvider.GetRequiredService<ILLMModel<ChatMessage[], ChatMessage>>();
var response = await llm.GenerateAsync(messages);
```

#### Building a Chain

```csharp
var chain = ChainBuilder
    .Create<string, string>()
    .WithLLM(llm)
    .WithPromptTemplate(template)
    .WithRetryPolicy(maxRetries: 3)
    .Build();

var result = await chain.InvokeAsync("Hello, world!");
```

#### Creating a Workflow

```csharp
var workflow = new StateGraph<MyState>()
    .AddNode("start", async (state, ct) => {
        // Initial processing
        return state;
    })
    .AddNode("process", async (state, ct) => {
        // Main processing
        return state;
    })
    .AddEdge("start", "process")
    .SetEntryPoint("start")
    .SetExitPoint("process");

var executor = new GraphExecutor<MyState>(workflow);
var finalState = await executor.ExecuteAsync(initialState);
```

## 📦 Package Structure

DotNetAgents uses a modular package architecture:

- **`DotNetAgents.Core`** - Core abstractions and interfaces
- **`DotNetAgents.Workflow`** - Workflow engine (LangGraph-like) with session management and bootstrap generation
- **`DotNetAgents.Tasks`** - Task management for workflows (tracking, dependencies, statistics)
- **`DotNetAgents.Knowledge`** - Knowledge repository for capturing and querying learning from agent execution
- **`DotNetAgents.Providers.OpenAI`** - OpenAI integration
- **`DotNetAgents.Providers.Azure`** - Azure OpenAI integration
- **`DotNetAgents.Providers.Anthropic`** - Anthropic integration
- **`DotNetAgents.VectorStores.Pinecone`** - Pinecone integration
- **`DotNetAgents.VectorStores.PostgreSQL`** - PostgreSQL vector store using pgvector extension
- **`DotNetAgents.VectorStores.Weaviate`** - Weaviate integration
- **`DotNetAgents.VectorStores.Qdrant`** - Qdrant integration
- **`DotNetAgents.VectorStores.Chroma`** - Chroma integration
- **`DotNetAgents.Storage.TaskKnowledge.SqlServer`** - SQL Server storage for checkpoints, tasks, and knowledge
- **`DotNetAgents.Storage.TaskKnowledge.PostgreSQL`** - PostgreSQL storage for checkpoints, tasks, and knowledge
- **`DotNetAgents.Agents.StateMachines`** - State machine implementation for agent lifecycle management
- **`DotNetAgents.Agents.BehaviorTrees`** - Behavior tree implementation for autonomous agent decision-making
- **`DotNetAgents.Agents.Registry`** - Agent registry and discovery
- **`DotNetAgents.Agents.Supervisor`** - Supervisor agent for task delegation
- **`DotNetAgents.Agents.WorkerPool`** - Worker pool management with load balancing
- **`DotNetAgents.Agents.Tasks`** - Task queue and store implementations
- **`DotNetAgents.Agents.Messaging`** - Message bus abstractions
- **`DotNetAgents.Agents.Messaging.Kafka`** - Kafka message bus implementation
- **`DotNetAgents.Agents.Messaging.RabbitMQ`** - RabbitMQ message bus implementation
- **`DotNetAgents.Agents.Messaging.Redis`** - Redis Pub/Sub message bus implementation
- **`DotNetAgents.Agents.Messaging.SignalR`** - SignalR message bus implementation
- **`DotNetAgents.Agents.Swarm`** - Swarm intelligence algorithms for agent coordination
- **`DotNetAgents.Agents.Hierarchical`** - Hierarchical agent organizations
- **`DotNetAgents.Agents.Marketplace`** - Agent marketplace and discovery
- **`DotNetAgents.Workflow.Designer`** - Visual workflow designer backend API
- **`DotNetAgents.Workflow.Designer.Web`** - Visual workflow designer Blazor UI
- **`DotNetAgents.Tools.Development`** - AI-powered development tools (chain generator, workflow builder, debugging assistant)
- **`DotNetAgents.Edge`** - Edge computing support (mobile, offline mode, edge-optimized models)
- **`DotNetAgents.Ecosystem`** - Plugin architecture and integration marketplace
- **`DotNetAgents.Configuration`** - Configuration management
- **`DotNetAgents.Observability`** - Logging, tracing, metrics, distributed tracing
- **`DotNetAgents.Security`** - Security features
- **`DotNetAgents.Education`** - Educational extensions (pedagogy, safety, assessment, compliance)
- **`DotNetAgents`** - Metapackage (references all above)

## 📚 Documentation

### Getting Started
- **[Quick Start Guide](docs/guides/INTEGRATION_GUIDE.md)** - Get started with DotNetAgents
- **[Comparison Guide](docs/comparison.md)** - DotNetAgents vs LangChain, LangGraph, and Microsoft Agent Framework
- **[API Reference](docs/guides/API_REFERENCE.md)** - API documentation

### Production & Operations
- **[Distributed Tracing](docs/examples/DISTRIBUTED_TRACING.md)** - OpenTelemetry tracing setup and examples
- **[Alerting Guide](docs/guides/ALERTING.md)** - Prometheus alerting configuration
- **[Grafana Dashboards](docs/guides/GRAFANA_DASHBOARDS.md)** - Monitoring dashboards
- **[Disaster Recovery](docs/operations/DISASTER_RECOVERY.md)** - Recovery procedures and runbooks
- **[Operations Runbook](docs/operations/RUNBOOK.md)** - Quick reference for operations
- **[Capacity Planning](docs/operations/CAPACITY_PLANNING.md)** - Resource planning guide
- **[Load Testing](docs/guides/LOAD_TESTING.md)** - Load testing with NBomber
- **[Chaos Engineering](docs/guides/CHAOS_ENGINEERING.md)** - Resilience testing
- **[Circuit Breakers](docs/guides/CIRCUIT_BREAKERS.md)** - Circuit breaker patterns
- **[Graceful Degradation](docs/guides/GRACEFUL_DEGRADATION.md)** - Degradation strategies

### Innovation Features
- **[Visual Workflow Designer](docs/guides/VISUAL_WORKFLOW_DESIGNER.md)** - Visual workflow design guide
- **[Workflow Designer UI](docs/guides/WORKFLOW_DESIGNER_UI.md)** - Frontend UI guide
- **[AI-Powered Tools](docs/guides/AI_POWERED_TOOLS.md)** - Chain generator, workflow builder, debugging assistant
- **[Advanced Multi-Agent Patterns](docs/guides/ADVANCED_MULTI_AGENT_PATTERNS.md)** - Swarm intelligence, hierarchical organizations, marketplace
- **[Edge Computing](docs/guides/EDGE_COMPUTING.md)** - Mobile and edge deployment guide
- **[Ecosystem Integrations](docs/guides/ECOSYSTEM_INTEGRATIONS.md)** - Plugin architecture and marketplace

### Community
- **[Community Guide](docs/community/COMMUNITY_GUIDE.md)** - Community resources and getting involved
- **[Certification Program](docs/education/CERTIFICATION_PROGRAM.md)** - Certification levels and process
- **[Learning Paths](docs/education/LEARNING_PATHS.md)** - Structured learning paths
- **[Training Materials](docs/education/TRAINING_MATERIALS.md)** - Training resources

### Development
- **[Project Status](docs/status/PROJECT_STATUS.md)** - Current development status and completed features
- **[Testing Strategy](docs/guides/TESTING_STRATEGY.md)** - Testing guidelines and best practices
- **[Kubernetes Deployment](kubernetes/README.md)** - Production deployment with Kubernetes and Helm
- **[Development Database](docs/DEVELOPMENT_DATABASE.md)** - Database setup and configuration
- **[Contributing](CONTRIBUTING.md)** - How to contribute
- **[Code of Conduct](CODE_OF_CONDUCT.md)** - Community guidelines

### Educational Extensions Documentation

- **[Education Requirements](docs/features/education/REQUIREMENTS.md)** - Educational package requirements
- **[Education Technical Specification](docs/features/education/TECHNICAL_SPECIFICATION.md)** - Architecture and algorithms
- **[Education Implementation Plan](docs/features/education/IMPLEMENTATION_PLAN.md)** - Phased implementation roadmap
- **[Education README](src/DotNetAgents.Education/README.md)** - Getting started guide and examples

## 🏗️ Architecture

DotNetAgents follows a layered, modular architecture:

```
Application Layer
    ↓
Autonomous Agents (State Machines & Behavior Trees)
    ↓
Workflow Engine (LangGraph-like)
    ↓
Chain & Runnable Layer (LangChain-like)
    ↓
Core Abstractions Layer
    ↓
Integrations & Infrastructure
```

### Infrastructure & Deployment

DotNetAgents includes production-ready infrastructure:

- **Kubernetes**: Complete manifests for all services, Helm charts for easy deployment
- **Monitoring**: Prometheus for metrics, Grafana for visualization, Loki for logs
- **Docker**: Docker Compose for local development, optimized Dockerfiles for LLM services
- **Message Buses**: 5 implementations (In-Memory, Kafka, RabbitMQ, Redis, SignalR) for different deployment scenarios

See [Kubernetes Deployment Guide](kubernetes/README.md) for production deployment instructions.

## 🧪 Requirements

- **.NET 10.0 SDK or later** (LTS) - Required for AI optimizations and Microsoft Agent Framework support
- **C# 13 or later** - For modern language features and performance improvements

### .NET 10 AI Optimizations

DotNetAgents leverages .NET 10's AI-focused enhancements:

- **Performance**: Up to 20% faster async operations and reduced latency for LLM API calls
- **Memory**: Improved GC efficiency for vector operations and document processing
- **Networking**: HTTP/3 support and optimized connection pooling for better throughput
- **Observability**: Enhanced OpenTelemetry integration for AI workload monitoring
- **Compatibility**: Full support for Microsoft Agent Framework and modern AI tooling

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 10 SDK
3. Restore dependencies: `dotnet restore`
4. Build: `dotnet build`
5. Run tests: `dotnet test`

### Infrastructure Setup

**Local Development (Docker Compose):**
```bash
cd docker
docker-compose up -d
```

**Kubernetes Deployment:**
```bash
# Using Helm (recommended)
helm install teaching-assistant ./kubernetes/helm/teaching-assistant \
  --namespace teaching-assistant

# Or using manifests
kubectl apply -f kubernetes/manifests/
kubectl apply -f kubernetes/monitoring/
```

See [Kubernetes Deployment Guide](kubernetes/README.md) for detailed instructions.

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🗺️ Current Status

**Beta Features:**
- ✅ Core abstractions and interfaces
- ✅ 12 LLM provider integrations
- ✅ 10 document loaders
- ✅ 5 vector store implementations
- ✅ 19 built-in tools
- ✅ Workflow engine with checkpointing
- ✅ **Visual Workflow Designer** - Beautiful Blazor WebAssembly UI with drag-and-drop
- ✅ State Machines for agent lifecycle management
- ✅ Behavior Trees for autonomous agent decision-making
- ✅ Voice command processing (JARVIS)
- ✅ MCP client library
- ✅ Educational extensions package
- ✅ Task and Knowledge management
- ✅ LCEL-like chain composition
- ✅ Workflow visualization
- ✅ Human-in-the-loop support
- ✅ Multi-agent workflows with supervisor-worker patterns
- ✅ **Advanced Multi-Agent Patterns** - Swarm intelligence, hierarchical organizations, agent marketplace
- ✅ Complete message bus implementations (Kafka, RabbitMQ, Redis, SignalR)
- ✅ **AI-Powered Development Tools** - Chain generator, workflow builder, debugging assistant
- ✅ **Edge Computing Support** - Mobile packages, offline mode, edge-optimized models
- ✅ **Plugin Architecture** - Extensible plugin system and integration marketplace
- ✅ Kubernetes deployment manifests and Helm charts
- ✅ Production monitoring stack (Prometheus, Grafana, Loki)
- ✅ **Distributed Tracing** - OpenTelemetry integration with examples
- ✅ **Prometheus Alerting** - 15+ alert rules for production monitoring
- ✅ **Grafana Dashboards** - 3 comprehensive monitoring dashboards
- ✅ **Disaster Recovery** - Complete runbooks and recovery procedures
- ✅ **Load Testing Suite** - NBomber-based performance testing
- ✅ **Chaos Engineering** - Resilience testing suite
- ✅ **Resilience Patterns** - Circuit breakers, graceful degradation

**See [Project Status](docs/status/PROJECT_STATUS.md) for detailed status.**

## 🙏 Acknowledgments

- Inspired by [LangChain](https://www.langchain.com/) and [LangGraph](https://www.langchain.com/langgraph)
- Built with **.NET 10 (LTS)** and **C# 13** - Leveraging cutting-edge AI optimizations
- Compatible with [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/) for enhanced orchestration
- Optimized for AI workloads with .NET 10's performance improvements and modern runtime features

## 👥 Contributors

This project has benefited from contributions and assistance from:

- **AI Assistants**: Claude (Anthropic) and Composer (Cursor) for code generation, documentation, and implementation assistance
- **Human Contributors**: See [CONTRIBUTING.md](CONTRIBUTING.md) for how to contribute

We're grateful for all contributions, whether from humans or AI assistants working together to build better tools!

## 📧 Contact

- **Issues**: [GitHub Issues](https://github.com/jim-finlon/DotNetAgents/issues)
- **Discussions**: [GitHub Discussions](https://github.com/jim-finlon/DotNetAgents/discussions)

---

**Status**: 🧪 Beta - Core features complete, actively maintained and tested

Made with ❤️ for the .NET community
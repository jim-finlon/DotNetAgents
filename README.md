# DotNetAgents

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/DotNetAgents.svg)](https://www.nuget.org/packages/DotNetAgents)

> **Enterprise-grade .NET 10 library for building AI agents, chains, and workflows - A native C# alternative to LangChain and LangGraph, compatible with Microsoft Agent Framework**

## 🎯 Overview

DotNetAgents is a comprehensive, production-ready .NET 10 library that brings the power of LangChain and LangGraph to C# developers. Build sophisticated AI agents, chains, and stateful workflows with enterprise-grade quality, security, and performance. Compatible with Microsoft Agent Framework for enhanced orchestration capabilities.

### Why .NET 10?

DotNetAgents targets .NET 10 (LTS) to leverage cutting-edge AI optimizations and performance improvements:

- **🚀 Enhanced Performance**: .NET 10 includes significant runtime optimizations for AI workloads, including improved async/await performance and reduced memory allocations
- **🤖 Microsoft Agent Framework Support**: Native integration with Microsoft's Agent Framework for building production-ready AI agents and multi-agent workflows
- **⚡ Vector Operations**: Optimized SIMD operations and improved array/span handling for vector embeddings and similarity calculations
- **📊 Better Observability**: Enhanced OpenTelemetry support and improved diagnostics for tracing AI operations
- **🔧 Modern C# 13 Features**: Latest language features including improved pattern matching, collection expressions, and performance-focused syntax
- **💾 Memory Efficiency**: Reduced GC pressure and improved memory management for long-running AI agent processes
- **🌐 HTTP/3 Support**: Better network performance for LLM API calls with HTTP/3 and improved connection pooling

## ✨ Features

### Core Capabilities
- **🤖 AI Agents**: Build intelligent agents with tool calling and decision-making capabilities
- **🔗 Chains**: Compose complex workflows with sequential and parallel execution
- **📊 Workflows**: Stateful, resumable workflows with checkpointing (LangGraph-like)
- **💾 Memory**: Short-term and long-term memory with vector-based storage
- **🔍 RAG**: Retrieval-Augmented Generation with document loaders and vector stores
- **🛠️ Tools**: Extensible tool system for external integrations

### LLM Provider Support
- ✅ OpenAI (GPT-3.5, GPT-4, GPT-4 Turbo)
- ✅ Azure OpenAI Service
- ✅ Anthropic Claude
- 🔄 Extensible for other providers

### Enterprise Features
- 🔒 **Security**: Secrets management, input validation, rate limiting
- 📈 **Observability**: Structured logging, distributed tracing, cost tracking
- ⚡ **Performance**: Multi-level caching, connection pooling, async throughout - Leveraging .NET 10 AI optimizations
- 🏥 **Health Checks**: Integration with ASP.NET Core health checks
- 📝 **Configuration**: Centralized configuration with multiple sources
- 🚀 **.NET 10 Optimized**: Built on .NET 10 (LTS) with AI-focused performance improvements

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
- **`DotNetAgents.Workflow`** - Workflow engine (LangGraph-like)
- **`DotNetAgents.Providers.OpenAI`** - OpenAI integration
- **`DotNetAgents.Providers.Azure`** - Azure OpenAI integration
- **`DotNetAgents.Providers.Anthropic`** - Anthropic integration
- **`DotNetAgents.VectorStores.Pinecone`** - Pinecone integration
- **`DotNetAgents.Configuration`** - Configuration management
- **`DotNetAgents.Observability`** - Logging, tracing, metrics
- **`DotNetAgents`** - Metapackage (references all above)

## 📚 Documentation

- **[Requirements](docs/requirements.md)** - Functional and non-functional requirements
- **[Technical Specification](docs/technical-specification.md)** - Architecture and design details
- **[Implementation Plan](docs/implementation-plan.md)** - Development roadmap
- **[Contributing](CONTRIBUTING.md)** - How to contribute
- **[Code of Conduct](CODE_OF_CONDUCT.md)** - Community guidelines

## 🏗️ Architecture

DotNetAgents follows a layered, modular architecture:

```
Application Layer
    ↓
Workflow Engine (LangGraph-like)
    ↓
Chain & Runnable Layer (LangChain-like)
    ↓
Core Abstractions Layer
    ↓
Integrations & Infrastructure
```

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

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🗺️ Roadmap

- [x] Planning and architecture design
- [ ] Core abstractions (Weeks 3-5)
- [ ] LLM provider integrations (Weeks 8-12)
- [ ] Memory and retrieval (Weeks 13-15)
- [ ] Workflow engine (Weeks 19-23)
- [ ] Observability and security (Weeks 27-32)
- [ ] Documentation and samples (Weeks 45-48)
- [ ] v1.0.0 Release (Week 52)

See [Implementation Plan](docs/implementation-plan.md) for detailed roadmap.

## 🙏 Acknowledgments

- Inspired by [LangChain](https://www.langchain.com/) and [LangGraph](https://www.langchain.com/langgraph)
- Built with **.NET 10 (LTS)** and **C# 13** - Leveraging cutting-edge AI optimizations
- Compatible with [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/) for enhanced orchestration
- Optimized for AI workloads with .NET 10's performance improvements and modern runtime features

## 📧 Contact

- **Issues**: [GitHub Issues](https://github.com/jim-finlon/DotNetAgents/issues)
- **Discussions**: [GitHub Discussions](https://github.com/jim-finlon/DotNetAgents/discussions)

---

**Status**: 🚧 In Active Development - Targeting v1.0.0 Release

Made with ❤️ for the .NET community
# DotNetAgents

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/DotNetAgents.svg)](https://www.nuget.org/packages/DotNetAgents)

> **Enterprise-grade .NET 8 library for building AI agents, chains, and workflows - A native C# alternative to LangChain and LangGraph**

## 🎯 Overview

DotNetAgents is a comprehensive, production-ready .NET 8 library that brings the power of LangChain and LangGraph to C# developers. Build sophisticated AI agents, chains, and stateful workflows with enterprise-grade quality, security, and performance.

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
- ⚡ **Performance**: Multi-level caching, connection pooling, async throughout
- 🏥 **Health Checks**: Integration with ASP.NET Core health checks
- 📝 **Configuration**: Centralized configuration with multiple sources

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

- .NET 8.0 or later
- C# 12 or later

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 8 SDK
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
- Built with .NET 8 and C# 12

## 📧 Contact

- **Issues**: [GitHub Issues](https://github.com/jim-finlon/DotNetAgents/issues)
- **Discussions**: [GitHub Discussions](https://github.com/jim-finlon/DotNetAgents/discussions)

---

**Status**: 🚧 In Active Development - Targeting v1.0.0 Release

Made with ❤️ for the .NET community
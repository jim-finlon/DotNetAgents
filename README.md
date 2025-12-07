# DotLangChain

[![.NET](https://github.com/dotlangchain/dotlangchain/actions/workflows/ci.yml/badge.svg)](https://github.com/dotlangchain/dotlangchain/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/DotLangChain.Core.svg)](https://www.nuget.org/packages/DotLangChain.Core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**DotLangChain** is a .NET 9 library providing enterprise-grade document ingestion, embedding generation, vector storage integration, and agent orchestration capabilities. The library serves as a native .NET alternative to LangChain and LangGraph, optimized for performance, security, and seamless integration with existing .NET ecosystems.

## 🚀 Quick Start

### Installation

```bash
dotnet add package DotLangChain.Core
dotnet add package DotLangChain.Providers.OpenAI
dotnet add package DotLangChain.VectorStores.Qdrant
```

### Basic Usage

```csharp
using DotLangChain.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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
        options.DefaultModel = "gpt-4o";
    });
    
    dlc.AddQdrant(options =>
    {
        options.Host = "localhost";
        options.Port = 6333;
    });
});
```

## 📚 Documentation

- **[Requirements](docs/REQUIREMENTS.md)**: Functional and non-functional requirements
- **[Technical Specifications](docs/TECHNICAL_SPECIFICATIONS.md)**: Architecture and implementation details
- **[API Reference](docs/API_REFERENCE.md)**: Complete API documentation
- **[Getting Started Guide](docs/GETTING_STARTED.md)**: Step-by-step tutorial (coming soon)
- **[Samples](samples/)**: Example applications

### Implementation Guides

- **[Build & CI/CD](docs/BUILD_AND_CICD.md)**: Build configuration and CI/CD pipelines
- **[Testing Strategy](docs/TESTING_STRATEGY.md)**: Testing approach and guidelines
- **[Performance Benchmarks](docs/PERFORMANCE_BENCHMARKS.md)**: Performance targets and benchmarks
- **[Error Handling](docs/ERROR_HANDLING.md)**: Exception hierarchy and error handling
- **[Versioning & Migration](docs/VERSIONING_AND_MIGRATION.md)**: Versioning strategy and migration guides
- **[Package Metadata](docs/PACKAGE_METADATA.md)**: Package organization and distribution

## ✨ Features

### Document Ingestion
- **Multiple Formats**: PDF, DOCX, XLSX, HTML, Markdown, Text, Email, Images (OCR)
- **Flexible Splitting**: Character-based, token-based, sentence-based, semantic, recursive
- **Metadata Preservation**: Track source, lineage, and custom metadata

### Embeddings
- **Multiple Providers**: OpenAI, Azure OpenAI, Ollama, HuggingFace, Cohere, Custom HTTP
- **Batch Processing**: Efficient batch embedding with configurable sizes
- **Caching**: Built-in caching for improved performance
- **Normalization**: Automatic embedding normalization

### Vector Stores
- **Multiple Backends**: Qdrant, Milvus, Pinecone, PostgreSQL (pgvector), Redis, Elasticsearch, In-Memory
- **Advanced Search**: Similarity search, hybrid search, metadata filtering, MMR
- **High Performance**: Optimized for large-scale vector operations

### LLM Integration
- **Multiple Providers**: OpenAI, Azure OpenAI, Anthropic Claude, Ollama, vLLM, Google Gemini, AWS Bedrock, Groq
- **Streaming**: Native streaming support via `IAsyncEnumerable<T>`
- **Tool Calling**: Strongly-typed function calling
- **Structured Output**: JSON mode and structured responses
- **Multimodal**: Vision and multimodal input support

### Agent Orchestration
- **Graph-Based**: Define agents as directed graphs
- **State Management**: Strongly-typed state with persistence support
- **Tool System**: Attribute-based and fluent API tool definitions
- **Built-in Patterns**: ReAct, Plan-and-Execute, Reflection, Multi-Agent, RAG

### Security & Observability
- **OWASP Compliant**: Security-first design
- **Input Sanitization**: Prompt injection prevention
- **OpenTelemetry**: Comprehensive observability
- **Audit Logging**: Security event logging

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                      │
│  (Chains, Agents, Patterns, Pre-built Flows)           │
└─────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────┐
│              Orchestration Layer                        │
│           (Graph Execution Engine)                      │
└─────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────┐
│                 Core Services Layer                     │
│  (Documents, Embeddings, Vector Stores, LLM)           │
└─────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────┐
│              Infrastructure Layer                       │
│  (Caching, Resilience, Observability, Security)        │
└─────────────────────────────────────────────────────────┘
```

## 📦 Packages

### Core Packages
- `DotLangChain.Abstractions` - Interfaces and contracts
- `DotLangChain.Core` - Core implementations

### Provider Packages
- `DotLangChain.Providers.OpenAI` - OpenAI integration
- `DotLangChain.Providers.Anthropic` - Anthropic Claude integration
- `DotLangChain.Providers.Ollama` - Ollama (local) integration
- `DotLangChain.Providers.AzureOpenAI` - Azure OpenAI integration

### Vector Store Packages
- `DotLangChain.VectorStores.Qdrant` - Qdrant integration
- `DotLangChain.VectorStores.PgVector` - PostgreSQL pgvector integration
- `DotLangChain.VectorStores.InMemory` - In-memory store (dev/test)

### Extension Packages
- `DotLangChain.Extensions.DependencyInjection` - DI extensions
- `DotLangChain.Extensions.Observability` - OpenTelemetry integration

## 🔧 Requirements

- .NET 9.0 or later
- At least one LLM provider (cloud or local)
- Optional: Vector store for RAG applications (in-memory available for development)

## 🤝 Contributing

Contributions are welcome! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 9.0 SDK
3. Restore dependencies: `dotnet restore`
4. Build: `dotnet build`
5. Run tests: `dotnet test`

For detailed setup instructions, see [BUILD_AND_CICD.md](docs/BUILD_AND_CICD.md).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by [LangChain](https://github.com/langchain-ai/langchain) and [LangGraph](https://github.com/langchain-ai/langgraph)
- Built with the .NET community in mind

## 📞 Support

- **Documentation**: See [docs/](docs/) directory
- **Issues**: [GitHub Issues](https://github.com/dotlangchain/dotlangchain/issues)
- **Discussions**: [GitHub Discussions](https://github.com/dotlangchain/dotlangchain/discussions)

---

**Note**: This project is currently in active development. APIs may change before the 1.0.0 release.


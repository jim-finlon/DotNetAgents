# Vector Store Comparison and Usage Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

DotNetAgents supports multiple vector store implementations for storing and retrieving embeddings. Each implementation is registered as a plugin and optimized for different use cases.

## Table of Contents

1. [Available Implementations](#available-implementations)
2. [PostgreSQL (pgvector)](#postgresql-pgvector)
3. [Pinecone](#pinecone)
4. [Weaviate](#weaviate)
5. [Qdrant](#qdrant)
6. [Chroma](#chroma)
7. [Comparison](#comparison)
8. [Migration Between Stores](#migration-between-stores)
9. [Best Practices](#best-practices)

## Available Implementations

| Implementation | Package | Type | Best For |
|---------------|---------|------|----------|
| PostgreSQL | `DotNetAgents.VectorStores.PostgreSQL` | Self-hosted | Existing PostgreSQL infrastructure |
| Pinecone | `DotNetAgents.VectorStores.Pinecone` | Cloud | Managed service, high scale |
| Weaviate | `DotNetAgents.VectorStores.Weaviate` | Self-hosted/Cloud | GraphQL, hybrid search |
| Qdrant | `DotNetAgents.VectorStores.Qdrant` | Self-hosted/Cloud | High performance, filtering |
| Chroma | `DotNetAgents.VectorStores.Chroma` | Self-hosted | Simple, local development |

## PostgreSQL (pgvector)

### Overview

PostgreSQL with pgvector extension provides vector storage within your existing PostgreSQL database.

### Prerequisites

- PostgreSQL 12+ with pgvector extension
- `Npgsql` NuGet package (automatically included)

### Installation

```bash
# Install pgvector extension in PostgreSQL
CREATE EXTENSION IF NOT EXISTS vector;
```

### Registration

```csharp
using DotNetAgents.VectorStores.PostgreSQL;

services.AddPostgreSQLVectorStore(options =>
{
    options.ConnectionString = "Host=localhost;Database=vectors;Username=postgres;Password=password";
    options.TableName = "embeddings";
    options.Dimension = 1536; // OpenAI embedding dimension
    options.CreateTableIfNotExists = true;
});
```

### Usage

```csharp
var vectorStore = serviceProvider.GetRequiredService<IVectorStore>();

// Add vectors
await vectorStore.AddAsync(new VectorRecord
{
    Id = "doc-1",
    Vector = embedding,
    Metadata = new Dictionary<string, object>
    {
        ["title"] = "Document 1",
        ["category"] = "technical"
    }
});

// Search
var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 10,
    options: new VectorSearchOptions
    {
        DistanceMetric = DistanceMetric.Cosine,
        MinScore = 0.7
    });
```

### Advantages

- ✅ Integrated with existing PostgreSQL infrastructure
- ✅ ACID transactions
- ✅ SQL queries for metadata filtering
- ✅ No additional infrastructure needed
- ✅ Cost-effective for small to medium scale

### Limitations

- ⚠️ Performance may degrade with very large datasets
- ⚠️ Requires PostgreSQL administration

## Pinecone

### Overview

Pinecone is a managed vector database service optimized for production workloads.

### Prerequisites

- Pinecone account and API key
- `Pinecone.NET` NuGet package (automatically included)

### Registration

```csharp
using DotNetAgents.VectorStores.Pinecone;

services.AddPineconeVectorStore(options =>
{
    options.ApiKey = configuration["Pinecone:ApiKey"];
    options.Environment = "us-west1-gcp";
    options.IndexName = "documents";
    options.Dimension = 1536;
});
```

### Usage

```csharp
var vectorStore = serviceProvider.GetRequiredService<IVectorStore>();

// Add vectors (batch recommended)
var records = new[]
{
    new VectorRecord { Id = "doc-1", Vector = embedding1, Metadata = metadata1 },
    new VectorRecord { Id = "doc-2", Vector = embedding2, Metadata = metadata2 }
};

await vectorStore.AddBatchAsync(records);

// Search
var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 10,
    options: new VectorSearchOptions
    {
        Filter = new Dictionary<string, object> { ["category"] = "technical" }
    });
```

### Advantages

- ✅ Fully managed service
- ✅ Excellent performance and scalability
- ✅ Built-in metadata filtering
- ✅ No infrastructure management

### Limitations

- ⚠️ Cost can be high at scale
- ⚠️ Requires internet connection
- ⚠️ Vendor lock-in

## Weaviate

### Overview

Weaviate is an open-source vector database with GraphQL API and hybrid search capabilities.

### Prerequisites

- Weaviate server (self-hosted or cloud)
- `Weaviate.NET` NuGet package (automatically included)

### Registration

```csharp
using DotNetAgents.VectorStores.Weaviate;

services.AddWeaviateVectorStore(options =>
{
    options.ServerUrl = "http://localhost:8080";
    options.ApiKey = configuration["Weaviate:ApiKey"]; // Optional
    options.ClassName = "Document";
    options.Vectorizer = "text2vec-openai";
});
```

### Usage

```csharp
var vectorStore = serviceProvider.GetRequiredService<IVectorStore>();

// Add vectors
await vectorStore.AddAsync(new VectorRecord
{
    Id = "doc-1",
    Vector = embedding,
    Metadata = new Dictionary<string, object>
    {
        ["title"] = "Document 1",
        ["content"] = "Document content"
    }
});

// Hybrid search (vector + keyword)
var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 10,
    options: new VectorSearchOptions
    {
        HybridSearch = true,
        QueryText = "technical documentation"
    });
```

### Advantages

- ✅ Hybrid search (vector + keyword)
- ✅ GraphQL API
- ✅ Self-hosted or cloud options
- ✅ Good performance

### Limitations

- ⚠️ More complex setup than simple stores
- ⚠️ Requires GraphQL knowledge for advanced queries

## Qdrant

### Overview

Qdrant is a high-performance vector database with advanced filtering capabilities.

### Prerequisites

- Qdrant server (self-hosted or cloud)
- `Qdrant.Client` NuGet package (automatically included)

### Registration

```csharp
using DotNetAgents.VectorStores.Qdrant;

services.AddQdrantVectorStore(options =>
{
    options.Host = "localhost";
    options.Port = 6333;
    options.ApiKey = configuration["Qdrant:ApiKey"]; // Optional
    options.CollectionName = "documents";
    options.VectorSize = 1536;
});
```

### Usage

```csharp
var vectorStore = serviceProvider.GetRequiredService<IVectorStore>();

// Add vectors with payload
await vectorStore.AddAsync(new VectorRecord
{
    Id = "doc-1",
    Vector = embedding,
    Metadata = new Dictionary<string, object>
    {
        ["title"] = "Document 1",
        ["category"] = "technical",
        ["score"] = 95
    }
});

// Search with filtering
var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 10,
    options: new VectorSearchOptions
    {
        Filter = new Dictionary<string, object>
        {
            ["must"] = new[]
            {
                new Dictionary<string, object> { ["key"] = "category", ["match"] = new { value = "technical" } },
                new Dictionary<string, object> { ["key"] = "score", ["range"] = new { gt = 80 } }
            }
        }
    });
```

### Advantages

- ✅ Excellent performance
- ✅ Advanced filtering
- ✅ Self-hosted or cloud
- ✅ Payload-based filtering

### Limitations

- ⚠️ More complex filtering syntax
- ⚠️ Requires Qdrant server setup

## Chroma

### Overview

Chroma is a simple, local vector database ideal for development and small-scale deployments.

### Prerequisites

- Chroma server (local or remote)
- `Chroma.NET` NuGet package (automatically included)

### Registration

```csharp
using DotNetAgents.VectorStores.Chroma;

services.AddChromaVectorStore(options =>
{
    options.Host = "localhost";
    options.Port = 8000;
    options.CollectionName = "documents";
    options.Dimension = 1536;
});
```

### Usage

```csharp
var vectorStore = serviceProvider.GetRequiredService<IVectorStore>();

// Add vectors
await vectorStore.AddAsync(new VectorRecord
{
    Id = "doc-1",
    Vector = embedding,
    Metadata = new Dictionary<string, object>
    {
        ["title"] = "Document 1"
    }
});

// Search
var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 10);
```

### Advantages

- ✅ Simple setup
- ✅ Good for development
- ✅ Local deployment
- ✅ Easy to use

### Limitations

- ⚠️ Limited scalability
- ⚠️ Basic features compared to others
- ⚠️ Not ideal for production at scale

## Comparison

### Feature Matrix

| Feature | PostgreSQL | Pinecone | Weaviate | Qdrant | Chroma |
|---------|-----------|----------|----------|--------|--------|
| **Managed Service** | ❌ | ✅ | ⚠️ | ⚠️ | ❌ |
| **Self-Hosted** | ✅ | ❌ | ✅ | ✅ | ✅ |
| **Metadata Filtering** | ✅ | ✅ | ✅ | ✅ | ⚠️ |
| **Hybrid Search** | ⚠️ | ⚠️ | ✅ | ⚠️ | ❌ |
| **Performance** | Good | Excellent | Very Good | Excellent | Good |
| **Scalability** | Medium | Excellent | Very Good | Excellent | Limited |
| **Cost** | Low | High | Medium | Medium | Low |
| **Setup Complexity** | Medium | Low | Medium | Medium | Low |

### When to Choose

**PostgreSQL (pgvector):**
- ✅ Already using PostgreSQL
- ✅ Need ACID transactions
- ✅ Small to medium scale
- ✅ Cost-sensitive

**Pinecone:**
- ✅ Need managed service
- ✅ High scale requirements
- ✅ Don't want infrastructure management
- ✅ Budget allows for managed service

**Weaviate:**
- ✅ Need hybrid search
- ✅ Want GraphQL API
- ✅ Need self-hosted option
- ✅ Complex metadata queries

**Qdrant:**
- ✅ Need high performance
- ✅ Advanced filtering requirements
- ✅ Self-hosted preferred
- ✅ Production workloads

**Chroma:**
- ✅ Development/testing
- ✅ Simple use cases
- ✅ Local deployment
- ✅ Small datasets

## Migration Between Stores

### Exporting Vectors

```csharp
// Export from source store
var sourceStore = serviceProvider.GetRequiredService<IVectorStore>();
var allVectors = await sourceStore.GetAllAsync();

// Serialize for backup
var json = JsonSerializer.Serialize(allVectors);
await File.WriteAllTextAsync("vectors-backup.json", json);
```

### Importing Vectors

```csharp
// Import to target store
var targetStore = serviceProvider.GetRequiredService<IVectorStore>();
var vectors = JsonSerializer.Deserialize<List<VectorRecord>>(json);

foreach (var vector in vectors)
{
    await targetStore.AddAsync(vector);
}
```

## Best Practices

### 1. Batch Operations

```csharp
// Use batch operations for better performance
var records = new List<VectorRecord> { /* ... */ };
await vectorStore.AddBatchAsync(records);
```

### 2. Index Management

```csharp
// Create indexes for better search performance
await vectorStore.CreateIndexAsync();
```

### 3. Metadata Strategy

```csharp
// Store searchable metadata
var metadata = new Dictionary<string, object>
{
    ["title"] = "Document Title",
    ["category"] = "technical",
    ["tags"] = new[] { "csharp", "dotnet" },
    ["created"] = DateTime.UtcNow
};
```

### 4. Error Handling

```csharp
try
{
    await vectorStore.AddAsync(record);
}
catch (VectorStoreException ex)
{
    _logger.LogError(ex, "Failed to add vector");
    // Retry or handle error
}
```

### 5. Monitoring

```csharp
// Track vector store metrics
var metrics = vectorStore.GetMetrics();
Console.WriteLine($"Total vectors: {metrics.TotalVectors}");
Console.WriteLine($"Search latency: {metrics.AverageSearchLatencyMs}ms");
```

## Related Documentation

- [RAG Guide](./RAG_GUIDE.md)
- [Vector Store Interfaces](../../src/DotNetAgents.Abstractions/VectorStores/IVectorStore.cs)
- [Plugin Architecture](../PLUGIN_ARCHITECTURE_MIGRATION.md)

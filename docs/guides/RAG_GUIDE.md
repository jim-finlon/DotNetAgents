# RAG (Retrieval-Augmented Generation) Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This guide demonstrates how to build RAG (Retrieval-Augmented Generation) pipelines using DotNetAgents, combining document loading, embeddings, vector stores, and LLM generation.

## Table of Contents

1. [RAG Pipeline Overview](#rag-pipeline-overview)
2. [Complete RAG Example](#complete-rag-example)
3. [Document Loading](#document-loading)
4. [Text Splitting](#text-splitting)
5. [Embedding Generation](#embedding-generation)
6. [Vector Storage](#vector-storage)
7. [Retrieval](#retrieval)
8. [Generation](#generation)
9. [Advanced Patterns](#advanced-patterns)

## RAG Pipeline Overview

A typical RAG pipeline consists of:

1. **Document Loading** - Load documents from various sources
2. **Text Splitting** - Split documents into chunks
3. **Embedding Generation** - Generate embeddings for chunks
4. **Vector Storage** - Store embeddings in vector database
5. **Retrieval** - Retrieve relevant chunks for query
6. **Generation** - Generate response using retrieved context

## Complete RAG Example

```csharp
using DotNetAgents.Documents.Loaders;
using DotNetAgents.Documents.Splitters;
using DotNetAgents.Abstractions.Embeddings;
using DotNetAgents.Abstractions.VectorStores;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Providers.OpenAI;

// 1. Setup services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddOpenAI(apiKey, "gpt-4");
services.AddPostgreSQLVectorStore(connectionString);
services.AddSingleton<IEmbeddingService, OpenAIEmbeddingService>();

var serviceProvider = services.BuildServiceProvider();

// 2. Load documents
var pdfLoader = new PdfDocumentLoader();
var documents = await pdfLoader.LoadAsync("document.pdf");

// 3. Split into chunks
var splitter = new RecursiveCharacterTextSplitter(
    chunkSize: 1000,
    chunkOverlap: 200
);
var chunks = await splitter.SplitAsync(documents);

// 4. Generate embeddings and store
var embeddingService = serviceProvider.GetRequiredService<IEmbeddingService>();
var vectorStore = serviceProvider.GetRequiredService<IVectorStore>();

foreach (var chunk in chunks)
{
    var embedding = await embeddingService.GenerateEmbeddingAsync(chunk.Content);
    
    await vectorStore.AddAsync(new VectorRecord
    {
        Id = chunk.Id,
        Vector = embedding,
        Metadata = chunk.Metadata
    });
}

// 5. Query and generate
var query = "What is the main topic?";
var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(query);

var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 5,
    options: new VectorSearchOptions
    {
        DistanceMetric = DistanceMetric.Cosine,
        MinScore = 0.7
    }
);

// 6. Generate response with context
var context = string.Join("\n\n", results.Select(r => r.Metadata["content"].ToString()));
var prompt = $@"Use the following context to answer the question.

Context:
{context}

Question: {query}

Answer:";

var llm = serviceProvider.GetRequiredService<ILLMModel<string, string>>();
var response = await llm.GenerateAsync(prompt);

Console.WriteLine($"Answer: {response}");
```

## Document Loading

### Loading Multiple Formats

```csharp
// PDF
var pdfLoader = new PdfDocumentLoader();
var pdfDocs = await pdfLoader.LoadAsync("document.pdf");

// CSV
var csvLoader = new CsvDocumentLoader();
var csvDocs = await csvLoader.LoadAsync("data.csv");

// Excel
var excelLoader = new ExcelDocumentLoader();
var excelDocs = await excelLoader.LoadAsync("data.xlsx");

// Combine all documents
var allDocuments = pdfDocs.Concat(csvDocs).Concat(excelDocs).ToList();
```

### Loading from Directory

```csharp
var allDocuments = new List<Document>();

var loaders = new Dictionary<string, IDocumentLoader>
{
    [".pdf"] = new PdfDocumentLoader(),
    [".csv"] = new CsvDocumentLoader(),
    [".txt"] = new TextDocumentLoader(),
    [".md"] = new MarkdownDocumentLoader()
};

foreach (var file in Directory.GetFiles("documents"))
{
    var extension = Path.GetExtension(file);
    if (loaders.TryGetValue(extension, out var loader))
    {
        var docs = await loader.LoadAsync(file);
        allDocuments.AddRange(docs);
    }
}
```

## Text Splitting

### Recursive Character Splitter

```csharp
var splitter = new RecursiveCharacterTextSplitter(
    chunkSize: 1000,
    chunkOverlap: 200,
    separators: new[] { "\n\n", "\n", ". ", " ", "" }
);

var chunks = await splitter.SplitAsync(documents);
```

### Semantic Splitter

```csharp
var semanticSplitter = new SemanticTextSplitter(
    embeddingService: embeddingService,
    chunkSize: 1000,
    similarityThreshold: 0.8
);

var chunks = await semanticSplitter.SplitAsync(documents);
```

## Embedding Generation

### Using OpenAI Embeddings

```csharp
var embeddingService = new OpenAIEmbeddingService(
    apiKey: apiKey,
    model: "text-embedding-3-large",
    dimension: 3072
);

var embedding = await embeddingService.GenerateEmbeddingAsync("Text to embed");
```

### Batch Embedding

```csharp
var texts = chunks.Select(c => c.Content).ToList();
var embeddings = await embeddingService.GenerateEmbeddingsAsync(texts);

for (int i = 0; i < chunks.Count; i++)
{
    chunks[i].Metadata["embedding"] = embeddings[i];
}
```

## Vector Storage

### PostgreSQL (pgvector)

```csharp
services.AddPostgreSQLVectorStore(options =>
{
    options.ConnectionString = connectionString;
    options.TableName = "document_embeddings";
    options.Dimension = 1536;
    options.CreateTableIfNotExists = true;
});

var vectorStore = serviceProvider.GetRequiredService<IVectorStore>();

// Add vectors
foreach (var chunk in chunks)
{
    await vectorStore.AddAsync(new VectorRecord
    {
        Id = chunk.Id,
        Vector = chunk.Embedding,
        Metadata = new Dictionary<string, object>
        {
            ["content"] = chunk.Content,
            ["source"] = chunk.Metadata["source"],
            ["page"] = chunk.Metadata["page"]
        }
    });
}
```

### Pinecone

```csharp
services.AddPineconeVectorStore(options =>
{
    options.ApiKey = pineconeApiKey;
    options.Environment = "us-west1-gcp";
    options.IndexName = "documents";
    options.Dimension = 1536;
});
```

## Retrieval

### Basic Retrieval

```csharp
var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(query);

var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 5
);
```

### Retrieval with Filtering

```csharp
var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 5,
    options: new VectorSearchOptions
    {
        Filter = new Dictionary<string, object>
        {
            ["source"] = "document.pdf",
            ["category"] = "technical"
        },
        MinScore = 0.7
    }
);
```

### Hybrid Search (Weaviate)

```csharp
// Weaviate supports hybrid search (vector + keyword)
var results = await vectorStore.SearchAsync(
    queryVector: queryEmbedding,
    limit: 5,
    options: new VectorSearchOptions
    {
        HybridSearch = true,
        QueryText = query
    }
);
```

## Generation

### Simple Generation

```csharp
var context = string.Join("\n\n", results.Select(r => r.Metadata["content"].ToString()));

var prompt = $@"Context:
{context}

Question: {query}

Answer:";

var response = await llm.GenerateAsync(prompt);
```

### Using Prompt Templates

```csharp
var template = new PromptTemplate(@"
Use the following context to answer the question. If you don't know the answer, say so.

Context:
{context}

Question: {question}

Answer:");

var formattedPrompt = await template.FormatAsync(new Dictionary<string, object>
{
    ["context"] = context,
    ["question"] = query
});

var response = await llm.GenerateAsync(formattedPrompt);
```

### Chain-Based RAG

```csharp
var ragChain = ChainBuilder
    .Create<string, string>()
    .WithStep(async (query, ct) =>
    {
        // Retrieve context
        var embedding = await embeddingService.GenerateEmbeddingAsync(query);
        var results = await vectorStore.SearchAsync(embedding, limit: 5);
        var context = string.Join("\n", results.Select(r => r.Metadata["content"]));
        return new { query, context };
    })
    .WithStep(async (input, ct) =>
    {
        // Generate response
        var prompt = $"Context: {input.context}\n\nQuestion: {input.query}\n\nAnswer:";
        return await llm.GenerateAsync(prompt, cancellationToken: ct);
    })
    .Build();

var answer = await ragChain.InvokeAsync("What is the main topic?");
```

## Advanced Patterns

### Multi-Query Retrieval

```csharp
// Generate multiple query variations
var queryVariations = await llm.GenerateAsync($@"
Generate 3 different ways to ask this question: {query}

Return as JSON array of strings.");

var queries = JsonSerializer.Deserialize<string[]>(queryVariations);

// Retrieve for each variation
var allResults = new List<VectorSearchResult>();
foreach (var q in queries)
{
    var embedding = await embeddingService.GenerateEmbeddingAsync(q);
    var results = await vectorStore.SearchAsync(embedding, limit: 3);
    allResults.AddRange(results);
}

// Deduplicate and rerank
var uniqueResults = allResults
    .GroupBy(r => r.Id)
    .Select(g => g.OrderByDescending(r => r.Score).First())
    .OrderByDescending(r => r.Score)
    .Take(5)
    .ToList();
```

### Reranking

```csharp
// Initial retrieval
var initialResults = await vectorStore.SearchAsync(queryEmbedding, limit: 20);

// Rerank using cross-encoder or LLM
var rerankedResults = await RerankAsync(query, initialResults, limit: 5);

async Task<List<VectorSearchResult>> RerankAsync(
    string query,
    List<VectorSearchResult> results,
    int limit)
{
    // Use LLM to score relevance
    var scores = new List<(VectorSearchResult result, double score)>();
    
    foreach (var result in results)
    {
        var prompt = $@"Rate the relevance of this document to the query (0-1):

Query: {query}
Document: {result.Metadata["content"]}

Relevance score:";
        
        var scoreText = await llm.GenerateAsync(prompt);
        var score = double.Parse(scoreText.Trim());
        scores.Add((result, score));
    }
    
    return scores
        .OrderByDescending(s => s.score)
        .Take(limit)
        .Select(s => s.result)
        .ToList();
}
```

### Context Compression

```csharp
// Compress retrieved context to fit token limits
var context = string.Join("\n\n", results.Select(r => r.Metadata["content"]));

var compressionPrompt = $@"Compress the following context while preserving key information:

Context:
{context}

Compressed context:";

var compressedContext = await llm.GenerateAsync(compressionPrompt);

// Use compressed context for final generation
var finalPrompt = $"Context: {compressedContext}\n\nQuestion: {query}\n\nAnswer:";
var answer = await llm.GenerateAsync(finalPrompt);
```

## Best Practices

1. **Chunk Size**: Use 500-1000 tokens per chunk for optimal retrieval
2. **Overlap**: Use 10-20% overlap between chunks
3. **Metadata**: Store rich metadata for filtering
4. **Retrieval**: Retrieve 3-5 chunks, then rerank if needed
5. **Context Window**: Compress context if it exceeds model limits
6. **Error Handling**: Handle missing context gracefully

## Related Documentation

- [Document Loaders Guide](./DOCUMENT_LOADERS.md)
- [Vector Stores Guide](./VECTOR_STORES.md)
- [LLM Providers Guide](./LLM_PROVIDERS.md)
- [RAG Sample](../../samples/DotNetAgents.Samples.RAG/)

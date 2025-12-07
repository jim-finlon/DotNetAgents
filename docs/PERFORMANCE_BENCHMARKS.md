# Performance Benchmarks

**Version:** 1.0.0  
**Date:** December 7, 2025  
**Status:** Draft

---

## 1. Overview

This document defines performance targets, benchmark scenarios, and measurement methodology for DotLangChain components.

---

## 2. Performance Targets

### 2.1 Document Ingestion

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| PDF text extraction throughput | ≥ 100 pages/second | Pages processed per second |
| DOCX parsing throughput | ≥ 200 pages/second | Pages processed per second |
| HTML parsing throughput | ≥ 500 pages/second | Pages processed per second |
| Text splitting throughput | ≥ 1000 chunks/second | Chunks produced per second |

### 2.2 Embedding Operations

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Cached embedding retrieval | < 1ms P99 | Latency from cache hit |
| Single embedding generation | < 100ms P95 | End-to-end latency (excludes network) |
| Batch embedding (100 items) | < 500ms P95 | Total batch time / items |
| Embedding cache hit rate | > 80% | Hits / (Hits + Misses) |

### 2.3 Vector Store Operations

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Vector upsert (single) | < 10ms P95 | Single record insertion |
| Vector upsert (batch 1000) | < 500ms P95 | Batch insertion time |
| Similarity search (top-10) | < 50ms P95 | Query latency |
| Similarity search (top-100) | < 200ms P95 | Query latency |
| Collection creation | < 1s | Setup time |

### 2.4 LLM Operations

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Streaming first-token latency | < 50ms overhead | Overhead beyond provider latency |
| Completion latency (100 tokens) | < 2s overhead | Overhead beyond provider latency |
| Tool call processing | < 10ms overhead | JSON parsing + validation time |

### 2.5 Agent/Graph Execution

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Node execution overhead | < 100μs per node | Time spent in graph engine (excludes node logic) |
| State transition overhead | < 50μs | State serialization/deserialization |
| Graph compilation | < 10ms | Graph builder compilation |
| Event streaming overhead | < 1ms per event | Event emission overhead |

### 2.6 Memory Operations

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Message retrieval (100 messages) | < 5ms | Memory lookup time |
| Message storage | < 1ms | Insertion time |
| Summary generation | < 100ms overhead | LLM call overhead excluded |

---

## 3. Benchmark Scenarios

### 3.1 Document Ingestion Benchmarks

**Scenario 1: PDF Text Extraction**
- Input: 100 PDF documents (varying sizes: 1-50 pages)
- Measure: Total time, throughput (pages/sec), memory allocations
- Environment: Clean state, no caching

**Scenario 2: Multi-format Document Loading**
- Input: Mixed documents (PDF, DOCX, HTML, Markdown, 100 each)
- Measure: Per-format throughput, total time
- Environment: All loaders registered

**Scenario 3: Large Document Processing**
- Input: Single 1000-page PDF
- Measure: Peak memory, processing time, chunk count
- Environment: Memory-constrained (8GB limit)

### 3.2 Text Splitting Benchmarks

**Scenario 1: Recursive Character Splitter**
- Input: 10MB text document
- Options: ChunkSize=1000, ChunkOverlap=200
- Measure: Split time, chunk count, memory allocations

**Scenario 2: Token-aware Splitting**
- Input: 10MB text document
- Tokenizer: cl100k_base (OpenAI)
- Measure: Token counting overhead, split time

**Scenario 3: Semantic Splitting**
- Input: 10MB text document
- Embedding model: text-embedding-3-small
- Measure: Embedding generation time, similarity calculation time

### 3.3 Embedding Benchmarks

**Scenario 1: Single Embedding**
- Input: Text of varying lengths (10, 100, 1000, 10000 characters)
- Provider: OpenAI text-embedding-3-small
- Measure: Latency (P50, P95, P99), token count

**Scenario 2: Batch Embedding**
- Input: Arrays of 10, 50, 100, 500 texts
- Provider: OpenAI text-embedding-3-small
- Measure: Total time, per-item time, token usage

**Scenario 3: Caching Performance**
- Input: Repeated embedding requests (same text)
- Cache: IDistributedCache (Redis)
- Measure: Cache hit rate, cache retrieval time

### 3.4 Vector Store Benchmarks

**Scenario 1: Single Vector Upsert**
- Input: 1536-dimensional vectors
- Store: Qdrant
- Measure: Insertion latency, index update time

**Scenario 2: Batch Upsert**
- Input: 1,000; 10,000; 100,000 vectors
- Store: Qdrant
- Measure: Total time, throughput (vectors/sec)

**Scenario 3: Similarity Search**
- Input: Query vectors, collection sizes (1K, 10K, 100K, 1M vectors)
- Store: Qdrant
- Measure: Query latency (P50, P95, P99), recall@10

**Scenario 4: Metadata Filtering**
- Input: Queries with metadata filters
- Store: Qdrant
- Measure: Filter overhead, query latency

### 3.5 LLM Benchmarks

**Scenario 1: Simple Completion**
- Input: 5-message conversation
- Provider: OpenAI GPT-4o
- Measure: Completion latency, token usage

**Scenario 2: Streaming Response**
- Input: Prompt generating 1000 tokens
- Provider: OpenAI GPT-4o
- Measure: First-token latency, total time, chunk count

**Scenario 3: Tool Calling**
- Input: Request with 5 tool definitions
- Provider: OpenAI GPT-4o
- Measure: Tool call extraction time, JSON parsing time

### 3.6 Graph Execution Benchmarks

**Scenario 1: Linear Graph**
- Input: 10-node linear graph
- Measure: Node execution time, total graph time, overhead

**Scenario 2: Conditional Branching**
- Input: Graph with 5 conditional edges
- Measure: Decision time, total execution time

**Scenario 3: Parallel Execution**
- Input: Graph with 5 parallel nodes
- Measure: Parallelization efficiency, total time vs sequential

**Scenario 4: State Persistence**
- Input: Graph with checkpointing enabled
- Store: Redis
- Measure: Checkpoint save/load time, serialization overhead

---

## 4. Measurement Methodology

### 4.1 Benchmark Environment

**Hardware Requirements**:
- CPU: 8+ cores (AMD Ryzen 7 or Intel i7 equivalent)
- RAM: 32GB
- Storage: SSD (NVMe preferred)
- Network: Stable connection for cloud provider tests

**Software Requirements**:
- .NET 9.0 SDK
- Docker (for containerized dependencies)
- BenchmarkDotNet

### 4.2 Benchmark Execution

**Warmup**:
- 3 warmup iterations (discarded)
- JIT compilation allowed to complete

**Measurement**:
- 10-20 measurement iterations
- Statistics: Mean, Median, P95, P99, Min, Max
- Memory: Allocated bytes, GC collections

**Example Benchmark**:

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90, warmupCount: 3, iterationCount: 20)]
public class DocumentLoaderBenchmarks
{
    private readonly IDocumentLoader _loader;
    private readonly byte[] _pdfBytes;
    
    public DocumentLoaderBenchmarks()
    {
        _loader = new PdfDocumentLoader(NullLogger<PdfDocumentLoader>.Instance);
        _pdfBytes = File.ReadAllBytes("test-data/sample-10-pages.pdf");
    }
    
    [Benchmark(Baseline = true)]
    public async Task<Document> LoadPdf()
    {
        using var stream = new MemoryStream(_pdfBytes);
        return await _loader.LoadAsync(stream, "sample.pdf");
    }
}
```

### 4.3 Reporting

Benchmark results SHALL be reported in:
- **Markdown tables** (for documentation)
- **JSON files** (for CI tracking)
- **Charts** (for trend visualization)

**Result Format**:

```
| Method | Mean | Error | StdDev | Gen0 | Gen1 | Gen2 | Allocated |
|------- |------|-------|--------|------|------|------|-----------|
| LoadPdf | 123.4 ms | 2.1 ms | 1.8 ms | 1000 | 500 | 100 | 5.2 MB |
```

---

## 5. Performance Budget

### 5.1 Allocated Budgets

| Component | CPU Budget | Memory Budget | Latency Budget |
|-----------|-----------|---------------|----------------|
| Document Loading | 50% CPU | 500MB/doc | 100ms/page |
| Embedding Cache | 5% CPU | 1GB cache | 1ms retrieval |
| Vector Search | 30% CPU | 100MB/query | 50ms/query |
| Graph Engine | 10% CPU | 50MB/graph | 100μs/node |

### 5.2 Regression Thresholds

- **Latency**: Fail if > 10% degradation from baseline
- **Throughput**: Fail if < 90% of baseline
- **Memory**: Fail if > 20% increase from baseline

---

## 6. Continuous Monitoring

### 6.1 Benchmark Execution Schedule

- **On every commit**: Core benchmarks (document loading, splitting)
- **Nightly**: Full benchmark suite
- **Before release**: Complete performance validation

### 6.2 Performance Tracking

Results tracked in:
- **BenchmarkDotNet** artifact storage
- **GitHub Actions** artifacts
- **Performance dashboard** (future)

### 6.3 Alerting

Alerts triggered when:
- Latency exceeds P95 target by > 20%
- Throughput drops below target by > 10%
- Memory usage exceeds budget by > 30%

---

## 7. Optimization Guidelines

### 7.1 Optimization Principles

1. **Measure First**: Profile before optimizing
2. **Optimize Hot Paths**: Focus on frequently executed code
3. **Avoid Premature Optimization**: Optimize only when benchmarks show issues
4. **Document Trade-offs**: Record any performance vs. maintainability trade-offs

### 7.2 Common Optimizations

- **Object Pooling**: For frequently allocated objects
- **Span<T>/Memory<T>**: For zero-allocation string operations
- **IAsyncEnumerable**: For streaming large datasets
- **Caching**: For expensive operations (embeddings, tokenization)
- **Parallel Processing**: For independent operations

---

## 8. Benchmark Data

### 8.1 Test Data Requirements

- **Representative**: Real-world document sizes and formats
- **Varied**: Different content types and structures
- **Reproducible**: Same data for consistent results
- **Anonymized**: No sensitive information

### 8.2 Test Data Storage

- Location: `tests/DotLangChain.Tests.Benchmarks/Data/`
- Formats: PDF, DOCX, HTML, Markdown, JSON
- Naming: `{type}-{size}-{variant}.{ext}` (e.g., `pdf-10pages-standard.pdf`)

---

## 9. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-07 | - | Initial draft |


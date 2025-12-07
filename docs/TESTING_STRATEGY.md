# Testing Strategy

**Version:** 1.0.0  
**Date:** December 7, 2025  
**Status:** Draft

---

## 1. Testing Philosophy

### 1.1 Principles

- **Test-Driven Development (TDD)**: Write tests before implementation where feasible
- **Behavior-Driven Development (BDD)**: Tests describe expected behavior
- **Test Isolation**: Each test is independent and can run in any order
- **Fast Feedback**: Unit tests complete in < 1 second, integration tests in < 30 seconds
- **Deterministic**: Tests produce the same results every run

### 1.2 Test Pyramid

```
        /\
       /  \      E2E Tests (5%)
      /    \
     /------\    Integration Tests (25%)
    /        \
   /----------\  Unit Tests (70%)
```

---

## 2. Test Categories

### 2.1 Unit Tests

**Purpose**: Test individual components in isolation

**Location**: `tests/DotLangChain.Tests.Unit/`

**Scope**:
- Pure functions (splitters, transformers)
- Business logic (graph execution, state management)
- Abstractions (interfaces without external dependencies)
- Utilities (sanitizers, validators)

**Tools**:
- **Framework**: xUnit.net
- **Assertions**: FluentAssertions
- **Mocking**: NSubstitute
- **Fixtures**: AutoFixture

**Example**:

```csharp
[Fact]
public async Task RecursiveCharacterTextSplitter_ShouldSplitOnSeparators()
{
    // Arrange
    var splitter = new RecursiveCharacterTextSplitter(_logger);
    var document = new Document
    {
        Id = "test-1",
        Content = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph."
    };
    
    // Act
    var chunks = await splitter.SplitAsync(document, new TextSplitterOptions
    {
        ChunkSize = 50,
        ChunkOverlap = 10
    }).ToListAsync();
    
    // Assert
    chunks.Should().HaveCount(3);
    chunks[0].Content.Should().Contain("First paragraph");
    chunks[1].Content.Should().Contain("Second paragraph");
}
```

### 2.2 Integration Tests

**Purpose**: Test components with real dependencies

**Location**: `tests/DotLangChain.Tests.Integration/`

**Scope**:
- Provider integrations (OpenAI, Ollama, Qdrant)
- Database operations (PostgreSQL, Redis)
- Document loaders with actual files
- End-to-end workflows (RAG, agent execution)

**Tools**:
- **Framework**: xUnit.net
- **Containers**: Testcontainers.NET
- **Fixtures**: xUnit IClassFixture / IAsyncLifetime

**Example**:

```csharp
public class QdrantVectorStoreTests : IClassFixture<QdrantFixture>
{
    private readonly QdrantFixture _fixture;
    
    public QdrantVectorStoreTests(QdrantFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task Upsert_ShouldStoreVectors()
    {
        // Arrange
        var store = _fixture.CreateVectorStore("test-collection");
        var records = new[]
        {
            new VectorRecord
            {
                Id = "1",
                Vector = new float[1536],
                Content = "Test content"
            }
        };
        
        // Act
        await store.UpsertAsync(records);
        
        // Assert
        var retrieved = await store.GetAsync(new[] { "1" });
        retrieved.Should().HaveCount(1);
    }
}
```

**Fixture Example**:

```csharp
public class QdrantFixture : IAsyncLifetime
{
    private readonly IContainer _container;
    public string Host => "localhost";
    public int Port { get; private set; }
    
    public QdrantFixture()
    {
        _container = new ContainerBuilder()
            .WithImage("qdrant/qdrant:v1.12.0")
            .WithPortBinding(6333, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6333))
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Port = _container.GetMappedPublicPort(6333);
    }
    
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

### 2.3 Benchmark Tests

**Purpose**: Measure and track performance

**Location**: `tests/DotLangChain.Tests.Benchmarks/`

**Tools**: BenchmarkDotNet

**Example**:

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class TextSplitterBenchmarks
{
    private readonly string _largeDocument = File.ReadAllText("large-document.txt");
    private readonly RecursiveCharacterTextSplitter _splitter = new(new NullLogger<RecursiveCharacterTextSplitter>());
    
    [Benchmark]
    public async Task SplitLargeDocument()
    {
        var document = new Document
        {
            Id = "bench-1",
            Content = _largeDocument
        };
        
        await foreach (var _ in _splitter.SplitAsync(document))
        {
            // Consume chunks
        }
    }
}
```

---

## 3. Test Coverage Requirements

### 3.1 Coverage Targets

| Component | Target Coverage | Critical Path Coverage |
|-----------|----------------|----------------------|
| Core Logic | ≥ 90% | 100% |
| Providers | ≥ 80% | ≥ 95% |
| Abstractions | ≥ 85% | 100% |
| Utilities | ≥ 95% | 100% |
| **Overall** | **≥ 80%** | **≥ 90%** |

### 3.2 Coverage Exclusions

- Generated code (source generators)
- Exception constructors
- Property getters/setters without logic
- Platform-specific code paths not testable on current OS

**Exclusion Example**:

```csharp
[ExcludeFromCodeCoverage]
internal static class PlatformHelper
{
    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
```

---

## 4. Test Data Management

### 4.1 Test Fixtures

- **Location**: `tests/DotLangChain.Tests.Unit/Fixtures/`
- **Format**: JSON, XML, plain text
- **Naming**: `{Component}_{Scenario}.{ext}`

**Example Structure**:
```
Fixtures/
├── Documents/
│   ├── sample.pdf
│   ├── sample.docx
│   └── sample.html
├── Embeddings/
│   └── expected-vectors.json
└── Graphs/
    └── simple-workflow.json
```

### 4.2 Test Data Generation

Use **AutoFixture** for generating test data:

```csharp
var fixture = new Fixture();
var document = fixture.Create<Document>();
var options = fixture.Create<TextSplitterOptions>();
```

For specific scenarios, use customizations:

```csharp
var fixture = new Fixture();
fixture.Customize<TextSplitterOptions>(c => c
    .With(x => x.ChunkSize, 100)
    .With(x => x.ChunkOverlap, 20));
```

---

## 5. Mocking Strategies

### 5.1 When to Mock

✅ **Mock**:
- External HTTP services (unless using Testcontainers)
- File system operations (use in-memory alternatives)
- Time-dependent logic (use `ITimeProvider` abstraction)
- Expensive operations (LLM calls in unit tests)

❌ **Don't Mock**:
- Pure functions
- Abstractions being tested
- Already-tested dependencies
- In integration tests (use real dependencies)

### 5.2 Mock Examples

```csharp
// HTTP client mock
var mockHttp = new MockHttpMessageHandler();
mockHttp.When("https://api.openai.com/v1/embeddings")
    .Respond("application/json", """{"data": [{"embedding": [0.1, 0.2]}]}""");
var httpClient = new HttpClient(mockHttp);

// Logger mock (use NullLogger in tests)
var logger = NullLogger<MyService>.Instance;

// Time provider mock
var mockTime = Substitute.For<ITimeProvider>();
mockTime.UtcNow.Returns(new DateTime(2025, 1, 1));
```

---

## 6. Test Organization

### 6.1 Naming Conventions

- **Test Class**: `{ClassUnderTest}Tests` (e.g., `RecursiveCharacterTextSplitterTests`)
- **Test Method**: `{MethodName}_{Scenario}_{ExpectedResult}` (e.g., `SplitAsync_WithOverlap_ShouldIncludeOverlappingText`)

### 6.2 Test Structure

Follow **Arrange-Act-Assert (AAA)** pattern:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var sut = new ClassUnderTest();
    var input = CreateTestInput();
    
    // Act
    var result = await sut.MethodAsync(input);
    
    // Assert
    result.Should().Be(expectedResult);
}
```

### 6.3 Test Categories

Use xUnit traits to categorize tests:

```csharp
[Fact]
[Trait("Category", "Unit")]
[Trait("Component", "Documents")]
public async Task TestName() { }

[Fact]
[Trait("Category", "Integration")]
[Trait("Provider", "Qdrant")]
public async Task IntegrationTest() { }
```

Filter tests:
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Component=Documents"
```

---

## 7. Performance Testing

### 7.1 Benchmark Scenarios

Required benchmarks (see `PERFORMANCE_BENCHMARKS.md` for details):

1. Document loading throughput
2. Text splitting performance
3. Embedding generation latency
4. Vector search query time
5. Graph execution overhead
6. Memory allocation patterns

### 7.2 Performance Test Execution

```bash
# Run all benchmarks
dotnet run --project tests/DotLangChain.Tests.Benchmarks -c Release

# Run specific benchmark
dotnet run --project tests/DotLangChain.Tests.Benchmarks -c Release --filter "*TextSplitter*"
```

### 7.3 Performance Regression Detection

- Benchmarks run in CI on every commit
- Results compared against baseline
- Failures require investigation if > 5% degradation

---

## 8. Test Execution

### 8.1 Local Execution

```bash
# Run all tests
dotnet test

# Run specific project
dotnet test tests/DotLangChain.Tests.Unit/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run in parallel (default)
dotnet test --parallel

# Run sequentially (for debugging)
dotnet test --no-parallel
```

### 8.2 CI Execution

- Unit tests: Run on every commit
- Integration tests: Run on PR and main branch
- Benchmarks: Run nightly
- Coverage: Collected and reported to Codecov

### 8.3 Test Parallelization

- Unit tests: Fully parallel (no shared state)
- Integration tests: Parallelizable but may use shared fixtures
- Use `[Collection]` attribute for tests that cannot run in parallel:

```csharp
[Collection("DatabaseTests")]
public class DatabaseTest1 { }
```

---

## 9. Assertion Libraries

### 9.1 FluentAssertions

Primary assertion library for readable tests:

```csharp
result.Should().NotBeNull();
result.Should().BeEquivalentTo(expected);
result.Should().BeGreaterThan(0);
collection.Should().HaveCount(5).And.Contain(x => x.Id == "123");
```

### 9.2 xUnit Assertions

Use for simple assertions:

```csharp
Assert.NotNull(result);
Assert.Equal(expected, actual);
Assert.True(condition);
```

---

## 10. Test Maintenance

### 10.1 Test Review Checklist

- [ ] Test name clearly describes scenario
- [ ] Test is isolated (no shared state)
- [ ] Test is deterministic (same results every run)
- [ ] Test uses appropriate level of abstraction
- [ ] Test assertions are specific and meaningful
- [ ] Test data is realistic but minimal
- [ ] Test follows AAA pattern

### 10.2 Test Refactoring

- Extract common setup to fixtures
- Use builder patterns for complex objects
- Create helper methods for repeated assertions
- Remove duplicate test logic

---

## 11. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-07 | - | Initial draft |


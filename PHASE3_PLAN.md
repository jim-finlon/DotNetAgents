# Phase 3: Core Implementation - Planning Document

**Next Phase After:** Core Abstractions merge  
**Target Branch:** `feature/core-implementation`  
**Status:** Planning

---

## Overview

Phase 3 will implement the concrete classes that realize the abstractions defined in Phase 2. This includes document loaders, text splitters, the graph execution engine, tool system, memory implementations, and security components.

---

## Project Structure

```
src/DotLangChain.Core/
├── Documents/
│   ├── Loaders/
│   │   ├── PdfDocumentLoader.cs
│   │   ├── DocxDocumentLoader.cs
│   │   ├── MarkdownDocumentLoader.cs
│   │   ├── HtmlDocumentLoader.cs
│   │   ├── TextDocumentLoader.cs
│   │   └── DocumentLoaderRegistry.cs
│   ├── Splitters/
│   │   ├── RecursiveCharacterTextSplitter.cs
│   │   ├── CharacterTextSplitter.cs
│   │   ├── TokenTextSplitter.cs
│   │   ├── SentenceTextSplitter.cs
│   │   └── MarkdownTextSplitter.cs
│   └── Extractors/
├── Embeddings/
│   └── (Caching decorators, if needed)
├── Agents/
│   ├── Graph/
│   │   ├── GraphBuilder.cs
│   │   ├── CompiledGraph.cs
│   │   └── GraphNode.cs
│   ├── State/
│   └── Tools/
│       ├── ToolExecutor.cs
│       └── ToolRegistry.cs
├── Memory/
│   ├── BufferMemory.cs
│   ├── WindowMemory.cs
│   └── TokenBufferMemory.cs
├── Security/
│   ├── InputSanitizer.cs
│   └── SecretProvider.cs
└── Common/
    └── Exceptions/
        ├── DocumentException.cs
        ├── EmbeddingException.cs
        ├── VectorStoreException.cs
        ├── LLMException.cs
        ├── GraphException.cs
        ├── ToolException.cs
        └── SecurityException.cs
```

---

## Implementation Priority

### High Priority (Must Have for MVP)
1. **Document Loaders** (Priority: High)
   - TextDocumentLoader (plain text, simple)
   - MarkdownDocumentLoader
   - DocumentLoaderRegistry

2. **Text Splitters** (Priority: High)
   - RecursiveCharacterTextSplitter (core implementation)
   - CharacterTextSplitter (simple fallback)

3. **Graph Engine** (Priority: High)
   - GraphBuilder
   - CompiledGraph
   - Basic node execution

4. **Exception Classes** (Priority: High)
   - All exception types from ERROR_HANDLING.md

5. **Security Components** (Priority: High)
   - DefaultInputSanitizer
   - ConfigurationSecretProvider

### Medium Priority (Important but can follow)
6. **Tool System**
   - ToolRegistry
   - ToolExecutor
   - Reflection-based tool discovery

7. **Memory Implementations**
   - BufferMemory
   - WindowMemory

8. **Additional Document Loaders**
   - PdfDocumentLoader (requires PdfPig)
   - DocxDocumentLoader (requires OpenXml)
   - HtmlDocumentLoader (requires HtmlAgilityPack)

9. **Additional Text Splitters**
   - TokenTextSplitter
   - SentenceTextSplitter
   - MarkdownTextSplitter

### Low Priority (Can defer)
10. **Advanced Memory**
    - TokenBufferMemory
    - SummaryMemory
    - VectorMemory

---

## Dependencies

### Required Packages
```xml
<!-- Already in Abstractions -->
Microsoft.Extensions.Logging.Abstractions
System.Text.Json

<!-- Need to add -->
Microsoft.Extensions.DependencyInjection.Abstractions
Microsoft.Extensions.Options
Microsoft.Extensions.Http
Polly.Extensions
OpenTelemetry.Api
System.Threading.Channels
```

### Optional Packages (for specific loaders)
```xml
<!-- PDF support -->
PdfPig (optional)

<!-- DOCX support -->
DocumentFormat.OpenXml (optional)

<!-- HTML support -->
HtmlAgilityPack (optional)
```

---

## Implementation Guidelines

### 1. Document Loaders
- All loaders must implement `IDocumentLoader`
- Support stream, file path, and URI loading
- Handle errors gracefully with `DocumentException`
- Preserve metadata from source files
- Validate file extensions

### 2. Text Splitters
- All splitters implement `ITextSplitter`
- Support streaming via `IAsyncEnumerable<T>`
- Respect `TextSplitterOptions`
- Handle edge cases (empty documents, very large documents)
- Optimize for performance (use `Span<T>` where possible)

### 3. Graph Engine
- Implement `GraphBuilder<TState>` with fluent API
- Validate graph structure on compilation
- Support conditional edges and cycles
- Handle timeouts and max steps
- Emit events for observability
- Thread-safe execution

### 4. Exception Classes
- All inherit from `DotLangChainException`
- Include error codes per ERROR_HANDLING.md
- Provide context data
- Static factory methods for common scenarios

### 5. Security
- Input sanitization with configurable levels
- Prompt injection detection
- Secure secret handling
- No secrets in logs

---

## Testing Strategy

### Unit Tests
- Test each loader with sample files
- Test splitters with various text sizes
- Test graph builder with different topologies
- Test exception creation and properties
- Test sanitizer with injection attempts

### Integration Tests
- End-to-end document loading and splitting
- Graph execution with real state
- Tool execution with mock LLM

---

## Performance Considerations

- Use object pooling for frequently allocated objects
- Prefer `Span<T>`/`Memory<T>` for string operations
- Cache compiled graphs
- Batch operations where possible
- Profile and optimize hot paths

---

## Success Criteria

- [ ] All high-priority components implemented
- [ ] All unit tests pass
- [ ] Code coverage ≥ 80%
- [ ] Performance benchmarks meet targets
- [ ] Documentation complete
- [ ] Ready for provider implementations

---

## Estimated Timeline

- **Week 1:** Document loaders and splitters
- **Week 2:** Graph engine core
- **Week 3:** Tool system and exceptions
- **Week 4:** Memory implementations and security
- **Week 5:** Testing, documentation, optimization

---

## Notes

- Start with simplest implementations first
- Test as you go
- Refactor based on learnings
- Keep performance in mind from the start
- Document design decisions in LESSONS_LEARNED.md

---

**Status:** Ready to begin after Phase 2 merge


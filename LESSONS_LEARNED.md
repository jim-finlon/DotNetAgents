# Lessons Learned

**Last Updated:** 2025-12-07  
**Purpose:** Document key decisions, learnings, and best practices discovered during development

---

## Development Philosophy

### Design Principles
- **Interface-First Design**: Define abstractions before implementations
- **Composition over Inheritance**: Prefer dependency injection and composition
- **Fail Fast**: Validate inputs early, throw meaningful exceptions
- **Async by Default**: All I/O operations are async from the start
- **Security First**: OWASP compliance is not optional

### Architecture Decisions
- **Provider Pattern**: All external services (LLMs, vector stores) use provider pattern for flexibility
- **Strongly-Typed State**: Agent state uses strongly-typed classes for compile-time safety
- **Streaming First**: Prefer `IAsyncEnumerable<T>` for large data sets
- **Null Safety**: Use nullable reference types throughout

---

## Implementation Insights

### What Worked Well
- Starting with comprehensive documentation before coding
- Using `Directory.Build.props` for shared configuration
- Testcontainers.NET for integration tests
- FluentAssertions for readable test assertions

### Challenges Encountered
- Balancing API flexibility with type safety
- Managing version compatibility across multiple provider packages
- Performance optimization without sacrificing code clarity

### Solutions Applied
- Use of records for immutable data types
- Frozen collections for thread-safe, high-performance lookups
- Object pooling for frequently allocated objects
- Source generators for performance-critical serialization

---

## Code Patterns

### Effective Patterns
1. **Builder Pattern**: Used extensively for graph construction and configuration
2. **Factory Pattern**: Provider factories for creating service instances
3. **Decorator Pattern**: Caching, retry, and observability decorators
4. **Strategy Pattern**: Pluggable text splitters, embedding providers

### Anti-Patterns to Avoid
- ❌ Service locator pattern (use DI instead)
- ❌ Static mutable state
- ❌ Catching generic `Exception` (catch specific types)
- ❌ Blocking async code (use `async/await` properly)

---

## Testing Practices

### Best Practices
- Write tests for edge cases, not just happy paths
- Use Testcontainers for integration tests with real services
- Mock external HTTP services in unit tests
- Use fixtures for common test setup

### Common Pitfalls
- Tests that depend on external services being available
- Tests that aren't deterministic (time-dependent, random)
- Over-mocking (mocking everything instead of using real objects where appropriate)

---

## Performance Learnings

### Optimization Insights
- `Span<T>` and `Memory<T>` are essential for zero-allocation string operations
- Object pooling significantly reduces GC pressure
- Streaming (`IAsyncEnumerable`) is crucial for large documents
- Batch operations are always more efficient than single operations

### Performance Pitfalls
- Premature optimization (measure first!)
- Allocating large objects unnecessarily
- Blocking async code paths
- Not using connection pooling

---

## Security Practices

### Key Practices
- Always sanitize user input (prompt injection prevention)
- Never log secrets or API keys
- Use secure defaults (TLS 1.2+, strong encryption)
- Validate all inputs from external sources

### Common Mistakes
- Trusting user-provided URLs without validation (SSRF risk)
- Storing secrets in code or configuration files
- Not implementing rate limiting
- Exposing internal implementation details in error messages

---

## Documentation Insights

### Documentation Best Practices
- Keep documentation close to code (XML comments)
- Examples are more valuable than descriptions
- Document "why" not just "what"
- Keep API reference up to date with code changes

### Documentation Challenges
- Balancing detail with readability
- Keeping examples current with API changes
- Managing multiple versions of documentation

---

## Community & Feedback

### What Users Value
- Clear, concise APIs
- Good error messages
- Comprehensive examples
- Fast issue response

### Areas for Improvement
- More sample applications
- Video tutorials
- Better migration guides
- Performance tuning guides

---

## Future Considerations

### Technical Debt
- Consider source generators for tool schema generation
- Evaluate AOT compatibility for all components
- Investigate native interop for performance-critical paths

### Feature Requests
- More pre-built agent patterns
- Additional vector store integrations
- Enhanced observability dashboards
- Better debugging tools for graph execution

---

## Revision History

| Date | Author | Change |
|------|--------|--------|
| 2025-12-07 | - | Initial document created |


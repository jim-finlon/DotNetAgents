# Error Handling Strategy

**Version:** 1.0.0  
**Date:** December 7, 2025  
**Status:** Draft

---

## 1. Exception Hierarchy

### 1.1 Base Exception

All custom exceptions SHALL inherit from `DotLangChainException`:

```csharp
namespace DotLangChain.Core.Exceptions;

/// <summary>
/// Base exception for all DotLangChain errors.
/// </summary>
public class DotLangChainException : Exception
{
    public string? ErrorCode { get; }
    public Dictionary<string, object?>? Context { get; }
    
    public DotLangChainException(string message, string? errorCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
    
    public DotLangChainException(string message, string? errorCode, Dictionary<string, object?>? context, Exception? innerException = null)
        : this(message, errorCode, innerException)
    {
        Context = context;
    }
}
```

### 1.2 Exception Categories

| Exception Type | Base | Error Code Prefix | Use Case |
|----------------|------|-------------------|----------|
| `DocumentException` | `DotLangChainException` | `DLC001` | Document loading/splitting failures |
| `EmbeddingException` | `DotLangChainException` | `DLC002` | Embedding generation failures |
| `VectorStoreException` | `DotLangChainException` | `DLC003` | Vector store operation failures |
| `LLMException` | `DotLangChainException` | `DLC004` | LLM completion failures |
| `GraphException` | `DotLangChainException` | `DLC005` | Graph execution failures |
| `ToolException` | `DotLangChainException` | `DLC006` | Tool execution failures |
| `SecurityException` | `DotLangChainException` | `DLC007` | Security violations |

### 1.3 Exception Examples

**DocumentException**:

```csharp
namespace DotLangChain.Core.Exceptions;

public class DocumentException : DotLangChainException
{
    public string? FilePath { get; }
    public string? FileExtension { get; }
    
    public DocumentException(string message, string? filePath = null, Exception? innerException = null)
        : base(message, "DLC001", innerException)
    {
        FilePath = filePath;
        FileExtension = filePath != null ? Path.GetExtension(filePath) : null;
    }
    
    public static DocumentException UnsupportedFormat(string extension)
    {
        return new DocumentException(
            $"Unsupported document format: {extension}",
            errorCode: "DLC001-001")
        {
            Context = new Dictionary<string, object?> { ["extension"] = extension }
        };
    }
    
    public static DocumentException LoadFailed(string filePath, Exception innerException)
    {
        return new DocumentException(
            $"Failed to load document from {filePath}",
            filePath: filePath,
            innerException: innerException)
        {
            ErrorCode = "DLC001-002"
        };
    }
}
```

**LLMException**:

```csharp
public class LLMException : DotLangChainException
{
    public string? ProviderName { get; }
    public string? Model { get; }
    public int? StatusCode { get; }
    
    public LLMException(string message, string? providerName = null, Exception? innerException = null)
        : base(message, "DLC004", innerException)
    {
        ProviderName = providerName;
    }
    
    public static LLMException RateLimitExceeded(string providerName, TimeSpan? retryAfter = null)
    {
        return new LLMException(
            $"Rate limit exceeded for provider {providerName}",
            providerName: providerName)
        {
            ErrorCode = "DLC004-001",
            Context = retryAfter.HasValue 
                ? new Dictionary<string, object?> { ["retry_after_seconds"] = retryAfter.Value.TotalSeconds }
                : null
        };
    }
    
    public static LLMException ContextLengthExceeded(string providerName, int promptTokens, int maxTokens)
    {
        return new LLMException(
            $"Context length exceeded: {promptTokens} tokens requested, {maxTokens} tokens allowed",
            providerName: providerName)
        {
            ErrorCode = "DLC004-002",
            Context = new Dictionary<string, object?>
            {
                ["prompt_tokens"] = promptTokens,
                ["max_tokens"] = maxTokens
            }
        };
    }
}
```

---

## 2. Error Codes

### 2.1 Error Code Format

Format: `DLC{Category}-{SubCategory}-{Specific}`

- **Category**: 3-digit (001-999)
- **SubCategory**: 3-digit (001-999)
- **Specific**: Optional, 3-digit (001-999)

**Examples**:
- `DLC001-001`: Unsupported document format
- `DLC004-001`: LLM rate limit exceeded
- `DLC004-002`: LLM context length exceeded

### 2.2 Error Code Registry

| Code | Category | Description |
|------|----------|-------------|
| DLC001-001 | Documents | Unsupported format |
| DLC001-002 | Documents | Load failed |
| DLC001-003 | Documents | Invalid metadata |
| DLC002-001 | Embeddings | Provider unavailable |
| DLC002-002 | Embeddings | Rate limit exceeded |
| DLC002-003 | Embeddings | Invalid dimensions |
| DLC003-001 | VectorStore | Connection failed |
| DLC003-002 | VectorStore | Collection not found |
| DLC003-003 | VectorStore | Dimension mismatch |
| DLC004-001 | LLM | Rate limit exceeded |
| DLC004-002 | LLM | Context length exceeded |
| DLC004-003 | LLM | Invalid response format |
| DLC005-001 | Graph | Max steps exceeded |
| DLC005-002 | Graph | Node not found |
| DLC005-003 | Graph | Invalid state transition |
| DLC006-001 | Tools | Tool not found |
| DLC006-002 | Tools | Invalid parameters |
| DLC007-001 | Security | Potential injection detected |
| DLC007-002 | Security | Invalid credentials |

---

## 3. Retry Policies

### 3.1 Standard Retry Strategy

All external service calls SHALL implement retry with exponential backoff:

```csharp
public static class ResiliencePolicies
{
    public static ResiliencePipeline<T> CreateStandardPipeline<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .HandleResult(r => r is HttpResponseMessage { StatusCode: HttpStatusCode.TooManyRequests }),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry {AttemptNumber} after {Delay}ms: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Rate limit");
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(60))
            .Build();
    }
}
```

### 3.2 Provider-Specific Retry Policies

**OpenAI**:
- Max attempts: 3
- Initial delay: 1s
- Exponential backoff: 2x
- Retry on: 429, 500-599, network errors

**Qdrant**:
- Max attempts: 5
- Initial delay: 100ms
- Exponential backoff: 1.5x
- Retry on: Connection errors, 503

**Anthropic**:
- Max attempts: 3
- Initial delay: 2s
- Exponential backoff: 2x
- Retry on: 429, 500-599, network errors

---

## 4. Circuit Breaker

### 4.1 Circuit Breaker Strategy

```csharp
public static ResiliencePipeline<T> CreateCircuitBreakerPipeline<T>(ILogger logger)
{
    return new ResiliencePipelineBuilder<T>()
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder<T>()
                .Handle<HttpRequestException>()
                .HandleResult(r => r is HttpResponseMessage { StatusCode: >= HttpStatusCode.InternalServerError }),
            OnOpened = args =>
            {
                logger.LogError("Circuit breaker opened for {Duration}s", args.BreakDuration.TotalSeconds);
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                logger.LogInformation("Circuit breaker closed");
                return ValueTask.CompletedTask;
            }
        })
        .Build();
}
```

---

## 5. Graceful Degradation

### 5.1 Fallback Strategies

**Embedding Service**:
- Primary: Cloud provider (OpenAI)
- Fallback: Local provider (Ollama)
- Last resort: Error with helpful message

**Vector Store**:
- Primary: Qdrant
- Fallback: PostgreSQL pgvector
- Last resort: In-memory (warning logged)

**LLM Service**:
- Primary: GPT-4o
- Fallback: GPT-4
- Last resort: Error (no suitable fallback)

### 5.2 Degradation Example

```csharp
public class EmbeddingServiceWithFallback : IEmbeddingService
{
    private readonly IEmbeddingService _primary;
    private readonly IEmbeddingService? _fallback;
    private readonly ILogger _logger;
    
    public async Task<EmbeddingResult> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _primary.EmbedAsync(text, cancellationToken);
        }
        catch (EmbeddingException ex) when (_fallback != null)
        {
            _logger.LogWarning(ex, "Primary embedding service failed, using fallback");
            return await _fallback.EmbedAsync(text, cancellationToken);
        }
    }
}
```

---

## 6. Error Logging

### 6.1 Logging Levels

| Level | Exception Type | Action |
|-------|---------------|--------|
| `Error` | Unrecoverable failures | Alert operations |
| `Warning` | Recoverable failures, retries | Monitor trends |
| `Information` | Expected failures (e.g., validation) | Debugging |
| `Debug` | Detailed error context | Development only |

### 6.2 Structured Logging

```csharp
try
{
    await operation.ExecuteAsync();
}
catch (DocumentException ex)
{
    _logger.LogError(
        ex,
        "Document operation failed: {ErrorCode} | File: {FilePath} | Extension: {Extension}",
        ex.ErrorCode,
        ex.FilePath,
        ex.FileExtension);
    
    throw;
}
```

### 6.3 Error Context

Always include:
- Error code
- Operation context (file path, provider, etc.)
- User ID (if available, redacted)
- Timestamp
- Correlation ID (for distributed tracing)

---

## 7. User-Facing Errors

### 7.1 Error Message Guidelines

✅ **Good**:
- Clear, actionable messages
- Include what went wrong and why
- Suggest remediation steps
- Avoid technical jargon

❌ **Bad**:
- "An error occurred"
- Stack traces in user messages
- Internal implementation details

### 7.2 Error Message Examples

```csharp
// Good
throw new DocumentException(
    $"Unable to load document '{fileName}'. The file format '{extension}' is not supported. Supported formats: PDF, DOCX, HTML, Markdown.",
    errorCode: "DLC001-001");

// Bad
throw new DocumentException($"Error: {ex.Message}", innerException: ex);
```

---

## 8. Cancellation Support

### 8.1 CancellationToken Usage

All async operations SHALL accept `CancellationToken`:

```csharp
public async Task<Document> LoadAsync(
    string filePath,
    CancellationToken cancellationToken = default)
{
    cancellationToken.ThrowIfCancellationRequested();
    
    // Long-running operation
    await ProcessFileAsync(filePath, cancellationToken);
}
```

### 8.2 Cancellation Handling

```csharp
try
{
    await operation.ExecuteAsync(cancellationToken);
}
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    _logger.LogInformation("Operation cancelled by user");
    throw; // Re-throw to caller
}
```

---

## 9. Validation Errors

### 9.1 Input Validation

Validate early, fail fast:

```csharp
public async Task<EmbeddingResult> EmbedAsync(string text, CancellationToken cancellationToken = default)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(text);
    
    if (text.Length > MaxInputLength)
    {
        throw new ArgumentException(
            $"Text length {text.Length} exceeds maximum {MaxInputLength}",
            nameof(text));
    }
    
    // Proceed with operation
}
```

### 9.2 Validation Exception Types

- `ArgumentException`: Invalid argument value
- `ArgumentNullException`: Null argument
- `ArgumentOutOfRangeException`: Out-of-range value

---

## 10. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-07 | - | Initial draft |


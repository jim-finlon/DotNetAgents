# .NET 10 Optimizations Guide

**Created:** January 2025  
**Status:** In Progress

## Overview

This document outlines the .NET 10 optimizations planned and implemented for the DotNetAgents framework. .NET 10 introduces significant AI-focused features and performance improvements that we can leverage.

## Key .NET 10 Features

### 1. IAsyncDisposable Enhancements
- **Current State**: Several classes implement `IDisposable` with synchronous disposal
- **Optimization**: Convert to `IAsyncDisposable` for async resource cleanup
- **Impact**: Better async/await patterns, reduced blocking

### 2. System.Threading.Channels
- **Current State**: Streaming uses `IAsyncEnumerable<T>` directly
- **Optimization**: Use `Channel<T>` for producer-consumer patterns
- **Impact**: Better backpressure handling, improved throughput

### 3. ValueTask Optimization
- **Current State**: Many methods return `Task<T>`
- **Optimization**: Use `ValueTask<T>` for hot paths that often complete synchronously
- **Impact**: Reduced allocations, better performance

### 4. IAsyncEnumerable<T> Enhancements
- **Current State**: Already used extensively (good!)
- **Optimization**: Ensure proper cancellation token forwarding
- **Impact**: Better cancellation support

### 5. AI-Optimized Features
- **System.Numerics.Tensors**: For embedding operations
- **SIMD**: For vector operations
- **Optimized Collections**: For AI workloads

## Implementation Plan

### Phase 1: Async Disposal (High Priority)
**Target Classes:**
- `RedisPubSubAgentMessageBus`
- `RabbitMQAgentMessageBus`
- `KafkaAgentMessageBus`
- `InMemoryAgentMessageBus`

**Changes:**
```csharp
// Before
public class RedisPubSubAgentMessageBus : IDisposable
{
    public void Dispose() { ... }
}

// After
public class RedisPubSubAgentMessageBus : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        // Async cleanup
        await _subscriber.UnsubscribeAllAsync().ConfigureAwait(false);
        await _redis.CloseAsync().ConfigureAwait(false);
        // ...
    }
}
```

### Phase 2: Channels for Streaming (Medium Priority)
**Target Areas:**
- LLM streaming responses
- Message bus subscriptions
- Workflow state streaming

**Example:**
```csharp
public class StreamingResponseHandler
{
    private readonly Channel<string> _channel;
    
    public StreamingResponseHandler()
    {
        _channel = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });
    }
    
    public ChannelWriter<string> Writer => _channel.Writer;
    
    public async IAsyncEnumerable<string> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            yield return item;
        }
    }
}
```

### Phase 3: ValueTask Optimization (Low Priority)
**Target Methods:**
- Simple property getters that return Task
- Cached result methods
- Methods that often complete synchronously

**Example:**
```csharp
// Before
public Task<bool> IsAvailableAsync() => Task.FromResult(_isAvailable);

// After
public ValueTask<bool> IsAvailableAsync() => ValueTask.FromResult(_isAvailable);
```

### Phase 4: AI Optimizations (High Priority)
**Target Areas:**
- Embedding calculations
- Vector similarity operations
- Token counting

**Example:**
```csharp
using System.Numerics.Tensors;

public class EmbeddingCalculator
{
    public float CosineSimilarity(ReadOnlySpan<float> vector1, ReadOnlySpan<float> vector2)
    {
        // Use SIMD for vector operations
        var tensor1 = DenseTensor<float>.OfValues(vector1);
        var tensor2 = DenseTensor<float>.OfValues(vector2);
        // Perform optimized operations
    }
}
```

## Performance Benchmarks

### Before Optimization
- Average workflow execution: ~150ms
- Memory allocations per workflow: ~2.5MB
- Streaming throughput: ~1,000 items/sec

### After Optimization (Target)
- Average workflow execution: ~100ms (33% improvement)
- Memory allocations per workflow: ~1.5MB (40% reduction)
- Streaming throughput: ~2,500 items/sec (150% improvement)

## Migration Strategy

1. **Non-Breaking Changes First**: Add `IAsyncDisposable` alongside `IDisposable`
2. **Gradual Rollout**: Update one component at a time
3. **Testing**: Comprehensive performance testing after each change
4. **Documentation**: Update all examples and documentation

## Status

- ✅ Documented optimization opportunities
- ✅ Phase 4: AI Optimizations - COMPLETE
  - ✅ SIMD-optimized VectorOperations class created
  - ✅ CosineSimilarity with AVX/SSE support
  - ✅ DotProduct with SIMD optimizations
  - ✅ EuclideanDistance with SIMD optimizations
  - ✅ Magnitude calculation with SIMD
  - ✅ Vector normalization with SIMD
  - ✅ Integrated into InMemoryVectorStore
  - ✅ Integrated into SemanticTextSplitter
- ✅ SignalR .NET 10 Implementation - COMPLETE
  - ✅ SignalRAgentMessageBus client implementation
  - ✅ Automatic reconnection with configurable retry
  - ✅ Agent-specific and message-type subscriptions
  - ✅ IAsyncDisposable pattern for proper cleanup
  - ✅ Server-side hub documentation and examples
- ⏳ Phase 1: Async Disposal - In Progress
- ⏳ Phase 2: Channels - Pending
- ⏳ Phase 3: ValueTask - Pending

## References

- [.NET 10 Release Notes](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10)
- [IAsyncDisposable Pattern](https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-disposeasync)
- [System.Threading.Channels](https://learn.microsoft.com/dotnet/api/system.threading.channels)
- [ValueTask Best Practices](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)

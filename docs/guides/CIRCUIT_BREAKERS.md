# Circuit Breaker Guide for DotNetAgents

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

Circuit breakers prevent cascading failures by stopping calls to failing services. DotNetAgents includes built-in circuit breaker support.

## Using Circuit Breakers

### Basic Usage

```csharp
var circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions
{
    FailureThreshold = 5,
    FailureWindow = TimeSpan.FromSeconds(30),
    OpenDuration = TimeSpan.FromSeconds(60)
});

var result = await circuitBreaker.ExecuteAsync(
    async ct => await externalService.CallAsync(ct),
    cancellationToken);
```

### With LLM Models

```csharp
var circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions
{
    FailureThreshold = 3,
    FailureWindow = TimeSpan.FromSeconds(60),
    OpenDuration = TimeSpan.FromSeconds(120)
});

var resilientModel = new ResilientLLMModel<ChatMessage[], ChatMessage>(
    openAIModel,
    circuitBreaker: circuitBreaker);
```

### Configuration Options

```csharp
var options = new CircuitBreakerOptions
{
    // Number of failures before opening circuit
    FailureThreshold = 5,
    
    // Time window for counting failures
    FailureWindow = TimeSpan.FromSeconds(30),
    
    // How long circuit stays open before trying half-open
    OpenDuration = TimeSpan.FromSeconds(60),
    
    // Custom logic to determine if exception counts as failure
    ShouldCountAsFailure = ex => ex is HttpRequestException || ex is TimeoutException
};
```

## Circuit Breaker States

### Closed (Normal)
- Circuit is closed, requests flow through normally
- Failures are counted
- Opens if failure threshold exceeded

### Open (Failing)
- Circuit is open, requests are immediately rejected
- Prevents further load on failing service
- Automatically transitions to half-open after OpenDuration

### Half-Open (Testing)
- Allows one request through to test if service recovered
- On success: transitions to closed
- On failure: transitions back to open

## Monitoring Circuit Breakers

### Metrics

```csharp
_metricsCollector.RecordGauge("circuit_breaker_state", 
    circuitBreaker.State == CircuitBreakerState.Open ? 1 : 0,
    tags: new Dictionary<string, object> { ["service"] = "llm" });
```

### Prometheus Query

```promql
dotnetagents_circuit_breaker_state{state="open"}
```

## Best Practices

1. **Set Appropriate Thresholds**: Balance between sensitivity and stability
2. **Monitor State Changes**: Alert when circuit opens
3. **Use with Retry**: Combine circuit breakers with retry policies
4. **Test Failures**: Regularly test circuit breaker behavior
5. **Document Fallbacks**: Document what happens when circuit is open

## Related Documentation

- [Graceful Degradation](./GRACEFUL_DEGRADATION.md)
- [Error Handling](./ERROR_HANDLING.md)
- [Alerting Guide](./ALERTING.md)

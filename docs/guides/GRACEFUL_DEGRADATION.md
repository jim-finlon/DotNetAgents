# Graceful Degradation Guide for DotNetAgents

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This guide explains how to implement graceful degradation strategies in DotNetAgents to maintain service availability when components fail.

## Degradation Strategies

### 1. LLM Provider Fallback

When the primary LLM provider fails, automatically fall back to a secondary provider.

```csharp
services.AddOpenAI(options => { /* ... */ })
        .AddFallback<AnthropicModel>(fallbackOptions => { /* ... */ });
```

**Implementation:**

```csharp
public class FallbackLLMModel<TInput, TOutput> : ILLMModel<TInput, TOutput>
{
    private readonly ILLMModel<TInput, TOutput> _primary;
    private readonly ILLMModel<TInput, TOutput> _fallback;
    private readonly ILogger<FallbackLLMModel<TInput, TOutput>> _logger;

    public async Task<TOutput> GenerateAsync(
        TInput input,
        LLMOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _primary.GenerateAsync(input, options, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (IsTransientError(ex))
        {
            _logger.LogWarning(ex, "Primary LLM failed, falling back to secondary");
            return await _fallback.GenerateAsync(input, options, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static bool IsTransientError(Exception ex)
    {
        return ex is HttpRequestException ||
               ex is TaskCanceledException ||
               ex is TimeoutException;
    }
}
```

### 2. Database Fallback to Cache

When database is unavailable, serve from cache.

```csharp
public class ResilientAgentRegistry : IAgentRegistry
{
    private readonly IAgentRegistry _primary;
    private readonly IMemoryCache _cache;

    public async Task<AgentInfo?> GetByIdAsync(
        string agentId,
        CancellationToken cancellationToken = default)
    {
        // Try cache first
        var cacheKey = $"agent:{agentId}";
        if (_cache.TryGetValue(cacheKey, out AgentInfo? cached))
        {
            return cached;
        }

        try
        {
            var agent = await _primary.GetByIdAsync(agentId, cancellationToken)
                .ConfigureAwait(false);
            
            if (agent != null)
            {
                _cache.Set(cacheKey, agent, TimeSpan.FromMinutes(5));
            }
            
            return agent;
        }
        catch (Exception ex) when (IsDatabaseError(ex))
        {
            // Return cached value even if stale
            return cached;
        }
    }
}
```

### 3. Message Bus Fallback

When primary message bus fails, use in-memory fallback.

```csharp
public class FallbackMessageBus : IAgentMessageBus
{
    private readonly IAgentMessageBus _primary;
    private readonly InMemoryAgentMessageBus _fallback;
    private readonly ILogger<FallbackMessageBus> _logger;

    public async Task<MessageSendResult> SendAsync(
        AgentMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _primary.SendAsync(message, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary message bus failed, using in-memory fallback");
            return await _fallback.SendAsync(message, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
```

### 4. Reduced Functionality Mode

When critical components fail, operate in reduced functionality mode.

```csharp
public class DegradedModeService
{
    private readonly IAgentRegistry _registry;
    private readonly ILogger<DegradedModeService> _logger;
    private bool _isDegraded;

    public async Task<AgentInfo?> GetAvailableWorkerAsync(
        string? requiredCapability = null,
        CancellationToken cancellationToken = default)
    {
        if (_isDegraded)
        {
            // In degraded mode, return any available agent
            // Ignore capability requirements
            return await _registry.GetAllAsync(cancellationToken)
                .FirstOrDefaultAsync(a => a.Status == AgentStatus.Available);
        }

        // Normal operation
        return await _registry.GetAvailableWorkerAsync(requiredCapability, cancellationToken)
            .ConfigureAwait(false);
    }

    public void EnableDegradedMode()
    {
        _isDegraded = true;
        _logger.LogWarning("System operating in degraded mode");
    }
}
```

### 5. Circuit Breaker with Fallback

Combine circuit breakers with fallback strategies.

```csharp
public class ResilientService
{
    private readonly CircuitBreaker _circuitBreaker;
    private readonly IBackupService _backup;

    public async Task<Result> ProcessAsync(Input input)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(
                async ct => await _primaryService.ProcessAsync(input, ct),
                cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException) when (_circuitBreaker.State == CircuitBreakerState.Open)
        {
            // Circuit breaker is open, use backup
            return await _backup.ProcessAsync(input, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
```

## Configuration

### Enable Degradation Features

```csharp
services.AddDotNetAgents(options =>
{
    options.EnableGracefulDegradation = true;
    options.FallbackLLMProvider = "anthropic";
    options.CacheFallbackEnabled = true;
    options.DegradedModeThreshold = 0.5; // Enable degraded mode if >50% failures
});
```

## Monitoring Degradation

### Metrics

Track degradation events:

```csharp
_metricsCollector.IncrementCounter("degradation.enabled", tags: new Dictionary<string, object>
{
    ["component"] = "agent_registry",
    ["reason"] = "database_unavailable"
});
```

### Alerts

Set up alerts for degradation:

```yaml
- alert: DegradedModeActive
  expr: dotnetagents_degraded_mode_active == 1
  for: 5m
  annotations:
    summary: "System operating in degraded mode"
```

## Best Practices

1. **Fail Fast**: Detect failures quickly and switch to fallback
2. **Monitor Fallbacks**: Track when fallbacks are used
3. **Test Fallbacks**: Regularly test fallback mechanisms
4. **Document Behavior**: Document what functionality is lost in degraded mode
5. **Automatic Recovery**: Automatically return to normal mode when primary recovers

## Related Documentation

- [Circuit Breakers](./CIRCUIT_BREAKERS.md)
- [Disaster Recovery](../operations/DISASTER_RECOVERY.md)
- [Alerting Guide](./ALERTING.md)

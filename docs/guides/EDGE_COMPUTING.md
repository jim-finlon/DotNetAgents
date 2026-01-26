# Edge Computing Support Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

DotNetAgents.Edge provides support for mobile and edge deployments with offline capabilities, optimized models, and reduced resource requirements.

## Features

- ✅ **Offline Mode**: Execute agents without network connectivity
- ✅ **Offline Cache**: Cache results for offline access
- ✅ **Edge Models**: Support for quantized/pruned models
- ✅ **Network Monitoring**: Automatic online/offline detection
- ✅ **Mobile-Friendly**: Optimized for iOS/Android deployments

## Installation

```bash
dotnet add package DotNetAgents.Edge
```

## Basic Usage

### Setting Up Edge Agent

```csharp
using DotNetAgents.Edge;

services.AddDotNetAgentsEdge(config =>
{
    config.ModelType = EdgeModelType.Quantized;
    config.MaxModelSizeMB = 100;
    config.Quantization = QuantizationLevel.Q4;
    config.MaxContextLength = 2048;
});

// Register edge agent
services.AddEdgeAgent();
```

### Using Edge Agent

```csharp
var edgeAgent = serviceProvider.GetRequiredService<IEdgeAgent>();

// Execute with automatic offline fallback
var result = await edgeAgent.ExecuteAsync("What is the weather?");

Console.WriteLine($"Output: {result.Output}");
Console.WriteLine($"Was offline: {result.WasOffline}");
Console.WriteLine($"Confidence: {result.ConfidenceScore:P2}");

if (result.Warnings.Any())
{
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"Warning: {warning}");
    }
}
```

## Offline Mode

### Automatic Offline Fallback

The edge agent automatically falls back to offline mode when:
- Network is unavailable
- Online execution fails
- Offline mode is explicitly requested

```csharp
// Automatically uses offline mode if online fails
var result = await edgeAgent.ExecuteAsync(input);
```

### Explicit Offline Execution

```csharp
// Force offline execution
var result = await edgeAgent.ExecuteOfflineAsync(input);
```

### Checking Status

```csharp
if (edgeAgent.IsOnline)
{
    Console.WriteLine("Agent is online");
}
else
{
    Console.WriteLine("Agent is offline");
}

var mode = edgeAgent.OfflineMode;
switch (mode)
{
    case OfflineModeStatus.Online:
        Console.WriteLine("Full online capabilities");
        break;
    case OfflineModeStatus.Offline:
        Console.WriteLine("Offline mode - cached/local only");
        break;
    case OfflineModeStatus.Degraded:
        Console.WriteLine("Degraded mode - some features unavailable");
        break;
}
```

## Offline Cache

### Custom Cache Implementation

```csharp
public class FileSystemCache : IOfflineCache
{
    private readonly string _cacheDirectory;

    public FileSystemCache(string cacheDirectory)
    {
        _cacheDirectory = cacheDirectory;
        Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_cacheDirectory, $"{key}.cache");
        if (File.Exists(filePath))
        {
            return await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        return null;
    }

    public async Task SetAsync(string key, string value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_cacheDirectory, $"{key}.cache");
        await File.WriteAllTextAsync(filePath, value, cancellationToken);
    }

    // ... implement other methods
}

// Register custom cache
services.AddSingleton<IOfflineCache, FileSystemCache>();
```

### Cache Management

```csharp
var cache = serviceProvider.GetRequiredService<IOfflineCache>();

// Get cache size
var size = await cache.GetSizeAsync();
Console.WriteLine($"Cache size: {size / 1024 / 1024} MB");

// Clear cache
await cache.ClearAsync();
```

## Edge Models

### Model Configuration

```csharp
var config = new EdgeModelConfiguration
{
    ModelType = EdgeModelType.Quantized,
    Quantization = QuantizationLevel.Q4,
    MaxModelSizeMB = 100,
    MaxContextLength = 2048,
    UseGpuAcceleration = false, // Set to true if GPU available
    ModelPath = "path/to/model"
};
```

### Model Types

- **Quantized**: Reduced precision (Q8, Q4, Q2)
- **Pruned**: Removed parameters
- **Distilled**: Knowledge distillation
- **Custom**: Custom edge model

### Implementing Edge Model Provider

```csharp
public class LocalLLMProvider : IEdgeModelProvider
{
    public async Task<string> GenerateAsync(string input, CancellationToken cancellationToken = default)
    {
        // Load local quantized model
        // Generate response
        // Return result
        return await Task.FromResult("Generated response");
    }
}

// Register
services.AddSingleton<IEdgeModelProvider, LocalLLMProvider>();
```

## Network Monitoring

### Custom Network Monitor

```csharp
public class ConnectivityMonitor : INetworkMonitor
{
    public bool IsOnline => Connectivity.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler<NetworkStatusEventArgs>? StatusChanged;

    public ConnectivityMonitor()
    {
        Connectivity.ConnectivityChanged += (sender, e) =>
        {
            StatusChanged?.Invoke(this, new NetworkStatusEventArgs
            {
                IsOnline = e.NetworkAccess == NetworkAccess.Internet
            });
        };
    }
}

// Register
services.AddSingleton<INetworkMonitor, ConnectivityMonitor>();
```

## Mobile Deployment

### iOS Configuration

```xml
<!-- In .csproj -->
<PropertyGroup>
  <TargetFramework>net10.0-ios</TargetFramework>
  <SupportedOSPlatformVersion>14.0</SupportedOSPlatformVersion>
</PropertyGroup>
```

### Android Configuration

```xml
<!-- In .csproj -->
<PropertyGroup>
  <TargetFramework>net10.0-android</TargetFramework>
  <SupportedOSPlatformVersion>21.0</SupportedOSPlatformVersion>
</PropertyGroup>
```

### .NET MAUI Integration

```csharp
// In MauiProgram.cs
builder.Services.AddDotNetAgentsEdge();
builder.Services.AddEdgeAgent();
```

## Best Practices

1. **Cache Strategy**
   - Cache frequently used queries
   - Set appropriate TTL values
   - Monitor cache size

2. **Model Selection**
   - Use quantized models for mobile
   - Balance size vs. accuracy
   - Test on target devices

3. **Offline Handling**
   - Provide clear user feedback
   - Use cached results when possible
   - Queue operations for sync

4. **Resource Management**
   - Monitor memory usage
   - Limit model size
   - Use GPU when available

## Limitations

- Edge models may have lower accuracy than full models
- Offline mode has limited capabilities
- Cache size is constrained by device storage
- Some features require online connectivity

## Related Documentation

- [Agent Executor](./AGENT_EXECUTOR.md)
- [Model Configuration](./MODEL_CONFIGURATION.md)
- [Mobile Development](../architecture/MOBILE_DEVELOPMENT.md)

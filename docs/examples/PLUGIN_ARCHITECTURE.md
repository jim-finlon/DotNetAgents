# Plugin Architecture Examples

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This guide provides comprehensive examples of the DotNetAgents plugin architecture, showing how to create, register, and use plugins.

## Table of Contents

1. [Creating a Plugin](#creating-a-plugin)
2. [Plugin Registration](#plugin-registration)
3. [Plugin Discovery](#plugin-discovery)
4. [Plugin Dependencies](#plugin-dependencies)
5. [Plugin Lifecycle](#plugin-lifecycle)
6. [Complete Example](#complete-example)

## Creating a Plugin

### Basic Plugin

```csharp
using DotNetAgents.Ecosystem;

namespace MyProject.Plugins;

public class MyFeaturePlugin : PluginBase
{
    public MyFeaturePlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "myfeature",
            Name = "My Feature",
            Version = "1.0.0",
            Description = "A custom feature plugin for DotNetAgents",
            Author = "Your Name",
            License = "MIT",
            Dependencies = [],
            MinimumCoreVersion = "1.0.0"
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("My Feature plugin initialized");
        
        // Access services
        var configuration = context.Configuration;
        var services = context.Services;
        
        // Register additional services if needed
        // Note: Services should be registered in ServiceCollectionExtensions
        
        return Task.CompletedTask;
    }

    protected override Task OnShutdownAsync(CancellationToken cancellationToken)
    {
        Logger?.LogInformation("My Feature plugin shutting down");
        return Task.CompletedTask;
    }
}
```

### Plugin with Dependencies

```csharp
public class AdvancedFeaturePlugin : PluginBase
{
    public AdvancedFeaturePlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "advancedfeature",
            Name = "Advanced Feature",
            Version = "1.0.0",
            Description = "Advanced feature that depends on other plugins",
            Author = "Your Name",
            License = "MIT",
            Dependencies = ["core", "statemachines"], // Depends on core and state machines
            MinimumCoreVersion = "1.0.0"
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Advanced Feature plugin initialized");
        
        // Dependencies are guaranteed to be initialized before this plugin
        var stateMachinePlugin = context.Services.GetService<IPluginRegistry>()?
            .GetPlugin("statemachines");
        
        if (stateMachinePlugin != null)
        {
            Logger?.LogInformation("State Machines plugin is available");
        }
        
        return Task.CompletedTask;
    }
}
```

## Plugin Registration

### Manual Registration

```csharp
using DotNetAgents.Ecosystem;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        // Register the plugin
        services.AddPlugin(new MyFeaturePlugin());
        
        // Register plugin-specific services
        services.AddSingleton<IMyFeatureService, MyFeatureService>();
        
        return services;
    }
}
```

### Using the Plugin

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Enable plugin system
services.AddDotNetAgentsEcosystem();

// Register your plugin
services.AddMyFeature();

var serviceProvider = services.BuildServiceProvider();
```

## Plugin Discovery

### Automatic Discovery

Plugins are automatically discovered when `EnablePluginDiscovery()` is called:

```csharp
services.AddDotNetAgentsEcosystem();
services.EnablePluginDiscovery(); // Automatically discovers all plugins

// Plugins are discovered and initialized at application startup
var serviceProvider = services.BuildServiceProvider();
var host = serviceProvider.GetRequiredService<IHost>();
await host.StartAsync();
```

### Manual Discovery

```csharp
var pluginDiscovery = serviceProvider.GetRequiredService<IPluginDiscovery>();

// Discover plugins in specific assemblies
var assemblies = new[] { typeof(MyFeaturePlugin).Assembly };
var pluginTypes = pluginDiscovery.DiscoverPluginTypes(assemblies);

// Create plugin instances
var plugins = pluginDiscovery.CreatePluginInstances(pluginTypes, serviceProvider);

foreach (var plugin in plugins)
{
    Console.WriteLine($"Discovered plugin: {plugin.Name} v{plugin.Version}");
}
```

## Plugin Dependencies

### Declaring Dependencies

```csharp
Metadata = new PluginMetadata
{
    Id = "dependent-plugin",
    Dependencies = ["core", "statemachines", "workflow"],
    MinimumCoreVersion = "1.0.0"
};
```

### Dependency Resolution

Dependencies are automatically resolved and plugins are initialized in the correct order:

```csharp
// Plugin registry handles dependency resolution
var registry = serviceProvider.GetRequiredService<IPluginRegistry>();

// Get all plugins in dependency order
var plugins = await registry.GetPluginsAsync();

// Initialize plugins (dependencies first)
var lifecycleManager = serviceProvider.GetRequiredService<IPluginLifecycleManager>();
await lifecycleManager.InitializePluginsAsync(cancellationToken);
```

## Plugin Lifecycle

### Initialization

```csharp
protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
{
    Logger?.LogInformation("Initializing plugin...");
    
    // Access configuration
    var configValue = context.Configuration["MyFeature:Setting"];
    
    // Access services
    var loggerFactory = context.LoggerFactory;
    var logger = loggerFactory.CreateLogger<MyFeaturePlugin>();
    
    // Initialize plugin resources
    InitializeResources();
    
    Logger?.LogInformation("Plugin initialized successfully");
    return Task.CompletedTask;
}
```

### Shutdown

```csharp
protected override Task OnShutdownAsync(CancellationToken cancellationToken)
{
    Logger?.LogInformation("Shutting down plugin...");
    
    // Cleanup resources
    CleanupResources();
    
    Logger?.LogInformation("Plugin shut down successfully");
    return Task.CompletedTask;
}
```

## Complete Example

### 1. Create Plugin Class

```csharp
// MyFeaturePlugin.cs
using DotNetAgents.Ecosystem;

namespace MyProject.Plugins;

public class MyFeaturePlugin : PluginBase
{
    private IMyFeatureService? _service;

    public MyFeaturePlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "myfeature",
            Name = "My Feature",
            Version = "1.0.0",
            Description = "Custom feature plugin",
            Author = "Your Name",
            License = "MIT",
            Dependencies = [],
            MinimumCoreVersion = "1.0.0"
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Initializing My Feature plugin");
        
        _service = context.Services.GetService<IMyFeatureService>();
        if (_service != null)
        {
            Logger?.LogInformation("My Feature service is available");
        }
        
        return Task.CompletedTask;
    }

    protected override Task OnShutdownAsync(CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Shutting down My Feature plugin");
        _service = null;
        return Task.CompletedTask;
    }
}
```

### 2. Create Service Interface and Implementation

```csharp
// IMyFeatureService.cs
public interface IMyFeatureService
{
    Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default);
}

// MyFeatureService.cs
public class MyFeatureService : IMyFeatureService
{
    private readonly ILogger<MyFeatureService> _logger;

    public MyFeatureService(ILogger<MyFeatureService> logger)
    {
        _logger = logger;
    }

    public Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing input: {Input}", input);
        return Task.FromResult($"Processed: {input}");
    }
}
```

### 3. Create Service Registration Extension

```csharp
// ServiceCollectionExtensions.cs
using DotNetAgents.Ecosystem;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        // Register plugin
        services.AddPlugin(new MyFeaturePlugin());
        
        // Register services
        services.AddSingleton<IMyFeatureService, MyFeatureService>();
        
        return services;
    }
}
```

### 4. Use in Application

```csharp
// Program.cs
using DotNetAgents.Ecosystem;
using MyProject.Plugins;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

// Enable plugin system
services.AddDotNetAgentsEcosystem();

// Register your plugin
services.AddMyFeature();

var serviceProvider = services.BuildServiceProvider();

// Use the service
var featureService = serviceProvider.GetRequiredService<IMyFeatureService>();
var result = await featureService.ProcessAsync("Hello, World!");

Console.WriteLine($"Result: {result}");

// Check plugin status
var pluginRegistry = serviceProvider.GetRequiredService<IPluginRegistry>();
var plugin = await pluginRegistry.GetPluginAsync("myfeature");
if (plugin != null)
{
    Console.WriteLine($"Plugin: {plugin.Name} v{plugin.Version}");
    Console.WriteLine($"Description: {plugin.Description}");
}
```

## Best Practices

1. **Plugin ID**: Use lowercase, hyphenated IDs (e.g., `my-feature`, not `MyFeature`)
2. **Dependencies**: Always declare dependencies in metadata
3. **Lifecycle**: Clean up resources in `OnShutdownAsync`
4. **Logging**: Use the provided `Logger` property for logging
5. **Services**: Register services in `ServiceCollectionExtensions`, not in `OnInitializeAsync`
6. **Error Handling**: Handle errors gracefully in lifecycle methods
7. **Versioning**: Follow semantic versioning for plugin versions

## Related Documentation

- [Plugin Architecture Migration Guide](../PLUGIN_ARCHITECTURE_MIGRATION.md)
- [Ecosystem Integrations Guide](../guides/ECOSYSTEM_INTEGRATIONS.md)
- [Plugin Base Class Reference](../../src/DotNetAgents.Ecosystem/PluginBase.cs)

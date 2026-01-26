# Plugin Architecture Migration Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Migration Guide

## Overview

DotNetAgents has been restructured to use a plugin architecture. This guide helps you migrate your existing code to use the new plugin system.

## What Changed?

### Before (Direct Package Usage)

Previously, you would directly reference packages and use their extension methods:

```csharp
services.AddInMemoryAgentRegistry();
services.AddInMemoryAgentMessageBus();
// State machines were used directly without registration
```

### After (Plugin Architecture)

Now, packages are registered as plugins and can be discovered automatically:

```csharp
services.AddDotNetAgentsEcosystem(); // Enable plugin system
services.AddStateMachines(); // Register State Machines plugin
services.AddInMemoryAgentRegistry();
services.AddInMemoryAgentMessageBus();
```

## Migration Steps

### Step 1: Add Ecosystem Package

Add the `DotNetAgents.Ecosystem` package reference to your project:

```xml
<ProjectReference Include="..\..\src\DotNetAgents.Ecosystem\DotNetAgents.Ecosystem.csproj" />
```

Or if using NuGet:

```bash
dotnet add package DotNetAgents.Ecosystem
```

### Step 2: Enable Plugin System

In your service registration, add:

```csharp
services.AddDotNetAgentsEcosystem();
```

This enables:
- Plugin discovery
- Auto-registration
- Dependency resolution
- Lifecycle management

### Step 3: Register Plugins

Register the plugins you need using their extension methods:

```csharp
// Agent Capabilities
services.AddStateMachines();
services.AddBehaviorTrees();
services.AddSwarmIntelligence();
services.AddHierarchicalAgents();
services.AddAgentMarketplace();

// Integrations
services.AddMcpClients();
services.AddDotNetAgentsEdge();

// Infrastructure (automatically registered when you use their extension methods)
services.AddRabbitMQMessageBus(options => { ... });
services.AddPostgreSQLVectorStore(connectionString);
services.AddOpenAI(apiKey, modelName);
```

### Step 4: Initialize Plugins (Optional)

If you need to initialize plugins explicitly:

```csharp
var serviceProvider = services.BuildServiceProvider();
await serviceProvider.InitializePluginsAsync();
```

**Note:** Plugins are automatically initialized if you use `EnablePluginDiscovery()`.

## Plugin Categories

### Agent Capabilities (Plugins)

These are now plugins and should be registered:

- **State Machines**: `services.AddStateMachines()`
- **Behavior Trees**: `services.AddBehaviorTrees()`
- **Swarm Intelligence**: `services.AddSwarmIntelligence()`
- **Hierarchical Agents**: `services.AddHierarchicalAgents()`
- **Agent Marketplace**: `services.AddAgentMarketplace()`

### Infrastructure (Auto-Registered as Plugins)

These register themselves as plugins when you use their extension methods:

- **Message Buses**: `AddRabbitMQMessageBus()`, `AddKafkaAgentMessageBus()`, etc.
- **Vector Stores**: `AddPostgreSQLVectorStore()`, `AddPineconeVectorStore()`, etc.
- **Storage**: `AddPostgreSQLCheckpointStore()`, `AddSqlServerTaskStore()`, etc.
- **LLM Providers**: `AddOpenAI()`, `AddAnthropic()`, `AddOllama()`, etc.

## Automatic Plugin Discovery

You can enable automatic plugin discovery from loaded assemblies:

```csharp
services.AddDotNetAgentsEcosystem();
services.EnablePluginDiscovery(); // Discovers and registers all plugins automatically
```

This will:
1. Scan all loaded assemblies for `IPlugin` implementations
2. Create plugin instances
3. Register them in the plugin registry
4. Resolve dependencies
5. Initialize them in dependency order

## Backward Compatibility

**Good News:** Your existing code will continue to work! The plugin registration is **additive** - it doesn't break existing functionality.

However, to take advantage of:
- Plugin discovery
- Dependency resolution
- Lifecycle management
- Plugin metadata

You should migrate to the new approach.

## Examples

### Example 1: Basic Setup with Plugins

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Enable plugin system
services.AddDotNetAgentsEcosystem();

// Register plugins
services.AddStateMachines();
services.AddBehaviorTrees();

// Register infrastructure (auto-registers as plugins)
services.AddInMemoryAgentRegistry();
services.AddInMemoryAgentMessageBus();
services.AddOpenAI(apiKey, "gpt-4");

var serviceProvider = services.BuildServiceProvider();

// Initialize plugins (optional if using EnablePluginDiscovery)
await serviceProvider.InitializePluginsAsync();
```

### Example 2: Automatic Discovery

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Enable plugin system with auto-discovery
services.AddDotNetAgentsEcosystem();
services.EnablePluginDiscovery();

// All plugins from loaded assemblies will be discovered and registered automatically
// You still need to register infrastructure explicitly:
services.AddInMemoryAgentRegistry();
services.AddOpenAI(apiKey, "gpt-4");

var serviceProvider = services.BuildServiceProvider();
// Plugins are initialized automatically by the hosted service
```

### Example 3: Querying Plugins

```csharp
var pluginRegistry = serviceProvider.GetRequiredService<IPluginRegistry>();

// Get all plugins
var allPlugins = await pluginRegistry.GetAllAsync();

// Get plugins by category
var agentPlugins = await pluginRegistry.GetByCategoryAsync("Agent Capabilities");

// Get specific plugin
var stateMachinePlugin = await pluginRegistry.GetAsync("statemachines");
```

## Breaking Changes

**None!** The plugin architecture is fully backward compatible. All existing extension methods continue to work as before.

## Benefits of Migration

1. **Plugin Discovery**: Automatically find and register plugins
2. **Dependency Management**: Plugins declare dependencies that are resolved automatically
3. **Lifecycle Management**: Proper initialization and shutdown order
4. **Metadata**: Rich plugin information (version, description, tags)
5. **Extensibility**: Third parties can create plugins easily

## Troubleshooting

### Plugin Not Found

If a plugin isn't being discovered:

1. Ensure the package is referenced in your project
2. Call the appropriate `Add*()` extension method
3. Or enable `EnablePluginDiscovery()` to auto-discover

### Dependency Resolution Errors

If you see dependency errors:

1. Check plugin metadata for required dependencies
2. Ensure dependent plugins are registered first
3. Use explicit registration order if needed

### Version Conflicts

If you see package version conflicts:

1. Update all `Microsoft.Extensions.*` packages to version 10.0.0
2. Ensure Ecosystem package is referenced (requires 10.0.0)

## Next Steps

1. Review your service registration code
2. Add `AddDotNetAgentsEcosystem()` call
3. Add plugin registration calls for features you use
4. Test your application
5. Consider enabling auto-discovery for convenience

## Support

For questions or issues, please:
- Check the [Plugin Development Guide](docs/guides/PLUGIN_DEVELOPMENT.md)
- Review [Architecture Documentation](docs/architecture/ARCHITECTURE_SUMMARY.md)
- Open an issue on GitHub

---

**Note:** This migration is optional but recommended. Your existing code will continue to work without changes.

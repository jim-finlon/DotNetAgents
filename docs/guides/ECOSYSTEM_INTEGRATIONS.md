# Ecosystem & Integrations Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

DotNetAgents.Ecosystem provides a plugin architecture and integration marketplace for extending DotNetAgents with third-party tools, providers, and workflows.

## Features

- ✅ **Plugin Architecture**: Extensible plugin system
- ✅ **Integration Marketplace**: Discover and share integrations
- ✅ **Plugin Registry**: Manage and discover plugins
- ✅ **Category Support**: Organize plugins by category
- ✅ **Metadata Support**: Rich plugin metadata

## Installation

```bash
dotnet add package DotNetAgents.Ecosystem
```

## Plugin Architecture

### Creating a Plugin

```csharp
using DotNetAgents.Ecosystem;

public class MyPlugin : IPlugin
{
    public string Id => "my-plugin";
    public string Name => "My Plugin";
    public string Version => "1.0.0";
    public string Description => "A custom plugin for DotNetAgents";
    public string Author => "Your Name";
    public string License => "MIT";

    public async Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        // Initialize your plugin
        // Access services via context.Services
        // Access configuration via context.Configuration
        // Create loggers via context.LoggerFactory
    }

    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        // Cleanup resources
        return Task.CompletedTask;
    }
}
```

### Plugin with Metadata

```csharp
public class MyPluginWithMetadata : IPluginWithMetadata
{
    public string Id => "my-plugin";
    public string Name => "My Plugin";
    public string Version => "1.0.0";
    public string Description => "A custom plugin";
    public string Author => "Your Name";
    public string License => "MIT";

    public PluginMetadata Metadata => new PluginMetadata
    {
        Id = Id,
        Name = Name,
        Version = Version,
        Description = Description,
        Author = Author,
        License = License,
        Category = "tools",
        Tags = new List<string> { "custom", "example" },
        Dependencies = new List<string>(),
        RepositoryUrl = "https://github.com/username/my-plugin",
        DocumentationUrl = "https://github.com/username/my-plugin#readme"
    };

    public async Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        // Initialize plugin
    }

    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```

### Registering Plugins

```csharp
using DotNetAgents.Ecosystem;

services.AddDotNetAgentsEcosystem();

// Register plugin
var pluginRegistry = serviceProvider.GetRequiredService<IPluginRegistry>();
await pluginRegistry.RegisterAsync(new MyPlugin());
```

## Integration Marketplace

### Publishing Integrations

```csharp
var marketplace = serviceProvider.GetRequiredService<IIntegrationMarketplace>();

var integration = new IntegrationListing
{
    Name = "My Integration",
    Description = "A custom integration",
    Category = "tools",
    Type = IntegrationType.Plugin,
    PublisherId = "publisher-1",
    Tags = new List<string> { "custom", "example" },
    RepositoryUrl = "https://github.com/username/my-integration",
    DocumentationUrl = "https://github.com/username/my-integration#readme"
};

var integrationId = await marketplace.PublishAsync(integration);
```

### Searching Integrations

```csharp
// Simple search
var results = await marketplace.SearchAsync("custom tool");

// Advanced search with filters
var filters = new IntegrationFilters
{
    MinRating = 4.0,
    RequiredTags = new List<string> { "custom" },
    Type = IntegrationType.Plugin
};

var filteredResults = await marketplace.SearchAsync("tool", filters);
```

### Getting Integrations by Category

```csharp
var tools = await marketplace.GetByCategoryAsync("tools");
```

## Plugin Categories

Common plugin categories:

- **tools**: Custom tools and utilities
- **providers**: LLM/embedding providers
- **workflows**: Workflow templates
- **integrations**: Third-party integrations
- **extensions**: Framework extensions

## Best Practices

### Plugin Development

1. **Clear Purpose**: Define clear plugin purpose
2. **Good Documentation**: Document plugin usage
3. **Error Handling**: Handle errors gracefully
4. **Resource Management**: Clean up resources properly
5. **Testing**: Write tests for your plugin

### Integration Publishing

1. **Complete Metadata**: Provide complete metadata
2. **Clear Description**: Write clear descriptions
3. **Good Examples**: Provide usage examples
4. **Documentation**: Link to documentation
5. **Maintenance**: Keep integrations updated

## Use Cases

### Custom Tool Plugin

```csharp
public class CustomToolPlugin : IPluginWithMetadata
{
    // ... implement IPlugin

    public async Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        // Register custom tools
        var toolRegistry = context.Services.GetRequiredService<IToolRegistry>();
        // ... register tools
    }
}
```

### Provider Integration

```csharp
public class CustomProviderPlugin : IPlugin
{
    public async Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        // Register custom LLM provider
        var services = context.Services;
        // ... register provider
    }
}
```

### Workflow Template

```csharp
public class WorkflowTemplatePlugin : IPlugin
{
    public async Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        // Register workflow templates
        // ... register templates
    }
}
```

## Related Documentation

- [Plugin Development Guide](../community/PLUGIN_DEVELOPMENT.md)
- [Integration Marketplace](../community/INTEGRATION_MARKETPLACE.md)
- [Contributing Guide](../../CONTRIBUTING.md)

---

**Last Updated:** January 2026

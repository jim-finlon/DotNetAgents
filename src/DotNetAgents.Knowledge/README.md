# DotNetAgents.Knowledge

Knowledge repository for capturing and querying learning from agent execution.

## Overview

The `DotNetAgents.Knowledge` package provides a knowledge repository system that allows agents to capture and learn from successes, failures, and discoveries during execution. Knowledge items can be session-specific or global, and can be queried to help agents make better decisions.

## Features

- **Knowledge Capture**: Capture successes, failures, and patterns
- **Duplicate Detection**: Fast O(1) duplicate detection via content hash
- **Relevance Matching**: Find relevant knowledge based on tech stack and tags
- **Search & Query**: Full-text search and advanced querying
- **Reference Counting**: Track how often knowledge items are referenced
- **Error Integration**: Automatic knowledge capture from exceptions

## Quick Start

```csharp
using DotNetAgents.Knowledge;
using DotNetAgents.Knowledge.Models;

// Register services
services.AddDotNetAgentsKnowledge();

// Use knowledge repository
var knowledgeRepo = serviceProvider.GetRequiredService<IKnowledgeRepository>();

// Add knowledge
var knowledge = await knowledgeRepo.AddKnowledgeAsync(new KnowledgeItem
{
    SessionId = "session-123",
    Title = "Database connection pooling issue",
    Description = "Connection pool exhausted under load",
    Solution = "Increased max pool size to 200",
    Category = KnowledgeCategory.Solution,
    Severity = KnowledgeSeverity.Warning,
    Tags = new[] { "database", "performance" },
    TechStack = new[] { "dotnet", "sql-server" }
}, cancellationToken);

// Query relevant knowledge
var relevantKnowledge = await knowledgeRepo.GetRelevantKnowledgeAsync(
    techStackTags: new[] { "dotnet", "sql-server" },
    projectTags: new[] { "database" },
    maxResults: 5,
    cancellationToken);

// Search knowledge
var searchResults = await knowledgeRepo.SearchKnowledgeAsync(
    searchText: "connection pool",
    sessionId: "session-123",
    includeGlobal: true,
    cancellationToken);
```

## Error Handling Integration

Knowledge can be automatically captured from exceptions:

```csharp
try
{
    await tool.ExecuteAsync(input, cancellationToken);
}
catch (Exception ex)
{
    var knowledgeRepo = serviceProvider.GetRequiredService<IKnowledgeRepository>();
    await knowledgeRepo.AddKnowledgeAsync(new KnowledgeItem
    {
        SessionId = sessionId,
        Title = "Tool execution failure",
        Description = ex.Message,
        ErrorMessage = ex.Message,
        StackTrace = ex.StackTrace,
        Category = KnowledgeCategory.Error,
        Severity = KnowledgeSeverity.Error,
        ToolName = tool.Name
    }, cancellationToken);
    throw;
}
```

## Models

- **`KnowledgeItem`**: Represents a knowledge item with all properties
- **`KnowledgeCategory`**: Enum for knowledge categories (Error, Solution, BestPractice, etc.)
- **`KnowledgeSeverity`**: Enum for severity levels (Info, Warning, Error, Critical)
- **`KnowledgeQuery`**: Query parameters for filtering knowledge
- **`PagedResult<T>`**: Paginated result set

## Storage

The package provides storage abstractions (`IKnowledgeStore`) with implementations:
- `InMemoryKnowledgeStore`: For testing and development
- Database implementations: SQL Server and PostgreSQL (coming in Phase 6)

## Documentation

For detailed API documentation, see the XML documentation comments in the code.

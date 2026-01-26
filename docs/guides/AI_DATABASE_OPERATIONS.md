# AI-Powered Database Operations Guide

This guide covers AI-powered database operations in DotNetAgents, including query optimization, type mapping, and procedure conversion.

## Overview

DotNetAgents provides AI-powered tools for database operations:

- **Query Optimization**: PostgreSQL-specific optimizations with AI
- **Type Mapping**: Intelligent type recommendations based on data analysis
- **Procedure Conversion**: Convert stored procedures between database systems

## Prerequisites

AI-powered features require an LLM provider:

```csharp
services.AddOpenAI(apiKey, "gpt-4");
services.AddDatabaseAI();
```

## Query Optimization

### Basic Usage

```csharp
var optimizer = serviceProvider.GetRequiredService<AIQueryOptimizer>();

var result = await optimizer.OptimizeAsync(
    "SELECT * FROM users WHERE age > 25 ORDER BY name");

Console.WriteLine($"Optimized: {result.OptimizedQuery}");
Console.WriteLine($"Confidence: {result.ConfidenceScore}%");

foreach (var suggestion in result.Suggestions)
{
    Console.WriteLine($"{suggestion.Type}: {suggestion.Description} ({suggestion.Impact} impact)");
}
```

### With Schema Context

```csharp
var schemaContext = @"
Tables:
- users (id, name, age, email)
- orders (id, user_id, total)

Indexes:
- users.id (PRIMARY KEY)
- users.email (UNIQUE)
";

var result = await optimizer.OptimizeAsync(query, schemaContext);
```

## Type Mapping

### Basic Usage

```csharp
var typeMapper = serviceProvider.GetRequiredService<AITypeMapper>();

var column = new Column
{
    Name = "user_id",
    DataType = "int",
    IsNullable = false
};

var recommendation = await typeMapper.SuggestMappingAsync(column);

Console.WriteLine($"Recommended: {recommendation.RecommendedType}");
Console.WriteLine($"Rationale: {recommendation.Rationale}");
Console.WriteLine($"Confidence: {recommendation.ConfidenceScore}%");
```

### With Data Distribution

```csharp
var dataDistribution = @"
Total Rows: 1,000,000
Distinct Values: 500,000
Min Value: 1
Max Value: 1,000,000
";

var recommendation = await typeMapper.SuggestMappingAsync(column, dataDistribution);
```

## Procedure Conversion

### Basic Usage

```csharp
var converter = serviceProvider.GetRequiredService<AIProcedureConverter>();

var procedure = new StoredProcedure
{
    SchemaName = "dbo",
    ProcedureName = "GetUserOrders",
    Definition = @"
        CREATE PROCEDURE GetUserOrders
        @UserId INT
        AS
        BEGIN
            SELECT * FROM orders WHERE user_id = @UserId
        END
    "
};

var result = await converter.ConvertAsync(procedure, "PostgreSQL");

Console.WriteLine($"Converted: {result.ConvertedDefinition}");
Console.WriteLine($"Confidence: {result.ConfidenceScore}%");
Console.WriteLine($"Notes: {string.Join(", ", result.ConversionNotes)}");
```

## Using as Tools

All AI operations are available as tools for agents:

```csharp
var toolRegistry = serviceProvider.GetRequiredService<IToolRegistry>();

// Register AI tools
toolRegistry.Register(new AIQueryOptimizerTool(optimizer));
toolRegistry.Register(new AITypeMapperTool(typeMapper));
toolRegistry.Register(new AIProcedureConverterTool(converter));
```

## Best Practices

1. **Provide context** - Include schema information for better results
2. **Review AI suggestions** - Always review AI-generated code before execution
3. **Use confidence scores** - Lower confidence scores may need manual review
4. **Test conversions** - Always test converted procedures before production use

---

**See Also:**
- [Database Management Guide](./DATABASE_MANAGEMENT.md)
- [Integration Guide](./INTEGRATION_GUIDE.md)

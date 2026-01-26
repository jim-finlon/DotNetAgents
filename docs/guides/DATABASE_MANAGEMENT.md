# Database Management Guide

This guide covers the comprehensive database management capabilities in DotNetAgents, including schema analysis, AI-powered operations, validation, and orchestration.

## Overview

DotNetAgents provides a complete suite of database management tools for agents:

- **Schema Analysis**: Extract and analyze database structures
- **AI-Powered Operations**: Query optimization, type mapping, procedure conversion
- **Validation**: Pre-flight and post-operation validation
- **Orchestration**: Complex operation coordination with checkpointing
- **Security**: Secure connection management and secrets handling

## Development Database Access

### Anubis Development Server

For development and testing, you have access to both PostgreSQL and MSSQL servers on **Anubis** (`192.168.4.25`):

- **PostgreSQL**: Port 5432, Username: `ai`
- **SQL Server**: Port 1433, Username: `ai`
- **Full Access**: Create, modify, and delete databases, schemas, and data as needed
- **Credentials**: Stored in `.env` file (gitignored)

**Connection strings are available in the `.env` file:**
- `POSTGRES_CONNECTION_STRING` - PostgreSQL connection
- `SQL_SERVER_CONNECTION_STRING` - SQL Server connection

See [Development Database Configuration](../DEVELOPMENT_DATABASE.md) for more details.

## Installation

Install the database packages:

```bash
# Core database packages
dotnet add package DotNetAgents.Database.Abstractions
dotnet add package DotNetAgents.Database.Analysis
dotnet add package DotNetAgents.Database.AI
dotnet add package DotNetAgents.Database.Validation
dotnet add package DotNetAgents.Database.Orchestration
dotnet add package DotNetAgents.Database.Security

# Or install the metapackage (includes all)
dotnet add package DotNetAgents
```

## Schema Analysis

### Basic Usage

```csharp
using DotNetAgents.Database.Analysis;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddDatabaseSchemaAnalyzers();
services.AddSchemaAnalyzerFactory();

var serviceProvider = services.BuildServiceProvider();
var factory = serviceProvider.GetRequiredService<SchemaAnalyzerFactory>();

// Get analyzer for connection string
var analyzer = await factory.GetAnalyzerAsync(connectionString);
if (analyzer != null)
{
    var schema = await analyzer.AnalyzeAsync(connectionString);
    
    Console.WriteLine($"Database: {schema.Name}");
    Console.WriteLine($"Tables: {schema.TableCount}");
    Console.WriteLine($"Total Objects: {schema.TotalObjectCount}");
}
```

### Supported Databases

- **SQL Server**: Full schema analysis
- **PostgreSQL**: Full schema analysis

## AI-Powered Operations

### Query Optimization

```csharp
using DotNetAgents.Database.AI;
using DotNetAgents.Providers.OpenAI;

services.AddOpenAI(apiKey, "gpt-4");
services.AddDatabaseAI();

var optimizer = serviceProvider.GetRequiredService<AIQueryOptimizer>();
var result = await optimizer.OptimizeAsync(
    "SELECT * FROM users WHERE age > 25",
    schemaContext: "Schema information here");

Console.WriteLine($"Optimized: {result.OptimizedQuery}");
Console.WriteLine($"Confidence: {result.ConfidenceScore}%");
```

### Type Mapping

```csharp
var typeMapper = serviceProvider.GetRequiredService<AITypeMapper>();
var column = new Column { Name = "user_id", DataType = "int" };

var recommendation = await typeMapper.SuggestMappingAsync(column);
Console.WriteLine($"Recommended: {recommendation.RecommendedType}");
Console.WriteLine($"Rationale: {recommendation.Rationale}");
```

### Procedure Conversion

```csharp
var converter = serviceProvider.GetRequiredService<AIProcedureConverter>();
var procedure = new StoredProcedure
{
    SchemaName = "dbo",
    ProcedureName = "GetUsers",
    Definition = "CREATE PROCEDURE GetUsers..."
};

var result = await converter.ConvertAsync(procedure, "PostgreSQL");
Console.WriteLine($"Converted: {result.ConvertedDefinition}");
```

## Validation

### Pre-Flight Validation

```csharp
using DotNetAgents.Database.Validation;

services.AddDatabaseValidation();

var validator = serviceProvider.GetRequiredService<IDatabaseValidator>();
var result = await validator.ValidateAsync(connectionString, new ValidationOptions
{
    Type = ValidationType.Schema,
    CheckPermissions = true,
    CheckDiskSpace = false
});

if (result.IsValid)
{
    Console.WriteLine("✅ Validation passed");
}
else
{
    Console.WriteLine($"❌ Validation failed: {string.Join(", ", result.Errors)}");
}
```

## Orchestration

### Operation Execution

```csharp
using DotNetAgents.Database.Orchestration;

services.AddDatabaseOrchestration();

var orchestrator = serviceProvider.GetRequiredService<IDatabaseOperationOrchestrator>();

var operation = new DatabaseOperation
{
    Type = "migration",
    SourceConnectionString = sourceConn,
    TargetConnectionString = targetConn,
    Parameters = new Dictionary<string, object> { ["batch_size"] = 1000 }
};

var result = await orchestrator.ExecuteAsync(operation, "migration-1");
```

## Security

### Secure Connection Management

```csharp
using DotNetAgents.Database.Security;
using DotNetAgents.Security.Secrets;

services.AddSingleton<ISecretsProvider, EnvironmentSecretsProvider>();
services.AddDatabaseSecurity();

var connectionManager = serviceProvider.GetRequiredService<ISecureConnectionManager>();
var connectionString = await connectionManager.GetSecureConnectionStringAsync("DatabaseConnection");
```

## Tools for Agents

All database operations are available as tools for agents:

- `database_schema_analyze` - Analyze database schemas
- `database_validate` - Validate database operations
- `database_index` - Manage database indexes
- `ai_query_optimizer` - Optimize queries using AI
- `ai_type_mapper` - Intelligent type mapping
- `ai_procedure_converter` - Convert stored procedures

## Examples

See the [Database Management Sample](../../samples/DotNetAgents.Samples.DatabaseManagement/) for complete examples.

## Best Practices

1. **Always validate connections** before operations
2. **Use secure connection management** for production
3. **Enable checkpointing** for long-running operations
4. **Monitor metrics** for performance tracking
5. **Use AI features** for complex migrations and optimizations

---

**See Also:**
- [AI Database Operations Guide](./AI_DATABASE_OPERATIONS.md)
- [Integration Guide](./INTEGRATION_GUIDE.md)
- [Development Database Configuration](../DEVELOPMENT_DATABASE.md)

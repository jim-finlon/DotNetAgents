# Development Database Configuration

## PostgreSQL Development Server

This document contains connection information for the development PostgreSQL server.

### Server Details

- **Host**: `anubis`
- **Port**: `5432` (standard PostgreSQL port)
- **Username**: `ai`
- **Password**: `YOUR_PASSWORD` (contact administrator for actual password)
- **Environment**: Development playground - full access granted

### Connection String Format

#### Standard Connection String
```
Host=anubis;Port=5432;Username=ai;Password=YOUR_PASSWORD;Database={database_name}
```

#### Npgsql Connection String (for .NET)
```
Host=anubis;Port=5432;Username=ai;Password=YOUR_PASSWORD;Database={database_name};Pooling=true;MinPoolSize=0;MaxPoolSize=100
```

### Example Usage

#### Task and Knowledge Stores
```csharp
var connectionString = "Host=anubis;Port=5432;Username=ai;Password=YOUR_PASSWORD;Database=dotnetagents_dev";

services.AddPostgreSQLTaskStore(connectionString);
services.AddPostgreSQLKnowledgeStore(connectionString);
services.AddPostgreSQLCheckpointStore<MyState>(connectionString);
```

#### Vector Store
```csharp
var connectionString = "Host=anubis;Port=5432;Username=ai;Password=YOUR_PASSWORD;Database=dotnetagents_vectors";

services.AddPostgreSQLVectorStore(
    connectionString,
    tableName: "embeddings",
    vectorDimensions: 1536,
    distanceFunction: VectorDistanceFunction.Cosine);
```

### Database Recommendations

For development, you can create separate databases for different purposes:

- `dotnetagents_dev` - General development database for tasks and knowledge
- `dotnetagents_vectors` - Vector store database
- `dotnetagents_workflow` - Workflow checkpoint storage
- `dotnetagents_test` - Testing database

### Creating Databases

You can create databases using psql or through your application:

```sql
CREATE DATABASE dotnetagents_dev;
CREATE DATABASE dotnetagents_vectors;
CREATE DATABASE dotnetagents_workflow;
CREATE DATABASE dotnetagents_test;
```

### Security Note

⚠️ **IMPORTANT**: This is a development server with full access. Never commit connection strings with passwords to version control. Use environment variables or secure configuration management in production.

### Environment Variable Setup

For local development, you can set environment variables:

**PowerShell:**
```powershell
$env:POSTGRES_HOST="anubis"
$env:POSTGRES_PORT="5432"
$env:POSTGRES_USER="ai"
$env:POSTGRES_PASSWORD="YOUR_PASSWORD"
$env:POSTGRES_DATABASE="dotnetagents_dev"
```

**Connection String from Environment:**
```csharp
var connectionString = $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST")};" +
    $"Port={Environment.GetEnvironmentVariable("POSTGRES_PORT")};" +
    $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};" +
    $"Database={Environment.GetEnvironmentVariable("POSTGRES_DATABASE")}";
```

### Testing Connection

To test the connection programmatically:

```csharp
using Npgsql;

var connectionString = "Host=anubis;Port=5432;Username=ai;Password=YOUR_PASSWORD;Database=postgres";
await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
var version = await connection.PostgreSqlVersion;
Console.WriteLine($"PostgreSQL Version: {version}");
```

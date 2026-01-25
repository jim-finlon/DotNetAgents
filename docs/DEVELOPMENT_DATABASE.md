# Development Database Configuration

## PostgreSQL Development Server

This document contains connection information for the development PostgreSQL server.

### Server Details

- **Host**: `192.168.4.25` (or hostname `anubis` if DNS is configured)
- **Port**: `5432` (standard PostgreSQL port)
- **Username**: `ai`
- **Password**: See `.env` file in project root (gitignored for security)
- **Environment**: Development playground - full access granted
- **Server Name**: Anubis

### Connection String Format

#### Standard Connection String
```
Host=192.168.4.25;Port=5432;Username=ai;Password={password};Database={database_name}
```

#### Npgsql Connection String (for .NET)
```
Host=192.168.4.25;Port=5432;Username=ai;Password={password};Database={database_name};Pooling=true;MinPoolSize=0;MaxPoolSize=100
```

**Note**: Actual password is stored in `.env` file (gitignored). Use environment variables or the `.env` file for local development.

### Example Usage

#### Task and Knowledge Stores
```csharp
// Get connection string from configuration or environment
var connectionString = configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=192.168.4.25;Port=5432;Username=ai;Password={password};Database=dotnetagents_dev";

services.AddPostgreSQLTaskStore(connectionString);
services.AddPostgreSQLKnowledgeStore(connectionString);
services.AddPostgreSQLCheckpointStore<MyState>(connectionString);
```

#### Vector Store
```csharp
// Get connection string from configuration or environment
var connectionString = configuration.GetConnectionString("PostgreSQLVectors")
    ?? "Host=192.168.4.25;Port=5432;Username=ai;Password={password};Database=dotnetagents_vectors";

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
- `teaching_assistant` - TeachingAssistant application database (PostgreSQL + MSSQL)

**Note**: The TeachingAssistant project uses the `teaching_assistant` database. Connection strings are configured in the `.env` file:
- PostgreSQL: `CONNECTIONSTRINGS__DEFAULTCONNECTION`
- MSSQL: `CONNECTIONSTRINGS__MSSQLCONNECTION`

### Creating Databases

You can create databases using psql or through your application:

```sql
CREATE DATABASE dotnetagents_dev;
CREATE DATABASE dotnetagents_vectors;
CREATE DATABASE dotnetagents_workflow;
CREATE DATABASE dotnetagents_test;
CREATE DATABASE teaching_assistant;
```

### Security Note

⚠️ **IMPORTANT**: 
- This is a development server with full access
- Never commit connection strings with passwords to version control
- The `.env` file is gitignored and contains actual credentials
- Use environment variables or secure configuration management in production
- See `infrastructure/DEV_DATABASE.md` for more details on configuration precedence

### Related Documentation

- `infrastructure/DEV_DATABASE.md` - Detailed database setup instructions
- `.env` file (project root) - Contains actual connection strings (gitignored)

### Environment Variable Setup

For local development, you can set environment variables:

**PowerShell:**
```powershell
$env:POSTGRES_HOST="192.168.4.25"
$env:POSTGRES_PORT="5432"
$env:POSTGRES_USER="ai"
$env:POSTGRES_PASSWORD="{password_from_env_file}"
$env:POSTGRES_DATABASE="dotnetagents_dev"
```

**Note**: The `.env` file in the project root contains the actual connection strings. ASP.NET Core doesn't automatically load `.env` files, so you can:
- Use `DotNetEnv` package to load `.env` files programmatically
- Set environment variables manually
- Use `appsettings.Development.json` (gitignored) for local development

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

// Get connection string from environment or configuration
var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
    ?? "Host=192.168.4.25;Port=5432;Username=ai;Password={password};Database=postgres";

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
var version = await connection.PostgreSqlVersion;
Console.WriteLine($"PostgreSQL Version: {version}");
```

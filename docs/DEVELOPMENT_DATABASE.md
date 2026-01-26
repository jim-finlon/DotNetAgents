# Development Database Configuration

## Database Servers on Anubis

Both PostgreSQL and MSSQL servers are hosted on **Anubis** at `192.168.4.25` for AI development work.

### PostgreSQL Server

#### Connection Details

- **Host**: `192.168.4.25` (or hostname `anubis` if DNS is configured)
- **Port**: `5432` (standard PostgreSQL port)
- **Username**: `ai`
- **Password**: See `.env` file in project root (gitignored for security)
- **Environment**: Development playground - **full access granted** for development, testing, proof of concept, and end-to-end testing
- **Server Name**: Anubis

### MSSQL Server (Same Server)

#### Connection Details

- **Host**: `192.168.4.25` (same server as PostgreSQL)
- **Port**: `1433` (standard SQL Server port)
- **Username**: `ai`
- **Password**: Same as PostgreSQL (see `.env` file)
- **Environment**: Development playground - **full access granted** for development, testing, proof of concept, and end-to-end testing
- **Server Name**: Anubis

**Note**: Both PostgreSQL and MSSQL servers are available on the same host (Anubis) with the same credentials. You have **full access** to create, modify, and delete databases, schemas, and data as needed for:
- Application development
- Proof of concept work
- End-to-end testing
- Schema migrations and experiments
- Multi-database testing scenarios
- Database management feature development and testing
- AI-powered database operations testing

### Connection String Format

#### PostgreSQL Connection String
```
Host=192.168.4.25;Port=5432;Username=ai;Password={password};Database={database_name};Pooling=true;MinPoolSize=0;MaxPoolSize=100
```

#### SQL Server Connection String
```
Server=192.168.4.25,1433;Database={database_name};User Id=ai;Password={password};TrustServerCertificate=true;Encrypt=true
```

**Note**: Actual passwords are stored in `.env` file (gitignored). Use environment variables or the `.env` file for local development.

### Environment Variables

The `.env` file in the project root contains connection strings. To use it:

1. **Copy `.env.example` to `.env`**:
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env`** and fill in your credentials

3. **Load the .env file** in your application (ASP.NET Core doesn't auto-load .env):
   - Use `DotNetEnv` package to load `.env` files programmatically
   - Or set environment variables manually
   - Or use `appsettings.Development.json` (gitignored) for local development

**PowerShell:**
```powershell
$env:POSTGRES_CONNECTION_STRING="Host=192.168.4.25;Port=5432;Username=ai;Password=YOUR_PASSWORD;Database=dotnetagents_dev"
$env:SQL_SERVER_CONNECTION_STRING="Server=192.168.4.25,1433;Database=dotnetagents_dev;User Id=ai;Password=YOUR_PASSWORD;TrustServerCertificate=true"
```

**Bash:**
```bash
export POSTGRES_CONNECTION_STRING="Host=192.168.4.25;Port=5432;Username=ai;Password=YOUR_PASSWORD;Database=dotnetagents_dev"
export SQL_SERVER_CONNECTION_STRING="Server=192.168.4.25,1433;Database=dotnetagents_dev;User Id=ai;Password=YOUR_PASSWORD;TrustServerCertificate=true"
```

### Database Recommendations

For development, you can create separate databases for different purposes:

- `dotnetagents_dev` - General development database for tasks and knowledge
- `dotnetagents_vectors` - Vector store database
- `dotnetagents_workflow` - Workflow checkpoint storage
- `dotnetagents_test` - Testing database
- `teaching_assistant` - TeachingAssistant application database (PostgreSQL + MSSQL)

### Creating Databases

You can create databases using psql, sqlcmd, or through your application:

**PostgreSQL:**
```sql
CREATE DATABASE dotnetagents_dev;
CREATE DATABASE dotnetagents_vectors;
CREATE DATABASE dotnetagents_workflow;
CREATE DATABASE dotnetagents_test;
```

**SQL Server:**
```sql
CREATE DATABASE dotnetagents_dev;
CREATE DATABASE dotnetagents_vectors;
CREATE DATABASE dotnetagents_workflow;
CREATE DATABASE dotnetagents_test;
GO
```

### Required PostgreSQL Extensions

For vector stores and advanced features, ensure the following PostgreSQL extensions are installed:

```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "vector";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
```

### Example Usage

#### Using Connection Strings from Environment

```csharp
// Get connection string from environment
var postgresConn = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
var sqlServerConn = Environment.GetEnvironmentVariable("SQL_SERVER_CONNECTION_STRING");

// Use with database services
services.AddPostgreSQLTaskStore(postgresConn);
services.AddPostgreSQLKnowledgeStore(postgresConn);
```

#### Using with Database Management Tools

```csharp
using DotNetAgents.Database.Analysis;

var factory = serviceProvider.GetRequiredService<SchemaAnalyzerFactory>();
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");

var analyzer = await factory.GetAnalyzerAsync(connectionString);
if (analyzer != null)
{
    var schema = await analyzer.AnalyzeAsync(connectionString);
    Console.WriteLine($"Database: {schema.Name}, Tables: {schema.TableCount}");
}
```

### Testing Connection

**PostgreSQL:**
```csharp
using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
var version = await connection.PostgreSqlVersion;
Console.WriteLine($"PostgreSQL Version: {version}");
```

**SQL Server:**
```csharp
using Microsoft.Data.SqlClient;

var connectionString = Environment.GetEnvironmentVariable("SQL_SERVER_CONNECTION_STRING");
await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();
var version = connection.ServerVersion;
Console.WriteLine($"SQL Server Version: {version}");
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
- `.env.example` - Template for connection strings (safe to commit)
- `.env` - Actual connection strings (gitignored, do not commit)
- `docs/guides/DATABASE_MANAGEMENT.md` - Database management capabilities guide

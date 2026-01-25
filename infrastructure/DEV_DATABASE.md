# Development Database Setup

## Database Servers

Both PostgreSQL and MSSQL servers are hosted on **Anubis** at `192.168.4.25`.

### PostgreSQL Server

#### Connection Details

- **Host**: 192.168.4.25
- **Database**: `teaching_assistant` (or create as needed)
- **Username**: `ai`
- **Port**: 5432 (default PostgreSQL port)

### MSSQL Server

#### Connection Details

- **Host**: 192.168.4.25
- **Database**: `teaching_assistant` (or create as needed)
- **Username**: `ai`
- **Password**: Same as PostgreSQL credentials
- **Port**: 1433 (default MSSQL port)

### Access

You have **full access** to both PostgreSQL and MSSQL servers to create, modify, and delete databases, schemas, and data for:
- Application development
- Proof of concept work
- End-to-end testing
- Schema migrations and experiments
- Multi-database testing scenarios

### Configuration

Connection strings are configured via (in order of precedence):
1. **Environment Variables** (highest priority)
   - Set `ConnectionStrings__DefaultConnection` environment variable
   - Or use `.env` file and load it manually (ASP.NET Core doesn't auto-load .env)
2. **appsettings.Development.json** (create from example template)
3. **appsettings.json** (default/fallback - should be empty for security)

**Note**: The `.env` file in the root directory contains the connection string but ASP.NET Core doesn't automatically load it. You can:
- Set environment variables manually
- Use `appsettings.Development.json` (gitignored)
- Or use a package like `DotNetEnv` to load `.env` files programmatically

### Required Extensions

Ensure the following PostgreSQL extensions are installed:
- `uuid-ossp` - UUID generation
- `pgcrypto` - Cryptographic functions
- `vector` - pgvector for embeddings
- `pg_trgm` - Trigram matching for text search

### PostgreSQL Setup Script

To initialize the PostgreSQL database with required extensions:

```sql
CREATE DATABASE teaching_assistant;

\c teaching_assistant

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "vector";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
```

### MSSQL Setup Script

To initialize the MSSQL database:

```sql
CREATE DATABASE teaching_assistant;
GO

USE teaching_assistant;
GO

-- MSSQL-specific setup can be added here as needed
```

### Notes

- Credentials are stored in `.env` file (gitignored)
- Do not commit credentials to version control
- Use this server for development/testing only
- Production deployments should use their own database infrastructure

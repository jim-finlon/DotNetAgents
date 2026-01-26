# Development Database Setup Summary

**Date:** January 25, 2026  
**Server:** Anubis (192.168.4.25)

## Quick Start

1. **Copy the environment template:**
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env`** and fill in your credentials (already configured for Anubis server)

3. **Load environment variables** in your application:
   ```csharp
   // Option 1: Use DotNetEnv package
   DotNetEnv.Env.Load();
   
   // Option 2: Set environment variables manually
   // Option 3: Use appsettings.Development.json
   ```

4. **Use connection strings:**
   ```csharp
   var postgresConn = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
   var sqlServerConn = Environment.GetEnvironmentVariable("SQL_SERVER_CONNECTION_STRING");
   ```

## Available Servers

### PostgreSQL
- **Host:** 192.168.4.25
- **Port:** 5432
- **Username:** ai
- **Full Access:** Yes - create, modify, delete databases as needed

### SQL Server
- **Host:** 192.168.4.25 (same server)
- **Port:** 1433
- **Username:** ai
- **Full Access:** Yes - create, modify, delete databases as needed

## Environment Variables

The `.env` file contains:
- `POSTGRES_CONNECTION_STRING` - PostgreSQL connection
- `SQL_SERVER_CONNECTION_STRING` - SQL Server connection
- `CONNECTIONSTRINGS__DEFAULTCONNECTION` - For configuration binding
- `CONNECTIONSTRINGS__MSSQLCONNECTION` - For configuration binding
- `CONNECTIONSTRINGS__POSTGRESQLVECTORS` - For vector stores

## Security

- ✅ `.env` file is gitignored
- ✅ `.env.example` is safe to commit (no credentials)
- ✅ Never commit actual credentials
- ✅ Use environment variables in production

## Related Documentation

- [Development Database Configuration](./DEVELOPMENT_DATABASE.md) - Complete guide
- [Infrastructure Database Setup](../infrastructure/DEV_DATABASE.md) - Detailed setup
- [Database Management Guide](./guides/DATABASE_MANAGEMENT.md) - Using database features

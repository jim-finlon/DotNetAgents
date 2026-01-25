# Database Setup Summary

## Current Status

I attempted to create the database and run migrations, but encountered two issues:

1. **psql not available**: The PostgreSQL command-line client (`psql`) is not in the system PATH
2. **Circular dependency**: There's a circular dependency in `DotNetAgents.Agents.StateMachines` and `DotNetAgents.Agents.BehaviorTrees` projects preventing EF Core migrations from running

## What Was Created

1. **Database setup scripts** in `Scripts/` directory
2. **Documentation** for manual setup
3. **Connection strings** configured in `.env` and `appsettings.Development.json.example`

## Next Steps

### Option 1: Fix Circular Dependency and Use EF Core (Recommended)

1. **Fix the circular dependency** in:
   - `src/DotNetAgents.Agents.StateMachines/DotNetAgents.Agents.StateMachines.csproj`
   - `src/DotNetAgents.Agents.BehaviorTrees/DotNetAgents.Agents.BehaviorTrees.csproj`

2. **Create the database** using a PostgreSQL client:
   - Connect to `192.168.4.25` as user `ai`
   - Run: `CREATE DATABASE teaching_assistant;`

3. **Run EF Core migrations**:
   ```bash
   dotnet ef database update --project TeachingAssistant.Data --startup-project TeachingAssistant.API
   ```

### Option 2: Use PostgreSQL Client Tool

1. **Connect** to PostgreSQL on Anubis (`192.168.4.25`) using:
   - pgAdmin
   - DBeaver
   - Azure Data Studio
   - Or any PostgreSQL client

2. **Create database**:
   ```sql
   CREATE DATABASE teaching_assistant;
   ```

3. **Create extensions**:
   ```sql
   \c teaching_assistant
   CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
   CREATE EXTENSION IF NOT EXISTS "pgcrypto";
   CREATE EXTENSION IF NOT EXISTS "vector";
   CREATE EXTENSION IF NOT EXISTS "pg_trgm";
   ```

4. **Once circular dependency is fixed**, generate migration SQL:
   ```bash
   dotnet ef migrations script --startup-project TeachingAssistant.API --idempotent --output Migrations/FullMigration.sql
   ```

5. **Run the generated SQL** against the database

### Option 3: Use the Standalone C# Script

1. **Compile** `Scripts/CreateDatabase.cs` (requires Npgsql NuGet package)
2. **Run** it to create the database and extensions
3. **Then** proceed with EF Core migrations once circular dependency is fixed

## MSSQL Setup (Optional)

If you want to set up MSSQL as well:

1. **Connect** to MSSQL on Anubis (`192.168.4.25:1433`) as user `ai`
2. **Create database**:
   ```sql
   CREATE DATABASE teaching_assistant;
   GO
   ```
3. **Configure** EF Core for MSSQL (if needed) or use raw SQL

## Connection Strings

All connection strings are configured in:
- `.env` (root directory, gitignored)
- `appsettings.Development.json.example` (template)

**PostgreSQL**: `Host=192.168.4.25;Database=teaching_assistant;Username=ai;Password=ai8989@`
**MSSQL**: `Server=192.168.4.25,1433;Database=teaching_assistant;User Id=ai;Password=ai8989@;TrustServerCertificate=True`

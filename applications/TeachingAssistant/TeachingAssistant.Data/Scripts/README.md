# Database Setup Scripts

## Setup Instructions

Due to circular dependency issues in the DotNetAgents projects, you have two options:

### Option 1: Use EF Core Migrations (Recommended)

1. **Fix the circular dependency** in `DotNetAgents.Agents.StateMachines` and `DotNetAgents.Agents.BehaviorTrees` projects
2. **Create the database** (if it doesn't exist):
   ```bash
   # Using psql (if available):
   psql -h 192.168.4.25 -U ai -d postgres -c "CREATE DATABASE teaching_assistant;"
   
   # Or use a PostgreSQL client tool (pgAdmin, DBeaver, etc.)
   ```
3. **Run migrations**:
   ```bash
   dotnet ef database update --project TeachingAssistant.Data --startup-project TeachingAssistant.API
   ```

### Option 2: Manual SQL Setup

1. **Connect to PostgreSQL** on Anubis (192.168.4.25) as user `ai`
2. **Create the database**:
   ```sql
   CREATE DATABASE teaching_assistant;
   ```
3. **Run the initial migration SQL** from `Migrations/InitialMigration.sql`
4. **Generate full migration SQL** (once circular dependency is fixed):
   ```bash
   dotnet ef migrations script --startup-project TeachingAssistant.API --idempotent --output Migrations/FullMigration.sql
   ```
5. **Run the generated SQL** against the database

### Connection Details

- **Host**: 192.168.4.25 (Anubis)
- **Username**: ai
- **Password**: (stored in .env file)
- **Database**: teaching_assistant

# Teaching Assistant API

## Development Databases

Both PostgreSQL and MSSQL servers are hosted on **Anubis** at `192.168.4.25`.

### PostgreSQL
- **Host**: 192.168.4.25
- **Port**: 5432
- **Username**: `ai`
- **Database**: `teaching_assistant`

### MSSQL
- **Host**: 192.168.4.25
- **Port**: 1433
- **Username**: `ai`
- **Database**: `teaching_assistant`
- **Same credentials as PostgreSQL**

### Configuration

Connection strings can be configured via:

1. **Environment Variables** (recommended for local development):
   ```bash
   export ConnectionStrings__DefaultConnection="Host=192.168.4.25;Database=teaching_assistant;Username=ai;Password=ai8989@"
   ```

2. **appsettings.Development.json** (create from `appsettings.Development.json.example`):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=192.168.4.25;Database=teaching_assistant;Username=ai;Password=ai8989@"
     }
   }
   ```

3. **.env file** (root directory):
   ```
   CONNECTIONSTRINGS__DEFAULTCONNECTION=Host=192.168.4.25;Database=teaching_assistant;Username=ai;Password=ai8989@
   ```

### Database Setup

Ensure the database exists and required extensions are installed:

```sql
CREATE DATABASE teaching_assistant;

\c teaching_assistant

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "vector";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
```

### Running Migrations

```bash
dotnet ef database update --project TeachingAssistant.Data --startup-project TeachingAssistant.API
```

### Notes

- Credentials are stored in `.env` and `appsettings.Development.json` (both gitignored)
- Do not commit credentials to version control
- Use the Anubis server for development/testing only

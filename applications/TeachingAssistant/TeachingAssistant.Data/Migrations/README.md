# Database Migrations

This directory contains Entity Framework Core migrations for the TeachingAssistant database.

## Creating Migrations

To create a new migration:

```bash
dotnet ef migrations add <MigrationName> --project applications/TeachingAssistant/TeachingAssistant.Data --startup-project <StartupProject>
```

## Applying Migrations

To apply migrations to the database:

```bash
dotnet ef database update --project applications/TeachingAssistant/TeachingAssistant.Data --startup-project <StartupProject>
```

## Initial Migration Notes

The initial migration should:
1. Create PostgreSQL extensions: `uuid-ossp`, `pgcrypto`, `vector`, `pg_trgm`
2. Create enum types for all enums (SubscriptionTier, GuardianRole, Subject, etc.)
3. Create all tables with proper indexes
4. Create check constraints
5. Set up full-text search indexes (via raw SQL)

## Manual SQL for Full-Text Search

After running migrations, execute this SQL manually to create the full-text search index:

```sql
CREATE INDEX idx_content_search ON content_units 
USING GIN (to_tsvector('english', title || ' ' || COALESCE(summary, '')));
```

## Manual SQL for Topic Path Index

```sql
CREATE INDEX idx_content_topic ON content_units USING GIN (topic_path);
```

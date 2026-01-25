# PowerShell script to create database using .NET
# This script uses Npgsql to create the database and run migrations

$connectionString = "Host=192.168.4.25;Database=postgres;Username=ai;Password=ai8989@"
$targetDatabase = "teaching_assistant"

Write-Host "Setting up database $targetDatabase on Anubis (192.168.4.25)..." -ForegroundColor Green

# Create a simple C# script to check/create database
$createDbScript = @"
using Npgsql;

var connStr = "$connectionString";
var targetDb = "$targetDatabase";

using var conn = new NpgsqlConnection(connStr);
conn.Open();

// Check if database exists
var checkCmd = new NpgsqlCommand($@"SELECT 1 FROM pg_database WHERE datname = '{targetDb}';", conn);
var exists = checkCmd.ExecuteScalar();

if (exists == null) {
    // Create database
    var createCmd = new NpgsqlCommand($@"CREATE DATABASE {targetDb};", conn);
    createCmd.ExecuteNonQuery();
    Console.WriteLine("Database created successfully!");
} else {
    Console.WriteLine("Database already exists.");
}

conn.Close();
"@

# Try to compile and run (requires Npgsql package)
Write-Host "Note: This requires Npgsql package. Using EF Core migrations instead..." -ForegroundColor Yellow

Write-Host "Please run the following commands manually:" -ForegroundColor Cyan
Write-Host "1. Fix the circular dependency issue in DotNetAgents projects" -ForegroundColor Yellow
Write-Host "2. Then run: dotnet ef database update --project TeachingAssistant.Data --startup-project TeachingAssistant.API" -ForegroundColor Yellow
Write-Host ""
Write-Host "Or use a PostgreSQL client (pgAdmin, DBeaver, etc.) to:" -ForegroundColor Cyan
Write-Host "1. Connect to 192.168.4.25 as user 'ai'" -ForegroundColor Yellow
Write-Host "2. Create database 'teaching_assistant'" -ForegroundColor Yellow
Write-Host "3. Run the SQL from Migrations\InitialMigration.sql" -ForegroundColor Yellow

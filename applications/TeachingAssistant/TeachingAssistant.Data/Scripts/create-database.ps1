# PowerShell script to create database and run initial migration
# Usage: .\create-database.ps1

$pgHost = "192.168.4.25"
$pgUser = "ai"
$pgPassword = "ai8989@"
$pgDatabase = "teaching_assistant"

Write-Host "Creating database $pgDatabase on $pgHost..." -ForegroundColor Green

# Set PGPASSWORD environment variable for psql
$env:PGPASSWORD = $pgPassword

# Create database if it doesn't exist
$createDbQuery = "SELECT 1 FROM pg_database WHERE datname = '$pgDatabase';"
$dbExists = psql -h $pgHost -U $pgUser -d postgres -tAc $createDbQuery 2>&1

if ($dbExists -match "1") {
    Write-Host "Database $pgDatabase already exists." -ForegroundColor Yellow
} else {
    Write-Host "Creating database $pgDatabase..." -ForegroundColor Green
    psql -h $pgHost -U $pgUser -d postgres -c "CREATE DATABASE $pgDatabase;" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database created successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to create database." -ForegroundColor Red
        exit 1
    }
}

# Run initial migration SQL
Write-Host "Running initial migration..." -ForegroundColor Green
$migrationScript = Join-Path $PSScriptRoot "..\Migrations\InitialMigration.sql"
if (Test-Path $migrationScript) {
    psql -h $pgHost -U $pgUser -d $pgDatabase -f $migrationScript 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Migration completed with warnings/errors. Check output above." -ForegroundColor Yellow
    }
} else {
    Write-Host "Migration script not found at $migrationScript" -ForegroundColor Red
    exit 1
}

Write-Host "Database setup complete!" -ForegroundColor Green

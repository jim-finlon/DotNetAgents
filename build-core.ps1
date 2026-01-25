# Build script for Core libraries
# Usage: .\build-core.ps1 [-Configuration Release|Debug] [-NoRestore]

param(
    [string]$Configuration = "Release",
    [switch]$NoRestore
)

Write-Host "Building DotNetAgents Core Libraries..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

if (-not $NoRestore) {
    Write-Host "Restoring dependencies..." -ForegroundColor Green
    dotnet restore DotNetAgents.Core.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Restore failed!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Building solution..." -ForegroundColor Green
dotnet build DotNetAgents.Core.sln --no-restore --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build completed successfully!" -ForegroundColor Green

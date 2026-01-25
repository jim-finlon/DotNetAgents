# Build script for all solutions
# Usage: .\build-all.ps1 [-Configuration Release|Debug] [-NoRestore] [-SkipTests]

param(
    [string]$Configuration = "Release",
    [switch]$NoRestore,
    [switch]$SkipTests
)

Write-Host "Building DotNetAgents - All Solutions..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

if (-not $NoRestore) {
    Write-Host "Restoring dependencies..." -ForegroundColor Green
    dotnet restore DotNetAgents.All.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Restore failed!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Building solution..." -ForegroundColor Green
dotnet build DotNetAgents.All.sln --no-restore --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

if (-not $SkipTests) {
    Write-Host "Running tests..." -ForegroundColor Green
    dotnet test DotNetAgents.All.sln --no-build --configuration $Configuration --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests failed!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Build and tests completed successfully!" -ForegroundColor Green

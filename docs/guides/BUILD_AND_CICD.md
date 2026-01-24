# Build & CI/CD Guide

**Version:** 1.0.0  
**Date:** December 7, 2025  
**Status:** Draft

---

## 1. Solution Structure

### 1.1 Project Organization

```
DotLangChain/
├── src/
│   ├── DotLangChain.Abstractions/
│   ├── DotLangChain.Core/
│   ├── DotLangChain.Providers.*/
│   ├── DotLangChain.VectorStores.*/
│   ├── DotLangChain.StateStores.*/
│   └── DotLangChain.Extensions/
├── tests/
│   ├── DotLangChain.Tests.Unit/
│   ├── DotLangChain.Tests.Integration/
│   └── DotLangChain.Tests.Benchmarks/
├── samples/
│   ├── DotLangChain.Samples.RAG/
│   ├── DotLangChain.Samples.Agent/
│   └── DotLangChain.Samples.MultiAgent/
├── Directory.Build.props
├── Directory.Build.targets
└── DotLangChain.sln
```

### 1.2 Project File Conventions

All `.csproj` files SHALL follow these conventions:

- Use `<LangVersion>13.0</LangVersion>` (C# 13, .NET 9)
- Use `<Nullable>enable</Nullable>`
- Use `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` for release builds
- Include XML documentation generation (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`)
- Use `<WarningsAsErrors />` and `<WarningsNotAsErrors />` appropriately
- Set `<NoWarn>` for unavoidable warnings (documented in each project)

---

## 2. Directory.Build.props

A `Directory.Build.props` file SHALL be created at the solution root with:

```xml
<Project>
  <PropertyGroup>
    <!-- .NET 9.0 targeting -->
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    
    <!-- Build settings -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn> <!-- Missing XML doc (to be removed as docs are added) -->
    
    <!-- Package metadata (override in individual projects) -->
    <Authors>DotLangChain Contributors</Authors>
    <Company>DotLangChain</Company>
    <Product>DotLangChain</Product>
    <Copyright>Copyright © 2025</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/dotlangchain/dotlangchain</PackageProjectUrl>
    <RepositoryUrl>https://github.com/dotlangchain/dotlangchain</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    
    <!-- Versioning -->
    <VersionPrefix>1.0.0</VersionPrefix>
    <VersionSuffix Condition="'$(Configuration)' == 'Debug'">dev</VersionSuffix>
    
    <!-- Build output -->
    <GeneratePackageOnBuild Condition="'$(Configuration)' == 'Release'">true</GeneratePackageOnBuild>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    
    <!-- Deterministic builds -->
    <Deterministic>true</Deterministic>
    <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
  </PropertyGroup>

  <PropertyGroup Condition="'$(CI)' == 'true'">
    <BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)artifacts/obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
    <BaseOutputPath>$(MSBuildThisFileDirectory)artifacts/bin/$(MSBuildProjectName)/</BaseOutputPath>
  </PropertyGroup>

  <ItemGroup>
    <!-- Common package references (override in individual projects as needed) -->
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All"/>
  </ItemGroup>
</Project>
```

---

## 3. Package Versioning

### 3.1 Semantic Versioning

All packages SHALL follow [Semantic Versioning 2.0.0](https://semver.org/):

- **MAJOR**: Breaking API changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### 3.2 Pre-release Versions

- `-alpha.N` - Early development, API may change
- `-beta.N` - Feature complete, may have bugs
- `-rc.N` - Release candidate, stable API
- No suffix - Production release

### 3.3 Package Naming Convention

- **Core**: `DotLangChain.{Component}` (e.g., `DotLangChain.Core`)
- **Providers**: `DotLangChain.Providers.{ProviderName}` (e.g., `DotLangChain.Providers.OpenAI`)
- **Vector Stores**: `DotLangChain.VectorStores.{StoreName}` (e.g., `DotLangChain.VectorStores.Qdrant`)
- **State Stores**: `DotLangChain.StateStores.{StoreName}` (e.g., `DotLangChain.StateStores.Redis`)
- **Extensions**: `DotLangChain.Extensions.{ExtensionName}` (e.g., `DotLangChain.Extensions.DependencyInjection`)

### 3.4 Version Synchronization

All packages in a release SHALL use the same version number (maintained via `Directory.Build.props`).

---

## 4. CI/CD Pipeline

### 4.1 GitHub Actions Workflow

The repository SHALL include a `.github/workflows/ci.yml` with:

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  DOTNET_VERSION: '9.0.x'
  NUGET_AUTH_TOKEN: ${{ secrets.NUGET_AUTH_TOKEN }}

jobs:
  build:
    name: Build & Test
    runs-on: ubuntu-latest
    
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0 # Required for SourceLink
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run unit tests
        run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage" --results-directory:"${{ github.workspace }}/TestResults"
      
      - name: Generate coverage report
        uses: danielpalme/ReportGenerator-GitHub-Action@5.1.25
        with:
          reports: '${{ github.workspace }}/TestResults/**/coverage.cobertura.xml'
          targetdir: '${{ github.workspace }}/coveragereport'
          reporttypes: 'HtmlInline_AzurePipelines;Cobertura'
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v4
        with:
          files: '${{ github.workspace }}/TestResults/**/coverage.cobertura.xml'
          fail_ci_if_error: false

  integration-tests:
    name: Integration Tests
    runs-on: ubuntu-latest
    needs: build
    
    services:
      qdrant:
        image: qdrant/qdrant:v1.12.0
        ports:
          - 6333:6333
      postgres:
        image: pgvector/pgvector:pg16
        env:
          POSTGRES_PASSWORD: test
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Run integration tests
        run: dotnet test tests/DotLangChain.Tests.Integration/ --configuration Release
        env:
          QDRANT_HOST: localhost
          QDRANT_PORT: 6333
          POSTGRES_CONNECTION_STRING: Host=localhost;Port=5432;Database=test;Username=postgres;Password=test

  security-scan:
    name: Security Scan
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Run security scan
        run: dotnet list package --vulnerable --include-transitive
      
      - name: Run OWASP Dependency Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: 'DotLangChain'
          path: '.'
          format: 'HTML'

  publish:
    name: Publish Packages
    runs-on: ubuntu-latest
    needs: [build, integration-tests, security-scan]
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Pack
        run: dotnet pack --configuration Release --no-build --output ./artifacts
      
      - name: Publish to NuGet
        run: dotnet nuget push ./artifacts/*.nupkg --api-key ${{ secrets.NUGET_AUTH_TOKEN }} --source https://api.nuget.org/v3/index.json --skip-duplicate
```

### 4.2 Local Build Scripts

A `build.sh` (Linux/macOS) and `build.ps1` (Windows) SHALL be provided:

**build.sh:**
```bash
#!/bin/bash
set -e

echo "Building DotLangChain..."
dotnet build --configuration Release

echo "Running tests..."
dotnet test --configuration Release --no-build

echo "Creating packages..."
dotnet pack --configuration Release --no-build --output ./artifacts
```

**build.ps1:**
```powershell
$ErrorActionPreference = "Stop"

Write-Host "Building DotLangChain..."
dotnet build --configuration Release

Write-Host "Running tests..."
dotnet test --configuration Release --no-build

Write-Host "Creating packages..."
dotnet pack --configuration Release --no-build --output ./artifacts
```

---

## 5. Build Targets

### 5.1 Common Targets

All projects SHALL support:

- `dotnet build` - Compile source code
- `dotnet test` - Run unit and integration tests
- `dotnet pack` - Create NuGet packages
- `dotnet publish` - Publish for deployment (if applicable)

### 5.2 Custom Targets

The solution MAY include custom MSBuild targets in `Directory.Build.targets`:

```xml
<Project>
  <Target Name="ValidatePublicApi" BeforeTargets="Build">
    <!-- Validate public API surface hasn't changed unexpectedly -->
  </Target>
  
  <Target Name="RunCodeAnalysis" AfterTargets="Build">
    <!-- Run static analysis tools -->
  </Target>
</Project>
```

---

## 6. Artifact Management

### 6.1 Build Artifacts

- **NuGet Packages**: `artifacts/*.nupkg` and `artifacts/*.snupkg`
- **Test Results**: `TestResults/`
- **Coverage Reports**: `coveragereport/`
- **Logs**: `artifacts/logs/`

### 6.2 Artifact Retention

- CI artifacts: Retained for 30 days
- Release artifacts: Retained indefinitely
- Test results: Retained for 90 days

---

## 7. Pre-commit Hooks

### 7.1 Recommended Hooks

Developers SHOULD install pre-commit hooks:

```bash
#!/bin/bash
# .git/hooks/pre-commit

dotnet format --verify-no-changes
dotnet build --no-restore
```

### 7.2 Code Formatting

The solution SHALL use `editorconfig` and `Directory.Build.props` for consistent formatting.

---

## 8. Dependency Management

### 8.1 Dependency Updates

- Dependencies SHALL be updated via Dependabot or Renovate
- Security updates SHALL be applied within 7 days
- Major version updates SHALL require explicit approval

### 8.2 Dependency Pinning

- Production dependencies SHALL specify exact versions (`<Version>9.0.0</Version>`)
- Pre-release dependencies MAY use ranges (`<Version>[9.0.0-*, 10.0.0)</Version>`)

---

## 9. Release Process

### 9.1 Release Checklist

1. [ ] All tests passing
2. [ ] Security scan clean
3. [ ] Coverage above 80%
4. [ ] Documentation updated
5. [ ] Version bumped in `Directory.Build.props`
6. [ ] Changelog updated
7. [ ] Release notes prepared
8. [ ] Packages built and validated
9. [ ] Tagged with version (e.g., `v1.0.0`)
10. [ ] Packages published to NuGet

### 9.2 Release Tags

- Format: `v{MAJOR}.{MINOR}.{PATCH}[-{PRERELEASE}]`
- Examples: `v1.0.0`, `v1.1.0-beta.1`, `v2.0.0-rc.1`

---

## 10. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-07 | - | Initial draft |


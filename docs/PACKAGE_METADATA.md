# Package Metadata & Distribution

**Version:** 1.0.0  
**Date:** December 7, 2025  
**Status:** Draft

---

## 1. Package Organization

### 1.1 Package Categories

| Category | Prefix | Purpose | Examples |
|----------|--------|---------|----------|
| Core | `DotLangChain.{Component}` | Core functionality | `DotLangChain.Core`, `DotLangChain.Abstractions` |
| Providers | `DotLangChain.Providers.{Name}` | LLM/embedding providers | `DotLangChain.Providers.OpenAI`, `DotLangChain.Providers.Anthropic` |
| Vector Stores | `DotLangChain.VectorStores.{Name}` | Vector database integrations | `DotLangChain.VectorStores.Qdrant`, `DotLangChain.VectorStores.PgVector` |
| State Stores | `DotLangChain.StateStores.{Name}` | State persistence | `DotLangChain.StateStores.Redis`, `DotLangChain.StateStores.PostgreSQL` |
| Extensions | `DotLangChain.Extensions.{Name}` | Extension libraries | `DotLangChain.Extensions.DependencyInjection`, `DotLangChain.Extensions.Observability` |

### 1.2 Package Dependencies

**Core Packages** (No external dependencies beyond .NET):
- `DotLangChain.Abstractions` - Interfaces only
- `DotLangChain.Core` - Depends on Abstractions + optional providers

**Provider Packages** (Lightweight wrappers):
- `DotLangChain.Providers.OpenAI` - Depends on Core + OpenAI SDK
- `DotLangChain.Providers.Anthropic` - Depends on Core + Anthropic SDK
- `DotLangChain.Providers.Ollama` - Depends on Core (HTTP client only)

**Vector Store Packages**:
- `DotLangChain.VectorStores.Qdrant` - Depends on Core + Qdrant client
- `DotLangChain.VectorStores.PgVector` - Depends on Core + Npgsql

---

## 2. NuGet Package Metadata

### 2.1 Required Metadata

All packages SHALL include:

```xml
<PropertyGroup>
  <!-- Identity -->
  <PackageId>DotLangChain.Core</PackageId>
  <Version>1.0.0</Version>
  <Title>DotLangChain Core</Title>
  <Authors>DotLangChain Contributors</Authors>
  <Company>DotLangChain</Company>
  <Product>DotLangChain</Product>
  
  <!-- Description -->
  <Description>Core library for DotLangChain - document ingestion, embeddings, vector stores, and agent orchestration for .NET.</Description>
  <Summary>Enterprise-grade AI orchestration library for .NET 9</Summary>
  
  <!-- License -->
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  
  <!-- Repository -->
  <RepositoryUrl>https://github.com/dotlangchain/dotlangchain</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  
  <!-- Project URL -->
  <PackageProjectUrl>https://github.com/dotlangchain/dotlangchain</PackageProjectUrl>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  
  <!-- Icons -->
  <PackageIcon>icon.png</PackageIcon>
  
  <!-- Tags -->
  <PackageTags>langchain;ai;llm;embeddings;rag;agents;dotnet;net9</PackageTags>
  
  <!-- Language -->
  <PackageLanguage>en-US</PackageLanguage>
  
  <!-- Symbols -->
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  
  <!-- SourceLink -->
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
</PropertyGroup>
```

### 2.2 Package Icons

- **Icon file**: `icon.png` (64x64 pixels, PNG format)
- **Location**: Solution root `assets/icon.png`
- **Reference**: `<PackageIcon>icon.png</PackageIcon>`
- **Include**: `<None Include="../../assets/icon.png" Pack="true" PackagePath="\"/></None>`

### 2.3 Package README

Each package SHALL include a README.md:

- **Location**: Package root (e.g., `src/DotLangChain.Core/README.md`)
- **Reference**: `<PackageReadmeFile>README.md</PackageReadmeFile>`
- **Content**: Quick start, examples, links to full documentation

**Example README.md**:

```markdown
# DotLangChain.Core

Core library for DotLangChain.

## Quick Start

```csharp
services.AddDotLangChain(builder =>
{
    builder.AddDocumentLoaders(docs => docs.AddPdf());
});
```

## Documentation

See [full documentation](https://dotlangchain.dev/docs) for details.

## License

MIT
```

---

## 3. Package Requirements

### 3.1 Required vs Optional Packages

**Required** (must install at least one):
- `DotLangChain.Core` - Core functionality
- At least one provider package (e.g., `DotLangChain.Providers.OpenAI`)

**Optional**:
- Provider packages (install only what you need)
- Vector store packages (use in-memory for development)
- State store packages (optional for stateless deployments)
- Extension packages (install as needed)

### 3.2 Package Recommendations

**Minimum Setup**:
```xml
<PackageReference Include="DotLangChain.Core" Version="1.0.0" />
<PackageReference Include="DotLangChain.Providers.Ollama" Version="1.0.0" />
```

**Production Setup**:
```xml
<PackageReference Include="DotLangChain.Core" Version="1.0.0" />
<PackageReference Include="DotLangChain.Providers.OpenAI" Version="1.0.0" />
<PackageReference Include="DotLangChain.VectorStores.Qdrant" Version="1.0.0" />
<PackageReference Include="DotLangChain.Extensions.DependencyInjection" Version="1.0.0" />
<PackageReference Include="DotLangChain.Extensions.Observability" Version="1.0.0" />
```

---

## 4. Package Distribution

### 4.1 NuGet.org Distribution

All packages SHALL be published to [NuGet.org](https://www.nuget.org/):

- **Primary feed**: https://api.nuget.org/v3/index.json
- **Requires**: NuGet account, API key
- **Process**: Automated via GitHub Actions (see `BUILD_AND_CICD.md`)

### 4.2 Alternative Feeds

Packages MAY be distributed via:

- **GitHub Packages**: `https://nuget.pkg.github.com/dotlangchain/index.json`
- **Azure Artifacts**: Organization-specific feeds
- **Private feeds**: For enterprise customers

### 4.3 Package Visibility

- **Public packages**: Available to all NuGet users
- **Pre-release packages**: Marked as pre-release, requires `--prerelease` flag
- **Unlisted packages**: Hidden from search but accessible via direct link (for rollbacks)

---

## 5. Package Size Optimization

### 5.1 Size Targets

| Package Type | Target Size | Maximum Size |
|--------------|-------------|--------------|
| Core | < 1 MB | < 2 MB |
| Provider | < 500 KB | < 1 MB |
| Vector Store | < 1 MB | < 2 MB |

### 5.2 Optimization Strategies

1. **Trim unused dependencies**: Use `<PrivateAssets>All</PrivateAssets>` for dev-only packages
2. **SourceLink**: Embed source via SourceLink (smaller than symbols package)
3. **Dependencies**: Minimize transitive dependencies
4. **IL trimming**: Enable trimming for AOT deployments (optional)

### 5.3 Dependency Analysis

Use `dotnet list package --include-transitive` to analyze dependencies:

```bash
dotnet list package --include-transitive --outdated
```

---

## 6. Package Signing

### 6.1 Code Signing (Future)

For enterprise customers, packages MAY be signed:

- **Certificate**: Code signing certificate from trusted CA
- **Timestamp**: RFC 3161 timestamp server
- **Verification**: NuGet validates signatures automatically

**Configuration**:

```xml
<PropertyGroup>
  <SignAssembly>true</SignAssembly>
  <AssemblyOriginatorKeyFile>key.snk</AssemblyOriginatorKeyFile>
</PropertyGroup>
```

---

## 7. Package Metadata Validation

### 7.1 Validation Checklist

Before publishing, validate:

- [ ] Package ID matches naming convention
- [ ] Version follows SemVer
- [ ] Description is clear and accurate
- [ ] Tags are relevant and searchable
- [ ] License is specified and valid
- [ ] Repository URL is correct
- [ ] Icon is included (if applicable)
- [ ] README is included
- [ ] Dependencies are correct
- [ ] Symbols package builds successfully

### 7.2 Validation Tools

```bash
# Validate package
dotnet pack --configuration Release
nuget verify DotLangChain.Core.1.0.0.nupkg -All

# Check package contents
nuget spec DotLangChain.Core.1.0.0.nupkg
```

---

## 8. Package Discovery

### 8.1 NuGet Search Optimization

To improve discoverability:

1. **Tags**: Include relevant keywords (langchain, ai, llm, rag, agents, dotnet)
2. **Description**: Include common use cases
3. **Summary**: Clear, concise one-liner
4. **README**: Comprehensive examples and documentation

### 8.2 Documentation Links

- **Package Project URL**: Links to GitHub repository
- **Documentation URL**: Links to docs site (if separate)
- **License URL**: Links to LICENSE file
- **Release Notes**: Links to CHANGELOG.md

---

## 9. Package Versioning in Code

### 9.1 Assembly Version

```xml
<PropertyGroup>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
  <InformationalVersion>1.0.0</InformationalVersion>
</PropertyGroup>
```

### 9.2 Version Attributes

```csharp
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
```

---

## 10. Package Support

### 10.1 Support Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions and community support
- **Documentation**: https://dotlangchain.dev/docs
- **Email**: support@dotlangchain.dev (for enterprise)

### 10.2 Support Policy

- **Latest version**: Full support
- **Previous major version**: Security fixes only (12 months)
- **Older versions**: Community support only

---

## 11. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-07 | - | Initial draft |


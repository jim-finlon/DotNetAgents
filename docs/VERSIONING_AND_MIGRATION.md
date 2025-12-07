# Versioning & Migration Guide

**Version:** 1.0.0  
**Date:** December 7, 2025  
**Status:** Draft

---

## 1. Versioning Strategy

### 1.1 Semantic Versioning

DotLangChain follows [Semantic Versioning 2.0.0](https://semver.org/):

- **MAJOR.MINOR.PATCH** (e.g., `1.2.3`)
- **MAJOR**: Breaking API changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### 1.2 Pre-release Versions

- **Alpha** (`1.0.0-alpha.1`): Early development, unstable API
- **Beta** (`1.0.0-beta.1`): Feature complete, testing phase
- **Release Candidate** (`1.0.0-rc.1`): Production-ready, final validation

### 1.3 Package Version Synchronization

All packages in a release SHALL use the same version:

- `DotLangChain.Core` v1.2.3
- `DotLangChain.Providers.OpenAI` v1.2.3
- `DotLangChain.VectorStores.Qdrant` v1.2.3

**Exception**: Provider packages MAY have independent versioning for provider-specific bug fixes (e.g., `DotLangChain.Providers.OpenAI` v1.2.4 while Core remains v1.2.3).

---

## 2. Breaking Changes Policy

### 2.1 Definition of Breaking Changes

A breaking change is any modification that:

1. Removes or renames public APIs (types, methods, properties)
2. Changes method signatures (parameters, return types)
3. Changes behavior in a way that breaks existing code
4. Removes or changes required configuration properties
5. Changes default behavior that users may depend on

### 2.2 Non-Breaking Changes

The following are **NOT** considered breaking:

1. Adding new types, methods, or properties
2. Adding optional parameters (with default values)
3. Adding new overloads
4. Improving performance (behavior-preserving)
5. Fixing bugs (restoring intended behavior)
6. Adding new exception types (only if existing code doesn't catch specific exceptions)

### 2.3 Deprecation Process

Breaking changes SHALL follow a deprecation cycle:

1. **Announcement** (1 major version before removal):
   - Mark APIs with `[Obsolete]` attribute
   - Document in CHANGELOG
   - Provide migration guide

2. **Warning Period** (1 major version):
   - Obsolete APIs generate compiler warnings
   - Obsolete APIs still function

3. **Removal** (next major version):
   - Obsolete APIs removed
   - Breaking change documented in CHANGELOG

**Example**:

```csharp
// Version 1.0.0
public interface ITextSplitter
{
    IAsyncEnumerable<DocumentChunk> SplitAsync(Document document, TextSplitterOptions? options = null);
}

// Version 1.1.0 - Deprecate old method
public interface ITextSplitter
{
    [Obsolete("Use SplitAsync with CancellationToken. This method will be removed in v2.0.0.")]
    IAsyncEnumerable<DocumentChunk> SplitAsync(Document document, TextSplitterOptions? options = null);
    
    IAsyncEnumerable<DocumentChunk> SplitAsync(
        Document document, 
        TextSplitterOptions? options = null,
        CancellationToken cancellationToken = default);
}

// Version 2.0.0 - Remove obsolete method
public interface ITextSplitter
{
    IAsyncEnumerable<DocumentChunk> SplitAsync(
        Document document, 
        TextSplitterOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

---

## 3. CHANGELOG Format

### 3.1 CHANGELOG.md Structure

The repository SHALL maintain a `CHANGELOG.md` following [Keep a Changelog](https://keepachangelog.com/):

```markdown
# Changelog

All notable changes to DotLangChain will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.0] - 2025-12-15

### Added
- Support for Anthropic Claude 4 models
- Token-aware text splitting with cl100k_base tokenizer
- In-memory vector store for development

### Changed
- Improved error messages for invalid document formats
- Updated OpenAI provider to support latest API version

### Deprecated
- `ITextSplitter.SplitAsync` without CancellationToken (will be removed in v2.0.0)

### Removed
- Support for .NET 8.0 (minimum version is now .NET 9.0)

### Fixed
- Memory leak in graph execution when using long-running workflows
- Incorrect embedding normalization for some providers

### Security
- Fixed potential SSRF vulnerability in URL-based document loading

## [1.1.0] - 2025-12-01

### Added
- Initial release
```

### 3.2 Change Categories

- **Added**: New features
- **Changed**: Changes in existing functionality
- **Deprecated**: Soon-to-be removed features
- **Removed**: Removed features
- **Fixed**: Bug fixes
- **Security**: Security fixes

---

## 4. Migration Guides

### 4.1 Version-Specific Migration Guides

Migration guides SHALL be created for each major version:

- `docs/migration/v1-to-v2.md`
- `docs/migration/v2-to-v3.md`

### 4.2 Migration Guide Template

```markdown
# Migration Guide: v1.x to v2.0

## Overview

Version 2.0 introduces breaking changes to improve API consistency and performance.

## Breaking Changes

### 1. CancellationToken Required

**Before (v1.x)**:
```csharp
await splitter.SplitAsync(document);
```

**After (v2.0)**:
```csharp
await splitter.SplitAsync(document, cancellationToken: cancellationToken);
```

**Reason**: Better cancellation support and async best practices.

### 2. Exception Hierarchy Changes

**Before (v1.x)**:
```csharp
catch (Exception ex) // Catch-all
```

**After (v2.0)**:
```csharp
catch (DocumentException ex) // Specific exception type
```

**Reason**: Better error handling and categorization.

## Automatic Migration

Use the migration tool:

```bash
dotnet tool install -g DotLangChain.MigrationTool
dotlangchain-migrate --from 1.9.0 --to 2.0.0
```

## Manual Migration Steps

1. Update package references
2. Review breaking changes
3. Update code to use new APIs
4. Run tests
5. Verify behavior

## Additional Resources

- [API Reference](../api/index.md)
- [Examples](../examples/index.md)
```

---

## 5. Version Lifecycle

### 5.1 Support Policy

| Version Type | Support Duration | Updates |
|--------------|------------------|---------|
| Latest Major | Indefinite | Security, features, fixes |
| Previous Major | 12 months | Security fixes only |
| Older Majors | None | None |

**Example**:
- v2.0.0 released: v2.x supported, v1.x supported (security only) for 12 months
- v3.0.0 released: v3.x supported, v2.x supported (security only) for 12 months, v1.x unsupported

### 5.2 End-of-Life Announcement

EOL announcements SHALL be made:

1. **6 months before EOL**: Announcement in CHANGELOG and release notes
2. **3 months before EOL**: Reminder in release notes
3. **At EOL**: Final announcement, no further updates

---

## 6. Package Compatibility

### 6.1 Version Compatibility Matrix

| DotLangChain.Core | DotLangChain.Providers.* | DotLangChain.VectorStores.* | Status |
|-------------------|--------------------------|----------------------------|--------|
| 1.0.0 | 1.0.0 | 1.0.0 | ✅ Compatible |
| 1.1.0 | 1.0.0 | 1.0.0 | ⚠️ May work (tested) |
| 1.2.0 | 1.0.0 | 1.0.0 | ❌ Incompatible (breaking changes) |
| 1.2.0 | 1.2.0 | 1.2.0 | ✅ Compatible |

### 6.2 Compatibility Testing

All package combinations SHALL be tested in CI:

```yaml
strategy:
  matrix:
    core: [1.0.0, 1.1.0, 1.2.0]
    provider: [1.0.0, 1.1.0, 1.2.0]
    vectorstore: [1.0.0, 1.1.0, 1.2.0]
```

---

## 7. Version Detection

### 7.1 Runtime Version Checking

```csharp
var version = typeof(DotLangChainException).Assembly.GetName().Version;
Console.WriteLine($"DotLangChain version: {version}");
```

### 7.2 Package Version in Code

Version information available via:

```csharp
[assembly: AssemblyInformationalVersion("1.2.3")]
[assembly: AssemblyFileVersion("1.2.3.0")]
```

---

## 8. Release Process

### 8.1 Version Bump Workflow

1. Update `Directory.Build.props`:
   ```xml
   <VersionPrefix>1.2.0</VersionPrefix>
   ```

2. Update `CHANGELOG.md`:
   - Add new version section
   - Document all changes

3. Create release branch:
   ```bash
   git checkout -b release/1.2.0
   ```

4. Tag release:
   ```bash
   git tag -a v1.2.0 -m "Release v1.2.0"
   ```

5. Merge to main and push tags

### 8.2 Pre-release Workflow

1. Update version to pre-release:
   ```xml
   <VersionPrefix>1.2.0</VersionPrefix>
   <VersionSuffix>beta.1</VersionSuffix>
   ```

2. Publish pre-release packages (marked as pre-release on NuGet)

3. Announce pre-release (GitHub Discussions, Discord, etc.)

---

## 9. Rollback Strategy

### 9.1 Package Rollback

If a release contains critical bugs:

1. **Immediate**: Publish patch release (e.g., v1.2.1) with fix
2. **Documentation**: Update release notes with rollback instructions
3. **Communication**: Announce rollback and fix timeline

### 9.2 Version Unlisting

In extreme cases (security vulnerability, data corruption):

1. Unlist broken package version from NuGet
2. Publish security advisory
3. Provide migration path to fixed version

---

## 10. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-07 | - | Initial draft |


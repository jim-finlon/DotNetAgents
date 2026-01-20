# NuGet Package Strategy Analysis: Single vs. Multi-Package

**Version:** 1.0  
**Date:** 2024  
**Status:** Decision Analysis

## Executive Summary

**Recommendation:** Start with a **hybrid approach** - structure code as multiple projects from day one, but initially publish as a single metapackage. This gives you the flexibility to split later without major refactoring.

## The Core Question

> "Should we rethink the single NuGet package, or would it be easy enough to split it out later should we need to?"

**Short Answer:** Splitting later is **moderately difficult** - it's doable but requires careful upfront planning. The better approach is to **structure for modularity from the start**, even if you ship as one package initially.

## Analysis: Can You Split Later?

### Difficulty Level: ⚠️ Moderate to Hard

**Why splitting later is challenging:**

1. **Dependency Coupling**
   - If core code directly references integration code, you'll need to refactor
   - Example: `Core` calling `OpenAIModel` directly instead of through `ILLMModel`
   - **Impact:** Requires dependency inversion refactoring

2. **Namespace and API Surface**
   - Users may have imported everything from one namespace
   - Breaking changes when moving types to new packages
   - **Impact:** Requires migration guide, potential breaking changes

3. **Versioning Complexity**
   - Single package = single version number
   - After split, need to coordinate versions across packages
   - **Impact:** Version management becomes more complex

4. **Testing and CI/CD**
   - Tests may be coupled across boundaries
   - Need to restructure test projects
   - **Impact:** Test refactoring required

5. **Documentation and Examples**
   - Examples may assume single package
   - Documentation needs updates
   - **Impact:** Documentation rewrite needed

### What Makes Splitting Easier:

✅ **If you structure correctly from the start:**
- Separate projects for each logical module
- Core has no dependencies on integrations (dependency inversion)
- Clear interfaces and abstractions
- Independent test projects per module

**Then splitting is:** ⚠️ Moderate difficulty
- Mainly involves:
  - Creating separate NuGet projects
  - Updating package references
  - Versioning strategy
  - Documentation updates

## Comparison: Single vs. Multi-Package

### Single Package Approach

**Pros:**
- ✅ Simpler initial development
- ✅ One version number to manage
- ✅ Easier for users to get started (one install)
- ✅ Less CI/CD complexity initially
- ✅ No dependency version conflicts between modules

**Cons:**
- ❌ Larger package size (users get everything)
- ❌ More dependencies pulled in (even unused ones)
- ❌ Harder to evolve independently
- ❌ Breaking changes affect all users
- ❌ Security vulnerabilities affect entire package
- ❌ Slower restore times for large projects

### Multi-Package Approach

**Pros:**
- ✅ Lean core package (faster, smaller)
- ✅ Users only install what they need
- ✅ Independent versioning (core stable, integrations evolve)
- ✅ Better separation of concerns
- ✅ Easier to add new providers without touching core
- ✅ Security isolation (vulnerability in one provider doesn't affect others)
- ✅ Aligns with LangChain's architecture
- ✅ Better for enterprise (compliance, auditing per component)

**Cons:**
- ❌ More complex project structure initially
- ❌ More CI/CD pipelines to manage
- ❌ Version coordination complexity
- ❌ Users need to understand which packages to install
- ❌ Potential version conflicts between packages

## Recommended Approach: Hybrid Strategy

### Phase 1: Structure for Modularity, Ship as Metapackage

**Project Structure (from day one):**
```
src/
├── DotNetAgents.Core/              # Core abstractions only
├── DotNetAgents.Workflow/          # Workflow engine
├── DotNetAgents.Integrations.OpenAI/
├── DotNetAgents.Integrations.Azure/
├── DotNetAgents.Integrations.Anthropic/
├── DotNetAgents.Integrations.Pinecone/
└── DotNetAgents/                   # Metapackage (references all)
```

**Key Principles:**
1. **Core has zero dependencies on integrations** (dependency inversion)
2. **Each integration is a separate project**
3. **Main package is a metapackage** that references all others
4. **Users can reference individual packages OR the metapackage**

**Benefits:**
- ✅ Code is already modular (easy to split later)
- ✅ Users get simple "one package" experience initially
- ✅ Advanced users can reference only what they need
- ✅ No refactoring needed when splitting
- ✅ Aligns with .NET best practices (see Microsoft.Extensions.*)

### Phase 2: Split When Needed

**When to split:**
- Package size becomes an issue (>10MB)
- Users request smaller packages
- Need independent versioning
- Security/compliance requires isolation

**Splitting process:**
1. Create separate `.nuspec` files for each project
2. Update CI/CD to build/publish multiple packages
3. Update metapackage to reference individual packages
4. Update documentation
5. Provide migration guide

**Difficulty:** ⚠️ Low (because structure is already modular)

## Real-World Examples

### Microsoft's Approach (Microsoft.Extensions.*)

Microsoft uses multi-package approach:
- `Microsoft.Extensions.DependencyInjection` (core)
- `Microsoft.Extensions.Logging` (core)
- `Microsoft.Extensions.Logging.Console` (integration)
- `Microsoft.Extensions.Logging.EventLog` (integration)
- `Microsoft.Extensions.Hosting` (metapackage)

**Why:** Users only install what they need, core stays lean.

### LangChain's Approach

LangChain uses modular packages:
- `langchain-core` (minimal dependencies)
- `langchain-openai` (OpenAI integration)
- `langchain-community` (community integrations)
- `langchain` (metapackage)

**Why:** Core stays stable, integrations evolve independently.

## Specific Recommendation for DotNetAgents

### Recommended Package Structure

```
DotNetAgents.Core (v1.0.0)
├── Core abstractions only
├── Minimal dependencies
└── ~500KB package size

DotNetAgents.Workflow (v1.0.0)
├── Depends on: DotNetAgents.Core
└── ~200KB package size

DotNetAgents.Providers.OpenAI (v1.0.0)
├── Depends on: DotNetAgents.Core
└── ~100KB package size

DotNetAgents.Providers.Azure (v1.0.0)
├── Depends on: DotNetAgents.Core
└── ~100KB package size

DotNetAgents.Providers.Anthropic (v1.0.0)
├── Depends on: DotNetAgents.Core
└── ~100KB package size

DotNetAgents.VectorStores.Pinecone (v1.0.0)
├── Depends on: DotNetAgents.Core
└── ~150KB package size

DotNetAgents (v1.0.0) [METAPACKAGE]
├── References all above packages
├── Convenience package
└── ~50KB (just metadata)
```

### Implementation Strategy

**Week 1-2: Set up project structure**
```csharp
// Core project - NO dependencies on integrations
namespace DotNetAgents.Core
{
    public interface ILLMModel<TInput, TOutput> { }
    public interface IVectorStore { }
    // ... core abstractions only
}

// Integration project - depends on Core
namespace DotNetAgents.Providers.OpenAI
{
    public class OpenAIModel : ILLMModel<ChatMessage[], ChatMessage>
    {
        // Implementation
    }
}

// Metapackage - references all
<ProjectReference Include="..\DotNetAgents.Core\DotNetAgents.Core.csproj" />
<ProjectReference Include="..\DotNetAgents.Providers.OpenAI\DotNetAgents.Providers.OpenAI.csproj" />
<!-- etc -->
```

**Benefits of this approach:**
1. ✅ Code is modular from day one
2. ✅ Easy to split later (just change packaging)
3. ✅ Users can choose granularity level
4. ✅ Core stays lean and stable
5. ✅ Integrations can evolve independently

## Decision Matrix

| Factor | Single Package | Multi-Package (Hybrid) | Pure Multi-Package |
|--------|---------------|------------------------|-------------------|
| **Initial Complexity** | Low | Medium | High |
| **Splitting Difficulty** | Hard | Easy | N/A |
| **User Experience** | Simple | Flexible | Complex |
| **Maintainability** | Medium | High | High |
| **Enterprise Fit** | Medium | High | High |
| **Alignment with LangChain** | Low | High | High |

## Final Recommendation

**Start with the hybrid approach:**

1. ✅ **Structure code as multiple projects** from day one
2. ✅ **Publish as metapackage initially** for simplicity
3. ✅ **Allow users to reference individual packages** if they want
4. ✅ **Split fully later** when/if needed (will be easy)

**Why this is best:**
- Gives you flexibility without initial complexity
- Aligns with industry best practices
- Easy to split later (no refactoring needed)
- Best of both worlds: simple for users, modular for maintainers

## Action Items

1. ✅ Update project structure to separate projects
2. ✅ Ensure Core has no integration dependencies
3. ✅ Create metapackage that references all
4. ✅ Document both installation methods (metapackage vs. individual)
5. ✅ Plan CI/CD to support both approaches

## Migration Path (If Starting Single)

If you've already committed to single package, here's how to migrate:

**Step 1: Refactor to separate projects** (2-3 weeks)
- Extract integrations to separate projects
- Ensure dependency inversion
- Update namespaces

**Step 2: Create package structure** (1 week)
- Create `.csproj` files for each package
- Set up versioning strategy
- Configure NuGet metadata

**Step 3: Update CI/CD** (1 week)
- Multi-package build pipeline
- Version coordination
- Publishing strategy

**Step 4: Documentation** (1 week)
- Update installation guides
- Migration guide for existing users
- Package reference documentation

**Total effort:** ~5-6 weeks

**Conclusion:** Better to structure correctly from the start (saves 5-6 weeks later).

---

**Recommendation:** Use hybrid approach - structure modularly, ship as metapackage, split when needed.
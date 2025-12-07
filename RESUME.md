# Resume Work Guide

**Purpose:** Quick reference for resuming development after a break or context loss

---

## Quick Start Checklist

### 1. Read START_HERE.md
📍 **Location:** `/START_HERE.md`

This file provides session initialization instructions and points to key files.

### 2. Review Session State
📍 **Location:** `/SESSION_STATE.md`

Check what was being worked on, current phase, and any active blockers.

### 3. Check Task List
📍 **Location:** `/TASK_LIST.md`

Review completed tasks and identify next steps.

### 4. Review Lessons Learned
📍 **Location:** `/LESSONS_LEARNED.md`

Understand key decisions and patterns established in previous sessions.

---

## Project Structure Quick Reference

### Documentation
```
docs/
├── REQUIREMENTS.md              # Functional and non-functional requirements
├── TECHNICAL_SPECIFICATIONS.md  # Architecture and implementation details
├── API_REFERENCE.md             # Complete API documentation
├── BUILD_AND_CICD.md            # Build and CI/CD configuration
├── TESTING_STRATEGY.md          # Testing approach and guidelines
├── PERFORMANCE_BENCHMARKS.md    # Performance targets and benchmarks
├── ERROR_HANDLING.md            # Exception hierarchy and error handling
├── VERSIONING_AND_MIGRATION.md  # Versioning strategy and migration guides
└── PACKAGE_METADATA.md          # Package distribution and metadata
```

### Project Management Files
```
├── START_HERE.md         # Session startup reminder (READ THIS FIRST)
├── TASK_LIST.md          # Detailed task breakdown
├── SESSION_STATE.md      # Current state and progress
├── LESSONS_LEARNED.md    # Key decisions and learnings
└── RESUME.md             # This file
```

### Source Code (To Be Created)
```
src/
├── DotLangChain.Abstractions/     # Interfaces and contracts
├── DotLangChain.Core/             # Core implementations
├── DotLangChain.Providers.*/      # LLM provider implementations
├── DotLangChain.VectorStores.*/   # Vector store integrations
├── DotLangChain.StateStores.*/    # State persistence implementations
└── DotLangChain.Extensions.*/     # Extension libraries
```

---

## Current Phase

**Phase:** Documentation & Setup  
**Status:** Documentation complete, starting solution structure setup  
**Next Steps:** Create solution file and build configuration

---

## Key Decisions Reference

### Architecture
- **.NET 9.0** targeting
- **Interface-first** design pattern
- **Provider pattern** for external services
- **Semantic Versioning** (SemVer 2.0.0)

### Technology Choices
- **xUnit** for testing
- **FluentAssertions** for test assertions
- **Testcontainers.NET** for integration tests
- **BenchmarkDotNet** for performance testing
- **Polly** for resilience patterns
- **OpenTelemetry** for observability

### Code Standards
- **C# 13** (LangVersion 13.0)
- **Nullable reference types** enabled
- **Treat warnings as errors** in release builds
- **XML documentation** required for public APIs
- **80%+ test coverage** target

---

## Common Workflows

### Starting a New Session
1. Read `/START_HERE.md`
2. Review `/SESSION_STATE.md`
3. Check `/TASK_LIST.md` for next tasks
4. Review recent changes in git history

### Before Context Compression
1. Update `/SESSION_STATE.md` with current progress
2. Update `/TASK_LIST.md` with completed tasks
3. Update `/LESSONS_LEARNED.md` with any new insights
4. Commit all changes with descriptive message

### Resuming After Break
1. Run through Quick Start Checklist (above)
2. Review git log: `git log --oneline -10`
3. Check for uncommitted changes: `git status`
4. Review any open issues or TODOs in code

### Starting a New Feature
1. Create feature branch: `git checkout -b feature/feature-name`
2. Review relevant documentation
3. Check `/TASK_LIST.md` for related tasks
4. Update `/TASK_LIST.md` with new tasks if needed

### Completing a Feature
1. Write/update tests
2. Update documentation if API changed
3. Update `/TASK_LIST.md` - mark tasks complete
4. Update `/SESSION_STATE.md` with progress
5. Commit with descriptive message
6. Create PR (if using branching workflow)

---

## Troubleshooting

### "Where was I?"
- Check `/SESSION_STATE.md` for current phase and focus
- Review `/TASK_LIST.md` for in-progress tasks
- Check git log for recent commits

### "What was decided?"
- Review `/LESSONS_LEARNED.md` for key decisions
- Check git commit messages for context
- Review documentation files for design decisions

### "What's next?"
- Check `/TASK_LIST.md` for pending tasks
- Review `/SESSION_STATE.md` for next steps
- Check milestones in `/TASK_LIST.md`

### "What's the architecture?"
- Read `/docs/TECHNICAL_SPECIFICATIONS.md`
- Review `/docs/REQUIREMENTS.md` for context
- Check `/docs/API_REFERENCE.md` for API design

---

## File Update Checklist

Update these files regularly:

### At Session Start
- [ ] Read `/START_HERE.md`
- [ ] Review `/SESSION_STATE.md`
- [ ] Check `/TASK_LIST.md`

### During Session
- [ ] Update `/TASK_LIST.md` as tasks are completed
- [ ] Update `/SESSION_STATE.md` at milestones
- [ ] Note decisions in `/LESSONS_LEARNED.md`

### Before Context Compression
- [ ] Update `/SESSION_STATE.md` with current state
- [ ] Update `/TASK_LIST.md` with progress
- [ ] Update `/LESSONS_LEARNED.md` with new insights
- [ ] Commit all changes

### When Starting Implementation
- [ ] Review `/docs/TECHNICAL_SPECIFICATIONS.md`
- [ ] Check `/docs/API_REFERENCE.md` for API contracts
- [ ] Review `/docs/ERROR_HANDLING.md` for exception patterns

---

## Quick Commands

### Git Commands
```bash
# Check status
git status

# View recent commits
git log --oneline -10

# Create feature branch
git checkout -b feature/feature-name

# Commit changes
git add .
git commit -m "Description of changes"

# View changes
git diff
```

### Build Commands
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run --project tests/DotLangChain.Tests.Benchmarks -c Release
```

---

## Revision History

| Date | Change |
|------|--------|
| 2025-12-07 | Initial resume guide created |


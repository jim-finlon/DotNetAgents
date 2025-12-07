# Git Workflow Guide

**Version:** 1.0.0  
**Date:** December 7, 2025

---

## Branching Strategy

### Branch Types

1. **`main`** - Production-ready code
   - Only contains tested, stable code
   - Protected branch (no direct pushes)
   - All merges via Pull Requests

2. **`feature/{phase-name}`** - Major development phases
   - Each major phase gets its own feature branch
   - Examples: `feature/core-abstractions`, `feature/core-implementation`, `feature/providers`
   - Merged to `main` after all tests pass

3. **`bugfix/{issue-name}`** - Bug fixes
   - For fixing bugs in `main`
   - Merged directly or via PR

4. **`release/{version}`** - Release preparation
   - For preparing releases
   - Example: `release/v1.0.0`

---

## Workflow

### Starting a New Phase

1. **Create feature branch from main**:
   ```bash
   git checkout main
   git pull origin main  # If remote exists
   git checkout -b feature/core-abstractions
   ```

2. **Work on the branch**:
   - Make commits with descriptive messages
   - Follow commit message conventions (see below)
   - Push regularly: `git push -u origin feature/core-abstractions`

3. **Ensure all tests pass**:
   ```bash
   dotnet test --configuration Release
   ```

4. **Merge to main**:
   ```bash
   git checkout main
   git merge feature/core-abstractions
   # Or create a PR if using GitHub/GitLab
   git push origin main
   ```

5. **Delete feature branch** (optional):
   ```bash
   git branch -d feature/core-abstractions
   git push origin --delete feature/core-abstractions  # If pushed to remote
   ```

6. **Start next phase**:
   - Create new feature branch from updated `main`
   - Repeat process

---

## Commit Message Conventions

Follow conventional commits format:

```
type(scope): description

[optional body]

[optional footer]
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `test`: Test additions or changes
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `chore`: Maintenance tasks
- `build`: Build system changes
- `ci`: CI/CD changes

### Examples

```bash
feat(abstractions): add IDocumentLoader interface
fix(core): handle null metadata in Document constructor
docs(api): update embedding service documentation
test(core): add tests for RecursiveCharacterTextSplitter
refactor(graph): simplify state transition logic
perf(embeddings): implement caching for embedding service
```

---

## Branch Naming Conventions

### Feature Branches

Format: `feature/{phase-name}` or `feature/{component-name}`

Examples:
- `feature/core-abstractions`
- `feature/core-implementation`
- `feature/providers-openai`
- `feature/vector-stores-qdrant`
- `feature/graph-execution-engine`

### Bugfix Branches

Format: `bugfix/{issue-description}`

Examples:
- `bugfix/embedding-cache-memory-leak`
- `bugfix/document-loader-null-exception`

### Release Branches

Format: `release/v{major}.{minor}.{patch}`

Examples:
- `release/v1.0.0`
- `release/v1.1.0`

---

## Development Workflow

### Daily Workflow

1. **Start of day**:
   ```bash
   git checkout main
   git pull origin main
   git checkout feature/current-phase
   git merge main  # Update feature branch with main
   ```

2. **During work**:
   - Make frequent, small commits
   - Write descriptive commit messages
   - Push regularly to remote

3. **Before merging**:
   - Ensure all tests pass
   - Run code analysis/linting
   - Update documentation if needed
   - Update SESSION_STATE.md and TASK_LIST.md

### Before Merging to Main

Checklist:
- [ ] All tests pass (`dotnet test`)
- [ ] Code builds without warnings (`dotnet build --configuration Release`)
- [ ] Documentation updated (if API changed)
- [ ] SESSION_STATE.md updated
- [ ] TASK_LIST.md updated with completed tasks
- [ ] LESSONS_LEARNED.md updated (if new insights)
- [ ] Commit messages follow conventions
- [ ] No merge conflicts

---

## Major Phases (Feature Branches)

Based on TASK_LIST.md, major phases include:

1. **`feature/core-abstractions`** - Core interfaces and contracts
2. **`feature/core-implementation`** - Core implementations (loaders, splitters, graph engine)
3. **`feature/providers`** - LLM provider implementations
4. **`feature/vector-stores`** - Vector store integrations
5. **`feature/state-stores`** - State persistence implementations
6. **`feature/extensions`** - Extension libraries (DI, Observability)
7. **`feature/testing`** - Test suite implementation
8. **`feature/samples`** - Sample applications

---

## Merge Strategies

### Fast-Forward Merge (Preferred)

When feature branch is ahead of main with no diverged commits:

```bash
git checkout main
git merge --ff-only feature/core-abstractions
```

### Merge Commit

When both branches have diverged:

```bash
git checkout main
git merge --no-ff feature/core-abstractions
```

Creates a merge commit preserving branch history.

### Squash Merge (Optional)

For feature branches with many small commits:

```bash
git checkout main
git merge --squash feature/core-abstractions
git commit -m "feat(core): implement core abstractions"
```

---

## Conflict Resolution

If conflicts occur during merge:

1. **Identify conflicts**:
   ```bash
   git status  # Shows conflicted files
   ```

2. **Resolve conflicts**:
   - Edit conflicted files
   - Remove conflict markers (`<<<<<<<`, `=======`, `>>>>>>>`)
   - Test thoroughly after resolution

3. **Complete merge**:
   ```bash
   git add <resolved-files>
   git commit  # Complete the merge
   ```

---

## Tagging Releases

After merging release branch to main:

```bash
git checkout main
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```

---

## Best Practices

1. **Keep branches short-lived** - Merge feature branches promptly after tests pass
2. **Regular integration** - Merge `main` into feature branches regularly
3. **Small, focused commits** - One logical change per commit
4. **Descriptive messages** - Clear commit messages help future you
5. **Test before merge** - Never merge broken code to `main`
6. **Update documentation** - Keep docs in sync with code changes

---

## Troubleshooting

### Undo last commit (keep changes)
```bash
git reset --soft HEAD~1
```

### Undo last commit (discard changes)
```bash
git reset --hard HEAD~1
```

### View branch differences
```bash
git diff main..feature/core-abstractions
```

### View commit history
```bash
git log --oneline --graph --all
```

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2025-12-07 | - | Initial draft |


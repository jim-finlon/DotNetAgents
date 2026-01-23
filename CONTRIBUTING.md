# Contributing to DotNetAgents

First off, thank you for considering contributing to DotNetAgents! It's people like you that make DotNetAgents such a great tool.

## Code of Conduct

This project adheres to a Code of Conduct that all contributors are expected to follow. Please read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the issue list as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

- **Clear title and description**
- **Steps to reproduce** - Be specific!
- **Expected behavior** - What should happen?
- **Actual behavior** - What actually happens?
- **Environment** - .NET version, OS, package versions
- **Code sample** - Minimal reproduction code if possible
- **Screenshots** - If applicable

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

- **Clear title and description**
- **Use case** - Why is this feature useful?
- **Proposed solution** - How should it work?
- **Alternatives** - Other solutions you've considered
- **Additional context** - Any other relevant information

### Pull Requests

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Make your changes**
   - Follow our coding standards (see `.cursorrules`)
   - Write tests for new features
   - Update documentation
   - Ensure all tests pass
4. **Commit your changes** (use conventional commits: `feat: add amazing feature`)
5. **Push to your branch** (`git push origin feature/amazing-feature`)
6. **Open a Pull Request**

### Pull Request Guidelines

- **Keep PRs focused** - One feature or fix per PR
- **Write clear commit messages** - Use conventional commits format
- **Include tests** - New features must include tests
- **Update documentation** - Update relevant docs
- **Check CI** - Ensure all checks pass
- **Request reviews** - Tag relevant maintainers

## Development Setup

### Prerequisites

- .NET 8 SDK or later
- Git
- Your favorite IDE (Visual Studio, Rider, VS Code)

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/jim-finlon/DotNetAgents.git
   cd DotNetAgents
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

### Project Structure

```
DotNetAgents/
├── src/              # Source code
├── tests/            # Test projects
├── samples/          # Sample applications
└── docs/             # Documentation
```

## Coding Standards

Please follow our coding standards defined in [.cursorrules](.cursorrules). Key points:

- **C# 12** features where appropriate
- **Nullable reference types** enabled
- **Async/await** throughout (no `.Result` or `.Wait()`)
- **XML documentation** for all public APIs
- **85% minimum test coverage**
- **SOLID principles**
- **Dependency inversion** (Core has zero dependencies on integrations)

## Testing

- Write tests for all new features
- Maintain >85% code coverage
- Use xUnit for unit tests
- Use Moq for mocking
- Use FluentAssertions for assertions
- Test both success and failure scenarios
- Test async/await patterns
- Test cancellation scenarios

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test project
dotnet test tests/DotNetAgents.Core.Tests/
```

## Documentation

- **XML comments** for all public APIs
- **README updates** for new features
- **Code examples** in documentation
- **Architecture docs** for significant changes

## Commit Message Format

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples

```
feat(core): add execution context support

Add ExecutionContext class for correlation IDs and context propagation
through chains and workflows.

Closes #123
```

```
fix(providers): handle OpenAI rate limiting correctly

Fix retry logic to properly respect rate limit headers from OpenAI API.

Fixes #456
```

## Code Review Process

1. **Automated checks** must pass (CI, tests, code analysis)
2. **At least one approval** required
3. **Address review comments** promptly
4. **Maintainers will merge** when ready

## Questions?

- **GitHub Issues**: For bug reports and feature requests
- **GitHub Discussions**: For questions and general discussion
- **Pull Requests**: For code contributions

## Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes
- Project documentation

Thank you for contributing to DotNetAgents! 🎉
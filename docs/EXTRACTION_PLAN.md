# Extract AiSessionPersistence Features to DotNetAgents

## Overview

Extract valuable components from the AiSessionPersistence project and adapt them to enhance DotNetAgents as library packages. The extracted features will follow DotNetAgents patterns (interface-based, .NET 10, dependency injection) and integrate seamlessly with existing components.

**Strategy**: Extract → Adapt → Integrate → Leave AiSessionPersistence unchanged for now (will refactor later to use DotNetAgents NuGet packages)

## Components to Extract

### 1. Task Management → `DotNetAgents.Tasks`
**Source**: `AiSessionPersistence.Core.Entities.Task`, `ITaskRepository`, task services

**Adaptations**:
- Convert from service pattern to library pattern
- Create `ITaskManager` interface (not repository pattern)
- Use records for task entities (DotNetAgents convention)
- Integrate with workflow execution context
- Support task creation/updates from workflow nodes

**Key Features**:
- Task CRUD operations
- Dependency tracking (`DependsOn`, `BlockedBy`)
- Task status management (Pending, InProgress, Completed, etc.)
- Task statistics and reporting
- Task ordering and reordering

**Integration Points**:
- Workflow nodes can create/update tasks
- Tasks can be linked to workflow runs
- Task completion can trigger workflow transitions

### 2. Knowledge Repository → `DotNetAgents.Knowledge`
**Source**: `AiSessionPersistence.Core.Entities.Lesson`, `ILessonRepository`, lesson services

**Adaptations**:
- Rename "Lesson" to "Knowledge" (more generic)
- Create `IKnowledgeRepository` interface
- Support both project-specific and global knowledge
- Integrate with error handling and tool execution

**Key Features**:
- Knowledge capture (successes, failures, patterns)
- Content hash for duplicate detection
- Relevance matching (tech stack, tags)
- Reference counting
- Search and query capabilities
- Export/import functionality

**Integration Points**:
- Error handlers can capture failures as knowledge
- Tool execution results can be stored as knowledge
- Agents can query knowledge before making decisions
- Workflow failures can be captured automatically

### 3. Enhanced Session Management → Enhance `DotNetAgents.Workflow`
**Source**: `SessionContext`, `SessionSnapshot`, `Project` concepts

**Adaptations**:
- Enhance `ICheckpointStore` with session context
- Add `ISessionManager` interface
- Create session snapshots with full context
- Support milestone tracking

**Key Features**:
- Session context tracking (recent files, commits, decisions)
- Session snapshots with full state
- Milestone tracking and completion
- Bootstrap/resume functionality

**Integration Points**:
- Workflow executor can capture session context
- Checkpoints can include session context
- Bootstrap engine can resume workflows with full context

### 4. Bootstrap Engine → `DotNetAgents.Workflow` (new component)
**Source**: `IBootstrapService`, bootstrap payload generation

**Adaptations**:
- Create `IBootstrapGenerator` interface
- Support multiple output formats (JSON, markdown)
- Integrate with workflow state and session context

**Key Features**:
- Generate bootstrap payloads for workflow resumption
- Include tasks, knowledge, session context
- Multiple output formats
- Performance optimized (caching)

## Implementation Plan

### Phase 1: Setup & Foundation (Week 1)

#### 1.1 Create New Projects
- [ ] Create `DotNetAgents.Tasks` project
  - Target: `net10.0`
  - References: `DotNetAgents.Core`
  - Follow DotNetAgents project structure
- [ ] Create `DotNetAgents.Knowledge` project
  - Target: `net10.0`
  - References: `DotNetAgents.Core`
- [ ] Create `DotNetAgents.Tasks.Tests` project
- [ ] Create `DotNetAgents.Knowledge.Tests` project

#### 1.2 Extract Core Models
- [ ] Extract and adapt `Task` entity → `WorkTask` record
  - Convert to record type
  - Use nullable reference types
  - Add XML documentation
  - Remove service-specific fields
- [ ] Extract and adapt `Lesson` entity → `KnowledgeItem` record
  - Rename for clarity
  - Convert to record type
  - Adapt to library pattern
- [ ] Extract `TaskStatus`, `TaskPriority` enums
- [ ] Extract `LessonCategory`, `LessonSeverity` → `KnowledgeCategory`, `KnowledgeSeverity`

### Phase 2: Task Management Package (Week 2)

#### 2.1 Core Interfaces
- [ ] Create `ITaskManager` interface
  - `CreateTaskAsync`, `UpdateTaskAsync`, `DeleteTaskAsync`
  - `GetTaskAsync`, `GetTasksAsync`
  - `GetTaskStatisticsAsync`
  - `ReorderTasksAsync`
  - `CheckDependenciesAsync`
- [ ] Create `ITaskStore` interface (storage abstraction)
  - Similar to `ICheckpointStore` pattern
  - In-memory implementation for testing
  - Database implementations (SQL Server, PostgreSQL) later

#### 2.2 Implementation
- [ ] Implement `TaskManager` class
  - Business logic for task operations
  - Dependency validation
  - Status transitions
- [ ] Implement `InMemoryTaskStore` (for testing)
- [ ] Create `WorkTask` record with all properties
- [ ] Create `TaskStatistics` record

#### 2.3 Integration
- [ ] Create workflow extension methods
  - `CreateTaskAsync` extension for workflow nodes
  - `UpdateTaskAsync` extension
  - `CompleteTaskAsync` extension
- [ ] Add task tracking to `GraphExecutor`
  - Optional task creation on workflow start
  - Task updates on node execution
  - Task completion on workflow end

#### 2.4 Testing
- [ ] Unit tests for `TaskManager`
- [ ] Unit tests for `InMemoryTaskStore`
- [ ] Integration tests with workflows

### Phase 3: Knowledge Repository Package (Week 3)

#### 3.1 Core Interfaces
- [ ] Create `IKnowledgeRepository` interface
  - `AddKnowledgeAsync`, `UpdateKnowledgeAsync`, `DeleteKnowledgeAsync`
  - `GetKnowledgeAsync`, `QueryKnowledgeAsync`
  - `SearchKnowledgeAsync`
  - `GetRelevantKnowledgeAsync`
  - `IncrementReferenceCountAsync`
- [ ] Create `IKnowledgeStore` interface (storage abstraction)

#### 3.2 Implementation
- [ ] Implement `KnowledgeRepository` class
  - Business logic for knowledge operations
  - Duplicate detection via content hash
  - Relevance matching algorithm
- [ ] Implement `InMemoryKnowledgeStore` (for testing)
- [ ] Create `KnowledgeItem` record
- [ ] Create `KnowledgeQuery` record

#### 3.3 Integration
- [ ] Create error handler integration
  - `CaptureFailureAsKnowledgeAsync` extension
  - Automatic knowledge capture on exceptions
- [ ] Create tool execution integration
  - `CaptureToolResultAsKnowledgeAsync` extension
- [ ] Add knowledge query to agent context
  - Agents can query knowledge before decisions

#### 3.4 Testing
- [ ] Unit tests for `KnowledgeRepository`
- [ ] Unit tests for duplicate detection
- [ ] Unit tests for relevance matching
- [ ] Integration tests with error handling

### Phase 4: Enhanced Session Management (Week 4)

#### 4.1 Enhance Workflow Package
- [ ] Create `ISessionManager` interface
  - `CreateSessionAsync`, `GetSessionAsync`
  - `UpdateSessionContextAsync`
  - `CreateSnapshotAsync`
  - `GetSnapshotAsync`
- [ ] Create `SessionContext` record
  - Recent files, commits, decisions
  - Open questions, assumptions
  - Recent commands, errors
- [ ] Create `SessionSnapshot` record
  - Full session state capture
  - Task summaries
  - Knowledge counts

#### 4.2 Enhance Checkpoint Store
- [ ] Extend `ICheckpointStore` with session context
  - Optional session context in checkpoints
  - Session-aware checkpoint retrieval
- [ ] Update `GraphExecutor` to capture session context
  - Track recent files accessed
  - Track decisions made
  - Track errors encountered

#### 4.3 Milestone Support
- [ ] Create `IMilestoneManager` interface
- [ ] Create `Milestone` record
- [ ] Integrate with workflow completion

#### 4.4 Testing
- [ ] Unit tests for session management
- [ ] Unit tests for snapshot creation
- [ ] Integration tests with workflows

### Phase 5: Bootstrap Engine (Week 5)

#### 5.1 Core Interface
- [ ] Create `IBootstrapGenerator` interface
  - `GenerateBootstrapAsync`
  - Support multiple formats (JSON, Markdown)
- [ ] Create `BootstrapPayload` record
  - Session context
  - Tasks
  - Knowledge items
  - Snapshots

#### 5.2 Implementation
- [ ] Implement `BootstrapGenerator` class
  - Format generation logic
  - Content aggregation
  - Performance optimization (caching)
- [ ] Create format generators
  - `JsonBootstrapFormatter`
  - `MarkdownBootstrapFormatter`

#### 5.3 Integration
- [ ] Add bootstrap generation to workflow executor
  - Generate bootstrap on workflow completion
  - Generate bootstrap on checkpoint creation
- [ ] Create extension methods for easy access

#### 5.4 Testing
- [ ] Unit tests for bootstrap generation
- [ ] Unit tests for format generators
- [ ] Integration tests

### Phase 6: Storage Implementations (Week 6)

#### 6.1 SQL Server Storage
- [ ] Create `DotNetAgents.Storage.SqlServer.Tasks` project
  - `SqlServerTaskStore` implementation
- [ ] Create `DotNetAgents.Storage.SqlServer.Knowledge` project
  - `SqlServerKnowledgeStore` implementation
- [ ] Create database migrations
- [ ] Add to `DotNetAgents.Storage.SqlServer` package

#### 6.2 PostgreSQL Storage
- [ ] Create `DotNetAgents.Storage.PostgreSQL.Tasks` project
  - `PostgreSQLTaskStore` implementation
- [ ] Create `DotNetAgents.Storage.PostgreSQL.Knowledge` project
  - `PostgreSQLKnowledgeStore` implementation
- [ ] Create database migrations
- [ ] Add to `DotNetAgents.Storage.PostgreSQL` package

### Phase 7: Documentation & Integration (Week 7)

#### 7.1 Documentation
- [ ] Create README for `DotNetAgents.Tasks`
- [ ] Create README for `DotNetAgents.Knowledge`
- [ ] Update main README with new packages
- [ ] Create integration guide
- [ ] Create migration guide from AiSessionPersistence

#### 7.2 Samples
- [ ] Create sample showing task management in workflows
- [ ] Create sample showing knowledge capture
- [ ] Create sample showing bootstrap generation

#### 7.3 Final Integration
- [ ] Update metapackage to include new packages
- [ ] Update `ServiceCollectionExtensions` for DI
- [ ] Create fluent API extensions
- [ ] Performance testing and optimization

## Code Adaptation Guidelines

### 1. Naming Conventions
- **Entities**: Use records, PascalCase (e.g., `WorkTask`, `KnowledgeItem`)
- **Interfaces**: Prefix with `I` (e.g., `ITaskManager`, `IKnowledgeRepository`)
- **Services**: Remove "Service" suffix, use manager pattern (e.g., `TaskManager` not `TaskService`)
- **Repositories**: Use "Store" suffix (e.g., `ITaskStore` not `ITaskRepository`)

### 2. Architecture Patterns
- **Library Pattern**: No API controllers, no HTTP dependencies
- **Interface-Based**: All public APIs use interfaces
- **Dependency Injection**: Use `IServiceCollection` extensions
- **Async/Await**: All operations async with `CancellationToken`

### 3. .NET 10 Migration
- Update target framework to `net10.0`
- Use C# 13 features where appropriate
- Update package references to .NET 10 compatible versions
- Use nullable reference types throughout

### 4. DotNetAgents Conventions
- Follow existing code style (4 spaces, 120 char limit)
- Use XML documentation for all public APIs
- Follow exception hierarchy (`AgentException` derivatives)
- Use `ExecutionContext` for correlation IDs
- Integrate with `IMetricsCollector` and `ICostTracker`

## File Structure

```
src/
├── DotNetAgents.Tasks/
│   ├── DotNetAgents.Tasks.csproj
│   ├── Models/
│   │   ├── WorkTask.cs
│   │   ├── TaskStatus.cs
│   │   ├── TaskPriority.cs
│   │   └── TaskStatistics.cs
│   ├── Interfaces/
│   │   ├── ITaskManager.cs
│   │   └── ITaskStore.cs
│   ├── Managers/
│   │   └── TaskManager.cs
│   ├── Stores/
│   │   └── InMemoryTaskStore.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Knowledge/
│   ├── DotNetAgents.Knowledge.csproj
│   ├── Models/
│   │   ├── KnowledgeItem.cs
│   │   ├── KnowledgeCategory.cs
│   │   ├── KnowledgeSeverity.cs
│   │   └── KnowledgeQuery.cs
│   ├── Interfaces/
│   │   ├── IKnowledgeRepository.cs
│   │   └── IKnowledgeStore.cs
│   ├── Repositories/
│   │   └── KnowledgeRepository.cs
│   ├── Stores/
│   │   └── InMemoryKnowledgeStore.cs
│   └── ServiceCollectionExtensions.cs
│
└── DotNetAgents.Workflow/ (enhanced)
    ├── Session/
    │   ├── ISessionManager.cs
    │   ├── SessionManager.cs
    │   ├── SessionContext.cs
    │   └── SessionSnapshot.cs
    ├── Bootstrap/
    │   ├── IBootstrapGenerator.cs
    │   ├── BootstrapGenerator.cs
    │   └── BootstrapPayload.cs
    └── Milestones/
        ├── IMilestoneManager.cs
        ├── MilestoneManager.cs
        └── Milestone.cs
```

## Integration Examples

### Task Management in Workflows
```csharp
var workflow = WorkflowBuilder<MyState>.Create()
    .AddNode("create_task", async (state, ct) =>
    {
        var taskManager = serviceProvider.GetRequiredService<ITaskManager>();
        var task = await taskManager.CreateTaskAsync(new WorkTask
        {
            Content = "Process user input",
            Priority = TaskPriority.High
        }, ct);
        state.TaskId = task.Id;
        return state;
    })
    .Build();
```

### Knowledge Capture on Errors
```csharp
try
{
    await tool.ExecuteAsync(input, ct);
}
catch (Exception ex)
{
    var knowledgeRepo = serviceProvider.GetRequiredService<IKnowledgeRepository>();
    await knowledgeRepo.AddKnowledgeAsync(new KnowledgeItem
    {
        Title = "Tool execution failure",
        Description = ex.Message,
        Category = KnowledgeCategory.Error,
        Severity = KnowledgeSeverity.High
    }, ct);
    throw;
}
```

### Bootstrap Generation
```csharp
var bootstrapGenerator = serviceProvider.GetRequiredService<IBootstrapGenerator>();
var bootstrap = await bootstrapGenerator.GenerateBootstrapAsync(
    sessionId: sessionId,
    format: BootstrapFormat.Json,
    cancellationToken: ct);
```

## Testing Strategy

### Unit Tests
- Test all manager/repository logic
- Test storage implementations
- Test integration points
- Target: >85% code coverage

### Integration Tests
- Test with real workflows
- Test with database storage
- Test bootstrap generation
- Test knowledge capture from errors

### Performance Tests
- Bootstrap generation < 500ms
- Task operations < 200ms
- Knowledge queries < 300ms

## Migration Notes

### From AiSessionPersistence
- **Project** → **Session** (more generic)
- **Lesson** → **Knowledge** (more generic)
- **Service** → **Manager** (library pattern)
- **Repository** → **Store** (storage abstraction)
- Remove API-specific code
- Remove MCP-specific code (can add later as separate package)

### To DotNetAgents
- Follow existing package structure
- Use existing storage patterns (`ICheckpointStore` style)
- Integrate with `ExecutionContext`
- Use existing DI patterns
- Follow existing testing patterns

## Success Criteria

1. ✅ All extracted components work as library packages
2. ✅ Seamless integration with existing DotNetAgents components
3. ✅ Full test coverage (>85%)
4. ✅ Documentation complete
5. ✅ Samples demonstrate usage
6. ✅ Performance targets met
7. ✅ Ready for NuGet publication
8. ✅ AiSessionPersistence can later use these packages

## Future: AiSessionPersistence Refactoring

After extraction, AiSessionPersistence can be refactored to:
- Use `DotNetAgents.Tasks` NuGet package
- Use `DotNetAgents.Knowledge` NuGet package
- Use `DotNetAgents.Workflow` enhanced features
- Focus on API/MCP server layer only
- Leverage DotNetAgents for agent execution

This creates a clean separation: DotNetAgents provides the library, AiSessionPersistence provides the service layer.

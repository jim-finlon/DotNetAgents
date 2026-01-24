# DotNetAgents Integration Guide

This guide covers integrating DotNetAgents packages into your application, with a focus on Tasks, Knowledge, and Session Management features.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Task Management](#task-management)
3. [Knowledge Repository](#knowledge-repository)
4. [Session Management](#session-management)
5. [Storage Options](#storage-options)
6. [Workflow Integration](#workflow-integration)
7. [Bootstrap Generation](#bootstrap-generation)

## Quick Start

### Installation

Install the packages you need:

```bash
# Core packages
dotnet add package DotNetAgents.Core
dotnet add package DotNetAgents.Workflow

# Task and Knowledge management
dotnet add package DotNetAgents.Tasks
dotnet add package DotNetAgents.Knowledge

# Storage (choose one or both)
dotnet add package DotNetAgents.Storage.SqlServer
dotnet add package DotNetAgents.Storage.PostgreSQL

# Or install the metapackage (includes everything)
dotnet add package DotNetAgents
```

### Basic Setup

```csharp
using DotNetAgents.Tasks;
using DotNetAgents.Knowledge;
using DotNetAgents.Workflow.Session;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Add logging
services.AddLogging(builder => builder.AddConsole());

// Add Tasks and Knowledge (uses in-memory storage by default)
services.AddDotNetAgentsTasks();
services.AddDotNetAgentsKnowledge();

// Add Session Management (includes bootstrap generator)
services.AddDotNetAgentsWorkflowSession();

var serviceProvider = services.BuildServiceProvider();
```

## Task Management

### Overview

The `DotNetAgents.Tasks` package provides task tracking and management capabilities that integrate seamlessly with workflows. Tasks can be created, updated, and tracked throughout workflow execution.

### Basic Usage

```csharp
using DotNetAgents.Tasks;
using DotNetAgents.Tasks.Models;

var taskManager = serviceProvider.GetRequiredService<ITaskManager>();

// Create a task
var task = await taskManager.CreateTaskAsync(new WorkTask
{
    SessionId = "session-123",
    WorkflowRunId = "run-456",
    Content = "Process user input",
    Priority = TaskPriority.High,
    Status = TaskStatus.Pending
}, cancellationToken);

// Update task status
var updatedTask = await taskManager.UpdateTaskAsync(task with
{
    Status = TaskStatus.InProgress,
    StartedAt = DateTimeOffset.UtcNow
}, cancellationToken);

// Get task statistics
var stats = await taskManager.GetTaskStatisticsAsync("session-123", cancellationToken);
Console.WriteLine($"Completion: {stats.CompletionPercentage}%");
```

### Task Statuses

- `Pending`: Task is created but not started
- `InProgress`: Task is currently being worked on
- `Completed`: Task finished successfully
- `Blocked`: Task is blocked by dependencies
- `Cancelled`: Task was cancelled
- `Review`: Task is pending review

### Task Priorities

- `Low`: Low priority task
- `Medium`: Normal priority (default)
- `High`: High priority task
- `Critical`: Critical priority task

### Dependencies

Tasks can have dependencies on other tasks:

```csharp
var task1 = await taskManager.CreateTaskAsync(new WorkTask
{
    SessionId = "session-123",
    Content = "Task 1",
    Status = TaskStatus.Pending
}, cancellationToken);

var task2 = await taskManager.CreateTaskAsync(new WorkTask
{
    SessionId = "session-123",
    Content = "Task 2",
    Status = TaskStatus.Pending,
    DependsOn = new[] { task1.Id }
}, cancellationToken);

// Check if dependencies are complete
var canStart = await taskManager.AreDependenciesCompleteAsync(task2.Id, cancellationToken);
```

## Knowledge Repository

### Overview

The `DotNetAgents.Knowledge` package provides a knowledge repository system that allows agents to capture and learn from successes, failures, and discoveries during execution.

### Basic Usage

```csharp
using DotNetAgents.Knowledge;
using DotNetAgents.Knowledge.Models;

var knowledgeRepo = serviceProvider.GetRequiredService<IKnowledgeRepository>();

// Add knowledge
var knowledge = await knowledgeRepo.AddKnowledgeAsync(new KnowledgeItem
{
    SessionId = "session-123",
    Title = "Database connection pooling issue",
    Description = "Connection pool exhausted under load",
    Solution = "Increased max pool size to 200",
    Category = KnowledgeCategory.Solution,
    Severity = KnowledgeSeverity.Warning,
    Tags = new[] { "database", "performance" },
    TechStack = new[] { "dotnet", "sql-server" }
}, cancellationToken);

// Query knowledge
var query = new KnowledgeQuery
{
    SessionId = "session-123",
    Category = KnowledgeCategory.Solution,
    Page = 1,
    PageSize = 10
};
var results = await knowledgeRepo.QueryKnowledgeAsync(query, cancellationToken);

// Search knowledge
var searchResults = await knowledgeRepo.SearchKnowledgeAsync(
    searchText: "connection pool",
    sessionId: "session-123",
    includeGlobal: true,
    cancellationToken);

// Get relevant knowledge
var relevantKnowledge = await knowledgeRepo.GetRelevantKnowledgeAsync(
    techStackTags: new[] { "dotnet", "sql-server" },
    projectTags: new[] { "database" },
    maxResults: 5,
    cancellationToken);
```

### Knowledge Categories

- `Error`: Error or failure encountered
- `Solution`: Solution to a problem
- `BestPractice`: Best practice or pattern
- `Tip`: Helpful tip or trick
- `Warning`: Warning or caution
- `Info`: General information

### Knowledge Severity

- `Info`: Informational
- `Warning`: Warning level
- `Error`: Error level
- `Critical`: Critical level

### Error Handling Integration

Knowledge can be automatically captured from exceptions:

```csharp
try
{
    await tool.ExecuteAsync(input, cancellationToken);
}
catch (Exception ex)
{
    await knowledgeRepo.AddKnowledgeAsync(new KnowledgeItem
    {
        SessionId = sessionId,
        Title = "Tool execution failure",
        Description = ex.Message,
        ErrorMessage = ex.Message,
        StackTrace = ex.StackTrace,
        Category = KnowledgeCategory.Error,
        Severity = KnowledgeSeverity.Error,
        ToolName = tool.Name
    }, cancellationToken);
    throw;
}
```

## Session Management

### Overview

Session management provides context tracking, snapshots, and milestone management for workflows.

### Basic Usage

```csharp
using DotNetAgents.Workflow.Session;

var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();

// Create a session
var session = await sessionManager.CreateSessionAsync(new SessionContext
{
    SessionId = "session-123",
    SessionName = "My Workflow Session",
    RecentFiles = new[] { "file1.cs", "file2.cs" },
    RecentCommands = new[] { "dotnet build", "dotnet test" }
}, cancellationToken);

// Update session context
await sessionManager.UpdateSessionContextAsync("session-123", context =>
{
    context.RecentFiles.Add("file3.cs");
    return context;
}, cancellationToken);

// Create a snapshot
var snapshot = await sessionManager.CreateSnapshotAsync(new WorkflowSnapshot
{
    SessionId = "session-123",
    WorkflowRunId = "run-456",
    Trigger = SnapshotTrigger.Manual,
    State = workflowState
}, cancellationToken);
```

## Storage Options

### In-Memory Storage (Default)

By default, Tasks and Knowledge use in-memory storage, which is suitable for development and testing:

```csharp
services.AddDotNetAgentsTasks(); // Uses InMemoryTaskStore
services.AddDotNetAgentsKnowledge(); // Uses InMemoryKnowledgeStore
```

### SQL Server Storage

```csharp
using DotNetAgents.Storage.SqlServer;

services.AddSqlServerTaskStore(connectionString, tableName: "WorkTasks");
services.AddSqlServerKnowledgeStore(connectionString, tableName: "KnowledgeItems");
```

### PostgreSQL Storage

```csharp
using DotNetAgents.Storage.PostgreSQL;

services.AddPostgreSQLTaskStore(connectionString, tableName: "work_tasks");
services.AddPostgreSQLKnowledgeStore(connectionString, tableName: "knowledge_items");
```

### Custom Storage

You can provide custom storage implementations:

```csharp
services.AddDotNetAgentsTasks(services =>
{
    services.AddSingleton<ITaskStore, MyCustomTaskStore>();
});

services.AddDotNetAgentsKnowledge(services =>
{
    services.AddSingleton<IKnowledgeStore, MyCustomKnowledgeStore>();
});
```

## Workflow Integration

### Creating Tasks in Workflows

Tasks can be created and updated directly from workflow nodes:

```csharp
var workflow = WorkflowBuilder<MyState>.Create()
    .AddNode("create_task", async (state, ct) =>
    {
        var taskManager = serviceProvider.GetRequiredService<ITaskManager>();
        var task = await taskManager.CreateTaskAsync(new WorkTask
        {
            SessionId = state.SessionId,
            WorkflowRunId = state.RunId,
            Content = "Process user input",
            Priority = TaskPriority.High
        }, ct);
        return state with { TaskId = task.Id };
    })
    .AddNode("process", async (state, ct) =>
    {
        // Update task status
        if (state.TaskId.HasValue)
        {
            var task = await taskManager.GetTaskAsync(state.TaskId.Value, ct);
            if (task != null)
            {
                await taskManager.UpdateTaskAsync(task with
                {
                    Status = TaskStatus.InProgress,
                    StartedAt = DateTimeOffset.UtcNow
                }, ct);
            }
        }
        
        // Process...
        return state;
    })
    .Build();
```

### Capturing Knowledge in Workflows

Knowledge can be captured from workflow nodes:

```csharp
.AddNode("capture_success", async (state, ct) =>
{
    var knowledgeRepo = serviceProvider.GetRequiredService<IKnowledgeRepository>();
    await knowledgeRepo.AddKnowledgeAsync(new KnowledgeItem
    {
        SessionId = state.SessionId,
        WorkflowRunId = state.RunId,
        Title = "Successful processing pattern",
        Description = $"Successfully processed: {state.Input}",
        Category = KnowledgeCategory.Solution,
        Severity = KnowledgeSeverity.Info
    }, ct);
    return state;
})
```

## Human-in-the-Loop Support

### Overview

DotNetAgents provides built-in support for human approval workflows through the `ApprovalNode<TState>` class. This allows workflows to pause and wait for human approval before proceeding.

### Basic Usage

```csharp
using DotNetAgents.Workflow.HumanInLoop;

// Create an approval handler (in-memory for example)
var approvalHandler = new InMemoryApprovalHandler<MyState>(logger);

// Create a workflow with an approval node
var workflow = WorkflowBuilder<MyState>.Create()
    .AddNode("process", async (state, ct) =>
    {
        // Process data...
        return state with { Processed = true };
    })
    .AddNode("approval", new ApprovalNode<MyState>(
        name: "approval",
        approvalHandler: approvalHandler,
        approvalMessage: "Please review the processed data before continuing.",
        timeout: TimeSpan.FromMinutes(30)))
    .AddNode("finalize", async (state, ct) =>
    {
        // Finalize after approval
        return state;
    })
    .AddEdge("process", "approval")
    .AddEdge("approval", "finalize")
    .SetEntryPoint("process")
    .SetExitPoint("finalize")
    .Build();
```

### Approval Handlers

#### In-Memory Approval Handler

For development and testing:

```csharp
var approvalHandler = new InMemoryApprovalHandler<MyState>(logger);

// Manually approve a pending request
await approvalHandler.ApproveAsync(
    workflowRunId: "run-123",
    nodeName: "approval",
    approved: true);
```

#### Custom Approval Handler

Implement `IApprovalHandler<TState>` for custom approval workflows:

```csharp
public class MyApprovalHandler<TState> : IApprovalHandler<TState> where TState : class
{
    public Task<bool> RequestApprovalAsync(
        string workflowRunId,
        string nodeName,
        TState state,
        string? message,
        CancellationToken cancellationToken = default)
    {
        // Send approval request (email, SignalR, webhook, etc.)
        // Return true if immediately approved, false if pending
        return Task.FromResult(false);
    }

    public Task<bool> IsApprovedAsync(
        string workflowRunId,
        string nodeName,
        CancellationToken cancellationToken = default)
    {
        // Check if approval has been granted
        return Task.FromResult(false);
    }
}
```

### Approval Workflow Pattern

```csharp
// 1. Workflow reaches approval node
// 2. ApprovalNode calls RequestApprovalAsync
// 3. If not immediately approved, workflow pauses
// 4. Approval handler polls IsApprovedAsync
// 5. Once approved, workflow continues
// 6. If rejected or timeout, workflow throws AgentException
```

### Integration with Web Applications

For web applications, you can create a SignalR-based approval handler:

```csharp
public class SignalRApprovalHandler<TState> : IApprovalHandler<TState> where TState : class
{
    private readonly IHubContext<ApprovalHub> _hubContext;

    public async Task<bool> RequestApprovalAsync(
        string workflowRunId,
        string nodeName,
        TState state,
        string? message,
        CancellationToken cancellationToken = default)
    {
        // Send approval request via SignalR
        await _hubContext.Clients.All.SendAsync(
            "ApprovalRequested",
            workflowRunId,
            nodeName,
            message,
            cancellationToken);

        return false; // Pending approval
    }

    public Task<bool> IsApprovedAsync(
        string workflowRunId,
        string nodeName,
        CancellationToken cancellationToken = default)
    {
        // Check approval status from database/cache
        return Task.FromResult(false);
    }
}
```

### Best Practices

1. **Set Timeouts**: Always set reasonable timeouts for approval nodes to prevent workflows from hanging indefinitely
2. **Clear Messages**: Provide clear approval messages explaining what needs approval
3. **State Inspection**: Approval handlers can inspect workflow state to make informed decisions
4. **Error Handling**: Handle approval rejections and timeouts gracefully in your workflow
5. **Persistence**: Use persistent approval handlers (database-backed) for production workloads

## Bootstrap Generation

### Overview

Bootstrap generation creates payloads that can be used to resume workflows with full context, including tasks, knowledge, and session state.

### Basic Usage

```csharp
using DotNetAgents.Workflow.Session.Bootstrap;

var bootstrapGenerator = serviceProvider.GetRequiredService<IBootstrapGenerator>();

// Prepare bootstrap data
var tasks = await taskManager.GetTasksBySessionAsync(sessionId, cancellationToken);
var stats = await taskManager.GetTaskStatisticsAsync(sessionId, cancellationToken);
var knowledge = await knowledgeRepo.QueryKnowledgeAsync(new KnowledgeQuery
{
    SessionId = sessionId,
    Page = 1,
    PageSize = 100
}, cancellationToken);

var bootstrapData = new BootstrapData
{
    SessionId = sessionId,
    WorkflowRunId = workflowRunId,
    ResumePoint = "Workflow paused for review",
    SessionName = "My Workflow Session",
    TaskSummary = new TaskSummaryData
    {
        Total = stats.Total,
        Pending = stats.Pending,
        InProgress = stats.InProgress,
        Completed = stats.Completed,
        Blocked = stats.Blocked,
        CompletionPercentage = stats.CompletionPercentage,
        Tasks = tasks.Select(t => new TaskItemData
        {
            Id = t.Id,
            Content = t.Content,
            Status = t.Status.ToString(),
            Priority = t.Priority.ToString(),
            Order = t.Order,
            Tags = t.Tags.ToList(),
            CompletedAt = t.CompletedAt
        }).ToList()
    },
    KnowledgeItems = knowledge.Items.Select(k => new KnowledgeItemData
    {
        Id = k.Id,
        Title = k.Title,
        Description = k.Description,
        Category = k.Category.ToString(),
        Severity = k.Severity.ToString(),
        Solution = k.Solution,
        Tags = k.Tags.ToList(),
        ReferenceCount = k.ReferenceCount
    }).ToList()
};

// Generate bootstrap payload
var bootstrap = await bootstrapGenerator.GenerateAsync(
    bootstrapData,
    BootstrapFormat.Json,
    cancellationToken);

Console.WriteLine(bootstrap.FormattedContent);
```

### Bootstrap Formats

- `Json`: JSON format (default)
- `CursorRules`: Cursor IDE rules format
- `Agent`: Agent framework format

## Best Practices

1. **Use Session IDs**: Always associate tasks and knowledge with session IDs for proper isolation
2. **Update Task Status**: Update task status as workflows progress (Pending → InProgress → Completed)
3. **Capture Knowledge**: Capture both successes and failures as knowledge for future reference
4. **Use Tags**: Tag knowledge items with relevant keywords for better searchability
5. **Track Tech Stack**: Include tech stack information in knowledge items for relevance matching
6. **Generate Bootstraps**: Generate bootstrap payloads at key workflow checkpoints for resumption
7. **Use Database Storage**: Use database storage (SQL Server or PostgreSQL) for production workloads

## Examples

See the `DotNetAgents.Samples.TasksAndKnowledge` sample for a complete working example demonstrating:
- Task creation and tracking
- Knowledge capture from successes and errors
- Task statistics
- Knowledge querying
- Bootstrap generation

## Additional Resources

- [Tasks Package README](../src/DotNetAgents.Tasks/README.md)
- [Knowledge Package README](../src/DotNetAgents.Knowledge/README.md)
- [Workflow Documentation](PROJECT_STATUS.md)
- [Samples](../samples/README.md)

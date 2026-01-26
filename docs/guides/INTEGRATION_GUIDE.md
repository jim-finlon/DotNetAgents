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
8. [Visual Workflow Designer](#visual-workflow-designer)
9. [AI-Powered Development Tools](#ai-powered-development-tools)
10. [Advanced Multi-Agent Patterns](#advanced-multi-agent-patterns)
11. [Edge Computing](#edge-computing)
12. [Observability & Tracing](#observability--tracing)
13. [Resilience Patterns](#resilience-patterns)
14. [Plugin Architecture](#plugin-architecture)

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

## Autonomous Agent Capabilities

DotNetAgents provides comprehensive support for autonomous agents through State Machines and Behavior Trees.

### State Machines

State machines manage agent lifecycle and operational states:

```csharp
using DotNetAgents.Agents.StateMachines;
using DotNetAgents.Agents.Registry;

// Create a worker pool state machine pattern
var stateMachine = StateMachinePatterns.CreateWorkerPoolPattern<MyContext>(
    logger,
    cooldownDuration: TimeSpan.FromSeconds(5));

// Register with agent registry
var stateMachineRegistry = new AgentStateMachineRegistry<MyContext>(agentRegistry, logger);
await stateMachineRegistry.RegisterAsync("agent-1", stateMachine);

// Transition states
await stateMachine.TransitionAsync("Busy", context);
```

**Key Features:**
- Hierarchical states (nested states)
- Parallel states (orthogonal regions)
- Timed and scheduled transitions
- State persistence and history
- Common patterns (Idle-Working, Error-Recovery, Worker Pool, Supervisor)

See [State Machines README](../../src/DotNetAgents.Agents.StateMachines/README.md) for detailed documentation.

### Behavior Trees

Behavior trees enable hierarchical decision-making for autonomous agents:

```csharp
using DotNetAgents.Agents.BehaviorTrees;

// Create a behavior tree
var sequence = new SequenceNode<MyContext>("ProcessSequence", logger)
    .AddChild(new ConditionNode<MyContext>("CheckCondition", ctx => ctx.IsValid, logger))
    .AddChild(new ActionNode<MyContext>("PerformAction", async (ctx, ct) => {
        // Action logic
        return BehaviorTreeNodeStatus.Success;
    }, logger));

var tree = new BehaviorTree<MyContext>("MyTree", sequence);
var executor = new BehaviorTreeExecutor<MyContext>(logger);
var result = await executor.ExecuteAsync(tree, context);
```

**Key Features:**
- Leaf nodes (Action, Condition)
- Composite nodes (Sequence, Selector, Parallel)
- Decorator nodes (Retry, Timeout, Cooldown, Conditional)
- LLM integration nodes
- Workflow integration nodes
- State machine integration nodes

See [Behavior Trees README](../../src/DotNetAgents.Agents.BehaviorTrees/README.md) for detailed documentation.

## Multi-Agent Messaging

DotNetAgents provides comprehensive multi-agent communication through message buses with 5 implementations for different deployment scenarios.

### Message Bus Implementations

#### In-Memory (Development/Testing)
```csharp
using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.Registry;

services.AddInMemoryAgentRegistry();
services.AddInMemoryAgentMessageBus();
```

**Use Cases:** Single-instance deployments, development, testing

#### Kafka (High-Throughput Production)
```csharp
using DotNetAgents.Agents.Messaging.Kafka;

services.AddKafkaAgentMessageBus(options =>
{
    options.BootstrapServers = "localhost:9092";
    options.TopicPrefix = "agents";
    options.ConsumerGroupId = "agent-workers";
});
```

**Use Cases:** High-throughput distributed systems, event streaming, microservices

#### RabbitMQ (Guaranteed Delivery)
```csharp
using DotNetAgents.Agents.Messaging.RabbitMQ;

services.AddRabbitMQAgentMessageBus(options =>
{
    options.HostName = "localhost";
    options.ExchangeName = "agent-messages";
    options.QueuePrefix = "agents";
});
```

**Use Cases:** Systems requiring guaranteed delivery, complex routing, request-response patterns

#### Redis (Real-Time Pub/Sub)
```csharp
using DotNetAgents.Agents.Messaging.Redis;

services.AddRedisAgentMessageBus(options =>
{
    options.ConnectionString = "localhost:6379";
    options.ChannelPrefix = "agents:";
});
```

**Use Cases:** Real-time pub/sub, when Redis infrastructure exists, ephemeral messaging

#### SignalR (Web-Based Real-Time)
```csharp
using DotNetAgents.Agents.Messaging.SignalR;

services.AddSignalRAgentMessageBus(options =>
{
    options.HubUrl = "https://your-server.com/hubs/agentmessages";
    options.ReconnectDelay = TimeSpan.FromSeconds(5);
});
```

**Use Cases:** Web-based applications, browser-to-server real-time communication

### Basic Usage

```csharp
using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Supervisor;

// Get services
var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();
var agentRegistry = serviceProvider.GetRequiredService<IAgentRegistry>();
var supervisor = serviceProvider.GetRequiredService<ISupervisorAgent>();

// Send a message to a specific agent
var message = new AgentMessage
{
    FromAgentId = "supervisor-1",
    ToAgentId = "worker-1",
    MessageType = "task_assignment",
    Payload = new { TaskId = "task-123", Data = "..." }
};

var result = await messageBus.SendAsync(message);

// Subscribe to messages
var subscription = await messageBus.SubscribeAsync("worker-1", async (msg, ct) =>
{
    Console.WriteLine($"Received: {msg.MessageType}");
    // Handle message
});

// Broadcast to all agents
var broadcast = new AgentMessage
{
    FromAgentId = "supervisor-1",
    ToAgentId = "*", // Broadcast
    MessageType = "system_update",
    Payload = new { Message = "System maintenance in 5 minutes" }
};

await messageBus.BroadcastAsync(broadcast);
```

### Supervisor-Worker Pattern

```csharp
using DotNetAgents.Agents.Supervisor;
using DotNetAgents.Agents.Tasks;

// Submit tasks to workers
var task = new WorkerTask
{
    TaskType = "analyze_document",
    Input = documentData,
    RequiredCapability = "document_analysis"
};

var taskId = await supervisor.SubmitTaskAsync(task);

// Get task result
var result = await supervisor.GetTaskResultAsync(taskId);
if (result?.Success == true)
{
    Console.WriteLine($"Task completed: {result.Output}");
}
```

### Multi-Agent Workflows

```csharp
using DotNetAgents.Workflow.MultiAgent;

// Create workflow with multi-agent nodes
var workflow = WorkflowBuilder<MultiAgentWorkflowState>.Create()
    .AddNode("delegate-tasks", new DelegateToWorkerNode<MultiAgentWorkflowState>(
        supervisor,
        state => CreateTasksFromState(state)))
    .AddNode("aggregate-results", new AggregateResultsNode<MultiAgentWorkflowState>(
        supervisor,
        AggregateResults,
        waitForAllTasks: true))
    .SetEntryPoint("delegate-tasks")
    .AddEdge("delegate-tasks", "aggregate-results")
    .Build();
```

See [Multi-Agent Workflows Plan](../architecture/MULTI_AGENT_WORKFLOWS_PLAN.md) for detailed documentation.

## Visual Workflow Designer

### Overview

The Visual Workflow Designer provides a beautiful, drag-and-drop interface for creating and managing workflows.

### Installation

```bash
dotnet add package DotNetAgents.Workflow.Designer
dotnet add package DotNetAgents.Workflow.Designer.Web
```

### Using the Visual Designer

**Web UI:**
```bash
cd src/DotNetAgents.Workflow.Designer.Web
dotnet run
```

Navigate to `https://localhost:5001` to access the visual designer.

**Backend API:**
```csharp
using DotNetAgents.Workflow.Designer;

services.AddScoped<IWorkflowDesignerService, WorkflowDesignerService>();

var designer = serviceProvider.GetRequiredService<IWorkflowDesignerService>();

// Save workflow
await designer.SaveWorkflowAsync(workflow);

// Validate workflow
var result = await designer.ValidateWorkflowAsync(workflow);

// Execute workflow
var executionId = await designer.ExecuteWorkflowAsync(workflow.Name);
```

See [Visual Workflow Designer Guide](./VISUAL_WORKFLOW_DESIGNER.md) for complete documentation.

## AI-Powered Development Tools

### Overview

AI-powered tools for generating chains, building workflows, and debugging issues.

### Installation

```bash
dotnet add package DotNetAgents.Tools.Development
```

### Chain Generator

Generate chain code from natural language:

```csharp
using DotNetAgents.Tools.Development;

var generator = new ChainGenerator(llm);
var result = await generator.GenerateAsync(
    "Create a chain that processes user input and generates a response");

Console.WriteLine(result.Code);
Console.WriteLine(result.Explanation);
```

### Workflow Builder

Build workflows from natural language:

```csharp
var builder = new WorkflowBuilder(llm);
var workflow = await builder.BuildAsync(
    "Create a workflow for document processing with validation");
```

### Debugging Assistant

Analyze execution logs and suggest fixes:

```csharp
var assistant = new DebuggingAssistant(llm);
var analysis = await assistant.AnalyzeAsync(executionLog);

foreach (var issue in analysis.Issues)
{
    Console.WriteLine($"Issue: {issue.Description}");
    Console.WriteLine($"Fix: {issue.SuggestedFix}");
}
```

See [AI-Powered Tools Guide](./AI_POWERED_TOOLS.md) for complete documentation.

## Advanced Multi-Agent Patterns

### Swarm Intelligence

Coordinate agents using swarm algorithms:

```csharp
using DotNetAgents.Agents.Swarm;

var swarm = new SwarmCoordinator("swarm-1", registry, workerPool);

// Add agents to swarm
await swarm.AddAgentAsync("agent-1");
await swarm.AddAgentAsync("agent-2");

// Distribute task using particle swarm optimization
var distribution = await swarm.DistributeTaskAsync(
    task,
    SwarmCoordinationStrategy.ParticleSwarm);
```

### Hierarchical Organizations

Organize agents into teams and departments:

```csharp
using DotNetAgents.Agents.Hierarchical;

var org = new HierarchicalAgentOrganization(registry);

// Create organization structure
var engineering = await org.CreateNodeAsync("Engineering");
var backendTeam = await org.CreateNodeAsync("Backend Team", engineering.Id);

// Add agents
await org.AddAgentToNodeAsync(backendTeam.Id, "agent-1", role: "Senior Developer");
```

### Agent Marketplace

Discover and share agents:

```csharp
using DotNetAgents.Agents.Marketplace;

var marketplace = new InMemoryAgentMarketplace(registry);

// Publish agent
var listing = new AgentListing { /* ... */ };
await marketplace.PublishAgentAsync(listing);

// Search agents
var results = await marketplace.SearchAgentsAsync("document analyzer");
```

See [Advanced Multi-Agent Patterns Guide](./ADVANCED_MULTI_AGENT_PATTERNS.md) for complete documentation.

## Edge Computing

### Overview

Edge computing support for mobile and offline deployments.

### Installation

```bash
dotnet add package DotNetAgents.Edge
```

### Basic Usage

```csharp
using DotNetAgents.Edge;

services.AddDotNetAgentsEdge(config =>
{
    config.ModelType = EdgeModelType.Quantized;
    config.MaxModelSizeMB = 100;
});

var edgeAgent = serviceProvider.GetRequiredService<IEdgeAgent>();

// Execute with automatic offline fallback
var result = await edgeAgent.ExecuteAsync(input);
```

See [Edge Computing Guide](./EDGE_COMPUTING.md) for complete documentation.

## Observability & Tracing

### Distributed Tracing

Set up OpenTelemetry tracing:

```csharp
using OpenTelemetry.Trace;

services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("MyAgentService", serviceVersion: "1.0.0"))
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddHttpClientInstrumentation()
               .AddConsoleExporter();
    });
```

### Prometheus Metrics

Metrics are automatically collected. Configure Prometheus to scrape:

```yaml
scrape_configs:
  - job_name: 'dotnetagents'
    static_configs:
      - targets: ['localhost:9090']
```

### Grafana Dashboards

Import dashboards from `kubernetes/grafana/dashboards/`:
- `dotnetagents-overview.json`
- `dotnetagents-agents.json`
- `dotnetagents-llm.json`

See [Distributed Tracing Examples](../examples/DISTRIBUTED_TRACING.md) and [Grafana Dashboards Guide](./GRAFANA_DASHBOARDS.md) for complete documentation.

## Resilience Patterns

### Circuit Breakers

```csharp
using DotNetAgents.Abstractions.Resilience;

var circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions
{
    FailureThreshold = 5,
    SuccessThreshold = 2,
    Timeout = TimeSpan.FromSeconds(30)
});

try
{
    await circuitBreaker.ExecuteAsync(async () =>
    {
        return await llm.GenerateAsync(messages);
    });
}
catch (CircuitBreakerOpenException)
{
    // Circuit is open, use fallback
}
```

### Graceful Degradation

```csharp
// LLM fallback
try
{
    return await primaryLLM.GenerateAsync(messages);
}
catch
{
    return await fallbackLLM.GenerateAsync(messages);
}

// Database fallback to cache
try
{
    return await database.GetAsync(key);
}
catch
{
    return await cache.GetAsync(key);
}
```

See [Circuit Breakers Guide](./CIRCUIT_BREAKERS.md) and [Graceful Degradation Guide](./GRACEFUL_DEGRADATION.md) for complete documentation.

## Plugin Architecture

### Overview

Extensible plugin system for third-party integrations.

### Installation

```bash
dotnet add package DotNetAgents.Ecosystem
```

### Creating a Plugin

```csharp
using DotNetAgents.Ecosystem;

public class MyPlugin : IPlugin
{
    public string Id => "my-plugin";
    public string Name => "My Plugin";
    public string Version => "1.0.0";
    // ... other properties

    public async Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        // Initialize plugin
    }

    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        // Cleanup
        return Task.CompletedTask;
    }
}
```

### Registering Plugins

```csharp
services.AddDotNetAgentsEcosystem();

var registry = serviceProvider.GetRequiredService<IPluginRegistry>();
await registry.RegisterAsync(new MyPlugin());
```

### Integration Marketplace

```csharp
var marketplace = serviceProvider.GetRequiredService<IIntegrationMarketplace>();

// Publish integration
var integration = new IntegrationListing { /* ... */ };
await marketplace.PublishAsync(integration);

// Search integrations
var results = await marketplace.SearchAsync("custom tool");
```

See [Ecosystem Integrations Guide](./ECOSYSTEM_INTEGRATIONS.md) for complete documentation.

## Examples

See the sample applications for complete working examples:

- **`DotNetAgents.Samples.BasicChain`**: Basic chain composition
- **`DotNetAgents.Samples.AgentWithTools`**: Agent with tool calling
- **`DotNetAgents.Samples.Workflow`**: Stateful workflows
- **`DotNetAgents.Samples.RAG`**: Retrieval-augmented generation
- **`DotNetAgents.Samples.Education`**: Educational extensions
- **`DotNetAgents.Samples.TasksAndKnowledge`**: Task creation, knowledge capture, bootstrap generation
- **`DotNetAgents.Samples.MultiAgent`**: Supervisor-worker patterns, agent registry, worker pool, message buses
- **`DotNetAgents.Samples.StateMachines`**: State machine patterns, registry integration, message bus integration
- **`DotNetAgents.Samples.Tracing`**: Distributed tracing with OpenTelemetry
- **`DotNetAgents.Samples.JARVISVoice`**: Voice command processing
- **`DotNetAgents.Workflow.Designer.Web`**: Visual workflow designer UI

## Additional Resources

### Core Packages
- [Tasks Package README](../../src/DotNetAgents.Tasks/README.md)
- [Knowledge Package README](../../src/DotNetAgents.Knowledge/README.md)
- [State Machines README](../../src/DotNetAgents.Agents.StateMachines/README.md)
- [Behavior Trees README](../../src/DotNetAgents.Agents.BehaviorTrees/README.md)

### New Features
- [Visual Workflow Designer Guide](./VISUAL_WORKFLOW_DESIGNER.md)
- [AI-Powered Tools Guide](./AI_POWERED_TOOLS.md)
- [Advanced Multi-Agent Patterns Guide](./ADVANCED_MULTI_AGENT_PATTERNS.md)
- [Edge Computing Guide](./EDGE_COMPUTING.md)
- [Ecosystem Integrations Guide](./ECOSYSTEM_INTEGRATIONS.md)
- [Distributed Tracing Examples](../examples/DISTRIBUTED_TRACING.md)

### Production & Operations
- [Alerting Guide](./ALERTING.md)
- [Grafana Dashboards Guide](./GRAFANA_DASHBOARDS.md)
- [Load Testing Guide](./LOAD_TESTING.md)
- [Chaos Engineering Guide](./CHAOS_ENGINEERING.md)
- [Circuit Breakers Guide](./CIRCUIT_BREAKERS.md)
- [Graceful Degradation Guide](./GRACEFUL_DEGRADATION.md)
- [Disaster Recovery](../operations/DISASTER_RECOVERY.md)
- [Operations Runbook](../operations/RUNBOOK.md)

### Architecture & Deployment
- [Multi-Agent Workflows Plan](../architecture/MULTI_AGENT_WORKFLOWS_PLAN.md)
- [Kubernetes Deployment](../../kubernetes/README.md)
- [Workflow Documentation](../status/PROJECT_STATUS.md)
- [Samples](../../samples/README.md)

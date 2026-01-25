# Multi-Agent Workflows & Supervisor-Worker Pattern - Implementation Plan

**Date:** January 2025  
**Status:** ✅ Core Features Complete  
**Target:** DotNetAgents v2.0  
**Last Updated:** January 2025

> **Note:** Core multi-agent infrastructure is complete. See [PROJECT_STATUS.md](../status/PROJECT_STATUS.md) for current status.

## Implementation Status

### ✅ Phase 1: Core Infrastructure (COMPLETE)
- ✅ Agent Registry (`DotNetAgents.Agents.Registry`)
- ✅ Agent Messaging Interfaces (`DotNetAgents.Agents.Messaging`)
- ✅ In-Memory Message Bus (`DotNetAgents.Agents.Messaging.InMemory`)
- ✅ Task Queue & Store (`DotNetAgents.Agents.Tasks`)
- ✅ Worker Pool (`DotNetAgents.Agents.WorkerPool`)
- ✅ Supervisor Agent (`DotNetAgents.Agents.Supervisor`)
- ✅ State Machines for Agent Lifecycle (`DotNetAgents.Agents.StateMachines`)
- ✅ Behavior Trees for Autonomous Decision-Making (`DotNetAgents.Agents.BehaviorTrees`)
- ✅ State-Based Worker Pool Selection
- ✅ Message Bus State Machine Integration

### 🔄 Phase 2: Storage & Persistence (PENDING)
- ⏳ PostgreSQL implementations
- ⏳ Database migrations

### 🔄 Phase 3: Distributed Messaging (PARTIALLY COMPLETE)
- ✅ Kafka implementation (`DotNetAgents.Agents.Messaging.Kafka`) - **COMPLETE**
- ✅ SignalR implementation (`DotNetAgents.Agents.Messaging.SignalR`) - **COMPLETE**
- ⏳ RabbitMQ implementation (`DotNetAgents.Agents.Messaging.RabbitMQ`) - **COMPLETE** (implementation exists)
- ⏳ Redis implementation (`DotNetAgents.Agents.Messaging.Redis`) - **COMPLETE** (implementation exists)

## Executive Summary

This document outlines a comprehensive plan to add multi-agent workflow capabilities to DotNetAgents, including supervisor-worker patterns, agent-to-agent communication, and horizontal scalability. The design leverages existing infrastructure (workflows, MCP, SignalR, checkpointing) while introducing new components for agent coordination.

## Current Infrastructure Analysis

### ✅ Existing Components We Can Leverage

1. **Workflow Engine** (`DotNetAgents.Workflow`)
   - `StateGraph<TState>` - Can represent agent coordination workflows
   - `GraphExecutor<TState>` - Can execute multi-agent workflows
   - Checkpointing - State persistence for agent coordination
   - Resume capability - Fault tolerance for agent workflows

2. **Agent Infrastructure** (`DotNetAgents.Core.Agents`)
   - `IAgent` interface - Base agent contract
   - `AgentExecutor` - ReAct pattern execution
   - Tool calling capabilities

3. **MCP Client** (`DotNetAgents.Mcp`)
   - Service discovery and routing
   - Tool execution across services
   - Health checking

4. **Real-time Communication** (`DotNetAgents.Voice.SignalR`)
   - SignalR hubs for real-time updates
   - Status broadcasting
   - Can be extended for agent-to-agent messaging

5. **Observability** (`DotNetAgents.Observability`)
   - OpenTelemetry tracing
   - Metrics collection
   - Cost tracking
   - Correlation IDs via `ExecutionContext`

6. **Storage** (`DotNetAgents.Storage.*`)
   - PostgreSQL/SQL Server for persistence
   - Can store agent state, messages, coordination data

### ❌ Missing Components

1. **Agent Registry & Discovery**
   - No centralized agent registry
   - No agent capability discovery
   - No agent health monitoring

2. **Agent-to-Agent Messaging**
   - No message bus/queue infrastructure
   - No direct agent communication channels
   - No message routing/routing rules

3. **Task Distribution**
   - No load balancing for agent tasks
   - No task queue management
   - No worker pool management

4. **Supervisor Pattern**
   - No supervisor agent implementation
   - No task delegation mechanisms
   - No result aggregation

5. **Distributed Coordination**
   - No distributed locking
   - No leader election
   - No consensus mechanisms

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Multi-Agent Orchestration Layer           │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐      ┌──────────────┐      ┌──────────┐ │
│  │  Supervisor   │◄────►│ Agent        │◄────►│ Worker   │ │
│  │  Agent        │      │ Registry     │      │ Pool     │ │
│  └──────┬───────┘      └──────┬───────┘      └────┬─────┘ │
│         │                      │                    │       │
│         │                      │                    │       │
│         ▼                      ▼                    ▼       │
│  ┌──────────────────────────────────────────────────────┐ │
│  │         Agent Message Bus (Event-Driven)              │ │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐         │ │
│  │  │ In-Memory │  │ SignalR   │  │ RabbitMQ │         │ │
│  │  │ Bus       │  │ Bus       │  │ Bus      │         │ │
│  │  └──────────┘  └──────────┘  └──────────┘         │ │
│  └──────────────────────────────────────────────────────┘ │
│         │                      │                    │       │
│         ▼                      ▼                    ▼       │
│  ┌──────────────────────────────────────────────────────┐ │
│  │         State Management & Checkpointing              │ │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐         │ │
│  │  │ Workflow  │  │ Agent     │  │ Message  │         │ │
│  │  │ State     │  │ State     │  │ Store    │         │ │
│  │  └──────────┘  └──────────┘  └──────────┘         │ │
│  └──────────────────────────────────────────────────────┘ │
│                                                               │
└─────────────────────────────────────────────────────────────┘
         │                      │                    │
         ▼                      ▼                    ▼
┌──────────────┐      ┌──────────────┐      ┌──────────┐
│  Agents      │      │  Tools       │      │  LLMs     │
│  (Workers)   │      │  Registry    │      │  Providers│
└──────────────┘      └──────────────┘      └──────────┘
```

## Component Design

### 1. Agent Registry & Discovery

**Package:** `DotNetAgents.Agents.Registry`

#### Interfaces

```csharp
namespace DotNetAgents.Agents.Registry;

/// <summary>
/// Represents an agent's capabilities and metadata.
/// </summary>
public record AgentCapabilities
{
    public string AgentId { get; init; } = string.Empty;
    public string AgentType { get; init; } = string.Empty;
    public string[] SupportedTools { get; init; } = Array.Empty<string>();
    public string[] SupportedIntents { get; init; } = Array.Empty<string>();
    public int MaxConcurrentTasks { get; init; } = 1;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents an agent's current status.
/// </summary>
public enum AgentStatus
{
    Unknown,
    Available,
    Busy,
    Unavailable,
    Error
}

/// <summary>
/// Represents an agent instance.
/// </summary>
public record AgentInfo
{
    public string AgentId { get; init; } = string.Empty;
    public string AgentType { get; init; } = string.Empty;
    public AgentStatus Status { get; init; }
    public AgentCapabilities Capabilities { get; init; } = new();
    public DateTimeOffset LastHeartbeat { get; init; }
    public string? Endpoint { get; init; } // For distributed agents
    public int CurrentTaskCount { get; init; }
}

/// <summary>
/// Registry for managing agent discovery and capabilities.
/// </summary>
public interface IAgentRegistry
{
    /// <summary>
    /// Registers an agent with its capabilities.
    /// </summary>
    Task RegisterAsync(AgentCapabilities capabilities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unregisters an agent.
    /// </summary>
    Task UnregisterAsync(string agentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an agent's status.
    /// </summary>
    Task UpdateStatusAsync(string agentId, AgentStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds agents by capability (tool or intent).
    /// </summary>
    Task<IReadOnlyList<AgentInfo>> FindByCapabilityAsync(
        string capability,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds agents by type.
    /// </summary>
    Task<IReadOnlyList<AgentInfo>> FindByTypeAsync(
        string agentType,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an agent by ID.
    /// </summary>
    Task<AgentInfo?> GetByIdAsync(string agentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all registered agents.
    /// </summary>
    Task<IReadOnlyList<AgentInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Records a heartbeat from an agent.
    /// </summary>
    Task RecordHeartbeatAsync(string agentId, CancellationToken cancellationToken = default);
}
```

#### Implementations

- `InMemoryAgentRegistry` - For single-instance deployments
- `PostgreSQLAgentRegistry` - For distributed deployments
- `RedisAgentRegistry` - For high-performance distributed deployments

### 2. Agent-to-Agent Messaging

**Package:** `DotNetAgents.Agents.Messaging`

#### Interfaces

```csharp
namespace DotNetAgents.Agents.Messaging;

/// <summary>
/// Represents a message between agents.
/// </summary>
public record AgentMessage
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string FromAgentId { get; init; } = string.Empty;
    public string ToAgentId { get; init; } = string.Empty; // "*" for broadcast
    public string MessageType { get; init; } = string.Empty;
    public object Payload { get; init; } = new();
    public Dictionary<string, string> Headers { get; init; } = new();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; init; }
    public TimeSpan? TimeToLive { get; init; }
}

/// <summary>
/// Result of sending a message.
/// </summary>
public record MessageSendResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string MessageId { get; init; } = string.Empty;
}

/// <summary>
/// Message bus for agent-to-agent communication.
/// </summary>
public interface IAgentMessageBus
{
    /// <summary>
    /// Sends a message to a specific agent.
    /// </summary>
    Task<MessageSendResult> SendAsync(
        AgentMessage message,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Broadcasts a message to all agents or agents matching a filter.
    /// </summary>
    Task<MessageSendResult> BroadcastAsync(
        AgentMessage message,
        Func<AgentInfo, bool>? filter = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes to messages for a specific agent.
    /// </summary>
    Task<IDisposable> SubscribeAsync(
        string agentId,
        Func<AgentMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes to messages by type.
    /// </summary>
    Task<IDisposable> SubscribeByTypeAsync(
        string messageType,
        Func<AgentMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default);
}
```

#### Implementations

**1. InMemoryAgentMessageBus** (✅ Implemented)
- **Use Case**: Single-instance deployments, development/testing, simple scenarios
- **Pros**: Zero dependencies, very fast, simple setup
- **Cons**: Not distributed, messages lost on restart, no persistence
- **Performance**: Very high throughput, very low latency
- **When to Use**: Development, testing, single-server deployments, low message volume

**2. KafkaAgentMessageBus** (🔄 To Implement - High Priority)
- **Use Case**: High-throughput distributed systems, event streaming, microservices, large-scale deployments
- **Pros**: Very high throughput, distributed, persistent, replay capability, partitioning, ordering guarantees
- **Cons**: Requires Kafka infrastructure, more complex setup, higher latency than in-memory
- **Performance**: Very high throughput, low-medium latency
- **When to Use**: Production systems requiring high scale, event sourcing, multi-instance deployments
- **Package**: `DotNetAgents.Agents.Messaging.Kafka` (uses Confluent.Kafka)

**3. RabbitMQAgentMessageBus** (🔄 To Implement - Medium Priority)
- **Use Case**: Traditional message queuing, guaranteed delivery, complex routing, request-response patterns
- **Pros**: Mature ecosystem, reliable delivery, flexible routing, good for request-response
- **Cons**: Lower throughput than Kafka, more complex than in-memory
- **Performance**: High throughput, low latency
- **When to Use**: Systems requiring guaranteed delivery, complex routing, traditional queue patterns
- **Package**: `DotNetAgents.Agents.Messaging.RabbitMQ` (uses RabbitMQ.Client)

**4. RedisPubSubAgentMessageBus** (🔄 To Implement - Medium Priority)
- **Use Case**: Real-time pub/sub, when Redis infrastructure already exists, caching scenarios
- **Pros**: Fast, simple, often already in infrastructure stack, good for real-time updates
- **Cons**: No persistence, limited message ordering guarantees, memory-based
- **Performance**: High throughput, very low latency
- **When to Use**: Real-time notifications, when Redis is already in use, ephemeral messaging
- **Package**: `DotNetAgents.Agents.Messaging.Redis` (uses StackExchange.Redis)

**5. SignalRAgentMessageBus** (🔄 To Implement - Medium Priority)
- **Use Case**: Real-time web-based deployments, browser-to-server communication, web applications
- **Pros**: WebSocket support, real-time bidirectional communication, integrated with ASP.NET Core
- **Cons**: Web-only, requires SignalR infrastructure, not suitable for backend-only scenarios
- **Performance**: Medium throughput, low latency for web clients
- **When to Use**: Web applications requiring real-time agent communication, browser-based UIs
- **Package**: `DotNetAgents.Agents.Messaging.SignalR` (uses Microsoft.AspNetCore.SignalR)

**6. AzureServiceBusAgentMessageBus** (🔄 To Implement - Low Priority)
- **Use Case**: Azure cloud deployments, managed service preference
- **Pros**: Managed service, scalable, integrated with Azure ecosystem, reliable
- **Cons**: Vendor lock-in, cost, Azure-specific
- **Performance**: High throughput, medium latency
- **When to Use**: Azure-native applications, preference for managed services
- **Package**: `DotNetAgents.Agents.Messaging.Azure` (uses Azure.Messaging.ServiceBus)

**7. AWSSqsAgentMessageBus** (🔄 To Implement - Low Priority)
- **Use Case**: AWS cloud deployments, managed service preference
- **Pros**: Managed service, scalable, integrated with AWS ecosystem
- **Cons**: Vendor lock-in, cost, AWS-specific
- **Performance**: High throughput, medium latency
- **When to Use**: AWS-native applications, preference for managed services
- **Package**: `DotNetAgents.Agents.Messaging.AWS` (uses AWSSDK.SQS)

#### Performance & Flexibility Comparison

| Implementation | Throughput | Latency | Persistence | Distributed | Complexity | Priority |
|---------------|-----------|---------|-------------|-------------|------------|----------|
| In-Memory | Very High | Very Low | ❌ | ❌ | Low | ✅ Done |
| Kafka | Very High | Low-Medium | ✅ | ✅ | Medium | 🔥 High |
| RabbitMQ | High | Low | ✅ | ✅ | Medium | ⚡ Medium |
| Redis Pub/Sub | High | Very Low | ❌ | ✅ | Low | ⚡ Medium |
| SignalR | Medium | Low | ❌ | ✅ | Medium | ⚡ Medium |
| Azure Service Bus | High | Medium | ✅ | ✅ | Low (managed) | 📋 Low |
| AWS SQS | High | Medium | ✅ | ✅ | Low (managed) | 📋 Low |

#### Selection Guide

**Choose In-Memory When:**
- Single-instance deployment
- Development/testing environment
- Low message volume (< 1000/sec)
- Messages can be ephemeral

**Choose Kafka When:**
- High-throughput requirements (> 10,000 messages/sec)
- Event streaming/event sourcing patterns
- Multi-instance distributed deployments
- Need message replay capability
- Microservices architecture

**Choose RabbitMQ When:**
- Need guaranteed message delivery
- Complex routing requirements
- Request-response patterns
- Traditional queue-based workflows

**Choose Redis When:**
- Redis infrastructure already exists
- Real-time pub/sub requirements
- Ephemeral messaging acceptable
- Low latency critical

**Choose SignalR When:**
- Web-based applications
- Browser-to-server real-time communication
- ASP.NET Core web applications

**Choose Cloud Services When:**
- Cloud-native deployments
- Preference for managed services
- Vendor ecosystem integration important

### 3. Supervisor-Worker Pattern

**Package:** `DotNetAgents.Agents.Supervisor`

#### Interfaces

```csharp
namespace DotNetAgents.Agents.Supervisor;

/// <summary>
/// Represents a task to be executed by a worker agent.
/// </summary>
public record WorkerTask
{
    public string TaskId { get; init; } = Guid.NewGuid().ToString();
    public string TaskType { get; init; } = string.Empty;
    public object Input { get; init; } = new();
    public string? RequiredCapability { get; init; }
    public string? PreferredAgentId { get; init; }
    public int Priority { get; init; } = 0;
    public TimeSpan? Timeout { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Result of a worker task execution.
/// </summary>
public record WorkerTaskResult
{
    public string TaskId { get; init; } = string.Empty;
    public bool Success { get; init; }
    public object? Output { get; init; }
    public string? ErrorMessage { get; init; }
    public string WorkerAgentId { get; init; } = string.Empty;
    public TimeSpan ExecutionTime { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Supervisor agent that delegates tasks to worker agents.
/// </summary>
public interface ISupervisorAgent
{
    /// <summary>
    /// Submits a task to be executed by a worker agent.
    /// </summary>
    Task<string> SubmitTaskAsync(
        WorkerTask task,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Submits multiple tasks for parallel execution.
    /// </summary>
    Task<IReadOnlyList<string>> SubmitTasksAsync(
        IEnumerable<WorkerTask> tasks,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the status of a task.
    /// </summary>
    Task<TaskStatus> GetTaskStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the result of a completed task.
    /// </summary>
    Task<WorkerTaskResult?> GetTaskResultAsync(
        string taskId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancels a pending or running task.
    /// </summary>
    Task<bool> CancelTaskAsync(
        string taskId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets statistics about task execution.
    /// </summary>
    Task<SupervisorStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about supervisor task execution.
/// </summary>
public record SupervisorStatistics
{
    public int TotalTasksSubmitted { get; init; }
    public int TasksCompleted { get; init; }
    public int TasksFailed { get; init; }
    public int TasksPending { get; init; }
    public int TasksInProgress { get; init; }
    public TimeSpan AverageExecutionTime { get; init; }
    public Dictionary<string, int> TasksByType { get; init; } = new();
    public Dictionary<string, int> TasksByAgent { get; init; } = new();
}
```

#### Implementation

```csharp
public class SupervisorAgent : ISupervisorAgent
{
    private readonly IAgentRegistry _agentRegistry;
    private readonly IAgentMessageBus _messageBus;
    private readonly ITaskStore _taskStore;
    private readonly ILogger<SupervisorAgent> _logger;
    
    // Implementation details...
}
```

### 4. Worker Pool Management

**Package:** `DotNetAgents.Agents.WorkerPool`

#### Interfaces

```csharp
namespace DotNetAgents.Agents.WorkerPool;

/// <summary>
/// Manages a pool of worker agents.
/// </summary>
public interface IWorkerPool
{
    /// <summary>
    /// Gets the current number of workers in the pool.
    /// </summary>
    int WorkerCount { get; }
    
    /// <summary>
    /// Gets the number of available workers.
    /// </summary>
    int AvailableWorkerCount { get; }
    
    /// <summary>
    /// Adds a worker to the pool.
    /// </summary>
    Task AddWorkerAsync(
        string agentId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a worker from the pool.
    /// </summary>
    Task RemoveWorkerAsync(
        string agentId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an available worker for a task.
    /// </summary>
    Task<AgentInfo?> GetAvailableWorkerAsync(
        string? requiredCapability = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets pool statistics.
    /// </summary>
    Task<WorkerPoolStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about worker pool.
/// </summary>
public record WorkerPoolStatistics
{
    public int TotalWorkers { get; init; }
    public int AvailableWorkers { get; init; }
    public int BusyWorkers { get; init; }
    public int TotalTasksProcessed { get; init; }
    public TimeSpan AverageTaskDuration { get; init; }
    public Dictionary<string, int> TasksByWorker { get; init; } = new();
}
```

### 5. Task Queue & Distribution

**Package:** `DotNetAgents.Agents.Tasks`

#### Interfaces

```csharp
namespace DotNetAgents.Agents.Tasks;

/// <summary>
/// Queue for managing worker tasks.
/// </summary>
public interface ITaskQueue
{
    /// <summary>
    /// Enqueues a task for execution.
    /// </summary>
    Task EnqueueAsync(
        WorkerTask task,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Dequeues the next available task.
    /// </summary>
    Task<WorkerTask?> DequeueAsync(
        string? agentId = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the number of pending tasks.
    /// </summary>
    Task<int> GetPendingCountAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Peek at the next task without dequeuing.
    /// </summary>
    Task<WorkerTask?> PeekAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Store for task state and results.
/// </summary>
public interface ITaskStore
{
    Task SaveAsync(WorkerTask task, CancellationToken cancellationToken = default);
    Task<WorkerTask?> GetAsync(string taskId, CancellationToken cancellationToken = default);
    Task SaveResultAsync(WorkerTaskResult result, CancellationToken cancellationToken = default);
    Task<WorkerTaskResult?> GetResultAsync(string taskId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(string taskId, TaskStatus status, CancellationToken cancellationToken = default);
}
```

#### Implementations

- `InMemoryTaskQueue` - For single-instance deployments
- `PostgreSQLTaskQueue` - For distributed deployments
- `RedisTaskQueue` - For high-performance distributed deployments
- `RabbitMQTaskQueue` - For message queue-based deployments

### 6. Multi-Agent Workflow Integration

**Package:** `DotNetAgents.Agents.Workflow`

#### Workflow Nodes for Multi-Agent

```csharp
namespace DotNetAgents.Agents.Workflow;

/// <summary>
/// State for multi-agent workflows.
/// </summary>
public record MultiAgentWorkflowState
{
    public string WorkflowId { get; init; } = Guid.NewGuid().ToString();
    public string SupervisorAgentId { get; init; } = string.Empty;
    public List<string> WorkerAgentIds { get; init; } = new();
    public List<WorkerTask> Tasks { get; init; } = new();
    public Dictionary<string, WorkerTaskResult> Results { get; init; } = new();
    public WorkflowStatus Status { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Workflow node that delegates to a worker agent.
/// </summary>
public class DelegateToWorkerNode<TState> : GraphNode<TState>
    where TState : class
{
    private readonly ISupervisorAgent _supervisor;
    private readonly Func<TState, WorkerTask> _taskFactory;
    
    // Implementation...
}

/// <summary>
/// Workflow node that aggregates results from multiple workers.
/// </summary>
public class AggregateResultsNode<TState> : GraphNode<TState>
    where TState : class
{
    private readonly Func<TState, IEnumerable<WorkerTaskResult>, TState> _aggregator;
    
    // Implementation...
}
```

## Leveraging Existing Infrastructure

### 1. Using StateGraph for Multi-Agent Coordination

```csharp
var workflow = WorkflowBuilder<MultiAgentWorkflowState>.Create()
    .AddNode("delegate_task_1", async (state, ct) =>
    {
        var task = new WorkerTask
        {
            TaskType = "process_document",
            Input = state.Document1
        };
        var taskId = await supervisor.SubmitTaskAsync(task, ct);
        return state with { Tasks = state.Tasks.Append(task).ToList() };
    })
    .AddNode("delegate_task_2", async (state, ct) =>
    {
        var task = new WorkerTask
        {
            TaskType = "process_document",
            Input = state.Document2
        };
        var taskId = await supervisor.SubmitTaskAsync(task, ct);
        return state with { Tasks = state.Tasks.Append(task).ToList() };
    })
    .AddNode("wait_for_results", async (state, ct) =>
    {
        // Wait for all tasks to complete
        var results = new Dictionary<string, WorkerTaskResult>();
        foreach (var task in state.Tasks)
        {
            var result = await supervisor.GetTaskResultAsync(task.TaskId, ct);
            if (result != null)
                results[task.TaskId] = result;
        }
        return state with { Results = results };
    })
    .AddNode("aggregate", async (state, ct) =>
    {
        // Aggregate results
        var aggregated = AggregateResults(state.Results.Values);
        return state with { Status = WorkflowStatus.Completed, Metadata = aggregated };
    })
    .SetEntryPoint("delegate_task_1")
    .AddEdge("delegate_task_1", "delegate_task_2")
    .AddEdge("delegate_task_2", "wait_for_results")
    .AddEdge("wait_for_results", "aggregate")
    .Build();
```

### 2. Using MCP for Distributed Agent Communication

- Extend MCP to support agent-to-agent messages
- Use MCP routing for agent discovery
- Leverage MCP health checks for agent availability

### 3. Using SignalR for Real-Time Updates

- Extend SignalR hubs for agent status updates
- Broadcast task assignments and completions
- Real-time supervisor dashboard

### 4. Using Checkpointing for Fault Tolerance

- Checkpoint multi-agent workflow state
- Resume workflows after agent failures
- State persistence for long-running multi-agent tasks

### 5. Using Observability for Monitoring

- Trace agent-to-agent communication
- Metrics for task distribution and execution
- Cost tracking per agent and task type

## Scalability Considerations

### Horizontal Scaling

1. **Stateless Agents**
   - Agents should be stateless or use external state storage
   - State stored in PostgreSQL/Redis, not in-memory

2. **Distributed Message Bus**
   - Use RabbitMQ, Azure Service Bus, or Redis Pub/Sub
   - Support multiple instances of supervisor and workers

3. **Load Balancing**
   - Round-robin task distribution
   - Capability-based routing
   - Priority-based queuing

4. **Agent Auto-Scaling**
   - Monitor queue depth
   - Automatically scale worker pools
   - Health-based agent replacement

### Performance Optimizations

1. **Batch Processing**
   - Batch task submissions
   - Batch result retrieval
   - Parallel task execution

2. **Caching**
   - Cache agent capabilities
   - Cache task results
   - Cache agent availability

3. **Connection Pooling**
   - Reuse HTTP connections for agent communication
   - Pool database connections
   - Pool message bus connections

## Implementation Phases

### Phase 1: Core Infrastructure (Weeks 1-2)
- [ ] `IAgentRegistry` interface and `InMemoryAgentRegistry`
- [ ] `IAgentMessageBus` interface and `InMemoryAgentMessageBus`
- [ ] Basic `ISupervisorAgent` implementation
- [ ] `IWorkerPool` interface and basic implementation
- [ ] Unit tests for core components

### Phase 2: Storage & Persistence (Weeks 3-4)
- [ ] `PostgreSQLAgentRegistry`
- [ ] `PostgreSQLTaskQueue` and `PostgreSQLTaskStore`
- [ ] Integration tests with PostgreSQL
- [ ] Migration scripts

### Phase 3: Distributed Messaging (Weeks 5-7)
- [ ] `KafkaAgentMessageBus` (High Priority - Core for production)
  - [ ] Create `DotNetAgents.Agents.Messaging.Kafka` package
  - [ ] Implement `KafkaAgentMessageBus` with Confluent.Kafka
  - [ ] Support topic partitioning and consumer groups
  - [ ] Message serialization/deserialization
  - [ ] Error handling and retry logic
  - [ ] Configuration options (bootstrap servers, topics, etc.)
- [ ] `RabbitMQAgentMessageBus` (Medium Priority)
  - [ ] Create `DotNetAgents.Agents.Messaging.RabbitMQ` package
  - [ ] Implement `RabbitMQAgentMessageBus` with RabbitMQ.Client
  - [ ] Exchange and queue management
  - [ ] Routing key support
- [ ] `RedisPubSubAgentMessageBus` (Medium Priority)
  - [ ] Create `DotNetAgents.Agents.Messaging.Redis` package
  - [ ] Implement `RedisPubSubAgentMessageBus` with StackExchange.Redis
  - [ ] Channel subscription management
- [ ] `SignalRAgentMessageBus` (Medium Priority)
  - [ ] Create `DotNetAgents.Agents.Messaging.SignalR` package
  - [ ] Implement `SignalRAgentMessageBus` with SignalR hub
  - [ ] Real-time web communication support
- [ ] Integration tests for all message bus implementations
- [ ] Performance benchmarks comparing implementations
- [ ] Documentation for selecting the right message bus

### Phase 4: Workflow Integration (Weeks 7-8)
- [ ] `MultiAgentWorkflowState`
- [ ] `DelegateToWorkerNode<TState>`
- [ ] `AggregateResultsNode<TState>`
- [ ] Workflow examples and documentation

### Phase 5: Advanced Features (Weeks 9-10)
- [ ] Load balancing strategies
- [ ] Auto-scaling support
- [ ] Advanced routing (capability-based, priority-based)
- [ ] Performance optimizations

### Phase 6: Observability & Monitoring (Week 11)
- [ ] OpenTelemetry integration
- [ ] Metrics collection
- [ ] Dashboard support
- [ ] Alerting

### Phase 7: Documentation & Samples (Week 12)
- [ ] API documentation
- [ ] Architecture documentation
- [ ] Sample applications
- [ ] Migration guide

## New Packages Structure

```
src/
├── DotNetAgents.Agents.Registry/
│   ├── IAgentRegistry.cs
│   ├── AgentInfo.cs
│   ├── AgentCapabilities.cs
│   ├── InMemoryAgentRegistry.cs
│   ├── PostgreSQLAgentRegistry.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.Messaging/ (Core interfaces)
│   ├── IAgentMessageBus.cs
│   ├── AgentMessage.cs
│   ├── MessageSendResult.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.Messaging.InMemory/ (✅ Implemented)
│   ├── InMemoryAgentMessageBus.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.Messaging.Kafka/ (🔥 High Priority)
│   ├── KafkaAgentMessageBus.cs
│   ├── KafkaOptions.cs
│   ├── KafkaMessageSerializer.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.Messaging.RabbitMQ/ (⚡ Medium Priority)
│   ├── RabbitMQAgentMessageBus.cs
│   ├── RabbitMQOptions.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.Messaging.Redis/ (⚡ Medium Priority)
│   ├── RedisPubSubAgentMessageBus.cs
│   ├── RedisOptions.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.Messaging.SignalR/ (⚡ Medium Priority)
│   ├── SignalRAgentMessageBus.cs
│   ├── AgentMessageHub.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.Supervisor/
│   ├── ISupervisorAgent.cs
│   ├── SupervisorAgent.cs
│   ├── WorkerTask.cs
│   ├── WorkerTaskResult.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.WorkerPool/
│   ├── IWorkerPool.cs
│   ├── WorkerPool.cs
│   ├── WorkerPoolStatistics.cs
│   └── ServiceCollectionExtensions.cs
│
├── DotNetAgents.Agents.Tasks/
│   ├── ITaskQueue.cs
│   ├── ITaskStore.cs
│   ├── InMemoryTaskQueue.cs
│   ├── PostgreSQLTaskQueue.cs
│   ├── PostgreSQLTaskStore.cs
│   └── ServiceCollectionExtensions.cs
│
└── DotNetAgents.Agents.Workflow/
    ├── MultiAgentWorkflowState.cs
    ├── DelegateToWorkerNode.cs
    ├── AggregateResultsNode.cs
    └── ServiceCollectionExtensions.cs
```

## Example Usage

### Basic Supervisor-Worker Pattern

```csharp
// Setup - Choose message bus based on deployment scenario

// Option 1: In-Memory (Development/Testing)
services.AddInMemoryAgentRegistry();
services.AddInMemoryAgentMessageBus();

// Option 2: Kafka (Production - High Scale)
services.AddInMemoryAgentRegistry(); // or PostgreSQLAgentRegistry
services.AddKafkaAgentMessageBus(options =>
{
    options.BootstrapServers = "localhost:9092";
    options.TopicPrefix = "agents";
    options.ConsumerGroupId = "agent-workers";
});

// Option 3: RabbitMQ (Production - Guaranteed Delivery)
services.AddInMemoryAgentRegistry();
services.AddRabbitMQAgentMessageBus(options =>
{
    options.HostName = "localhost";
    options.ExchangeName = "agent-messages";
});

// Option 4: Redis (Production - Real-time)
services.AddInMemoryAgentRegistry();
services.AddRedisAgentMessageBus(options =>
{
    options.ConnectionString = "localhost:6379";
    options.ChannelPrefix = "agents";
});

services.AddSupervisorAgent();
services.AddWorkerPool();

// Supervisor Agent
var supervisor = serviceProvider.GetRequiredService<ISupervisorAgent>();

// Submit tasks
var task1 = new WorkerTask
{
    TaskType = "analyze_document",
    Input = document1,
    RequiredCapability = "document_analysis"
};
var task2 = new WorkerTask
{
    TaskType = "analyze_document",
    Input = document2,
    RequiredCapability = "document_analysis"
};

var taskIds = await supervisor.SubmitTasksAsync(new[] { task1, task2 });

// Wait for results
var results = new List<WorkerTaskResult>();
foreach (var taskId in taskIds)
{
    var result = await supervisor.GetTaskResultAsync(taskId);
    if (result != null)
        results.Add(result);
}

// Process results
var aggregated = AggregateResults(results);
```

### Multi-Agent Workflow

```csharp
var workflow = WorkflowBuilder<MultiAgentWorkflowState>.Create()
    .AddNode("delegate_analysis", new DelegateToWorkerNode<MultiAgentWorkflowState>(
        supervisor,
        state => new WorkerTask
        {
            TaskType = "analyze",
            Input = state.Input
        }))
    .AddNode("delegate_summarization", new DelegateToWorkerNode<MultiAgentWorkflowState>(
        supervisor,
        state => new WorkerTask
        {
            TaskType = "summarize",
            Input = state.Results["analysis"]
        }))
    .AddNode("aggregate", new AggregateResultsNode<MultiAgentWorkflowState>(
        (state, results) => state with
        {
            Results = results.ToDictionary(r => r.TaskId),
            Status = WorkflowStatus.Completed
        }))
    .SetEntryPoint("delegate_analysis")
    .AddEdge("delegate_analysis", "delegate_summarization")
    .AddEdge("delegate_summarization", "aggregate")
    .Build();

var executor = new GraphExecutor<MultiAgentWorkflowState>(workflow);
var finalState = await executor.ExecuteAsync(initialState);
```

## Agent-to-Agent Communication Patterns

### 1. Request-Response Pattern

```csharp
// Agent A sends a request
var request = new AgentMessage
{
    FromAgentId = "agent_a",
    ToAgentId = "agent_b",
    MessageType = "data_request",
    Payload = new { Query = "get_user_data", UserId = "123" },
    CorrelationId = correlationId
};

var sendResult = await messageBus.SendAsync(request);

// Agent B subscribes and responds
await messageBus.SubscribeAsync("agent_b", async (message, ct) =>
{
    if (message.MessageType == "data_request")
    {
        var response = new AgentMessage
        {
            FromAgentId = "agent_b",
            ToAgentId = message.FromAgentId,
            MessageType = "data_response",
            Payload = await GetUserDataAsync(message.Payload),
            CorrelationId = message.CorrelationId
        };
        await messageBus.SendAsync(response, ct);
    }
});
```

### 2. Publish-Subscribe Pattern

```csharp
// Publisher agent
var eventMessage = new AgentMessage
{
    FromAgentId = "publisher",
    ToAgentId = "*", // Broadcast
    MessageType = "document_processed",
    Payload = new { DocumentId = "doc123", Status = "completed" }
};
await messageBus.BroadcastAsync(eventMessage);

// Subscriber agents
await messageBus.SubscribeByTypeAsync("document_processed", async (message, ct) =>
{
    // Handle document processed event
    await HandleDocumentProcessedAsync(message.Payload, ct);
});
```

### 3. Workflow Coordination Pattern

```csharp
// Use StateGraph with agent communication nodes
var workflow = WorkflowBuilder<WorkflowState>.Create()
    .AddNode("send_to_agent", async (state, ct) =>
    {
        var message = new AgentMessage
        {
            FromAgentId = "supervisor",
            ToAgentId = state.TargetAgentId,
            MessageType = "workflow_step",
            Payload = state.CurrentStep
        };
        await messageBus.SendAsync(message, ct);
        return state with { StepSent = true };
    })
    .AddNode("wait_for_response", async (state, ct) =>
    {
        // Wait for response message
        var response = await WaitForMessageAsync(
            state.TargetAgentId,
            "workflow_step_response",
            ct);
        return state with { StepResult = response.Payload };
    })
    .Build();
```

## Conclusion

This plan provides a comprehensive foundation for multi-agent workflows in DotNetAgents, leveraging existing infrastructure while introducing new components for agent coordination, messaging, and scalability. The design is modular, allowing incremental implementation and adoption based on specific use cases.

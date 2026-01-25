# DotNetAgents Architecture Summary

**Last Updated:** January 2025  
**Status:** Current Architecture Overview  
**Version:** 2.0 (with State Machines and Behavior Trees)

## Overview

DotNetAgents follows a layered, modular architecture designed for enterprise-grade AI agent systems. The framework provides comprehensive capabilities for building autonomous agents, multi-agent workflows, and complex AI applications.

## Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│              Application Layer                          │
│  (JARVIS, Teaching Assistant, Custom Applications)     │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│         Autonomous Agent Capabilities                   │
│  • State Machines (Lifecycle Management)               │
│  • Behavior Trees (Decision-Making)                     │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│              Workflow Engine                            │
│  • StateGraph (LangGraph-like)                         │
│  • Checkpointing & Resumption                           │
│  • Human-in-the-Loop                                    │
│  • Visualization                                        │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│         Chain & Runnable Layer                          │
│  • Chains (LangChain-like)                             │
│  • LCEL-like Composition                                │
│  • Runnable Interface                                   │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│           Core Abstractions                             │
│  • ILLMModel, IPromptTemplate                          │
│  • ITool, IVectorStore, IMemory                        │
│  • Agent Interfaces                                     │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│      Integrations & Infrastructure                      │
│  • 12 LLM Providers                                     │
│  • 5 Vector Stores                                      │
│  • 19 Built-in Tools                                    │
│  • Storage (PostgreSQL, SQL Server)                     │
│  • Observability (OpenTelemetry)                        │
└─────────────────────────────────────────────────────────┘
```

## Key Components

### State Machines (`DotNetAgents.Agents.StateMachines`)

**Purpose:** Manage agent lifecycle and operational states

**Features:**
- Basic state machines with transitions, guards, and actions
- Hierarchical state machines (nested states)
- Parallel state machines (orthogonal regions)
- Timed and scheduled transitions
- State persistence and history tracking
- Common patterns (Idle-Working, Error-Recovery, Worker Pool, Supervisor)
- Integration with workflows, worker pools, and message bus
- **Adapter pattern** for breaking circular dependencies
- **State machine patterns** for common use cases:
  - Supervisor Pattern (Monitoring → Analyzing → Delegating → Waiting)
  - Worker Pool Pattern (Available → Busy → CoolingDown)
  - Voice Session Pattern (Idle → Listening → Processing → Responding)
  - Dialog Pattern (Initial → CollectingInfo → Confirming → Executing → Completed)
  - Learning Session Pattern (Initialized → Learning → Assessment → Review → Completed)
  - Mastery Pattern (Novice → Learning → Proficient → Master)
  - Agent Execution Pattern (Initialized → Thinking → Acting → Observing → Finalizing)

**Use Cases:**
- Agent lifecycle management (Available → Busy → CoolingDown)
- Workflow state tracking
- Error recovery patterns
- Supervisor agent state management
- Voice command session lifecycle
- Educational learning session tracking
- Student mastery progression
- Agent execution lifecycle (ReAct pattern)

### Behavior Trees (`DotNetAgents.Agents.BehaviorTrees`)

**Purpose:** Hierarchical decision-making for autonomous agents

**Features:**
- Leaf nodes (Action, Condition)
- Composite nodes (Sequence, Selector, Parallel)
- Decorator nodes (Inverter, Repeater, UntilFail, Timeout, Cooldown, Retry, Conditional)
- LLM integration nodes (`LLMActionNode` for dynamic decision-making)
- Workflow integration nodes
- State machine integration nodes
- Observable execution with OpenTelemetry
- **Behavior tree implementations** for specific domains:
  - Task Routing Behavior Tree (Priority-based, Capability-based, Load-balanced)
  - Command Processing Behavior Tree (Simple, MultiStep, Ambiguous strategies)
  - Adaptive Learning Path Behavior Tree (Review Needed, Mastery Gap, Prerequisite-Based)
  - Tool Selection Behavior Tree (Exact Match, Capability Match, Description Match)

**Use Cases:**
- Autonomous agent decision-making
- Complex conditional logic
- Retry and error handling strategies
- LLM-based decision making
- Workflow orchestration from behavior trees
- Intelligent task routing in supervisor agents
- Command processing strategy selection
- Adaptive learning path determination
- Tool selection for agent executors

### Multi-Agent System

**Components:**
- **Agent Registry**: Agent discovery and status tracking
  - `InMemoryAgentRegistry` - Single-instance deployments
  - `PostgreSQLAgentRegistry` - Distributed deployments
- **Worker Pool**: Load balancing, auto-scaling, worker management
  - **Optional state machine integration** for lifecycle tracking
  - **Backward compatible** with `AgentStatus` enum
  - State-based worker selection
- **Supervisor Agent**: Task delegation and coordination
  - **State machine integration** (Monitoring → Analyzing → Delegating → Waiting)
  - **Behavior tree integration** for intelligent task routing
  - **LLM-based routing** for complex decision-making
- **Message Bus**: Agent-to-agent communication with 5 implementations:
  - **In-Memory** (`DotNetAgents.Agents.Messaging.InMemory`) - Development/testing
  - **Kafka** (`DotNetAgents.Agents.Messaging.Kafka`) - High-throughput distributed systems
  - **RabbitMQ** (`DotNetAgents.Agents.Messaging.RabbitMQ`) - Guaranteed delivery
  - **Redis** (`DotNetAgents.Agents.Messaging.Redis`) - Real-time pub/sub
  - **SignalR** (`DotNetAgents.Agents.Messaging.SignalR`) - Web-based real-time communication
- **Task Queue & Store**: Task management and persistence
  - `InMemoryTaskQueue` - Single-instance
  - `PostgreSQLTaskQueue` - Distributed
- **State-Based Selection**: Worker pool selection based on state machine states

**Patterns:**
- Supervisor-Worker: Hierarchical task delegation
- Peer-to-Peer: Direct agent communication
- Broadcast: One-to-many messaging
- Request-Response: Synchronous agent communication
- Publish-Subscribe: Event-driven agent communication

### Workflow Engine (`DotNetAgents.Workflow`)

**Features:**
- StateGraph workflows (LangGraph-like)
- Checkpointing and resumption
- Human-in-the-loop nodes (Approval, Decision, Input, Review)
- Multi-agent workflow nodes
- Visualization (DOT, Mermaid, JSON)
- Templates and patterns

### Core Abstractions (`DotNetAgents.Core`)

**Interfaces:**
- `ILLMModel<TInput, TOutput>`: LLM abstraction
- `IPromptTemplate`: Prompt templating
- `IRunnable<TInput, TOutput>`: Chain composition
- `ITool`: Tool execution
- `IVectorStore`: Vector storage and retrieval
- `IMemory`: Conversation memory

## Integration Points

### State Machines ↔ Workflows
- `StateMachineWorkflowNode`: Workflow nodes that trigger state transitions
- `StateConditionWorkflowNode`: Workflow nodes that check state machine state

### State Machines ↔ Worker Pool
- `StateBasedWorkerPool`: Worker selection based on state machine states
- State machine patterns for worker lifecycle (Available → Busy → CoolingDown)

### Behavior Trees ↔ State Machines
- `StateMachineConditionNode`: Behavior tree nodes that check state machine state
- `StateMachineActionNode`: Behavior tree nodes that trigger state transitions

### Behavior Trees ↔ Workflows
- `WorkflowActionNode`: Behavior tree nodes that execute workflows

### Behavior Trees ↔ LLMs
- `LLMActionNode`: Behavior tree nodes that use LLMs for decision-making

## Package Structure

### Core Libraries
- `DotNetAgents.Core`: Core abstractions and implementations
- `DotNetAgents.Workflow`: Workflow engine
- `DotNetAgents.Agents.StateMachines`: State machine implementation
- `DotNetAgents.Agents.BehaviorTrees`: Behavior tree implementation
- `DotNetAgents.Agents.Registry`: Agent registry
- `DotNetAgents.Agents.WorkerPool`: Worker pool management
- `DotNetAgents.Agents.Supervisor`: Supervisor agent
- `DotNetAgents.Agents.Messaging`: Message bus abstractions

### Application Packages
- `../JARVIS/`: JARVIS voice assistant implementation (standalone project)
  - **Voice Session State Machine** (Idle → Listening → Processing → Responding)
  - **Dialog State Machine** (Initial → CollectingInfo → Confirming → Executing → Completed)
  - **Command Processing Behavior Tree** (Simple/MultiStep/Ambiguous strategies)
- `../TeachingAssistant/`: Educational assistant implementation (standalone project)
  - **Learning Session State Machine** (Initialized → Learning → Assessment → Review → Completed)
  - **Mastery State Machine** (Novice → Learning → Proficient → Master)
  - **Adaptive Learning Path Behavior Tree** (Review Needed, Mastery Gap, Prerequisite-Based)

### Infrastructure
- `DotNetAgents.Storage.*`: Storage implementations
  - `DotNetAgents.Storage.TaskKnowledge.PostgreSQL` - PostgreSQL storage
  - `DotNetAgents.Storage.TaskKnowledge.SqlServer` - SQL Server storage
  - `DotNetAgents.Storage.Agents.PostgreSQL` - Agent registry storage
- `DotNetAgents.Observability`: Logging, tracing, metrics
  - OpenTelemetry integration
  - Cost tracking
  - Health checks
- `DotNetAgents.Security`: Security features
  - Secrets management
  - Input validation
  - Rate limiting
  - Audit logging
- `DotNetAgents.Configuration`: Configuration management

### Deployment & Infrastructure
- **Kubernetes**: Complete manifests and Helm charts
  - Application deployments (API, UIs)
  - LLM services (vLLM, Ollama)
  - Database (PostgreSQL with pgvector)
  - Ingress configuration
- **Monitoring Stack**:
  - Prometheus for metrics collection
  - Grafana for visualization
  - Loki for log aggregation
  - Promtail for log collection
- **Docker**: Docker Compose and Dockerfiles
  - vLLM container configuration
  - Ollama container configuration
  - GPU support

## Design Principles

1. **Modularity**: Each component is independently usable
2. **Extensibility**: Interface-based design allows custom implementations
3. **Observability**: Full OpenTelemetry integration throughout
4. **Performance**: Leverages .NET 10 AI optimizations
5. **Type Safety**: Strong typing throughout (C# 13)
6. **Async-First**: All I/O operations are async
7. **Cancellation Support**: CancellationToken support throughout

## See Also

- [State Machines README](../../src/DotNetAgents.Agents.StateMachines/README.md)
- [Behavior Trees README](../../src/DotNetAgents.Agents.BehaviorTrees/README.md)
- [Workflow Documentation](../../src/DotNetAgents.Workflow/README.md)
- [Project Status](../status/PROJECT_STATUS.md)

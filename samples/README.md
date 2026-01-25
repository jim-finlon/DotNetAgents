# DotNetAgents Samples

This directory contains sample applications demonstrating how to use the DotNetAgents library.

## Prerequisites

- .NET 10 SDK
- OpenAI API key (set as `OPENAI_API_KEY` environment variable) - Required for most samples

## Samples

### 1. BasicChain

Demonstrates basic chain composition with LLM and prompt templates.

**Features:**
- Simple LLM chain
- Chain with prompt template
- Sequential chain composition

**Run:**
```bash
cd DotNetAgents.Samples.BasicChain
dotnet run
```

### 2. AgentWithTools

Demonstrates an agent using built-in tools to answer questions and perform actions.

**Features:**
- Agent executor with ReAct pattern
- Tool registry
- Multiple built-in tools (calculator, datetime, web search, Wikipedia)
- **Agent Execution State Machine** (Initialized → Thinking → Acting → Observing → Finalizing)
- **Tool Selection Behavior Tree** (ExactMatch/CapabilityMatch/DescriptionMatch strategies)

**Run:**
```bash
cd DotNetAgents.Samples.AgentWithTools
dotnet run
```

**Note:** This sample demonstrates state machine integration for agent execution lifecycle and behavior tree integration for intelligent tool selection.

### 3. Workflow

Demonstrates a stateful workflow with checkpointing.

**Features:**
- StateGraph workflow
- Multiple workflow nodes
- State management
- Checkpointing

**Run:**
```bash
cd DotNetAgents.Samples.Workflow
dotnet run
```

### 4. RAG (Retrieval-Augmented Generation)

Demonstrates a RAG pipeline with document loading, chunking, embeddings, and retrieval.

**Features:**
- Document loading and chunking
- Embedding generation
- Vector store integration
- Retrieval chain
- Context-aware question answering

**Run:**
```bash
cd DotNetAgents.Samples.RAG
dotnet run
```

### 5. Education

Demonstrates educational extensions for AI-powered tutoring, assessment, and learning management.

**Features:**
- Socratic dialogue engine
- Spaced repetition (SM2 algorithm)
- Mastery calculation and tracking
- Content filtering (COPPA compliance)
- Assessment generation and evaluation
- Student profile management
- **Learning Session State Machine** (Initialized → Learning → Assessment → Review → Completed)
- **Mastery State Machine** (Novice → Learning → Proficient → Master)
- **Adaptive Learning Path Behavior Tree** (Review Needed, Mastery Gap, Prerequisite-Based)

**Run:**
```bash
cd DotNetAgents.Samples.Education
dotnet run
```

**Note:** This sample requires an OpenAI API key for full functionality (some features work without it). Demonstrates state machine integration for learning sessions and mastery tracking, plus behavior tree integration for adaptive learning paths.

### 6. TasksAndKnowledge

Demonstrates task management, knowledge capture, and bootstrap generation in workflows.

### 7. JARVISVoice

Demonstrates JARVIS Voice command processing with state machines and behavior trees.

**Features:**
- Voice session state machine (Idle → Listening → Processing → Responding → Idle)
- Command processing behavior tree (Simple/MultiStep/Ambiguous strategies)
- Dialog state machine (Initial → CollectingInfo → Confirming → Executing → Completed)
- Intelligent command routing based on confidence and completeness

**Run:**
```bash
cd DotNetAgents.Samples.JARVISVoice
dotnet run
```

**Note:** This sample demonstrates the integration of state machines and behavior trees into voice command processing. Full functionality requires LLM provider configuration.

**Features:**
- Task creation and tracking in workflows
- Knowledge capture from successes and errors
- Task statistics and completion tracking
- Knowledge repository querying
- Bootstrap payload generation for workflow resumption

**Run:**
```bash
cd DotNetAgents.Samples.TasksAndKnowledge
dotnet run
```

**Note:** This sample works without an OpenAI API key (LLM features are optional).

### 7. MultiAgent

Demonstrates supervisor-worker pattern with multi-agent workflows.

**Features:**
- Agent registry and registration
- Worker pool management
- Supervisor agent for task delegation
- Multi-agent workflow nodes (DelegateToWorkerNode, AggregateResultsNode)
- Task submission and result aggregation
- Worker pool and supervisor statistics
- **State machines for supervisor lifecycle management** (Monitoring → Analyzing → Delegating → Waiting)
- **Behavior trees for intelligent task routing** (Priority-based, Capability-based, Load-balanced)
- **Optional LLM-based routing** for advanced decision-making
- State machine integration with worker pool (optional)

**Run:**
```bash
cd DotNetAgents.Samples.MultiAgent
dotnet run
```

**Note:** This sample works without an OpenAI API key (uses in-memory implementations). State machines and behavior trees are demonstrated with the supervisor agent.

### 8. State Machines

Demonstrates state machine usage for agent lifecycle management.

**Features:**
- Basic state machines with transitions
- State machine patterns (Worker Pool, Error Recovery)
- Timed transitions (cooldown)
- Integration with agent registry
- State-based worker pool selection
- Message bus integration for state transitions

**Run:**
```bash
cd DotNetAgents.Samples.StateMachines
dotnet run
```

**Note:** This sample works without an OpenAI API key.

### 9. Behavior Trees (Coming Soon)

Demonstrates behavior trees for autonomous agent decision-making.

**Features:**
- Basic behavior tree construction
- Composite nodes (Sequence, Selector, Parallel)
- Decorator nodes (Retry, Timeout, Cooldown)
- LLM integration nodes
- Workflow integration nodes
- State machine integration nodes

## Setting Up

1. Set your OpenAI API key:
   ```bash
   # Windows PowerShell
   $env:OPENAI_API_KEY="your-api-key-here"
   
   # Linux/Mac
   export OPENAI_API_KEY="your-api-key-here"
   ```

2. Navigate to a sample directory and run:
   ```bash
   dotnet run
   ```

## Notes

- All samples require an OpenAI API key
- Some samples may incur API costs
- Samples are for demonstration purposes only

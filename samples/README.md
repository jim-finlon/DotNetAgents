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

**Run:**
```bash
cd DotNetAgents.Samples.AgentWithTools
dotnet run
```

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

**Run:**
```bash
cd DotNetAgents.Samples.Education
dotnet run
```

**Note:** This sample requires an OpenAI API key for full functionality (some features work without it).

### 6. TasksAndKnowledge

Demonstrates task management, knowledge capture, and bootstrap generation in workflows.

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

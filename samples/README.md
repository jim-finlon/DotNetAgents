# DotNetAgents Samples

This directory contains sample applications demonstrating how to use the DotNetAgents library.

## Prerequisites

- .NET 8 SDK
- OpenAI API key (set as `OPENAI_API_KEY` environment variable)

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

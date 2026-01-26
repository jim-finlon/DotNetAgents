# Visual Workflow Designer Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** In Development

## Overview

The Visual Workflow Designer provides a web-based, drag-and-drop interface for creating and managing DotNetAgents workflows. This guide explains the architecture and usage.

## Architecture

### Components

1. **Backend API** (`DotNetAgents.Workflow.Designer`)
   - REST API for workflow CRUD operations
   - Workflow validation
   - Workflow execution management
   - Real-time execution status

2. **Frontend UI** (Planned)
   - React/Blazor-based visual editor
   - Drag-and-drop node placement
   - Real-time execution visualization
   - Workflow export/import

## API Endpoints

### Workflow Management

```
GET    /api/workflows              - List all workflows
GET    /api/workflows/{id}         - Get workflow by ID
POST   /api/workflows              - Create new workflow
PUT    /api/workflows/{id}         - Update workflow
DELETE /api/workflows/{id}         - Delete workflow
POST   /api/workflows/{id}/validate - Validate workflow
```

### Workflow Execution

```
POST   /api/workflows/{id}/execute - Execute workflow
GET    /api/executions/{id}        - Get execution status
GET    /api/executions/{id}/state  - Get execution state
POST   /api/executions/{id}/cancel - Cancel execution
```

## Workflow Definition Format

```json
{
  "name": "my-workflow",
  "description": "Example workflow",
  "version": "1.0.0",
  "nodes": [
    {
      "id": "node-1",
      "name": "start",
      "type": "function",
      "x": 100,
      "y": 100,
      "config": {
        "handler": "MyHandler.ProcessAsync"
      }
    },
    {
      "id": "node-2",
      "name": "condition",
      "type": "condition",
      "x": 300,
      "y": 100,
      "config": {
        "condition": "state.value > 10"
      }
    }
  ],
  "edges": [
    {
      "id": "edge-1",
      "from": "node-1",
      "to": "node-2",
      "conditional": false
    }
  ],
  "entryPoint": "node-1",
  "exitPoints": ["node-2"]
}
```

## Node Types

### Function Node
Executes a C# function or method.

```json
{
  "type": "function",
  "config": {
    "handler": "Namespace.Class.MethodAsync",
    "parameters": {}
  }
}
```

### Condition Node
Branches based on state condition.

```json
{
  "type": "condition",
  "config": {
    "condition": "state.value > threshold"
  }
}
```

### Parallel Node
Executes multiple branches in parallel.

```json
{
  "type": "parallel",
  "config": {
    "branches": ["branch-1", "branch-2"]
  }
}
```

### Human-in-Loop Node
Pauses for human approval.

```json
{
  "type": "human-in-loop",
  "config": {
    "approvalRequired": true,
    "timeout": 3600
  }
}
```

## Real-Time Execution Visualization

The designer provides real-time visualization of workflow execution:

- **Node Status:** Color-coded nodes (pending, executing, completed, failed)
- **Execution Flow:** Animated edge highlighting showing current path
- **State Inspection:** Click nodes to view current state
- **Metrics:** Execution time, success rate per node

## Export/Import

### Export Workflow

```bash
curl http://localhost:5000/api/workflows/{id}/export
```

Returns workflow definition as JSON.

### Import Workflow

```bash
curl -X POST http://localhost:5000/api/workflows/import \
  -H "Content-Type: application/json" \
  -d @workflow.json
```

## Best Practices

1. **Validate Before Execution:** Always validate workflows before running
2. **Use Descriptive Names:** Name nodes and edges clearly
3. **Document Complex Logic:** Add descriptions to complex nodes
4. **Test Incrementally:** Test workflows as you build them
5. **Version Control:** Export workflows to version control

## Related Documentation

- [Workflow Guide](./WORKFLOW.md)
- [Multi-Agent Workflows](../examples/STATE_MACHINE_INTEGRATION.md)
- [API Reference](./API_REFERENCE.md)

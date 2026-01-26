# DotNetAgents Examples

This directory contains example documentation and code samples demonstrating DotNetAgents features.

## Examples Documentation

### Core Features

- **[DISTRIBUTED_TRACING.md](DISTRIBUTED_TRACING.md)** - Complete guide to distributed tracing with OpenTelemetry
  - OpenTelemetry setup
  - Multi-agent workflow tracing
  - Chain and LLM call tracing
  - Correlation ID propagation
  - Multiple exporters (Console, OTLP)
  - **Sample:** `samples/DotNetAgents.Samples.Tracing/`

- **[PLUGIN_ARCHITECTURE.md](PLUGIN_ARCHITECTURE.md)** - Comprehensive plugin architecture examples
  - Creating plugins
  - Plugin registration and discovery
  - Plugin dependencies
  - Plugin lifecycle management
  - Complete working examples

### Integration Examples

- **[BEHAVIOR_TREE_INTEGRATION.md](BEHAVIOR_TREE_INTEGRATION.md)** - Behavior tree integration examples
- **[STATE_MACHINE_INTEGRATION.md](STATE_MACHINE_INTEGRATION.md)** - State machine integration examples

## Sample Projects

All sample projects are located in `samples/` directory. See [Samples README](../../samples/README.md) for complete list.

### Quick Reference

| Sample | Features Demonstrated |
|--------|----------------------|
| **BasicChain** | Chain composition, prompt templates, sequential chains |
| **AgentWithTools** | Agent executor, tool registry, state machines, behavior trees |
| **Workflow** | Stateful workflows, checkpointing, graph execution |
| **RAG** | Document loading, embeddings, vector stores, retrieval |
| **Education** | Educational extensions, pedagogy, assessment, state machines |
| **MultiAgent** | Supervisor-worker pattern, agent registry, worker pool |
| **StateMachines** | State machine patterns, lifecycle management |
| **TasksAndKnowledge** | Task management, knowledge capture, bootstrap generation |
| **Tracing** | Distributed tracing, OpenTelemetry integration |
| **JARVISVoice** | Voice command processing, state machines, behavior trees |

## Running Examples

### Distributed Tracing Example

```bash
cd samples/DotNetAgents.Samples.Tracing
dotnet run
```

This demonstrates:
- Agent registration tracing
- Task submission tracing
- Inter-agent message tracing
- End-to-end workflow tracing

### Plugin Architecture Example

See [PLUGIN_ARCHITECTURE.md](PLUGIN_ARCHITECTURE.md) for complete code examples.

## Additional Resources

- [Integration Guide](../guides/INTEGRATION_GUIDE.md) - Complete integration examples
- [Samples README](../../samples/README.md) - All sample projects
- [API Reference](../guides/API_REFERENCE.md) - API documentation
- [Behavior Trees Guide](../guides/BEHAVIOR_TREES.md) - Behavior tree patterns
- [Message Buses Guide](../guides/MESSAGE_BUSES.md) - Message bus implementations
- [Vector Stores Guide](../guides/VECTOR_STORES.md) - Vector store comparison
- [Document Loaders Guide](../guides/DOCUMENT_LOADERS.md) - Document loader usage
- [LLM Providers Guide](../guides/LLM_PROVIDERS.md) - Provider comparison

---

**Last Updated:** January 2026

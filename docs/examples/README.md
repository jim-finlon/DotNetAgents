# DotNetAgents Examples

This directory contains example documentation and code samples demonstrating DotNetAgents features.

## Examples

### Distributed Tracing

**[DISTRIBUTED_TRACING.md](DISTRIBUTED_TRACING.md)** - Complete guide to distributed tracing with OpenTelemetry

**Features:**
- OpenTelemetry setup
- Multi-agent workflow tracing
- Chain and LLM call tracing
- Correlation ID propagation
- Multiple exporters (Console, OTLP)

**Sample Project:** `samples/DotNetAgents.Samples.Tracing/`

## Running Examples

### Distributed Tracing Example

```bash
cd samples/DotNetAgents.Samples.Tracing
dotnet run
```

This will demonstrate:
- Agent registration tracing
- Task submission tracing
- Inter-agent message tracing
- End-to-end workflow tracing

## Additional Resources

- [Integration Guide](../guides/INTEGRATION_GUIDE.md) - Complete integration examples
- [Samples README](../../samples/README.md) - All sample projects
- [API Reference](../guides/API_REFERENCE.md) - API documentation

---

**Last Updated:** January 2026

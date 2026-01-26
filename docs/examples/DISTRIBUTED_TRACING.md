# Distributed Tracing with DotNetAgents

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

DotNetAgents provides comprehensive distributed tracing support using OpenTelemetry. This guide demonstrates how to set up and use distributed tracing across multi-agent workflows, chains, and LLM calls.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Configuration](#configuration)
3. [Tracing Multi-Agent Workflows](#tracing-multi-agent-workflows)
4. [Tracing Chains](#tracing-chains)
5. [Tracing LLM Calls](#tracing-llm-calls)
6. [Correlation IDs](#correlation-ids)
7. [Exporting Traces](#exporting-traces)
8. [Best Practices](#best-practices)

## Quick Start

### 1. Install Required Packages

```bash
dotnet add package DotNetAgents.Observability
dotnet add package OpenTelemetry.Exporter.Console
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

### 2. Configure OpenTelemetry

```csharp
using DotNetAgents.Observability;
using DotNetAgents.Observability.Tracing;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry with DotNetAgents instrumentation
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("MyAgentService", serviceVersion: "1.0.0"))
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddHttpClientInstrumentation()
               .AddConsoleExporter(); // For development
        
        // For production, use OTLP exporter
        // tracing.AddOtlpExporter(options =>
        // {
        //     options.Endpoint = new Uri("http://jaeger:4317");
        // });
    });

var app = builder.Build();
```

## Configuration

### Basic Configuration

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddHttpClientInstrumentation();
    });
```

### Advanced Configuration

```csharp
services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "MyAgentService",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = "production",
            ["service.namespace"] = "agents"
        }))
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddHttpClientInstrumentation()
               .AddAspNetCoreInstrumentation()
               .AddSource("MyCustomSource")
               .SetSampler(new TraceIdRatioBasedSampler(1.0)) // 100% sampling
               .AddOtlpExporter(options =>
               {
                   options.Endpoint = new Uri("http://jaeger:4317");
                   options.Protocol = OtlpExportProtocol.Grpc;
               });
    });
```

## Tracing Multi-Agent Workflows

### Example: Supervisor-Worker Pattern with Tracing

```csharp
using System.Diagnostics;
using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.Supervisor;
using DotNetAgents.Observability.Agents;

public class TracedMultiAgentExample
{
    private readonly ISupervisorAgent _supervisor;
    private readonly IAgentMessageBus _messageBus;
    private readonly IAgentRegistry _registry;
    private static readonly ActivitySource ActivitySource = new("MyApplication");

    public async Task ExecuteWorkflowAsync(string correlationId)
    {
        // Start root activity for the entire workflow
        using var rootActivity = ActivitySource.StartActivity("workflow.execute");
        rootActivity?.SetTag("workflow.name", "multi-agent-task");
        rootActivity?.SetTag("correlation.id", correlationId);

        // Register agents with tracing
        var agentInfo = new AgentInfo
        {
            AgentId = "worker-1",
            AgentType = "worker",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities
            {
                AgentId = "worker-1",
                AgentType = "worker",
                SupportedTools = new[] { "calculator", "web_search" }
            }
        };

        using (var registrationActivity = AgentTracingExtensions
            .StartAgentRegistrationActivity(agentInfo))
        {
            await _registry.RegisterAsync(agentInfo.Capabilities);
        }

        // Submit task with tracing
        var task = new WorkerTask
        {
            TaskId = Guid.NewGuid().ToString(),
            TaskType = "calculation",
            RequiredCapability = "calculator",
            Priority = TaskPriority.Normal
        };

        using (var taskActivity = AgentTracingExtensions
            .StartTaskSubmissionActivity(task, correlationId))
        {
            await _supervisor.SubmitTaskAsync(task);
        }

        // Send message with tracing
        var message = new AgentMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageType = "task_assignment",
            FromAgentId = "supervisor",
            ToAgentId = "worker-1",
            CorrelationId = correlationId,
            Payload = new Dictionary<string, object>
            {
                ["task_id"] = task.TaskId
            }
        };

        using (var messageActivity = AgentTracingExtensions
            .StartAgentMessageActivity(message, correlationId))
        {
            await _messageBus.SendAsync(message);
        }
    }
}
```

### Tracing Agent-to-Agent Communication

```csharp
public async Task SendTracedMessageAsync(
    string fromAgentId,
    string toAgentId,
    string messageType,
    object payload,
    string? correlationId = null)
{
    var message = new AgentMessage
    {
        MessageId = Guid.NewGuid().ToString(),
        MessageType = messageType,
        FromAgentId = fromAgentId,
        ToAgentId = toAgentId,
        CorrelationId = correlationId ?? Activity.Current?.Id,
        Payload = payload
    };

    using var activity = AgentTracingExtensions
        .StartAgentMessageActivity(message, correlationId);
    
    try
    {
        await _messageBus.SendAsync(message);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.RecordException(ex);
        throw;
    }
}
```

## Tracing Chains

```csharp
using DotNetAgents.Core.Chains;
using DotNetAgents.Observability.Tracing;
using System.Diagnostics;

public class TracedChainExample
{
    private static readonly ActivitySource ActivitySource = new("DotNetAgents.Core");

    public async Task<string> ExecuteTracedChainAsync(
        string input,
        string correlationId)
    {
        using var chainActivity = ActivitySource.StartActivity("chain.execute");
        chainActivity?.SetTag("chain.name", "my_chain");
        chainActivity?.SetTag("correlation.id", correlationId);
        chainActivity?.SetTag("chain.input", input);

        try
        {
            var chain = new LLMChain(/* ... */);
            var result = await chain.RunAsync(input);
            
            chainActivity?.SetTag("chain.output", result);
            chainActivity?.SetStatus(ActivityStatusCode.Ok);
            
            return result;
        }
        catch (Exception ex)
        {
            chainActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            chainActivity?.RecordException(ex);
            throw;
        }
    }
}
```

## Tracing LLM Calls

```csharp
using DotNetAgents.Observability.Tracing;
using System.Diagnostics;

public class TracedLLMExample
{
    private static readonly ActivitySource ActivitySource = new("DotNetAgents.Core");

    public async Task<string> CallLLMWithTracingAsync(
        string prompt,
        string modelName,
        string? correlationId = null)
    {
        using var llmActivity = ActivitySource.StartLLMCallActivity(
            modelName, 
            correlationId ?? Activity.Current?.Id);
        
        llmActivity?.SetTag("llm.prompt", prompt);
        llmActivity?.SetTag("llm.temperature", 0.7);

        try
        {
            var result = await _llmModel.GenerateAsync(prompt);
            
            llmActivity?.SetTag("llm.response", result);
            llmActivity?.SetTag("llm.tokens.used", result.TokenCount);
            llmActivity?.SetStatus(ActivityStatusCode.Ok);
            
            return result.Text;
        }
        catch (Exception ex)
        {
            llmActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            llmActivity?.RecordException(ex);
            throw;
        }
    }
}
```

## Correlation IDs

Correlation IDs allow you to trace requests across multiple services and components.

### Using ExecutionContext

```csharp
using DotNetAgents.Abstractions.Execution;

public class CorrelationIdExample
{
    private readonly IExecutionContextProvider _contextProvider;

    public async Task ProcessRequestAsync(string requestId)
    {
        // Set correlation ID in execution context
        var context = _contextProvider.GetContext();
        context.CorrelationId = requestId;

        // All activities will automatically include this correlation ID
        using var activity = ActivitySource.StartActivity("process.request");
        // activity automatically has correlation.id tag from context

        await ProcessWithAgentsAsync();
    }
}
```

### Manual Correlation ID Propagation

```csharp
public async Task PropagateCorrelationIdAsync(string correlationId)
{
    // Set correlation ID on current activity
    Activity.Current?.SetTag("correlation.id", correlationId);

    // Pass to child operations
    await CallAgentAsync(correlationId);
    await CallLLMAsync(correlationId);
    await SendMessageAsync(correlationId);
}
```

## Exporting Traces

### Console Exporter (Development)

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddConsoleExporter();
    });
```

### OTLP Exporter (Production)

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddOtlpExporter(options =>
               {
                   options.Endpoint = new Uri("http://jaeger:4317");
                   options.Protocol = OtlpExportProtocol.Grpc;
               });
    });
```

### Jaeger Exporter

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddJaegerExporter(options =>
               {
                   options.AgentHost = "localhost";
                   options.AgentPort = 6831;
               });
    });
```

### Zipkin Exporter

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddZipkinExporter(options =>
               {
                   options.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
               });
    });
```

## Best Practices

### 1. Always Use Correlation IDs

```csharp
// Good: Pass correlation ID through the call chain
public async Task ProcessAsync(string correlationId)
{
    using var activity = ActivitySource.StartActivity("process");
    activity?.SetTag("correlation.id", correlationId);
    
    await Step1Async(correlationId);
    await Step2Async(correlationId);
}

// Bad: Don't lose correlation ID
public async Task ProcessAsync()
{
    using var activity = ActivitySource.StartActivity("process");
    // Missing correlation ID!
    await Step1Async(); // Correlation ID lost
}
```

### 2. Set Meaningful Tags

```csharp
activity?.SetTag("workflow.name", "order_processing");
activity?.SetTag("workflow.stage", "payment_validation");
activity?.SetTag("agent.count", agentCount);
activity?.SetTag("task.priority", task.Priority.ToString());
```

### 3. Record Exceptions

```csharp
try
{
    await ProcessAsync();
}
catch (Exception ex)
{
    activity?.RecordException(ex);
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    throw;
}
```

### 4. Use Activity Events for Important Milestones

```csharp
activity?.AddEvent(new ActivityEvent("task.assigned", 
    tags: new ActivityTagsCollection
    {
        ["agent.id"] = agentId,
        ["task.id"] = taskId
    }));

activity?.AddEvent(new ActivityEvent("task.completed",
    tags: new ActivityTagsCollection
    {
        ["task.duration_ms"] = duration.TotalMilliseconds
    }));
```

### 5. Set Appropriate Sampling Rates

```csharp
// Development: 100% sampling
tracing.SetSampler(new TraceIdRatioBasedSampler(1.0));

// Production: 10% sampling for high-volume services
tracing.SetSampler(new TraceIdRatioBasedSampler(0.1));

// Production: 100% for critical paths
tracing.SetSampler(new TraceIdRatioBasedSampler(1.0))
       .AddProcessor(new FilteringSpanProcessor(
           new TraceIdRatioBasedSampler(0.1),
           span => span.Kind == SpanKind.Server));
```

## Viewing Traces

### Jaeger UI

1. Start Jaeger: `docker run -d -p 16686:16686 jaegertracing/all-in-one`
2. Configure OTLP exporter to point to Jaeger
3. Open http://localhost:16686

### Zipkin UI

1. Start Zipkin: `docker run -d -p 9411:9411 openzipkin/zipkin`
2. Configure Zipkin exporter
3. Open http://localhost:9411

### Azure Application Insights

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddDotNetAgentsInstrumentation()
               .AddAzureMonitorTraceExporter(options =>
               {
                   options.ConnectionString = "InstrumentationKey=...";
               });
    });
```

## Example: Complete Multi-Agent Workflow with Tracing

See `samples/DotNetAgents.Samples.Tracing/` for a complete working example demonstrating:
- End-to-end distributed tracing
- Correlation ID propagation
- Multi-agent communication tracing
- Workflow execution tracing
- LLM call tracing

## Troubleshooting

### Traces Not Appearing

1. **Check Sampling**: Ensure sampling rate is > 0
2. **Verify Exporter**: Confirm exporter is configured correctly
3. **Check Activity Source**: Ensure activity source is registered
4. **Verify Tags**: Check that activities are being created

### High Overhead

1. **Reduce Sampling**: Lower sampling rate for high-volume services
2. **Filter Activities**: Use processors to filter out noisy activities
3. **Batch Exports**: Configure batching for exporters

## Related Documentation

- [Observability Guide](../guides/OBSERVABILITY.md)
- [Multi-Agent Workflows](../architecture/MULTI_AGENT_WORKFLOWS_PLAN.md)
- [Performance Benchmarks](../PERFORMANCE_BENCHMARKS.md)

using System.Diagnostics;
using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.Supervisor;
using DotNetAgents.Agents.WorkerPool;
using DotNetAgents.Observability;
using DotNetAgents.Observability.Agents;
using DotNetAgents.Observability.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace DotNetAgents.Samples.Tracing;

/// <summary>
/// Example demonstrating distributed tracing across multi-agent workflows.
/// This example shows:
/// 1. Setting up OpenTelemetry with DotNetAgents instrumentation
/// 2. Tracing agent registration and communication
/// 3. Tracing task submission and execution
/// 4. Correlation ID propagation
/// 5. Viewing traces in console output
/// </summary>
class Program
{
    private static readonly ActivitySource ApplicationActivitySource = new("DotNetAgents.Samples.Tracing");

    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - Distributed Tracing Example");
        Console.WriteLine("==========================================\n");

        // Setup services with OpenTelemetry
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Configure OpenTelemetry with DotNetAgents instrumentation
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: "DotNetAgents.Samples.Tracing",
                    serviceVersion: "1.0.0",
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing.AddDotNetAgentsInstrumentation()
                       .AddSource("DotNetAgents.Samples.Tracing")
                       .AddConsoleExporter(); // For development - shows traces in console
            });

        // Add observability services
        services.AddDotNetAgentsObservability();

        // Register in-memory implementations
        services.AddInMemoryAgentRegistry();
        services.AddInMemoryAgentMessageBus();
        services.AddInMemoryTaskQueue();
        services.AddWorkerPool();
        services.AddSupervisorAgent();

        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<IAgentRegistry>();
        var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();
        var workerPool = serviceProvider.GetRequiredService<IWorkerPool>();
        var supervisor = serviceProvider.GetRequiredService<ISupervisorAgent>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        // Generate correlation ID for this workflow execution
        var correlationId = Guid.NewGuid().ToString();
        Console.WriteLine($"Correlation ID: {correlationId}\n");

        // Start root activity for the entire workflow
        using var rootActivity = ApplicationActivitySource.StartActivity("workflow.execute");
        rootActivity?.SetTag("workflow.name", "tracing-demo");
        rootActivity?.SetTag("correlation.id", correlationId);
        rootActivity?.SetTag("workflow.type", "multi-agent");

        try
        {
            // Step 1: Register agents with tracing
            Console.WriteLine("Step 1: Registering agents with tracing...\n");
            await RegisterAgentsWithTracingAsync(registry, workerPool, correlationId);

            // Step 2: Submit tasks with tracing
            Console.WriteLine("\nStep 2: Submitting tasks with tracing...\n");
            await SubmitTasksWithTracingAsync(supervisor, correlationId);

            // Step 3: Demonstrate agent-to-agent communication with tracing
            Console.WriteLine("\nStep 3: Agent-to-agent communication with tracing...\n");
            await DemonstrateAgentCommunicationAsync(messageBus, correlationId);

            rootActivity?.SetStatus(ActivityStatusCode.Ok);
            rootActivity?.AddEvent(new ActivityEvent("workflow.completed",
                tags: new ActivityTagsCollection
                {
                    ["workflow.duration_ms"] = (DateTimeOffset.UtcNow - rootActivity.StartTimeUtc).TotalMilliseconds
                }));

            Console.WriteLine("\n✅ Workflow completed successfully!");
            Console.WriteLine("\nNote: Check the console output above for OpenTelemetry trace spans.");
            Console.WriteLine("      In production, configure OTLP exporter to send traces to Jaeger, Zipkin, or Application Insights.");
        }
        catch (Exception ex)
        {
            rootActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            rootActivity?.RecordException(ex);
            logger.LogError(ex, "Workflow failed");
            throw;
        }

        // Give time for traces to be exported
        await Task.Delay(1000);
    }

    private static async Task RegisterAgentsWithTracingAsync(
        IAgentRegistry registry,
        IWorkerPool workerPool,
        string correlationId)
    {
        var agentCapabilities = new AgentCapabilities
        {
            AgentId = "worker-1",
            AgentType = "processor",
            SupportedTools = new[] { "calculator", "text-processor" },
            SupportedIntents = new[] { "process", "calculate" },
            MaxConcurrentTasks = 3
        };

        // Start activity for agent registration
        using var registrationActivity = AgentTracingExtensions
            .StartAgentRegistrationActivity(new AgentInfo
            {
                AgentId = agentCapabilities.AgentId,
                AgentType = agentCapabilities.AgentType,
                Status = AgentStatus.Available,
                Capabilities = agentCapabilities
            });

        registrationActivity?.SetTag("correlation.id", correlationId);

        await registry.RegisterAsync(agentCapabilities);
        await workerPool.AddWorkerAsync(agentCapabilities.AgentId);

        registrationActivity?.SetStatus(ActivityStatusCode.Ok);
        registrationActivity?.AddEvent(new ActivityEvent("agent.registered",
            tags: new ActivityTagsCollection
            {
                ["agent.id"] = agentCapabilities.AgentId,
                ["agent.type"] = agentCapabilities.AgentType
            }));

        Console.WriteLine($"  ✓ Registered agent: {agentCapabilities.AgentId}");
    }

    private static async Task SubmitTasksWithTracingAsync(
        ISupervisorAgent supervisor,
        string correlationId)
    {
        var tasks = new[]
        {
            new WorkerTask
            {
                TaskId = "task-1",
                TaskType = "calculation",
                RequiredCapability = "calculator",
                Priority = TaskPriority.High,
                Input = new Dictionary<string, object> { ["expression"] = "2 + 2" }
            },
            new WorkerTask
            {
                TaskId = "task-2",
                TaskType = "processing",
                RequiredCapability = "text-processor",
                Priority = TaskPriority.Normal,
                Input = new Dictionary<string, object> { ["text"] = "Hello, World!" }
            }
        };

        foreach (var task in tasks)
        {
            // Start activity for task submission
            using var taskActivity = AgentTracingExtensions
                .StartTaskSubmissionActivity(task, correlationId);

            taskActivity?.AddEvent(new ActivityEvent("task.created",
                tags: new ActivityTagsCollection
                {
                    ["task.id"] = task.TaskId,
                    ["task.type"] = task.TaskType
                }));

            await supervisor.SubmitTaskAsync(task);

            taskActivity?.SetStatus(ActivityStatusCode.Ok);
            taskActivity?.AddEvent(new ActivityEvent("task.submitted",
                tags: new ActivityTagsCollection
                {
                    ["task.id"] = task.TaskId
                }));

            Console.WriteLine($"  ✓ Submitted task: {task.TaskId} ({task.TaskType})");
        }
    }

    private static async Task DemonstrateAgentCommunicationAsync(
        IAgentMessageBus messageBus,
        string correlationId)
    {
        var message = new AgentMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageType = "task_assignment",
            FromAgentId = "supervisor",
            ToAgentId = "worker-1",
            CorrelationId = correlationId,
            Payload = new Dictionary<string, object>
            {
                ["task_id"] = "task-1",
                ["instruction"] = "Process this task"
            }
        };

        // Start activity for message sending
        using var messageActivity = AgentTracingExtensions
            .StartAgentMessageActivity(message, correlationId);

        messageActivity?.AddEvent(new ActivityEvent("message.created",
            tags: new ActivityTagsCollection
            {
                ["message.id"] = message.MessageId,
                ["message.type"] = message.MessageType
            }));

        try
        {
            await messageBus.SendAsync(message);
            messageActivity?.SetStatus(ActivityStatusCode.Ok);
            messageActivity?.AddEvent(new ActivityEvent("message.sent",
                tags: new ActivityTagsCollection
                {
                    ["message.id"] = message.MessageId
                }));

            Console.WriteLine($"  ✓ Sent message: {message.MessageType} (ID: {message.MessageId})");
        }
        catch (Exception ex)
        {
            messageActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            messageActivity?.RecordException(ex);
            throw;
        }
    }
}

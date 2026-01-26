using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.StateMachines;
using DotNetAgents.Agents.Supervisor;
using DotNetAgents.Agents.Supervisor.BehaviorTrees;
using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.WorkerPool;
using DotNetAgents.Ecosystem;
using DotNetAgents.Workflow.Graph;
using DotNetAgents.Workflow.MultiAgent;
using DotNetAgents.Workflow.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.MultiAgent;

/// <summary>
/// Example demonstrating a supervisor-worker pattern with multi-agent workflows.
/// This example shows how to:
/// 1. Register worker agents with capabilities
/// 2. Create a supervisor agent
/// 3. Submit tasks to workers via the supervisor
/// 4. Use workflow nodes to delegate and aggregate results
/// </summary>
class Program
{
    /// <summary>
    /// Example workflow state for processing multiple items.
    /// </summary>
    private class ProcessingState : MultiAgentWorkflowState
    {
        public List<string> ItemsToProcess { get; set; } = new();
        public List<string> ProcessedItems { get; set; } = new();
        public Dictionary<string, string> ProcessingResults { get; set; } = new();
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - Multi-Agent Supervisor-Worker Example");
        Console.WriteLine("===================================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddDotNetAgentsEcosystem(); // Enable plugin system
        services.AddStateMachines(); // Register State Machines plugin

        // Register in-memory implementations
        services.AddInMemoryAgentRegistry();
        services.AddInMemoryAgentMessageBus();
        services.AddInMemoryTaskQueue(); // This also adds InMemoryTaskStore
        services.AddWorkerPool();
        
        // Register supervisor with state machine support
        services.AddSupervisorAgentWithStateMachine(stateMachineFactory: sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var stateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<SupervisorContext>>();
            
            // Create supervisor state machine pattern
            var stateMachine = StateMachinePatterns.CreateSupervisorPattern<SupervisorContext>(
                stateMachineLogger,
                waitingTimeout: TimeSpan.FromSeconds(10));
            
            // Wrap in adapter
            return new StateMachineAdapter<SupervisorContext>(stateMachine);
        });
        
        // Register behavior tree router for intelligent task routing
        services.AddSingleton<ITaskRouter>(sp =>
        {
            var workerPool = sp.GetRequiredService<IWorkerPool>();
            var logger = sp.GetRequiredService<ILogger<TaskRoutingBehaviorTree>>();
            return new TaskRoutingBehaviorTree(workerPool, highPriorityThreshold: 7, logger);
        });

        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<IAgentRegistry>();
        var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();
        var workerPool = serviceProvider.GetRequiredService<IWorkerPool>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        // Get supervisor from service provider (configured with state machine and behavior tree routing)
        var supervisor = serviceProvider.GetRequiredService<ISupervisorAgent>();
        
        Console.WriteLine("Supervisor configured with:");
        Console.WriteLine("  - State machine for lifecycle management");
        Console.WriteLine("  - Behavior tree for intelligent task routing\n");

        // Step 1: Register worker agents with different capabilities
        Console.WriteLine("Step 1: Registering worker agents...\n");

        var worker1Capabilities = new AgentCapabilities
        {
            AgentId = "worker-1",
            AgentType = "text-processor",
            SupportedTools = new[] { "text-transform", "text-analyze" },
            SupportedIntents = new[] { "process-text", "analyze-text" },
            MaxConcurrentTasks = 3
        };

        var worker2Capabilities = new AgentCapabilities
        {
            AgentId = "worker-2",
            AgentType = "text-processor",
            SupportedTools = new[] { "text-transform", "text-summarize" },
            SupportedIntents = new[] { "process-text", "summarize-text" },
            MaxConcurrentTasks = 2
        };

        var worker3Capabilities = new AgentCapabilities
        {
            AgentId = "worker-3",
            AgentType = "data-processor",
            SupportedTools = new[] { "data-transform", "data-validate" },
            SupportedIntents = new[] { "process-data", "validate-data" },
            MaxConcurrentTasks = 4
        };

        await registry.RegisterAsync(worker1Capabilities);
        await registry.RegisterAsync(worker2Capabilities);
        await registry.RegisterAsync(worker3Capabilities);

        // Add workers to the pool
        await workerPool.AddWorkerAsync("worker-1");
        await workerPool.AddWorkerAsync("worker-2");
        await workerPool.AddWorkerAsync("worker-3");

        Console.WriteLine("Registered 3 workers:");
        Console.WriteLine("  - worker-1: text-processor (max 3 concurrent tasks)");
        Console.WriteLine("  - worker-2: text-processor (max 2 concurrent tasks)");
        Console.WriteLine("  - worker-3: data-processor (max 4 concurrent tasks)");
        
        // Demonstrate state machine integration (optional)
        Console.WriteLine("\nNote: Worker pool supports optional state machine integration.");
        Console.WriteLine("      State machines enable timed transitions, guards, and complex lifecycle management.\n");

        // Step 2: Create tasks to process
        Console.WriteLine("Step 2: Creating tasks to process...\n");

        var state = new ProcessingState
        {
            ItemsToProcess = new List<string>
            {
                "Process document A",
                "Process document B",
                "Process document C",
                "Process document D"
            }
        };

        // Step 3: Create a workflow with delegate and aggregate nodes
        Console.WriteLine("Step 3: Creating multi-agent workflow...\n");

        var delegateLogger = serviceProvider.GetRequiredService<ILogger<DelegateToWorkerNode<ProcessingState>>>();
        var aggregateLogger = serviceProvider.GetRequiredService<ILogger<AggregateResultsNode<ProcessingState>>>();
        var executorLogger = serviceProvider.GetRequiredService<ILogger<GraphExecutor<ProcessingState>>>();

        var delegateNode = new DelegateToWorkerNode<ProcessingState>(
            "delegate-tasks",
            supervisor,
            CreateTasksFromState,
            delegateLogger);
        var aggregateNode = new AggregateResultsNode<ProcessingState>(
            "aggregate-results",
            supervisor,
            AggregateResults,
            waitForAllTasks: true,
            aggregateLogger);

        var workflow = WorkflowBuilder<ProcessingState>.Create()
            .AddNode("delegate-tasks", delegateNode.ExecuteAsync)
            .AddNode("aggregate-results", aggregateNode.ExecuteAsync)
            .SetEntryPoint("delegate-tasks")
            .AddEdge("delegate-tasks", "aggregate-results")
            .Build();

        // Step 4: Execute the workflow
        Console.WriteLine("Step 4: Executing workflow...\n");
        Console.WriteLine($"Processing {state.ItemsToProcess.Count} items...\n");

        var executor = new GraphExecutor<ProcessingState>(workflow, executorLogger);
        var finalState = await executor.ExecuteAsync(state, cancellationToken: CancellationToken.None);

        // Step 5: Display results
        Console.WriteLine("\nStep 5: Results\n");
        Console.WriteLine($"Total tasks submitted: {finalState.SubmittedTasks.Count}");
        Console.WriteLine($"Completed tasks: {finalState.CompletedTaskIds.Count}");
        Console.WriteLine($"Failed tasks: {finalState.FailedTaskIds.Count}");
        Console.WriteLine($"\nProcessing Results:");
        foreach (var kvp in finalState.ProcessingResults)
        {
            Console.WriteLine($"  - {kvp.Key}: {kvp.Value}");
        }

        // Step 6: Display worker pool statistics
        Console.WriteLine("\nStep 6: Worker Pool Statistics\n");
        var stats = await workerPool.GetStatisticsAsync();
        Console.WriteLine($"Total workers: {stats.TotalWorkers}");
        Console.WriteLine($"Available workers: {stats.AvailableWorkers}");
        Console.WriteLine($"Busy workers: {stats.BusyWorkers}");
        Console.WriteLine($"Total tasks processed: {stats.TotalTasksProcessed}");

        // Step 7: Display supervisor statistics
        Console.WriteLine("\nStep 7: Supervisor Statistics\n");
        var supervisorStats = await supervisor.GetStatisticsAsync();
        Console.WriteLine($"Total tasks submitted: {supervisorStats.TotalTasksSubmitted}");
        Console.WriteLine($"Completed tasks: {supervisorStats.TasksCompleted}");
        Console.WriteLine($"Failed tasks: {supervisorStats.TasksFailed}");
        Console.WriteLine($"Pending tasks: {supervisorStats.TasksPending}");
        Console.WriteLine($"In progress tasks: {supervisorStats.TasksInProgress}");
        
        // Display state machine state if enabled
        if (supervisor is SupervisorAgent supervisorAgent)
        {
            var currentState = supervisorAgent.GetCurrentState();
            if (!string.IsNullOrEmpty(currentState))
            {
                Console.WriteLine($"Current supervisor state: {currentState}");
            }
        }

        Console.WriteLine("\nExample completed successfully!");
    }

    /// <summary>
    /// Creates worker tasks from the workflow state.
    /// </summary>
    private static IEnumerable<WorkerTask> CreateTasksFromState(ProcessingState state)
    {
        var tasks = new List<WorkerTask>();
        var taskIdCounter = 1;

        foreach (var item in state.ItemsToProcess)
        {
            tasks.Add(new WorkerTask
            {
                TaskId = $"task-{taskIdCounter++}",
                TaskType = "process-item",
                Input = new Dictionary<string, object>
                {
                    ["item"] = item,
                    ["action"] = "process"
                },
                RequiredCapability = "text-processor",
                Priority = 5
            });
        }

        return tasks;
    }

    /// <summary>
    /// Aggregates task results into the workflow state.
    /// </summary>
    private static ProcessingState AggregateResults(
        ProcessingState state,
        Dictionary<string, WorkerTaskResult> results)
    {
        foreach (var (taskId, result) in results)
        {
            if (result.Success && result.Output is Dictionary<string, object> output)
            {
                var processedItem = output.TryGetValue("processed", out var value)
                    ? value?.ToString() ?? "Unknown"
                    : "Unknown";

                state.ProcessedItems.Add(processedItem);
                state.ProcessingResults[taskId] = $"Processed by {result.WorkerAgentId}: {processedItem}";
            }
            else
            {
                state.ProcessingResults[taskId] = $"Failed: {result.ErrorMessage ?? "Unknown error"}";
            }
        }

        return state;
    }
}

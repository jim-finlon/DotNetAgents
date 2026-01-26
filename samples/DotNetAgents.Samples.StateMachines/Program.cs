using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.StateMachines;
using DotNetAgents.Ecosystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.StateMachines;

/// <summary>
/// Example demonstrating state machine usage for agent lifecycle management.
/// This example shows how to:
/// 1. Create state machines with common patterns
/// 2. Register state machines with agent registry
/// 3. Use state machines for worker pool selection
/// 4. Trigger state transitions via message bus
/// </summary>
class Program
{
    /// <summary>
    /// Example agent context for state machine.
    /// </summary>
    private class AgentContext
    {
        public string AgentId { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public DateTime? LastTaskTime { get; set; }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - State Machine Example");
        Console.WriteLine("=====================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddDotNetAgentsEcosystem(); // Enable plugin system
        services.AddStateMachines(); // Register State Machines plugin
        services.AddInMemoryAgentRegistry();
        services.AddInMemoryAgentMessageBus();

        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<IAgentRegistry>();
        var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        // Step 1: Register an agent
        Console.WriteLine("Step 1: Registering agent...\n");

        var agentCapabilities = new AgentCapabilities
        {
            AgentId = "agent-1",
            AgentType = "worker",
            SupportedTools = new[] { "process", "analyze" },
            SupportedIntents = new[] { "work", "analyze" },
            MaxConcurrentTasks = 3
        };

        await registry.RegisterAsync(agentCapabilities);
        Console.WriteLine($"Registered agent: {agentCapabilities.AgentId}\n");

        // Step 2: Create a worker pool state machine using pattern
        Console.WriteLine("Step 2: Creating worker pool state machine...\n");

        var stateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<AgentContext>>();
        var stateMachine = StateMachinePatterns.CreateWorkerPoolPattern<AgentContext>(
            stateMachineLogger,
            cooldownDuration: TimeSpan.FromSeconds(2));

        var context = new AgentContext { AgentId = "agent-1" };
        Console.WriteLine($"Initial state: {stateMachine.CurrentState}\n");

        // Step 3: Register state machine with registry
        Console.WriteLine("Step 3: Registering state machine...\n");

        // Note: StateBasedWorkerPool uses AgentStateMachineRegistry<object>
        // For this example, we'll use object as the context type
        var objectStateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<object>>();
        var objectStateMachine = StateMachinePatterns.CreateWorkerPoolPattern<object>(
            objectStateMachineLogger,
            cooldownDuration: TimeSpan.FromSeconds(2));

        var objectContext = new { AgentId = "agent-1" } as object;
        var registryLogger = loggerFactory.CreateLogger<AgentStateMachineRegistry<object>>();
        var stateMachineRegistry = new AgentStateMachineRegistry<object>(registry, registryLogger);
        await stateMachineRegistry.RegisterAsync("agent-1", objectStateMachine);

        Console.WriteLine($"State machine registered for agent-1\n");

        // Step 4: Demonstrate state transitions
        Console.WriteLine("Step 4: Demonstrating state transitions...\n");

        Console.WriteLine($"Current state: {objectStateMachine.CurrentState}");
        await objectStateMachine.TransitionAsync("Busy", objectContext!);
        Console.WriteLine($"After transition to Busy: {objectStateMachine.CurrentState}");

        await Task.Delay(100);
        await objectStateMachine.TransitionAsync("CoolingDown", objectContext!);
        Console.WriteLine($"After transition to CoolingDown: {objectStateMachine.CurrentState}");

        // Wait for cooldown timeout
        Console.WriteLine("\nWaiting for cooldown timeout (2 seconds)...");
        await Task.Delay(2100);
        Console.WriteLine($"After cooldown: {objectStateMachine.CurrentState} (should be Available)\n");

        // Step 5: Demonstrate state-based worker selection
        Console.WriteLine("Step 5: Demonstrating state-based worker selection...\n");

        var workerPoolLogger = loggerFactory.CreateLogger<StateBasedWorkerPool>();
        var baseWorkerPool = new DotNetAgents.Agents.WorkerPool.WorkerPool(registry);
        var stateBasedPool = new StateBasedWorkerPool(
            baseWorkerPool,
            stateMachineRegistry,
            workerPoolLogger);

        await baseWorkerPool.AddWorkerAsync("agent-1");

        // Transition to Available
        await objectStateMachine.TransitionAsync("Available", objectContext!);
        var availableWorker = await stateBasedPool.GetAvailableWorkerInStateAsync("Available");
        Console.WriteLine($"Worker in Available state: {availableWorker?.AgentId ?? "None"}");

        // Transition to Busy
        await objectStateMachine.TransitionAsync("Busy", objectContext!);
        var busyWorker = await stateBasedPool.GetAvailableWorkerInStateAsync("Available");
        Console.WriteLine($"Worker in Available state (should be None): {busyWorker?.AgentId ?? "None"}\n");

        // Step 6: Demonstrate message bus integration
        Console.WriteLine("Step 6: Demonstrating message bus integration...\n");

        var integrationLogger = loggerFactory.CreateLogger<MessageBusStateMachineIntegration<object>>();
        var integration = new MessageBusStateMachineIntegration<object>(
            messageBus,
            stateMachineRegistry,
            integrationLogger);

        // Subscribe to state transitions
        var subscription = await integration.SubscribeToStateTransitionsAsync("agent-1");

        // Send state transition request
        await objectStateMachine.TransitionAsync("Available", objectContext!);
        Console.WriteLine($"Current state before message: {objectStateMachine.CurrentState}");
        await integration.SendStateTransitionRequestAsync("agent-1", "Busy");
        await Task.Delay(100); // Give time for message processing
        Console.WriteLine($"Current state after message: {stateMachineRegistry.GetAgentState("agent-1")}\n");

        subscription.Dispose();

        // Step 7: Demonstrate error recovery pattern
        Console.WriteLine("Step 7: Demonstrating error recovery pattern...\n");

        var errorRecoveryLogger = loggerFactory.CreateLogger<AgentStateMachine<object>>();
        var errorRecoveryMachine = StateMachinePatterns.CreateErrorRecoveryPattern<object>(errorRecoveryLogger).Build();

        var errorContext = new { AgentId = "agent-2" } as object;
        Console.WriteLine($"Error recovery machine initial state: {errorRecoveryMachine.CurrentState}");

        await errorRecoveryMachine.TransitionAsync("Working", errorContext!);
        Console.WriteLine($"After transition to Working: {errorRecoveryMachine.CurrentState}");

        await errorRecoveryMachine.TransitionAsync("Error", errorContext!);
        Console.WriteLine($"After transition to Error: {errorRecoveryMachine.CurrentState}");

        await errorRecoveryMachine.TransitionAsync("Recovery", errorContext!);
        Console.WriteLine($"After transition to Recovery: {errorRecoveryMachine.CurrentState}");

        await errorRecoveryMachine.TransitionAsync("Idle", errorContext!);
        Console.WriteLine($"After transition to Idle: {errorRecoveryMachine.CurrentState}\n");

        Console.WriteLine("Example completed successfully!");
    }
}

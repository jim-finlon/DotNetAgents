using DotNetAgents.Agents.StateMachines;
using DotNetAgents.Voice;
using DotNetAgents.Voice.BehaviorTrees;
using DotNetAgents.Voice.Dialog;
using DotNetAgents.Voice.Orchestration;
using DotNetAgents.Voice.StateMachines;
using DotNetAgents.Voice.Dialog.StateMachines;
using DotNetAgents.Voice.IntentClassification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.JARVISVoice;

/// <summary>
/// Sample application demonstrating JARVIS Voice with State Machines and Behavior Trees.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== JARVIS Voice Sample with State Machines and Behavior Trees ===\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Register LLM provider (mock for demo - replace with actual provider in production)
        // services.AddOpenAI(options => { ... });

        // Register voice command services
        // Note: This requires an LLM provider. For demo purposes, we'll skip full registration.
        // services.AddVoiceCommands();

        // Register a mock MCP adapter router (required for orchestrator)
        services.AddSingleton<Mcp.Routing.IMcpAdapterRouter>(sp =>
        {
            return new MockServices.MockMcpAdapterRouter();
        });

        // Register command parser (mock for demo)
        services.AddScoped<Parsing.ICommandParser>(sp =>
        {
            return new MockServices.MockCommandParser();
        });

        // Register command orchestration with state machine and behavior tree
        services.AddCommandOrchestrationWithStateMachine(
            stateMachineFactory: sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var stateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<VoiceSessionContext>>();
                var stateMachine = VoiceSessionStateMachinePattern.CreateVoiceSessionPattern<VoiceSessionContext>(
                    stateMachineLogger,
                    listeningTimeout: TimeSpan.FromSeconds(10),
                    processingTimeout: TimeSpan.FromSeconds(30));
                return new VoiceSessionStateMachineAdapter<VoiceSessionContext>(stateMachine);
            },
            behaviorTreeFactory: sp =>
            {
                var logger = sp.GetRequiredService<ILogger<CommandProcessingBehaviorTree>>();
                return new CommandProcessingBehaviorTree(
                    lowConfidenceThreshold: 0.6,
                    logger);
            });

        // Register dialog management with state machine
        services.AddDialogManagementWithStateMachine(
            stateMachineFactory: sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var stateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<DialogContext>>();
                var stateMachine = DialogStateMachinePattern.CreateDialogPattern<DialogContext>(
                    stateMachineLogger,
                    collectingTimeout: TimeSpan.FromMinutes(5),
                    confirmingTimeout: TimeSpan.FromMinutes(2));
                return new DialogStateMachineAdapter<DialogContext>(stateMachine);
            });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        Console.WriteLine("Services configured with:");
        Console.WriteLine("  ✓ Voice Session State Machine (Idle → Listening → Processing → Responding → Idle)");
        Console.WriteLine("  ✓ Command Processing Behavior Tree (Simple/MultiStep/Ambiguous strategies)");
        Console.WriteLine("  ✓ Dialog State Machine (Initial → CollectingInfo → Confirming → Executing → Completed)\n");

        // Demo 1: Voice Session State Machine
        await DemonstrateVoiceSessionStateMachine(serviceProvider, logger);

        // Demo 2: Behavior Tree Command Processing
        await DemonstrateBehaviorTreeCommandProcessing(serviceProvider, logger);

        // Demo 3: Dialog State Machine
        await DemonstrateDialogStateMachine(serviceProvider, logger);

        Console.WriteLine("\n=== Sample Complete ===");
    }

    static async Task DemonstrateVoiceSessionStateMachine(IServiceProvider serviceProvider, ILogger logger)
    {
        Console.WriteLine("--- Demo 1: Voice Session State Machine ---");
        Console.WriteLine("Demonstrating voice session lifecycle tracking...\n");

        var orchestrator = serviceProvider.GetRequiredService<ICommandWorkflowOrchestrator>();
        var parser = serviceProvider.GetRequiredService<Parsing.ICommandParser>();
        var userId = Guid.NewGuid();

        // Parse command first to get intent
        var commandText = "create a note about the meeting";
        Console.WriteLine($"Command: \"{commandText}\"");
        Console.WriteLine($"User ID: {userId}\n");

        try
        {
            // Parse the command to get intent
            var intent = await parser.ParseAsync(commandText, null, CancellationToken.None);

            // Create a sample command state with parsed intent
            var commandState = new CommandState
            {
                UserId = userId,
                RawText = commandText,
                Source = "cli",
                Intent = intent
            };

            Console.WriteLine($"Parsed Intent: {intent.Domain}.{intent.Action}");
            Console.WriteLine($"  Confidence: {intent.Confidence:F2}");
            Console.WriteLine($"  Complete: {intent.IsComplete}\n");

            // Execute command (this will trigger state machine transitions)
            var result = await orchestrator.ExecuteAsync(commandState, CancellationToken.None);

            // Get session state
            var sessionState = orchestrator.GetSessionState(userId);
            if (sessionState != null)
            {
                Console.WriteLine($"Voice Session State: {sessionState.SessionId}");
                Console.WriteLine($"  Current State: {sessionState.IsActive ? "Active" : "Inactive"}");
                Console.WriteLine($"  Listening Started: {sessionState.ListeningStartedAt}");
                Console.WriteLine($"  Processing Started: {sessionState.ProcessingStartedAt}");
                Console.WriteLine($"  Responding Started: {sessionState.RespondingStartedAt}");
                Console.WriteLine($"  Error Count: {sessionState.ErrorCount}");
            }

            Console.WriteLine($"\nCommand Status: {result.Status}");
            Console.WriteLine($"Command ID: {result.CommandId}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n");
            logger.LogError(ex, "Failed to demonstrate voice session state machine");
        }
    }

    static async Task DemonstrateBehaviorTreeCommandProcessing(IServiceProvider serviceProvider, ILogger logger)
    {
        Console.WriteLine("--- Demo 2: Behavior Tree Command Processing ---");
        Console.WriteLine("Demonstrating intelligent command strategy selection...\n");

        // Create a behavior tree instance for demonstration
        var behaviorTreeLogger = serviceProvider.GetRequiredService<ILogger<CommandProcessingBehaviorTree>>();
        var behaviorTree = new CommandProcessingBehaviorTree(
            lowConfidenceThreshold: 0.6,
            behaviorTreeLogger);

        // Test cases
        var testCases = new[]
        {
            new CommandState
            {
                UserId = Guid.NewGuid(),
                RawText = "create a note",
                Source = "cli",
                Intent = new Voice.IntentClassification.Intent
                {
                    Domain = "notes",
                    Action = "create",
                    Confidence = 0.95,
                    Parameters = new Dictionary<string, object> { { "title", "Meeting Notes" } }
                }
            },
            new CommandState
            {
                UserId = Guid.NewGuid(),
                RawText = "do something",
                Source = "cli",
                Intent = new Voice.IntentClassification.Intent
                {
                    Domain = "unknown",
                    Action = "unknown",
                    Confidence = 0.4
                }
            }
        };

        foreach (var commandState in testCases)
        {
            Console.WriteLine($"Command: \"{commandState.RawText}\"");
            Console.WriteLine($"  Intent Confidence: {commandState.Intent?.Confidence ?? 0:F2}");
            Console.WriteLine($"  Intent Complete: {commandState.Intent?.IsComplete ?? false}");

            var context = await behaviorTree.ProcessCommandAsync(commandState, CancellationToken.None);

            Console.WriteLine($"  Strategy: {context.Strategy}");
            if (context.NeedsClarification)
            {
                Console.WriteLine($"  Clarification: {context.ClarificationMessage}");
            }
            Console.WriteLine();
        }
    }

    static async Task DemonstrateDialogStateMachine(IServiceProvider serviceProvider, ILogger logger)
    {
        Console.WriteLine("--- Demo 3: Dialog State Machine ---");
        Console.WriteLine("Demonstrating multi-turn dialog state management...\n");

        var dialogManager = serviceProvider.GetRequiredService<IDialogManager>();
        var userId = Guid.NewGuid();

        try
        {
            // Note: This is a simplified demo. In a real scenario, you would have dialog handlers registered.
            Console.WriteLine($"Starting dialog for user {userId}");
            Console.WriteLine("(Note: Full dialog demo requires registered dialog handlers)\n");

            // The dialog manager would transition through states:
            // Initial → CollectingInfo → Confirming → Executing → Completed
            Console.WriteLine("Dialog State Machine Flow:");
            Console.WriteLine("  1. Initial - Dialog starts");
            Console.WriteLine("  2. CollectingInfo - Gathering required information");
            Console.WriteLine("  3. Confirming - Requesting user confirmation (if needed)");
            Console.WriteLine("  4. Executing - Executing the dialog action");
            Console.WriteLine("  5. Completed - Dialog finished successfully\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n");
        }
    }
}

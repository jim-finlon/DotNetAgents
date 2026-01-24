using DotNetAgents.Abstractions.Models;
using DotNetAgents.Providers.OpenAI;
using DotNetAgents.Workflow.Checkpoints;
using DotNetAgents.Workflow.Execution;
using DotNetAgents.Workflow.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.Workflow;

/// <summary>
/// Example demonstrating a stateful workflow with checkpointing.
/// </summary>
class Program
{
    private record WorkflowState
    {
        public string Input { get; init; } = string.Empty;
        public string Step1Result { get; init; } = string.Empty;
        public string Step2Result { get; init; } = string.Empty;
        public string FinalResult { get; init; } = string.Empty;
        public int StepCount { get; init; }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - Workflow Example");
        Console.WriteLine("================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Add OpenAI provider (requires OPENAI_API_KEY environment variable)
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("ERROR: OPENAI_API_KEY environment variable is not set.");
            Console.WriteLine("Please set it to your OpenAI API key to run this example.");
            return;
        }

        services.AddOpenAI(apiKey, "gpt-3.5-turbo");

        var serviceProvider = services.BuildServiceProvider();
        var llm = serviceProvider.GetRequiredService<ILLMModel<string, string>>();

        // Create a simple workflow that processes text through multiple steps
        var workflow = WorkflowBuilder<WorkflowState>.Create()
            .AddNode("start", async (state, ct) =>
            {
                Console.WriteLine($"  [Start] Processing input: {state.Input}");
                return state with { StepCount = state.StepCount + 1 };
            })
            .AddNode("step1", async (state, ct) =>
            {
                Console.WriteLine($"  [Step 1] Analyzing input...");
                var prompt = $"Analyze this text and provide a summary: {state.Input}";
                var result = await llm.GenerateAsync(prompt, cancellationToken: ct).ConfigureAwait(false);
                return state with
                {
                    Step1Result = result,
                    StepCount = state.StepCount + 1
                };
            })
            .AddNode("step2", async (state, ct) =>
            {
                Console.WriteLine($"  [Step 2] Generating insights...");
                var prompt = $"Based on this analysis, provide key insights:\n\n{state.Step1Result}";
                var result = await llm.GenerateAsync(prompt, cancellationToken: ct).ConfigureAwait(false);
                return state with
                {
                    Step2Result = result,
                    StepCount = state.StepCount + 1
                };
            })
            .AddNode("finalize", async (state, ct) =>
            {
                Console.WriteLine($"  [Finalize] Combining results...");
                var finalResult = $"Summary: {state.Step1Result}\n\nInsights: {state.Step2Result}";
                return state with
                {
                    FinalResult = finalResult,
                    StepCount = state.StepCount + 1
                };
            })
            .AddEdge("start", "step1")
            .AddEdge("step1", "step2")
            .AddEdge("step2", "finalize")
            .SetEntryPoint("start")
            .AddExitPoint("finalize")
            .Build();

        // Create checkpoint store and serializer
        var checkpointStore = new InMemoryCheckpointStore<WorkflowState>();
        var serializer = new JsonStateSerializer<WorkflowState>();

        // Create executor
        var executor = new GraphExecutor<WorkflowState>(
            workflow,
            checkpointStore,
            serializer);

        // Execute workflow
        var initialState = new WorkflowState
        {
            Input = "Artificial intelligence is revolutionizing how we work, learn, and interact with technology."
        };

        Console.WriteLine("Executing workflow...\n");
        var finalState = await executor.ExecuteAsync(initialState, cancellationToken: default).ConfigureAwait(false);

        Console.WriteLine("\nWorkflow completed!");
        Console.WriteLine($"\nFinal Result:\n{finalState.FinalResult}");
        Console.WriteLine($"\nTotal steps executed: {finalState.StepCount}");
    }
}
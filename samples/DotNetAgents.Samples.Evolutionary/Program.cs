using DotNetAgents.Agents.Evolutionary;
using DotNetAgents.Agents.Evolutionary.Evolution;
using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Agents.Evolutionary.Integration;
using DotNetAgents.Abstractions.Tools;
using DotNetAgents.Core.Tools;
using DotNetAgents.Tools.BuiltIn;
using DotNetAgents.Ecosystem;
using DotNetAgents.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register DotNetAgents ecosystem
        services.AddDotNetAgentsEcosystem();
        
        // Register OpenAI provider
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("Warning: OPENAI_API_KEY not set. Some features may not work.");
        }
        else
        {
            services.AddOpenAI(apiKey, "gpt-3.5-turbo");
        }

        // Register HTTP client for tools
        services.AddHttpClient();
        
        // Register tools
        services.AddSingleton<IToolRegistry>(sp =>
        {
            var registry = new ToolRegistry();
            registry.Register(new CalculatorTool());
            registry.Register(new DateTimeTool());
            return registry;
        });

        // Register evolutionary agents
        services.AddEvolutionaryAgents(config =>
        {
            config.SelectionOperatorType = "Tournament";
            config.CrossoverOperatorType = "Uniform";
            config.MutationOperatorType = "Standard";
            
            // Fitness weights
            config.CompletionWeight = 0.4;
            config.QualityWeight = 0.4;
            config.EfficiencyWeight = 0.2;
            
            // Evaluation tasks
            config.EvaluationTasks = new List<EvaluationTask>
            {
                new EvaluationTask
                {
                    Id = "math-1",
                    Input = "What is 15 * 23? Show your work.",
                    ExpectedKeywords = new List<string> { "345", "15", "23" }
                },
                new EvaluationTask
                {
                    Id = "math-2",
                    Input = "Calculate 42 + 17",
                    ExpectedKeywords = new List<string> { "59" }
                },
                new EvaluationTask
                {
                    Id = "knowledge-1",
                    Input = "What is the capital of France?",
                    ExpectedKeywords = new List<string> { "Paris" }
                },
                new EvaluationTask
                {
                    Id = "knowledge-2",
                    Input = "Who wrote Romeo and Juliet?",
                    ExpectedKeywords = new List<string> { "Shakespeare" }
                },
                new EvaluationTask
                {
                    Id = "tool-usage",
                    Input = "What is the current date and time?",
                    ExpectedKeywords = new List<string> { "date", "time" }
                }
            };
        });
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("=== Evolutionary Agents Sample ===");
    logger.LogInformation("This sample demonstrates how agents evolve over generations");
    logger.LogInformation("to improve their performance through genetic algorithms.\n");

    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))
    {
        logger.LogWarning("OPENAI_API_KEY not set. Evolution will run but may have limited functionality.");
        logger.LogInformation("Set OPENAI_API_KEY environment variable for full functionality.\n");
    }

    var engine = host.Services.GetRequiredService<IEvolutionEngine>();
    var adapter = host.Services.GetRequiredService<ChromosomeAdapter>();

    logger.LogInformation("Starting evolution...");
    logger.LogInformation("Population: 30, Generations: 10 (for demo)\n");

    var result = await engine.EvolveAsync(new EvolutionConfig
    {
        PopulationSize = 30,
        EliteCount = 3,
        MutationRate = 0.05,
        CrossoverRate = 0.8,
        TerminationCondition = new TerminationCondition
        {
            MaxGenerations = 10, // Short run for demo
            TargetFitness = 0.85
        }
    }, CancellationToken.None);

    logger.LogInformation("\n=== Evolution Complete ===");
    logger.LogInformation($"Best Fitness: {result.BestFitness:F3}");
    logger.LogInformation($"Generations: {result.FinalGeneration}");
    logger.LogInformation($"Termination Reason: {result.TerminationReason}");
    logger.LogInformation($"Total Time: {result.TotalTime.TotalSeconds:F1} seconds\n");

    // Show generation history
    logger.LogInformation("Generation History:");
    foreach (var genStats in result.GenerationHistory.TakeLast(5))
    {
        logger.LogInformation(
            "  Gen {Generation}: Best={Best:F3}, Avg={Avg:F3}, Diversity={Diversity:F3}",
            genStats.Generation,
            genStats.BestFitness,
            genStats.AverageFitness,
            genStats.Diversity);
    }

    // Test best agent
    logger.LogInformation("\n=== Testing Best Agent ===");
    
    var llm = host.Services.GetService<DotNetAgents.Abstractions.Models.ILLMModel<string, string>>();
    var toolRegistry = host.Services.GetRequiredService<DotNetAgents.Abstractions.Tools.IToolRegistry>();

    if (llm != null)
    {
        var bestAgent = adapter.ToAgentExecutor(
            result.BestAgent,
            llm,
            toolRegistry);

        var testQuestions = new[]
        {
            "What is 10 * 5?",
            "What is the capital of France?",
            "What is the current date?"
        };

        foreach (var question in testQuestions)
        {
            try
            {
                logger.LogInformation($"\nQuestion: {question}");
                var response = await bestAgent.InvokeAsync(question);
                logger.LogInformation($"Response: {response}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing agent");
            }
        }
    }
    else
    {
        logger.LogWarning("LLM not available - skipping agent test");
    }

    logger.LogInformation("\n=== Sample Complete ===");
    logger.LogInformation("The best agent chromosome has been evolved and can be used in production.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error running evolution sample");
    throw;
}

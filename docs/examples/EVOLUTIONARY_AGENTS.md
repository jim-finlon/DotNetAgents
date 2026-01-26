# Evolutionary Agents Examples

This document provides comprehensive examples of using the Evolutionary Agents plugin.

## Basic Evolution Example

```csharp
using DotNetAgents.Agents.Evolutionary;
using DotNetAgents.Agents.Evolutionary.Evolution;
using DotNetAgents.Agents.Evolutionary.Fitness;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register core services
services.AddDotNetAgentsEcosystem();
services.AddOpenAI(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
});

// Register evolutionary agents
services.AddEvolutionaryAgents(config =>
{
    config.PopulationSize = 50;
    config.SelectionOperatorType = "Tournament";
    config.CrossoverOperatorType = "Uniform";
    config.MutationOperatorType = "Standard";
    
    // Evaluation tasks
    config.EvaluationTasks = new List<EvaluationTask>
    {
        new EvaluationTask
        {
            Id = "math-1",
            Input = "What is 15 * 23?",
            ExpectedKeywords = new List<string> { "345" }
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
        }
    };
});

var serviceProvider = services.BuildServiceProvider();

// Run evolution
var engine = serviceProvider.GetRequiredService<IEvolutionEngine>();

var result = await engine.EvolveAsync(new EvolutionConfig
{
    PopulationSize = 50,
    EliteCount = 5,
    MutationRate = 0.05,
    CrossoverRate = 0.8,
    TerminationCondition = new TerminationCondition
    {
        MaxGenerations = 20,
        TargetFitness = 0.9
    }
}, cancellationToken);

Console.WriteLine($"Best Fitness: {result.BestFitness:F3}");
Console.WriteLine($"Generations: {result.FinalGeneration}");
Console.WriteLine($"Termination: {result.TerminationReason}");

// Use best agent
var adapter = serviceProvider.GetRequiredService<ChromosomeAdapter>();
var llm = serviceProvider.GetRequiredService<ILLMModel<string, string>>();
var toolRegistry = serviceProvider.GetRequiredService<IToolRegistry>();

var bestAgent = adapter.ToAgentExecutor(
    result.BestAgent,
    llm,
    toolRegistry);

var response = await bestAgent.InvokeAsync("What is 10 * 5?");
Console.WriteLine($"Response: {response}");
```

## Advanced Evolution with Speciation

```csharp
services.AddEvolutionaryAgents(config =>
{
    config.PopulationSize = 100;
    config.UseSpeciation = true;
    config.CompatibilityThreshold = 3.0;
    config.SpeciesTargetCount = 10;
    config.StagnationThreshold = 15;
    
    config.SelectionOperatorType = "Tournament";
    config.CrossoverOperatorType = "NEAT";
    config.MutationOperatorType = "Adaptive";
});

var result = await engine.EvolveAsync(new EvolutionConfig
{
    PopulationSize = 100,
    EliteCount = 10,
    MutationRate = 0.05,
    CrossoverRate = 0.8,
    UseSpeciation = true,
    CompatibilityThreshold = 3.0,
    SpeciesTargetCount = 10,
    StagnationThreshold = 15,
    TerminationCondition = new TerminationCondition
    {
        MaxGenerations = 50,
        TargetFitness = 0.95,
        StagnationGenerations = 10
    }
}, cancellationToken);
```

## Island Model Evolution

```csharp
// Create multiple population managers for islands
var islandManagers = new List<IPopulationManager>
{
    serviceProvider.GetRequiredService<IPopulationManager>(), // Island 1
    serviceProvider.GetRequiredService<IPopulationManager>(), // Island 2
    serviceProvider.GetRequiredService<IPopulationManager>()  // Island 3
};

var islandModel = new IslandModel(
    islandManagers: islandManagers,
    fitnessEvaluator: serviceProvider.GetRequiredService<IFitnessEvaluator>(),
    migrationInterval: 10,
    migrationCount: 5
);

// Initialize islands
var islands = new List<List<AgentChromosome>>();
for (int i = 0; i < 3; i++)
{
    var population = await islandManagers[i].InitializePopulationAsync(50, random);
    islands.Add(population);
}

// Evolve islands
for (int generation = 0; generation < 50; generation++)
{
    islands = await islandModel.EvolveIslandsAsync(
        islands,
        generation,
        random,
        cancellationToken);
    
    // Log best fitness from each island
    foreach (var island in islands)
    {
        var best = island.OrderByDescending(c => c.Fitness).First();
        Console.WriteLine($"Island {islands.IndexOf(island)}: Fitness = {best.Fitness:F3}");
    }
}
```

## Hive Mind Integration

```csharp
var hiveMind = serviceProvider.GetRequiredService<IHiveMind>();

// After evaluating a chromosome
var fitnessResult = await fitnessEvaluator.EvaluateAsync(chromosome, cancellationToken);

// Extract knowledge
var knowledge = await hiveMind.ExtractKnowledgeAsync(
    chromosomeId: chromosome.Id,
    generation: generation,
    taskInput: "Calculate 10 * 5",
    taskResult: "50",
    fitnessResult: fitnessResult,
    cancellationToken: cancellationToken);

// Store novel knowledge
foreach (var item in knowledge)
{
    var stored = await hiveMind.StoreIfNovelAsync(
        item,
        chromosome.Id,
        generation,
        noveltyThreshold: 0.8,
        cancellationToken: cancellationToken);
    
    if (stored)
    {
        Console.WriteLine($"Stored novel knowledge: {item.Title}");
    }
}

// Retrieve relevant knowledge for new agents
var relevantKnowledge = await hiveMind.GetRelevantKnowledgeAsync(
    chromosome: newChromosome,
    maxResults: 10,
    cancellationToken: cancellationToken);

Console.WriteLine($"Found {relevantKnowledge.Count} relevant knowledge items");
```

## Custom Fitness Evaluation

```csharp
public class CustomFitnessEvaluator : IFitnessEvaluator
{
    public async Task<FitnessResult> EvaluateAsync(
        AgentChromosome chromosome,
        CancellationToken cancellationToken = default)
    {
        var result = new FitnessResult();
        
        // Your custom evaluation logic
        result.CompletionRate = await EvaluateCompletionAsync(chromosome, cancellationToken);
        result.QualityScore = await EvaluateQualityAsync(chromosome, cancellationToken);
        result.EfficiencyScore = await EvaluateEfficiencyAsync(chromosome, cancellationToken);
        
        // Calculate overall fitness
        result.OverallFitness = 
            0.4 * result.CompletionRate +
            0.4 * result.QualityScore +
            0.2 * result.EfficiencyScore;
        
        return result;
    }
    
    // ... implementation details
}
```

## Monitoring Evolution Progress

```csharp
var engine = serviceProvider.GetRequiredService<IEvolutionEngine>();

// Subscribe to evolution events (if available)
// Or monitor metrics
var metrics = serviceProvider.GetRequiredService<IMeterFactory>();

// Evolution runs automatically update metrics:
// - evolution.generation.current
// - evolution.fitness.best
// - evolution.fitness.average
// - evolution.population.diversity

var result = await engine.EvolveAsync(config, cancellationToken);

// Access generation history
foreach (var genStats in result.GenerationHistory)
{
    Console.WriteLine($"Generation {genStats.Generation}:");
    Console.WriteLine($"  Best Fitness: {genStats.BestFitness:F3}");
    Console.WriteLine($"  Avg Fitness: {genStats.AverageFitness:F3}");
    Console.WriteLine($"  Diversity: {genStats.Diversity:F3}");
    Console.WriteLine($"  Species: {genStats.SpeciesCount}");
}
```

## Seeding Population with Known Agents

```csharp
var adapter = serviceProvider.GetRequiredService<ChromosomeAdapter>();
var innovationTracker = serviceProvider.GetRequiredService<InnovationTracker>();

// Create chromosome from existing agent configuration
var seedChromosome = adapter.FromAgentConfiguration(
    systemPrompt: "You are a helpful AI assistant.",
    temperature: 0.7,
    maxTokens: 4096,
    maxRetries: 3,
    modelIdentifier: "gpt-4",
    provider: "OpenAI",
    innovationTracker: innovationTracker);

// Initialize population with seed
var population = new List<AgentChromosome> { seedChromosome };

// Add random chromosomes
var factory = new AgentChromosomeFactory();
for (int i = 1; i < 100; i++)
{
    population.Add(AgentChromosomeFactory.CreateRandom(innovationTracker));
}

// Start evolution with seeded population
// (Note: This requires custom population initialization)
```

## Distributed Evaluation

```csharp
var distributedEvaluator = serviceProvider.GetRequiredService<IDistributedEvaluator>();

// Evaluate large population in parallel
var population = await populationManager.InitializePopulationAsync(500, random);

var fitnessResults = await distributedEvaluator.EvaluateDistributedAsync(
    population,
    cancellationToken);

// Assign fitness
foreach (var chromosome in population)
{
    if (fitnessResults.TryGetValue(chromosome.Id, out var result))
    {
        chromosome.Fitness = result.OverallFitness;
    }
}
```

## Saving and Loading Evolution Runs

```csharp
var evolutionStore = serviceProvider.GetRequiredService<IEvolutionStore>();

// Save evolution run
var run = new EvolutionRun
{
    Name = "Customer Support Agent Evolution",
    Config = config,
    Result = result
};

await evolutionStore.SaveEvolutionRunAsync(run, cancellationToken);

// Save generation snapshots
foreach (var genStats in result.GenerationHistory)
{
    var snapshot = new GenerationSnapshot
    {
        RunId = run.Id,
        Generation = genStats.Generation,
        Statistics = genStats
    };
    
    await evolutionStore.SaveGenerationSnapshotAsync(snapshot, cancellationToken);
}

// Load evolution run
var loadedRun = await evolutionStore.GetEvolutionRunAsync(run.Id, cancellationToken);

// Load chromosomes from a generation
var chromosomes = await evolutionStore.GetChromosomesAsync(
    run.Id,
    generation: 10,
    cancellationToken);
```

## Real-World Example: Customer Support Agent

```csharp
var tasks = new List<EvaluationTask>
{
    new EvaluationTask
    {
        Id = "order-inquiry",
        Input = "I need help with my order #12345",
        ExpectedKeywords = new List<string> { "order", "12345", "help" }
    },
    new EvaluationTask
    {
        Id = "return-request",
        Input = "How do I return an item?",
        ExpectedKeywords = new List<string> { "return", "refund" }
    },
    new EvaluationTask
    {
        Id = "account-balance",
        Input = "What's my account balance?",
        ExpectedKeywords = new List<string> { "balance", "account" }
    },
    new EvaluationTask
    {
        Id = "shipping-status",
        Input = "When will my order arrive?",
        ExpectedKeywords = new List<string> { "shipping", "arrive", "delivery" }
    }
};

services.AddEvolutionaryAgents(config =>
{
    config.PopulationSize = 100;
    config.EvaluationTasks = tasks;
    config.CompletionWeight = 0.4;  // High weight on completion
    config.QualityWeight = 0.4;     // High weight on quality
    config.EfficiencyWeight = 0.2;  // Lower weight on efficiency
});

var result = await engine.EvolveAsync(new EvolutionConfig
{
    PopulationSize = 100,
    EliteCount = 10,
    MutationRate = 0.05,
    CrossoverRate = 0.8,
    UseSpeciation = true,
    TerminationCondition = new TerminationCondition
    {
        MaxGenerations = 50,
        TargetFitness = 0.9
    }
}, cancellationToken);

// Deploy best agent
var bestAgent = adapter.ToAgentExecutor(
    result.BestAgent,
    llm,
    toolRegistry);

// Use in production
var response = await bestAgent.InvokeAsync("I need help with my order");
```

## See Also

- [Evolutionary Agents Guide](../guides/EVOLUTIONARY_AGENTS.md)
- [Evolutionary Agents README](../../src/DotNetAgents.Agents.Evolutionary/README.md)
- [Sample Project](../../samples/DotNetAgents.Samples.Evolutionary/)

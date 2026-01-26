# Evolutionary Agents Guide

## Overview

The **Evolutionary Agents** plugin (`DotNetAgents.Agents.Evolutionary`) is a world-class innovation that enables AI agents to evolve and improve themselves through genetic algorithms. This system applies artificial life principles (variation, competition, evaluation, reproduction, culling) to automatically optimize agent configurations over multiple generations.

## Why Evolutionary Agents?

Traditional agent development requires manual tuning of prompts, tool configurations, and strategies. Evolutionary Agents automates this process by:

- **Self-Improvement**: Agents evolve over generations, automatically finding better configurations
- **Exploration**: Genetic algorithms explore vast configuration spaces that would be impossible to search manually
- **Optimization**: Multiple fitness metrics ensure agents improve across multiple dimensions
- **Knowledge Accumulation**: Enhanced hive mind captures and shares learnings across generations
- **Innovation**: NEAT-style innovation tracking enables meaningful crossover between different agent structures

## Key Concepts

### Agent Chromosomes

An **Agent Chromosome** represents an agent's complete configuration as an evolvable genome. It contains multiple **genes**:

- **PromptGene**: System prompt, persona, instructions, examples
- **ToolConfigGene**: Enabled tools and their parameters
- **StrategyGene**: Retry policies, confidence thresholds, reflection settings
- **ModelGene**: LLM model selection
- **BehaviorTreeGene**: Behavior tree configuration (evolvable decision-making)
- **StateMachineGene**: State machine patterns (evolvable lifecycle management)
- **NumericGene**: Temperature, max tokens, retry counts, etc.

### Evolution Cycle

1. **Initialization**: Create initial population of random chromosomes
2. **Evaluation**: Evaluate fitness of each chromosome by running agents on tasks
3. **Selection**: Select parent chromosomes based on fitness
4. **Crossover**: Combine parent chromosomes to create offspring
5. **Mutation**: Introduce random variations in offspring
6. **Culling**: Remove low-performing chromosomes
7. **Repeat**: Continue for multiple generations

### Fitness Evaluation

Agents are evaluated on multiple metrics:

- **Task Completion Rate** (0-1): Percentage of tasks completed successfully
- **Solution Quality** (0-1): Average quality score from task evaluations
- **Efficiency Score** (0-1): Token usage, time, resource efficiency
- **Novelty Score** (0-1): Behavioral diversity from archive
- **Hive Mind Contributions** (0-1): Unique knowledge added
- **Consistency Score** (0-1): Performance across similar tasks

Overall fitness is a weighted sum of these metrics.

## Quick Start

### Installation

```bash
dotnet add package DotNetAgents.Agents.Evolutionary
```

### Basic Setup

```csharp
using DotNetAgents.Agents.Evolutionary;

// Register services
services.AddDotNetAgentsEcosystem();
services.AddEvolutionaryAgents(config =>
{
    config.PopulationSize = 100;
    config.UseSpeciation = true;
    config.SelectionOperatorType = "Tournament";
    config.CrossoverOperatorType = "NEAT";
    config.MutationOperatorType = "Adaptive";
    
    // Fitness weights
    config.CompletionWeight = 0.3;
    config.QualityWeight = 0.3;
    config.EfficiencyWeight = 0.2;
    config.NoveltyWeight = 0.1;
    config.ContributionWeight = 0.1;
    
    // Evaluation tasks
    config.EvaluationTasks = new List<EvaluationTask>
    {
        new EvaluationTask
        {
            Id = "task-1",
            Input = "Calculate 15 * 23",
            ExpectedKeywords = new List<string> { "345" }
        },
        new EvaluationTask
        {
            Id = "task-2",
            Input = "What is the capital of France?",
            ExpectedKeywords = new List<string> { "Paris" }
        }
        // Add more tasks...
    };
});
```

### Running Evolution

```csharp
var engine = serviceProvider.GetRequiredService<IEvolutionEngine>();

var result = await engine.EvolveAsync(new EvolutionConfig
{
    PopulationSize = 100,
    EliteCount = 5,
    MutationRate = 0.05,
    CrossoverRate = 0.8,
    UseSpeciation = true,
    CompatibilityThreshold = 3.0,
    SpeciesTargetCount = 10,
    TerminationCondition = new TerminationCondition
    {
        MaxGenerations = 50,
        TargetFitness = 0.95,
        StagnationGenerations = 10
    }
}, cancellationToken);

// Get best agent
var bestAgent = result.BestAgent;
Console.WriteLine($"Best Fitness: {result.BestFitness}");
Console.WriteLine($"Generations: {result.FinalGeneration}");

// Convert to agent executor
var adapter = serviceProvider.GetRequiredService<ChromosomeAdapter>();
var agentExecutor = adapter.ToAgentExecutor(
    bestAgent,
    llm,
    toolRegistry);

// Use evolved agent
var response = await agentExecutor.InvokeAsync("Your task here");
```

## Advanced Features

### NEAT-Style Innovation Tracking

The system uses innovation numbers (inspired by NEAT - NeuroEvolution of Augmenting Topologies) to enable meaningful crossover even when chromosome structures differ:

```csharp
var tracker = new InnovationTracker();
var innovationNumber = tracker.GetInnovationNumber(gene);
```

This allows:
- Matching genes by innovation number during crossover
- Handling disjoint and excess genes intelligently
- Preventing loss of valuable genetic material

### Semantic Genetic Operations

Use LLMs for semantic-aware mutation and crossover:

```csharp
// Semantic crossover merges prompts intelligently
var semanticCrossover = new SemanticCrossover(llm);
var offspring = semanticCrossover.Crossover(parentA, parentB, random);

// Semantic mutation rewrites prompts meaningfully
var semanticMutation = new SemanticPromptMutation();
semanticMutation.Mutate(chromosome, mutationRate, random);
```

### Island Model Evolution

Run multiple independent populations with periodic migration:

```csharp
var islandModel = new IslandModel(
    islandManagers: new List<IPopulationManager> { manager1, manager2, manager3 },
    fitnessEvaluator: fitnessEvaluator,
    migrationInterval: 10,  // Migrate every 10 generations
    migrationCount: 5       // Migrate 5 best chromosomes
);

var evolvedIslands = await islandModel.EvolveIslandsAsync(
    islands: initialIslands,
    generation: currentGeneration,
    random: random,
    cancellationToken: cancellationToken);
```

### Enhanced Hive Mind

The hive mind automatically extracts and stores knowledge from agent executions:

```csharp
var hiveMind = serviceProvider.GetRequiredService<IHiveMind>();

// Extract knowledge from execution
var knowledge = await hiveMind.ExtractKnowledgeAsync(
    chromosomeId: chromosome.Id,
    generation: generation,
    taskInput: taskInput,
    taskResult: taskResult,
    fitnessResult: fitnessResult,
    cancellationToken: cancellationToken);

// Store if novel
var stored = await hiveMind.StoreIfNovelAsync(
    knowledge: knowledge[0],
    chromosomeId: chromosome.Id,
    generation: generation,
    noveltyThreshold: 0.8,
    cancellationToken: cancellationToken);

// Retrieve relevant knowledge for new agents
var relevantKnowledge = await hiveMind.GetRelevantKnowledgeAsync(
    chromosome: newChromosome,
    maxResults: 10,
    cancellationToken: cancellationToken);
```

### Distributed Evaluation

Evaluate chromosomes in parallel using message buses:

```csharp
var distributedEvaluator = serviceProvider.GetRequiredService<IDistributedEvaluator>();

var results = await distributedEvaluator.EvaluateDistributedAsync(
    chromosomes: population,
    cancellationToken: cancellationToken);
```

### Observability

Monitor evolution progress with Prometheus metrics:

```csharp
// Metrics are automatically collected
// - evolution.runs.started
// - evolution.runs.completed
// - evolution.generation.current
// - evolution.fitness.best
// - evolution.fitness.average
// - evolution.population.diversity
// - evolution.chromosomes.evaluated
// - evolution.evaluation.duration
```

## Genetic Operators

### Selection Operators

**Tournament Selection** (default):
```csharp
var selection = new TournamentSelection(tournamentSize: 5);
var parent = selection.SelectParent(population, random);
```

**Roulette Wheel Selection**:
```csharp
var selection = new RouletteWheelSelection();
var parent = selection.SelectParent(population, random);
```

**Rank-Based Selection**:
```csharp
var selection = new RankBasedSelection(selectionPressure: 2.0);
var parent = selection.SelectParent(population, random);
```

**NSGA-II (Multi-Objective)**:
```csharp
var selection = new NSGA2Selection();
var parent = selection.SelectParent(population, random);
```

### Crossover Operators

**Single-Point Crossover**:
```csharp
var crossover = new SinglePointCrossover();
var offspring = crossover.Crossover(parentA, parentB, random);
```

**Uniform Crossover**:
```csharp
var crossover = new UniformCrossover(crossoverRate: 0.5);
var offspring = crossover.Crossover(parentA, parentB, random);
```

**NEAT-Style Crossover**:
```csharp
var crossover = new NEATCrossover();
var offspring = crossover.Crossover(parentA, parentB, random);
```

### Mutation Operators

**Standard Mutation**:
```csharp
var mutation = new StandardMutation();
mutation.Mutate(chromosome, mutationRate: 0.05, random);
```

**Adaptive Mutation**:
```csharp
var mutation = new AdaptiveMutation(
    baseMutationRate: 0.05,
    minMutationRate: 0.01,
    maxMutationRate: 0.2,
    diversityThreshold: 0.3);
mutation.Mutate(chromosome, mutationRate: 0.05, random);
```

## Configuration

### Evolution Configuration

```csharp
var config = new EvolutionConfig
{
    PopulationSize = 100,        // Number of chromosomes per generation
    EliteCount = 5,              // Top chromosomes to preserve
    MutationRate = 0.05,         // Probability of mutation
    CrossoverRate = 0.8,         // Probability of crossover
    UseSpeciation = true,        // Enable species-based evolution
    CompatibilityThreshold = 3.0, // Distance threshold for species
    SpeciesTargetCount = 10,      // Target number of species
    StagnationThreshold = 15,    // Generations before species extinction
    TerminationCondition = new TerminationCondition
    {
        MaxGenerations = 50,      // Maximum generations
        TargetFitness = 0.95,    // Target fitness to achieve
        StagnationGenerations = 10 // Generations without improvement
    }
};
```

### Fitness Configuration

```csharp
var fitnessConfig = new FitnessConfig
{
    CompletionWeight = 0.3,      // Weight for completion rate
    QualityWeight = 0.3,        // Weight for quality score
    EfficiencyWeight = 0.2,     // Weight for efficiency
    NoveltyWeight = 0.1,        // Weight for novelty
    ContributionWeight = 0.1,   // Weight for hive mind contributions
    ConsistencyWeight = 0.0,    // Weight for consistency
    TasksPerAgent = 10,         // Tasks to evaluate per agent
    EvaluationTimeout = TimeSpan.FromMinutes(30),
    UseHumanEvaluation = false,  // Enable human-in-the-loop
    HumanEvaluationThreshold = 0.8
};
```

## Best Practices

### 1. Evaluation Task Design

- **Diversity**: Include tasks that test different capabilities
- **Relevance**: Tasks should reflect real-world usage
- **Balance**: Mix easy and hard tasks
- **Quantity**: 10-20 tasks per agent provides good coverage

### 2. Population Size

- **Small (20-50)**: Faster evolution, less diversity
- **Medium (100-200)**: Good balance (recommended)
- **Large (500+)**: More diversity, slower evolution

### 3. Mutation and Crossover Rates

- **Mutation Rate**: 0.01-0.1 (lower = more exploitation, higher = more exploration)
- **Crossover Rate**: 0.7-0.9 (higher = more recombination)

### 4. Speciation

- Enable speciation for complex problems with multiple solution types
- Adjust compatibility threshold based on chromosome complexity
- Monitor species count to ensure diversity

### 5. Termination Conditions

- Set realistic target fitness (0.8-0.95)
- Use stagnation detection to avoid infinite runs
- Set maximum generations as safety limit

## Real-World Applications

### 1. Customer Support Agents

Evolve agents to handle different types of customer inquiries:

```csharp
var tasks = new List<EvaluationTask>
{
    new EvaluationTask { Input = "I need help with my order", ... },
    new EvaluationTask { Input = "How do I return an item?", ... },
    new EvaluationTask { Input = "What's my account balance?", ... }
};
```

### 2. Code Generation Agents

Evolve agents to generate better code:

```csharp
var tasks = new List<EvaluationTask>
{
    new EvaluationTask { Input = "Create a REST API endpoint", ... },
    new EvaluationTask { Input = "Implement authentication", ... },
    new EvaluationTask { Input = "Add error handling", ... }
};
```

### 3. Research Assistants

Evolve agents to improve research quality:

```csharp
var tasks = new List<EvaluationTask>
{
    new EvaluationTask { Input = "Research climate change impacts", ... },
    new EvaluationTask { Input = "Find recent AI breakthroughs", ... }
};
```

## Troubleshooting

### Low Fitness Scores

- **Check evaluation tasks**: Ensure tasks are appropriate and achievable
- **Review fitness weights**: Adjust weights to match priorities
- **Increase population size**: More diversity can help
- **Lower mutation rate**: Too much mutation can disrupt good solutions

### Premature Convergence

- **Increase mutation rate**: More exploration needed
- **Enable speciation**: Maintain diversity
- **Use island model**: Parallel evolution prevents convergence
- **Adjust selection pressure**: Lower pressure = more diversity

### Slow Evolution

- **Reduce population size**: Smaller populations evolve faster
- **Reduce tasks per agent**: Fewer evaluations per generation
- **Use distributed evaluation**: Parallel evaluation speeds up
- **Optimize fitness evaluation**: Cache results when possible

## Integration with Other Features

### Workflows

Use evolved agents in workflows:

```csharp
var workflow = new StateGraph<WorkflowState>()
    .AddNode("evolved-agent", async (state, ct) =>
    {
        var agent = adapter.ToAgentExecutor(bestAgent, llm, toolRegistry);
        state.Result = await agent.InvokeAsync(state.Input, ct);
        return state;
    });
```

### Multi-Agent Systems

Evolve agents for specific roles in multi-agent systems:

```csharp
// Evolve supervisor agent
var supervisorConfig = new EvolutionConfig { ... };
var supervisorResult = await engine.EvolveAsync(supervisorConfig);

// Evolve worker agents
var workerConfig = new EvolutionConfig { ... };
var workerResult = await engine.EvolveAsync(workerConfig);
```

### Knowledge Repository

The hive mind automatically integrates with the knowledge repository:

```csharp
// Knowledge is automatically extracted and stored
// Query knowledge for insights
var knowledge = await knowledgeRepository.GetRelevantKnowledgeAsync(
    techStackTags: new[] { "dotnet", "ai" },
    projectTags: new[] { "evolution" },
    maxResults: 10);
```

## Performance Considerations

- **Evaluation Time**: Fitness evaluation is the bottleneck - optimize task execution
- **Memory**: Large populations require more memory - consider checkpointing
- **Parallelization**: Use distributed evaluation for large populations
- **Caching**: Cache fitness results for identical chromosomes

## Future Enhancements

- **Co-evolution**: Evolve multiple agent types simultaneously
- **Reinforcement Learning**: Combine evolution with RL
- **Transfer Learning**: Transfer knowledge from evolved agents
- **Automated Task Generation**: Generate evaluation tasks automatically
- **3D Visualization**: Visualize fitness landscapes and evolution progress

## See Also

- [Evolutionary Agents README](../../src/DotNetAgents.Agents.Evolutionary/README.md)
- [Evolutionary Agents Example](../examples/EVOLUTIONARY_AGENTS.md)
- [Sample Project](../../samples/DotNetAgents.Samples.Evolutionary/)

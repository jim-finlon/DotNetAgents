using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Agents.Evolutionary.Genetics.Operators;
using Microsoft.Extensions.Logging;
using PromptGene = DotNetAgents.Agents.Evolutionary.Genetics.PromptGene;
using ToolConfigGene = DotNetAgents.Agents.Evolutionary.Genetics.ToolConfigGene;
using StrategyGene = DotNetAgents.Agents.Evolutionary.Genetics.StrategyGene;
using ModelGene = DotNetAgents.Agents.Evolutionary.Genetics.ModelGene;
using NumericGene = DotNetAgents.Agents.Evolutionary.Genetics.NumericGene;
using BehaviorTreeGene = DotNetAgents.Agents.Evolutionary.Genetics.BehaviorTreeGene;
using StateMachineGene = DotNetAgents.Agents.Evolutionary.Genetics.StateMachineGene;

namespace DotNetAgents.Agents.Evolutionary.Population;

/// <summary>
/// Manages agent populations, including initialization and generation creation.
/// </summary>
public sealed class PopulationManager : IPopulationManager
{
    private readonly InnovationTracker _innovationTracker;
    private readonly ISelectionOperator _selectionOperator;
    private readonly ICrossoverOperator _crossoverOperator;
    private readonly IMutationOperator _mutationOperator;
    private readonly double _crossoverRate;
    private readonly double _mutationRate;
    private readonly ILogger<PopulationManager>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopulationManager"/> class.
    /// </summary>
    /// <param name="innovationTracker">The innovation tracker.</param>
    /// <param name="selectionOperator">The selection operator.</param>
    /// <param name="crossoverOperator">The crossover operator.</param>
    /// <param name="mutationOperator">The mutation operator.</param>
    /// <param name="crossoverRate">The crossover rate.</param>
    /// <param name="mutationRate">The mutation rate.</param>
    /// <param name="logger">Optional logger instance.</param>
    public PopulationManager(
        InnovationTracker innovationTracker,
        ISelectionOperator selectionOperator,
        ICrossoverOperator crossoverOperator,
        IMutationOperator mutationOperator,
        double crossoverRate,
        double mutationRate,
        ILogger<PopulationManager>? logger = null)
    {
        _innovationTracker = innovationTracker ?? throw new ArgumentNullException(nameof(innovationTracker));
        _selectionOperator = selectionOperator ?? throw new ArgumentNullException(nameof(selectionOperator));
        _crossoverOperator = crossoverOperator ?? throw new ArgumentNullException(nameof(crossoverOperator));
        _mutationOperator = mutationOperator ?? throw new ArgumentNullException(nameof(mutationOperator));
        _crossoverRate = crossoverRate;
        _mutationRate = mutationRate;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<List<AgentChromosome>> InitializePopulationAsync(
        int size,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Population size must be positive.");

        var population = new List<AgentChromosome>(size);

        for (int i = 0; i < size; i++)
        {
            var chromosome = AgentChromosomeFactory.CreateRandom(_innovationTracker, random: random);
            chromosome.Generation = 0;
            population.Add(chromosome);
        }

        _logger?.LogInformation("Initialized population of {Size} chromosomes", size);

        return Task.FromResult(population);
    }

    /// <inheritdoc/>
    public Task<List<AgentChromosome>> CreateNextGenerationAsync(
        IReadOnlyList<AgentChromosome> currentPopulation,
        int eliteCount,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(currentPopulation);
        ArgumentNullException.ThrowIfNull(random);

        if (currentPopulation.Count == 0)
            throw new ArgumentException("Current population cannot be empty.", nameof(currentPopulation));

        var nextGeneration = new List<AgentChromosome>();

        // Preserve elite chromosomes
        var elite = currentPopulation
            .OrderByDescending(c => c.Fitness)
            .Take(eliteCount)
            .ToList();

        foreach (var eliteChromosome in elite)
        {
            var cloned = CloneChromosome(eliteChromosome);
            cloned.Generation = currentPopulation[0].Generation + 1;
            nextGeneration.Add(cloned);
        }

        _logger?.LogDebug("Preserved {EliteCount} elite chromosomes", eliteCount);

        // Generate remaining chromosomes through crossover and mutation
        var remainingCount = currentPopulation.Count - eliteCount;

        while (nextGeneration.Count < currentPopulation.Count)
        {
            AgentChromosome offspring;

            if (random.NextDouble() < _crossoverRate && currentPopulation.Count >= 2)
            {
                // Crossover
                var parents = _selectionOperator.SelectParents(currentPopulation, 2, random);
                offspring = _crossoverOperator.Crossover(parents[0], parents[1], random);
            }
            else
            {
                // Asexual reproduction (clone and mutate)
                var parent = _selectionOperator.SelectParent(currentPopulation, random);
                offspring = CloneChromosome(parent);
            }

            // Mutate
            _mutationOperator.Mutate(offspring, _mutationRate, random);

            offspring.Generation = currentPopulation[0].Generation + 1;
            nextGeneration.Add(offspring);
        }

        _logger?.LogInformation(
            "Created generation {Generation} with {Size} chromosomes",
            nextGeneration[0].Generation,
            nextGeneration.Count);

        return Task.FromResult(nextGeneration);
    }

    /// <inheritdoc/>
    public PopulationStatistics GetStatistics(IReadOnlyList<AgentChromosome> population)
    {
        ArgumentNullException.ThrowIfNull(population);

        if (population.Count == 0)
        {
            return new PopulationStatistics();
        }

        var fitnesses = population.Select(c => c.Fitness).ToList();

        // Calculate diversity (average compatibility distance)
        var diversity = 0.0;
        if (population.Count > 1)
        {
            var distances = new List<double>();
            for (int i = 0; i < population.Count; i++)
            {
                for (int j = i + 1; j < population.Count; j++)
                {
                    distances.Add(population[i].CalculateCompatibilityDistance(population[j]));
                }
            }

            diversity = distances.Count > 0 ? distances.Average() : 0.0;
        }

        return new PopulationStatistics
        {
            Size = population.Count,
            AverageFitness = fitnesses.Average(),
            BestFitness = fitnesses.Max(),
            WorstFitness = fitnesses.Min(),
            Diversity = diversity
        };
    }

    private static AgentChromosome CloneChromosome(AgentChromosome source)
    {
        var clone = new AgentChromosome
        {
            Generation = source.Generation,
            Fitness = source.Fitness,
            AdjustedFitness = source.AdjustedFitness,
            SpeciesId = source.SpeciesId,
            SystemPrompt = (PromptGene)source.SystemPrompt.Clone(),
            ToolConfiguration = (ToolConfigGene)source.ToolConfiguration.Clone(),
            Strategies = (StrategyGene)source.Strategies.Clone(),
            Model = (ModelGene)source.Model.Clone(),
            Temperature = (NumericGene)source.Temperature.Clone(),
            MaxTokens = (NumericGene)source.MaxTokens.Clone(),
            MaxRetries = (NumericGene)source.MaxRetries.Clone()
        };

        if (source.BehaviorTree != null)
        {
            clone.BehaviorTree = (BehaviorTreeGene)source.BehaviorTree.Clone();
        }

        if (source.StateMachine != null)
        {
            clone.StateMachine = (StateMachineGene)source.StateMachine.Clone();
        }

        foreach (var kvp in source.AdditionalNumericGenes)
        {
            clone.AdditionalNumericGenes[kvp.Key] = (NumericGene)kvp.Value.Clone();
        }

        foreach (var kvp in source.Metadata)
        {
            clone.Metadata[kvp.Key] = kvp.Value;
        }

        return clone;
    }
}

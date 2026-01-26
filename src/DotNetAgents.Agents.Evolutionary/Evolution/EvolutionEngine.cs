using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Agents.Evolutionary.Population;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary.Evolution;

/// <summary>
/// Orchestrates evolutionary cycles, managing population evolution over generations.
/// </summary>
public sealed class EvolutionEngine : IEvolutionEngine
{
    private readonly IPopulationManager _populationManager;
    private readonly IFitnessEvaluator _fitnessEvaluator;
    private readonly ILogger<EvolutionEngine>? _logger;
    private readonly Random _random = new();
    private bool _isRunning;
    private bool _isPaused;
    private readonly SemaphoreSlim _controlSemaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="EvolutionEngine"/> class.
    /// </summary>
    /// <param name="populationManager">The population manager.</param>
    /// <param name="fitnessEvaluator">The fitness evaluator.</param>
    /// <param name="logger">Optional logger instance.</param>
    public EvolutionEngine(
        IPopulationManager populationManager,
        IFitnessEvaluator fitnessEvaluator,
        ILogger<EvolutionEngine>? logger = null)
    {
        _populationManager = populationManager ?? throw new ArgumentNullException(nameof(populationManager));
        _fitnessEvaluator = fitnessEvaluator ?? throw new ArgumentNullException(nameof(fitnessEvaluator));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<EvolutionResult> EvolveAsync(
        EvolutionConfig config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        await _controlSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_isRunning)
                throw new InvalidOperationException("Evolution is already running.");

            _isRunning = true;
            _isPaused = false;
        }
        finally
        {
            _controlSemaphore.Release();
        }

        var startTime = DateTimeOffset.UtcNow;
        var result = new EvolutionResult
        {
            GenerationHistory = new List<GenerationStatistics>()
        };

        try
        {
            // Initialize population
            _logger?.LogInformation("Initializing population of size {Size}", config.PopulationSize);
            var population = await _populationManager.InitializePopulationAsync(
                config.PopulationSize,
                _random).ConfigureAwait(false);

            // Evaluate initial population
            _logger?.LogInformation("Evaluating initial population");
            var fitnessResults = await _fitnessEvaluator.EvaluateBatchAsync(
                population,
                cancellationToken).ConfigureAwait(false);

            // Assign fitness to chromosomes
            foreach (var chromosome in population)
            {
                if (fitnessResults.TryGetValue(chromosome.Id, out var fitnessResult))
                {
                    chromosome.Fitness = fitnessResult.OverallFitness;
                }
            }

            var generation = 0;
            var bestFitness = 0.0;
            var stagnationCount = 0;
            AgentChromosome? bestChromosome = null;

            // Evolution loop
            while (!ShouldTerminate(config, generation, bestFitness, stagnationCount, cancellationToken))
            {
                // Check for pause
                await WaitIfPausedAsync(cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    break;

                generation++;

                _logger?.LogInformation("Starting generation {Generation}", generation);

                // Create next generation
                var nextPopulation = await _populationManager.CreateNextGenerationAsync(
                    population,
                    config.EliteCount,
                    _random).ConfigureAwait(false);

                // Evaluate new generation
                fitnessResults = await _fitnessEvaluator.EvaluateBatchAsync(
                    nextPopulation,
                    cancellationToken).ConfigureAwait(false);

                // Assign fitness
                foreach (var chromosome in nextPopulation)
                {
                    if (fitnessResults.TryGetValue(chromosome.Id, out var fitnessResult))
                    {
                        chromosome.Fitness = fitnessResult.OverallFitness;
                    }
                }

                // Update best
                var currentBest = nextPopulation.OrderByDescending(c => c.Fitness).First();
                if (currentBest.Fitness > bestFitness)
                {
                    bestFitness = currentBest.Fitness;
                    bestChromosome = currentBest;
                    stagnationCount = 0;
                }
                else
                {
                    stagnationCount++;
                }

                // Record generation statistics
                var stats = _populationManager.GetStatistics(nextPopulation);
                result.GenerationHistory.Add(new GenerationStatistics
                {
                    Generation = generation,
                    AverageFitness = stats.AverageFitness,
                    BestFitness = stats.BestFitness,
                    WorstFitness = stats.WorstFitness,
                    Diversity = stats.Diversity
                });

                population = nextPopulation;

                _logger?.LogInformation(
                    "Generation {Generation} complete: Best Fitness={BestFitness}, Avg Fitness={AvgFitness}",
                    generation,
                    bestFitness,
                    stats.AverageFitness);
            }

            // Set result
            result.BestAgent = bestChromosome ?? population.OrderByDescending(c => c.Fitness).First();
            result.FinalGeneration = generation;
            result.BestFitness = bestFitness;
            result.TerminationReason = DetermineTerminationReason(config, generation, bestFitness, stagnationCount);
            result.TotalTime = DateTimeOffset.UtcNow - startTime;

            _logger?.LogInformation(
                "Evolution complete: {Reason}, Best Fitness={BestFitness}, Generations={Generations}",
                result.TerminationReason,
                bestFitness,
                generation);
        }
        finally
        {
            await _controlSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _isRunning = false;
                _isPaused = false;
            }
            finally
            {
                _controlSemaphore.Release();
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task PauseAsync()
    {
        await _controlSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!_isRunning)
                throw new InvalidOperationException("Evolution is not running.");

            _isPaused = true;
            _logger?.LogInformation("Evolution paused");
        }
        finally
        {
            _controlSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task ResumeAsync()
    {
        await _controlSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!_isRunning)
                throw new InvalidOperationException("Evolution is not running.");

            _isPaused = false;
            _logger?.LogInformation("Evolution resumed");
        }
        finally
        {
            _controlSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task StopAsync()
    {
        await _controlSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            _isRunning = false;
            _isPaused = false;
            _logger?.LogInformation("Evolution stopped");
        }
        finally
        {
            _controlSemaphore.Release();
        }
    }

    private bool ShouldTerminate(
        EvolutionConfig config,
        int generation,
        double bestFitness,
        int stagnationCount,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return true;

        var condition = config.TerminationCondition;
        if (condition == null)
            return false;

        if (condition.MaxGenerations.HasValue && generation >= condition.MaxGenerations.Value)
            return true;

        if (condition.TargetFitness.HasValue && bestFitness >= condition.TargetFitness.Value)
            return true;

        if (condition.StagnationGenerations.HasValue && stagnationCount >= condition.StagnationGenerations.Value)
            return true;

        return false;
    }

    private string DetermineTerminationReason(
        EvolutionConfig config,
        int generation,
        double bestFitness,
        int stagnationCount)
    {
        var condition = config.TerminationCondition;
        if (condition == null)
            return "No termination condition set";

        if (condition.MaxGenerations.HasValue && generation >= condition.MaxGenerations.Value)
            return $"Reached maximum generations ({condition.MaxGenerations.Value})";

        if (condition.TargetFitness.HasValue && bestFitness >= condition.TargetFitness.Value)
            return $"Achieved target fitness ({condition.TargetFitness.Value})";

        if (condition.StagnationGenerations.HasValue && stagnationCount >= condition.StagnationGenerations.Value)
            return $"Stagnation for {condition.StagnationGenerations.Value} generations";

        return "Unknown";
    }

    private async Task WaitIfPausedAsync(CancellationToken cancellationToken)
    {
        while (_isPaused && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        }
    }
}

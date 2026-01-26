using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Agents.Evolutionary.Population;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary.Evolution;

/// <summary>
/// Island model evolution that runs multiple independent populations with periodic migration.
/// </summary>
public sealed class IslandModel
{
    private readonly List<IPopulationManager> _islandManagers;
    private readonly IFitnessEvaluator _fitnessEvaluator;
    private readonly int _migrationInterval;
    private readonly int _migrationCount;
    private readonly ILogger<IslandModel>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IslandModel"/> class.
    /// </summary>
    /// <param name="islandManagers">The population managers for each island.</param>
    /// <param name="fitnessEvaluator">The fitness evaluator.</param>
    /// <param name="migrationInterval">Number of generations between migrations.</param>
    /// <param name="migrationCount">Number of chromosomes to migrate between islands.</param>
    /// <param name="logger">Optional logger instance.</param>
    public IslandModel(
        List<IPopulationManager> islandManagers,
        IFitnessEvaluator fitnessEvaluator,
        int migrationInterval = 10,
        int migrationCount = 5,
        ILogger<IslandModel>? logger = null)
    {
        _islandManagers = islandManagers ?? throw new ArgumentNullException(nameof(islandManagers));
        _fitnessEvaluator = fitnessEvaluator ?? throw new ArgumentNullException(nameof(fitnessEvaluator));
        _migrationInterval = migrationInterval;
        _migrationCount = migrationCount;
        _logger = logger;
    }

    /// <summary>
    /// Runs evolution on all islands with migration.
    /// </summary>
    /// <param name="islands">The island populations.</param>
    /// <param name="generation">The current generation.</param>
    /// <param name="random">Random number generator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated island populations.</returns>
    public async Task<List<List<AgentChromosome>>> EvolveIslandsAsync(
        List<List<AgentChromosome>> islands,
        int generation,
        Random random,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(islands);
        ArgumentNullException.ThrowIfNull(random);

        // Evolve each island independently
        var evolvedIslands = new List<List<AgentChromosome>>();

        for (int i = 0; i < islands.Count; i++)
        {
            var island = islands[i];
            var manager = _islandManagers[i];

            // Evaluate fitness
            var fitnessResults = await _fitnessEvaluator.EvaluateBatchAsync(
                island,
                cancellationToken).ConfigureAwait(false);

            foreach (var chromosome in island)
            {
                if (fitnessResults.TryGetValue(chromosome.Id, out var result))
                {
                    chromosome.Fitness = result.OverallFitness;
                }
            }

            // Create next generation
            var nextGeneration = await manager.CreateNextGenerationAsync(
                island,
                island.Count / 10, // 10% elite
                random).ConfigureAwait(false);

            evolvedIslands.Add(nextGeneration);
        }

        // Perform migration if interval reached
        if (generation > 0 && generation % _migrationInterval == 0)
        {
            PerformMigration(evolvedIslands, random);
            _logger?.LogInformation(
                "Performed migration between islands at generation {Generation}",
                generation);
        }

        return evolvedIslands;
    }

    private void PerformMigration(List<List<AgentChromosome>> islands, Random random)
    {
        if (islands.Count < 2)
            return;

        // Migrate best chromosomes between islands
        for (int i = 0; i < islands.Count; i++)
        {
            var sourceIsland = islands[i];
            var targetIsland = islands[(i + 1) % islands.Count];

            // Select best chromosomes to migrate
            var migrants = sourceIsland
                .OrderByDescending(c => c.Fitness)
                .Take(_migrationCount)
                .ToList();

            // Remove from source and add to target
            foreach (var migrant in migrants)
            {
                sourceIsland.Remove(migrant);
                targetIsland.Add(migrant);
            }

            // Trim target island to original size
            var targetSize = targetIsland.Count - _migrationCount;
            var toRemove = targetIsland
                .OrderBy(c => c.Fitness)
                .Take(_migrationCount)
                .ToList();

            foreach (var chromosome in toRemove)
            {
                targetIsland.Remove(chromosome);
            }
        }
    }
}

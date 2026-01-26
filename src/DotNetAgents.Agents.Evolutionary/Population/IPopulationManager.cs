using DotNetAgents.Agents.Evolutionary.Genetics;

namespace DotNetAgents.Agents.Evolutionary.Population;

/// <summary>
/// Interface for managing agent populations and species.
/// </summary>
public interface IPopulationManager
{
    /// <summary>
    /// Initializes a new population.
    /// </summary>
    /// <param name="size">The population size.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>The initial population.</returns>
    Task<List<AgentChromosome>> InitializePopulationAsync(
        int size,
        Random random);

    /// <summary>
    /// Creates the next generation from the current population.
    /// </summary>
    /// <param name="currentPopulation">The current population.</param>
    /// <param name="eliteCount">The number of elite chromosomes to preserve.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>The next generation population.</returns>
    Task<List<AgentChromosome>> CreateNextGenerationAsync(
        IReadOnlyList<AgentChromosome> currentPopulation,
        int eliteCount,
        Random random);

    /// <summary>
    /// Gets population statistics.
    /// </summary>
    /// <param name="population">The population to analyze.</param>
    /// <returns>Population statistics.</returns>
    PopulationStatistics GetStatistics(IReadOnlyList<AgentChromosome> population);
}

/// <summary>
/// Statistics about a population.
/// </summary>
public sealed class PopulationStatistics
{
    /// <summary>
    /// Gets or sets the population size.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the average fitness.
    /// </summary>
    public double AverageFitness { get; set; }

    /// <summary>
    /// Gets or sets the best fitness.
    /// </summary>
    public double BestFitness { get; set; }

    /// <summary>
    /// Gets or sets the worst fitness.
    /// </summary>
    public double WorstFitness { get; set; }

    /// <summary>
    /// Gets or sets the population diversity.
    /// </summary>
    public double Diversity { get; set; }
}

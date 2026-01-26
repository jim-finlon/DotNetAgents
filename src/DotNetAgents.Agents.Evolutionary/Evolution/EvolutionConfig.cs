namespace DotNetAgents.Agents.Evolutionary.Evolution;

/// <summary>
/// Configuration for evolution runs.
/// </summary>
public sealed class EvolutionConfig
{
    /// <summary>
    /// Gets or sets the population size.
    /// </summary>
    public int PopulationSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the number of elite chromosomes to preserve.
    /// </summary>
    public int EliteCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the mutation rate (0.0 to 1.0).
    /// </summary>
    public double MutationRate { get; set; } = 0.05;

    /// <summary>
    /// Gets or sets the crossover rate (0.0 to 1.0).
    /// </summary>
    public double CrossoverRate { get; set; } = 0.8;

    /// <summary>
    /// Gets or sets whether to use speciation.
    /// </summary>
    public bool UseSpeciation { get; set; } = true;

    /// <summary>
    /// Gets or sets the compatibility threshold for speciation.
    /// </summary>
    public double CompatibilityThreshold { get; set; } = 3.0;

    /// <summary>
    /// Gets or sets the target number of species.
    /// </summary>
    public int SpeciesTargetCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the stagnation threshold (generations without improvement before species extinction).
    /// </summary>
    public int StagnationThreshold { get; set; } = 15;

    /// <summary>
    /// Gets or sets the termination condition.
    /// </summary>
    public TerminationCondition? TerminationCondition { get; set; }
}

/// <summary>
/// Defines conditions for terminating evolution.
/// </summary>
public sealed class TerminationCondition
{
    /// <summary>
    /// Gets or sets the maximum number of generations.
    /// </summary>
    public int? MaxGenerations { get; set; }

    /// <summary>
    /// Gets or sets the target fitness to achieve.
    /// </summary>
    public double? TargetFitness { get; set; }

    /// <summary>
    /// Gets or sets the number of generations without improvement before termination.
    /// </summary>
    public int? StagnationGenerations { get; set; }
}

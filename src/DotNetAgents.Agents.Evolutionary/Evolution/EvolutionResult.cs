using DotNetAgents.Agents.Evolutionary.Genetics;

namespace DotNetAgents.Agents.Evolutionary.Evolution;

/// <summary>
/// Represents the result of an evolution run.
/// </summary>
public sealed class EvolutionResult
{
    /// <summary>
    /// Gets or sets the best chromosome found.
    /// </summary>
    public AgentChromosome BestAgent { get; set; } = null!;

    /// <summary>
    /// Gets or sets the final generation number.
    /// </summary>
    public int FinalGeneration { get; set; }

    /// <summary>
    /// Gets or sets the best fitness achieved.
    /// </summary>
    public double BestFitness { get; set; }

    /// <summary>
    /// Gets or sets the reason for termination.
    /// </summary>
    public string TerminationReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets generation statistics.
    /// </summary>
    public List<GenerationStatistics> GenerationHistory { get; set; } = new();

    /// <summary>
    /// Gets or sets the total evolution time.
    /// </summary>
    public TimeSpan TotalTime { get; set; }
}

/// <summary>
/// Statistics for a single generation.
/// </summary>
public sealed class GenerationStatistics
{
    /// <summary>
    /// Gets or sets the generation number.
    /// </summary>
    public int Generation { get; set; }

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
    /// Gets or sets the number of species.
    /// </summary>
    public int SpeciesCount { get; set; }

    /// <summary>
    /// Gets or sets the population diversity.
    /// </summary>
    public double Diversity { get; set; }
}

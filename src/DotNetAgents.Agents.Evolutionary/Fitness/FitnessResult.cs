namespace DotNetAgents.Agents.Evolutionary.Fitness;

/// <summary>
/// Represents the result of a fitness evaluation for an agent chromosome.
/// </summary>
public sealed class FitnessResult
{
    /// <summary>
    /// Gets or sets the overall fitness score (0.0 to 1.0).
    /// </summary>
    public double OverallFitness { get; set; }

    /// <summary>
    /// Gets or sets the task completion rate (0.0 to 1.0).
    /// </summary>
    public double CompletionRate { get; set; }

    /// <summary>
    /// Gets or sets the solution quality score (0.0 to 1.0).
    /// </summary>
    public double QualityScore { get; set; }

    /// <summary>
    /// Gets or sets the efficiency score (0.0 to 1.0).
    /// </summary>
    public double EfficiencyScore { get; set; }

    /// <summary>
    /// Gets or sets the novelty score (0.0 to 1.0).
    /// </summary>
    public double NoveltyScore { get; set; }

    /// <summary>
    /// Gets or sets the hive mind contribution score (0.0 to 1.0).
    /// </summary>
    public double ContributionScore { get; set; }

    /// <summary>
    /// Gets or sets the consistency score (0.0 to 1.0).
    /// </summary>
    public double ConsistencyScore { get; set; }

    /// <summary>
    /// Gets or sets the number of tasks evaluated.
    /// </summary>
    public int TasksEvaluated { get; set; }

    /// <summary>
    /// Gets or sets the number of tasks completed successfully.
    /// </summary>
    public int TasksCompleted { get; set; }

    /// <summary>
    /// Gets or sets the total evaluation time.
    /// </summary>
    public TimeSpan EvaluationTime { get; set; }

    /// <summary>
    /// Gets or sets additional evaluation metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

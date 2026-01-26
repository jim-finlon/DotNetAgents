namespace DotNetAgents.Agents.Evolutionary.Fitness;

/// <summary>
/// Configuration for fitness evaluation.
/// </summary>
public sealed class FitnessConfig
{
    /// <summary>
    /// Gets or sets the weight for completion rate in fitness calculation.
    /// </summary>
    public double CompletionWeight { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the weight for quality score in fitness calculation.
    /// </summary>
    public double QualityWeight { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the weight for efficiency score in fitness calculation.
    /// </summary>
    public double EfficiencyWeight { get; set; } = 0.2;

    /// <summary>
    /// Gets or sets the weight for novelty score in fitness calculation.
    /// </summary>
    public double NoveltyWeight { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the weight for contribution score in fitness calculation.
    /// </summary>
    public double ContributionWeight { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets the weight for consistency score in fitness calculation.
    /// </summary>
    public double ConsistencyWeight { get; set; }

    /// <summary>
    /// Gets or sets the number of tasks to evaluate per agent.
    /// </summary>
    public int TasksPerAgent { get; set; } = 10;

    /// <summary>
    /// Gets or sets the timeout for evaluation.
    /// </summary>
    public TimeSpan EvaluationTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets whether to use human-in-the-loop evaluation.
    /// </summary>
    public bool UseHumanEvaluation { get; set; }

    /// <summary>
    /// Gets or sets the fitness threshold above which human evaluation is triggered.
    /// </summary>
    public double HumanEvaluationThreshold { get; set; } = 0.8;
}

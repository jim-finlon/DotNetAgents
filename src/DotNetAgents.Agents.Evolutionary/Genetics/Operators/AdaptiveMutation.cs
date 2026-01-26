namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Adaptive mutation operator that adjusts mutation rate based on population diversity.
/// Increases mutation when diversity is low (premature convergence) and decreases when diversity is high.
/// </summary>
public sealed class AdaptiveMutation : IMutationOperator
{
    private readonly double _baseMutationRate;
    private readonly double _minMutationRate;
    private readonly double _maxMutationRate;
    private readonly double _diversityThreshold;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveMutation"/> class.
    /// </summary>
    /// <param name="baseMutationRate">The base mutation rate.</param>
    /// <param name="minMutationRate">The minimum mutation rate (default: 0.01).</param>
    /// <param name="maxMutationRate">The maximum mutation rate (default: 0.2).</param>
    /// <param name="diversityThreshold">Diversity threshold below which mutation increases (default: 0.3).</param>
    public AdaptiveMutation(
        double baseMutationRate = 0.05,
        double minMutationRate = 0.01,
        double maxMutationRate = 0.2,
        double diversityThreshold = 0.3)
    {
        _baseMutationRate = baseMutationRate;
        _minMutationRate = minMutationRate;
        _maxMutationRate = maxMutationRate;
        _diversityThreshold = diversityThreshold;
    }

    /// <inheritdoc/>
    public void Mutate(
        AgentChromosome chromosome,
        double mutationRate,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(chromosome);
        ArgumentNullException.ThrowIfNull(random);

        // Calculate population diversity (simplified - in practice, this would be passed in)
        // For now, we'll use a simple heuristic based on fitness variance
        // In a real implementation, this would be calculated from the population
        var effectiveMutationRate = CalculateEffectiveMutationRate(mutationRate);

        // Apply mutation with adjusted rate
        foreach (var gene in chromosome.AllGenes)
        {
            gene.Mutate(effectiveMutationRate, random);
        }
    }

    /// <summary>
    /// Calculates the effective mutation rate based on diversity.
    /// </summary>
    /// <param name="baseRate">The base mutation rate.</param>
    /// <param name="diversity">The population diversity (0.0 to 1.0).</param>
    /// <returns>The adjusted mutation rate.</returns>
    public double CalculateEffectiveMutationRate(double baseRate, double diversity = 0.5)
    {
        // If diversity is low, increase mutation rate
        if (diversity < _diversityThreshold)
        {
            var increaseFactor = 1.0 + (1.0 - diversity / _diversityThreshold);
            return Math.Min(_maxMutationRate, baseRate * increaseFactor);
        }

        // If diversity is high, decrease mutation rate
        var decreaseFactor = 1.0 - ((diversity - _diversityThreshold) / (1.0 - _diversityThreshold)) * 0.5;
        return Math.Max(_minMutationRate, baseRate * decreaseFactor);
    }
}

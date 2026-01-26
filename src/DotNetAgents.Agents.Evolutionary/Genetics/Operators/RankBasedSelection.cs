namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Rank-based selection operator that selects based on rank rather than absolute fitness.
/// This helps prevent premature convergence when fitness values are very different.
/// </summary>
public sealed class RankBasedSelection : ISelectionOperator
{
    private readonly double _selectionPressure;

    /// <summary>
    /// Initializes a new instance of the <see cref="RankBasedSelection"/> class.
    /// </summary>
    /// <param name="selectionPressure">Selection pressure (1.0 = uniform, 2.0 = linear, higher = more pressure on top ranks).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when selection pressure is less than 1.0.</exception>
    public RankBasedSelection(double selectionPressure = 2.0)
    {
        if (selectionPressure < 1.0)
            throw new ArgumentOutOfRangeException(nameof(selectionPressure), "Selection pressure must be at least 1.0.");

        _selectionPressure = selectionPressure;
    }

    /// <inheritdoc/>
    public AgentChromosome SelectParent(
        IReadOnlyList<AgentChromosome> population,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);

        if (population.Count == 0)
            throw new ArgumentException("Population cannot be empty.", nameof(population));

        if (population.Count == 1)
            return population[0];

        // Sort by fitness (descending)
        var ranked = population.OrderByDescending(c => c.Fitness).ToList();

        // Calculate rank probabilities
        var probabilities = new double[ranked.Count];
        var totalProbability = 0.0;

        for (int i = 0; i < ranked.Count; i++)
        {
            // Rank 1 (best) gets highest probability
            var rank = ranked.Count - i; // Reverse rank (1 = best)
            probabilities[i] = Math.Pow(rank, _selectionPressure);
            totalProbability += probabilities[i];
        }

        // Normalize probabilities
        for (int i = 0; i < probabilities.Length; i++)
        {
            probabilities[i] /= totalProbability;
        }

        // Select based on rank probabilities
        var randomValue = random.NextDouble();
        var cumulativeProbability = 0.0;

        for (int i = 0; i < ranked.Count; i++)
        {
            cumulativeProbability += probabilities[i];
            if (cumulativeProbability >= randomValue)
            {
                return ranked[i];
            }
        }

        // Fallback
        return ranked[^1];
    }

    /// <inheritdoc/>
    public IReadOnlyList<AgentChromosome> SelectParents(
        IReadOnlyList<AgentChromosome> population,
        int count,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);

        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        var parents = new List<AgentChromosome>(count);
        for (int i = 0; i < count; i++)
        {
            parents.Add(SelectParent(population, random));
        }

        return parents;
    }
}

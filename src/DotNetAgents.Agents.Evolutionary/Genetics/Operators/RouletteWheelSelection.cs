namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Roulette wheel (fitness-proportionate) selection operator.
/// </summary>
public sealed class RouletteWheelSelection : ISelectionOperator
{
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

        // Calculate total fitness (use adjusted fitness if available, otherwise raw fitness)
        var totalFitness = population.Sum(c => c.AdjustedFitness > 0 ? c.AdjustedFitness : c.Fitness);

        if (totalFitness <= 0)
        {
            // If all fitnesses are zero or negative, use uniform random selection
            return population[random.Next(population.Count)];
        }

        // Spin the roulette wheel
        var randomValue = random.NextDouble() * totalFitness;
        var cumulativeFitness = 0.0;

        foreach (var chromosome in population)
        {
            var fitness = chromosome.AdjustedFitness > 0 ? chromosome.AdjustedFitness : chromosome.Fitness;
            cumulativeFitness += fitness;

            if (cumulativeFitness >= randomValue)
            {
                return chromosome;
            }
        }

        // Fallback to last chromosome (shouldn't happen, but safety check)
        return population[^1];
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

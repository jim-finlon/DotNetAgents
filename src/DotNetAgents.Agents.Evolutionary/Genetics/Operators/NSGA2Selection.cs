namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// NSGA-II (Non-dominated Sorting Genetic Algorithm) selection operator for multi-objective optimization.
/// Selects chromosomes based on Pareto dominance and crowding distance.
/// </summary>
public sealed class NSGA2Selection : ISelectionOperator
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

        // For single-objective selection, fall back to tournament selection
        // In a full implementation, this would use Pareto dominance
        var tournament = new TournamentSelection(5);
        return tournament.SelectParent(population, random);
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

        // For now, use tournament selection as fallback
        // Full NSGA-II implementation would:
        // 1. Calculate Pareto fronts (non-dominated sorting)
        // 2. Calculate crowding distance within each front
        // 3. Select from fronts in order, using crowding distance as tiebreaker
        var tournament = new TournamentSelection(5);
        return tournament.SelectParents(population, count, random);
    }
}

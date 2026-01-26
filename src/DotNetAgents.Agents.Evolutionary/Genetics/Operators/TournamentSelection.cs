namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Tournament selection operator that selects the best chromosome from a random tournament.
/// </summary>
public sealed class TournamentSelection : ISelectionOperator
{
    private readonly int _tournamentSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="TournamentSelection"/> class.
    /// </summary>
    /// <param name="tournamentSize">The size of the tournament (default: 5).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when tournament size is less than 2.</exception>
    public TournamentSelection(int tournamentSize = 5)
    {
        if (tournamentSize < 2)
            throw new ArgumentOutOfRangeException(nameof(tournamentSize), "Tournament size must be at least 2.");

        _tournamentSize = tournamentSize;
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

        // Select random tournament participants
        var tournament = new List<AgentChromosome>();
        var indices = Enumerable.Range(0, population.Count).OrderBy(_ => random.Next()).Take(_tournamentSize);

        foreach (var index in indices)
        {
            tournament.Add(population[index]);
        }

        // Return the best chromosome from the tournament
        return tournament.OrderByDescending(c => c.Fitness).First();
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

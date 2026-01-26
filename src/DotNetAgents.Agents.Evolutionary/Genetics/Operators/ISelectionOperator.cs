namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Interface for selection operators that choose parent chromosomes for reproduction.
/// </summary>
public interface ISelectionOperator
{
    /// <summary>
    /// Selects a parent chromosome from the population based on fitness.
    /// </summary>
    /// <param name="population">The population of chromosomes.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>The selected parent chromosome.</returns>
    AgentChromosome SelectParent(
        IReadOnlyList<AgentChromosome> population,
        Random random);

    /// <summary>
    /// Selects multiple parent chromosomes from the population.
    /// </summary>
    /// <param name="population">The population of chromosomes.</param>
    /// <param name="count">The number of parents to select.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>The selected parent chromosomes.</returns>
    IReadOnlyList<AgentChromosome> SelectParents(
        IReadOnlyList<AgentChromosome> population,
        int count,
        Random random);
}

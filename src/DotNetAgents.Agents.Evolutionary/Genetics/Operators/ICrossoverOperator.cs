namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Interface for crossover operators that combine two parent chromosomes to create offspring.
/// </summary>
public interface ICrossoverOperator
{
    /// <summary>
    /// Performs crossover between two parent chromosomes to create offspring.
    /// </summary>
    /// <param name="parentA">The first parent chromosome.</param>
    /// <param name="parentB">The second parent chromosome.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>A new offspring chromosome created from the parents.</returns>
    AgentChromosome Crossover(
        AgentChromosome parentA,
        AgentChromosome parentB,
        Random random);
}

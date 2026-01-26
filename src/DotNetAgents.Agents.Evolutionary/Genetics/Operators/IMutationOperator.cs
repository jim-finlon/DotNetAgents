namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Interface for mutation operators that introduce random variations in chromosomes.
/// </summary>
public interface IMutationOperator
{
    /// <summary>
    /// Mutates a chromosome by introducing random variations.
    /// </summary>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <param name="mutationRate">The probability of mutation for each gene.</param>
    /// <param name="random">Random number generator.</param>
    void Mutate(
        AgentChromosome chromosome,
        double mutationRate,
        Random random);
}

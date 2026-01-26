namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Standard mutation operator that applies random mutations to genes.
/// </summary>
public sealed class StandardMutation : IMutationOperator
{
    /// <inheritdoc/>
    public void Mutate(
        AgentChromosome chromosome,
        double mutationRate,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(chromosome);
        ArgumentNullException.ThrowIfNull(random);

        // Mutate each gene with the given mutation rate
        foreach (var gene in chromosome.AllGenes)
        {
            gene.Mutate(mutationRate, random);
        }
    }
}

using DotNetAgents.Agents.Evolutionary.Genetics;

namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Single-point crossover operator that splits chromosomes at a random point and swaps genetic material.
/// </summary>
public sealed class SinglePointCrossover : ICrossoverOperator
{
    /// <inheritdoc/>
    public AgentChromosome Crossover(
        AgentChromosome parentA,
        AgentChromosome parentB,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(parentA);
        ArgumentNullException.ThrowIfNull(parentB);
        ArgumentNullException.ThrowIfNull(random);

        // Create offspring by cloning parent A
        var offspring = CloneChromosome(parentA);
        offspring.ParentAId = parentA.Id;
        offspring.ParentBId = parentB.Id;
        offspring.Generation = Math.Max(parentA.Generation, parentB.Generation) + 1;

        // Get all genes from both parents
        var genesA = parentA.AllGenes.ToList();
        var genesB = parentB.AllGenes.ToList();

        if (genesA.Count == 0 || genesB.Count == 0)
            return offspring;

        // Select random crossover point
        var crossoverPoint = random.Next(1, Math.Min(genesA.Count, genesB.Count));

        // For each gene type, decide which parent to take from
        // This is a simplified approach - more sophisticated methods would match by innovation number
        var takeFromB = random.NextDouble() < 0.5;

        if (takeFromB)
        {
            // Take genes from parent B after crossover point
            // In practice, we'd match genes by type and innovation number
            // For now, we'll do a simple swap of specific gene types
            if (parentB.Model != null)
            {
                offspring.Model = (ModelGene)parentB.Model.Clone();
            }

            if (parentB.Strategies != null)
            {
                offspring.Strategies = (StrategyGene)parentB.Strategies.Clone();
            }
        }

        return offspring;
    }

    private static AgentChromosome CloneChromosome(AgentChromosome source)
    {
        var clone = new AgentChromosome
        {
            Generation = source.Generation,
            Fitness = source.Fitness,
            AdjustedFitness = source.AdjustedFitness,
            SpeciesId = source.SpeciesId,
            SystemPrompt = (PromptGene)source.SystemPrompt.Clone(),
            ToolConfiguration = (ToolConfigGene)source.ToolConfiguration.Clone(),
            Strategies = (StrategyGene)source.Strategies.Clone(),
            Model = (ModelGene)source.Model.Clone(),
            Temperature = (NumericGene)source.Temperature.Clone(),
            MaxTokens = (NumericGene)source.MaxTokens.Clone(),
            MaxRetries = (NumericGene)source.MaxRetries.Clone()
        };

        if (source.BehaviorTree != null)
        {
            clone.BehaviorTree = (BehaviorTreeGene)source.BehaviorTree.Clone();
        }

        if (source.StateMachine != null)
        {
            clone.StateMachine = (StateMachineGene)source.StateMachine.Clone();
        }

        foreach (var kvp in source.AdditionalNumericGenes)
        {
            clone.AdditionalNumericGenes[kvp.Key] = (NumericGene)kvp.Value.Clone();
        }

        foreach (var kvp in source.Metadata)
        {
            clone.Metadata[kvp.Key] = kvp.Value;
        }

        return clone;
    }
}

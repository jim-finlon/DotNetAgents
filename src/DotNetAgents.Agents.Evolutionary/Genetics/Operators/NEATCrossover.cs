using DotNetAgents.Agents.Evolutionary.Genetics;

namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// NEAT-style crossover operator that matches genes by innovation number.
/// This enables meaningful crossover even when chromosome structures differ.
/// </summary>
public sealed class NEATCrossover : ICrossoverOperator
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

        // Create offspring by cloning parent A (more fit parent)
        var moreFitParent = parentA.Fitness >= parentB.Fitness ? parentA : parentB;
        var lessFitParent = parentA.Fitness >= parentB.Fitness ? parentB : parentA;

        var offspring = CloneChromosome(moreFitParent);
        offspring.ParentAId = parentA.Id;
        offspring.ParentBId = parentB.Id;
        offspring.Generation = Math.Max(parentA.Generation, parentB.Generation) + 1;

        // Get innovation histories
        var innovationsA = parentA.InnovationHistory.ToHashSet();
        var innovationsB = parentB.InnovationHistory.ToHashSet();

        // Matching genes: randomly choose from either parent
        var matching = innovationsA.Intersect(innovationsB).ToList();

        // For matching genes, randomly select from either parent
        // This is simplified - in full NEAT, we'd match by innovation number and average weights
        foreach (var innovation in matching)
        {
            if (random.NextDouble() < 0.5)
            {
                // Take from less fit parent (introduces diversity)
                // In practice, we'd match specific genes by innovation number
                // For now, we'll do a simple swap of some gene types
                if (random.NextDouble() < 0.3) // 30% chance to take from less fit parent
                {
                    if (lessFitParent.Model != null && lessFitParent.Model.InnovationNumber == innovation)
                    {
                        offspring.Model = (ModelGene)lessFitParent.Model.Clone();
                    }
                }
            }
        }

        // Disjoint genes (only in more fit parent): always include
        // Excess genes (only in less fit parent): include with 50% probability
        var excess = innovationsB.Except(innovationsA).ToList();
        foreach (var innovation in excess)
        {
            if (random.NextDouble() < 0.5)
            {
                // Include excess gene from less fit parent
                // In practice, we'd find the actual gene with this innovation number
                if (lessFitParent.BehaviorTree != null && lessFitParent.BehaviorTree.InnovationNumber == innovation)
                {
                    offspring.BehaviorTree = (BehaviorTreeGene)lessFitParent.BehaviorTree.Clone();
                }
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

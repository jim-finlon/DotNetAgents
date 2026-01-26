namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Semantic crossover operator that uses LLMs to intelligently merge parent prompts.
/// This maintains semantic coherence while introducing meaningful variation.
/// Note: Full LLM integration requires async operations, so this falls back to uniform crossover.
/// </summary>
public sealed class SemanticCrossover : ICrossoverOperator
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

        // For now, fall back to uniform crossover
        // Full implementation would require async LLM calls, which would need
        // a different interface design or async wrapper
        // In practice, semantic operations would be done in a separate async step
        var uniformCrossover = new UniformCrossover(0.5);
        return uniformCrossover.Crossover(parentA, parentB, random);
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

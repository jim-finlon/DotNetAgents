using DotNetAgents.Agents.Evolutionary.Genetics;

namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Uniform crossover operator that randomly selects genes from either parent for each position.
/// </summary>
public sealed class UniformCrossover : ICrossoverOperator
{
    private readonly double _crossoverRate;

    /// <summary>
    /// Initializes a new instance of the <see cref="UniformCrossover"/> class.
    /// </summary>
    /// <param name="crossoverRate">Probability of taking gene from parent B (default: 0.5).</param>
    public UniformCrossover(double crossoverRate = 0.5)
    {
        _crossoverRate = Math.Clamp(crossoverRate, 0.0, 1.0);
    }

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

        // Uniformly select genes from either parent
        if (random.NextDouble() < _crossoverRate && parentB.SystemPrompt != null)
        {
            offspring.SystemPrompt = (PromptGene)parentB.SystemPrompt.Clone();
        }

        if (random.NextDouble() < _crossoverRate && parentB.ToolConfiguration != null)
        {
            offspring.ToolConfiguration = (ToolConfigGene)parentB.ToolConfiguration.Clone();
        }

        if (random.NextDouble() < _crossoverRate && parentB.Strategies != null)
        {
            offspring.Strategies = (StrategyGene)parentB.Strategies.Clone();
        }

        if (random.NextDouble() < _crossoverRate && parentB.Model != null)
        {
            offspring.Model = (ModelGene)parentB.Model.Clone();
        }

        if (random.NextDouble() < _crossoverRate)
        {
            offspring.Temperature = (NumericGene)parentB.Temperature.Clone();
        }

        if (random.NextDouble() < _crossoverRate)
        {
            offspring.MaxTokens = (NumericGene)parentB.MaxTokens.Clone();
        }

        if (random.NextDouble() < _crossoverRate)
        {
            offspring.MaxRetries = (NumericGene)parentB.MaxRetries.Clone();
        }

        if (random.NextDouble() < _crossoverRate && parentB.BehaviorTree != null)
        {
            offspring.BehaviorTree = (BehaviorTreeGene)parentB.BehaviorTree.Clone();
        }

        if (random.NextDouble() < _crossoverRate && parentB.StateMachine != null)
        {
            offspring.StateMachine = (StateMachineGene)parentB.StateMachine.Clone();
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

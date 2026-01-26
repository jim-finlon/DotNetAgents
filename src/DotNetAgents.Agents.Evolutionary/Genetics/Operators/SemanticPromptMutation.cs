namespace DotNetAgents.Agents.Evolutionary.Genetics.Operators;

/// <summary>
/// Semantic prompt mutation operator that uses LLMs to rewrite prompts meaningfully.
/// This introduces semantic variation rather than random character-level changes.
/// Note: Full LLM integration requires async operations, so this applies enhanced prompt mutations.
/// </summary>
public sealed class SemanticPromptMutation : IMutationOperator
{
    /// <inheritdoc/>
    public void Mutate(
        AgentChromosome chromosome,
        double mutationRate,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(chromosome);
        ArgumentNullException.ThrowIfNull(random);

        // Apply enhanced mutations to prompt gene
        if (random.NextDouble() < mutationRate && chromosome.SystemPrompt != null)
        {
            var prompt = chromosome.SystemPrompt;

            // Enhanced prompt mutations
            if (random.NextDouble() < 0.3 && prompt.Instructions.Count > 0)
            {
                // Reorder instructions
                var shuffled = prompt.Instructions.OrderBy(_ => random.Next()).ToList();
                prompt.Instructions.Clear();
                prompt.Instructions.AddRange(shuffled);
            }

            if (random.NextDouble() < 0.2 && !string.IsNullOrWhiteSpace(prompt.Persona))
            {
                // Modify persona slightly
                var personaWords = prompt.Persona.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (personaWords.Length > 2)
                {
                    // Remove or add a word
                    if (random.NextDouble() < 0.5 && personaWords.Length > 3)
                    {
                        personaWords = personaWords.Skip(1).ToArray();
                    }
                    prompt.Persona = string.Join(" ", personaWords);
                }
            }
        }

        // Apply standard mutations to other genes
        var standardMutation = new StandardMutation();
        standardMutation.Mutate(chromosome, mutationRate * 0.5, random); // Lower rate for other genes
    }
}

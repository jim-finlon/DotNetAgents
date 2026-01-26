using System.Text.Json;

namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Represents the system prompt gene, including persona, instructions, and examples.
/// This gene can be mutated semantically using LLMs for meaningful variations.
/// </summary>
public sealed class PromptGene : IGene
{
    /// <summary>
    /// Gets or sets the innovation number for this gene.
    /// </summary>
    public int InnovationNumber { get; set; }

    /// <summary>
    /// Gets the gene type identifier.
    /// </summary>
    public string GeneType => "Prompt";

    /// <summary>
    /// Gets or sets the main system prompt content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent persona/role description.
    /// </summary>
    public string Persona { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of instructions for the agent.
    /// </summary>
    public List<string> Instructions { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of example interactions.
    /// </summary>
    public List<string> Examples { get; set; } = new();

    /// <summary>
    /// Gets or sets additional prompt metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptGene"/> class.
    /// </summary>
    public PromptGene()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptGene"/> class.
    /// </summary>
    /// <param name="content">The main prompt content.</param>
    /// <param name="persona">The agent persona.</param>
    /// <param name="instructions">List of instructions.</param>
    /// <param name="examples">List of examples.</param>
    /// <param name="innovationNumber">The innovation number.</param>
    public PromptGene(
        string content,
        string persona = "",
        List<string>? instructions = null,
        List<string>? examples = null,
        int innovationNumber = 0)
    {
        Content = content;
        Persona = persona;
        Instructions = instructions ?? new List<string>();
        Examples = examples ?? new List<string>();
        InnovationNumber = innovationNumber;
    }

    /// <inheritdoc/>
    public IGene Clone()
    {
        return new PromptGene(
            Content,
            Persona,
            new List<string>(Instructions),
            new List<string>(Examples),
            InnovationNumber)
        {
            Metadata = new Dictionary<string, object>(Metadata)
        };
    }

    /// <inheritdoc/>
    public void Mutate(double rate, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        // Standard mutation for prompt gene is limited - semantic mutation handled separately
        // This can do simple mutations like adding/removing instructions
        if (random.NextDouble() < rate && Instructions.Count > 0)
        {
            var idx = random.Next(Instructions.Count);
            // Simple mutation: slightly modify an instruction
            var instruction = Instructions[idx];
            if (random.NextDouble() < 0.5 && instruction.Length > 10)
            {
                // Remove a word or add punctuation variation
                var words = instruction.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 1 && random.NextDouble() < 0.3)
                {
                    words = words.Skip(1).ToArray(); // Remove first word
                    Instructions[idx] = string.Join(" ", words);
                }
            }
        }
    }

    /// <inheritdoc/>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Deserializes a PromptGene from JSON.
    /// </summary>
    public static PromptGene FromJson(string json)
    {
        return JsonSerializer.Deserialize<PromptGene>(json)
            ?? throw new InvalidOperationException("Failed to deserialize PromptGene");
    }

    /// <summary>
    /// Gets the full prompt text combining all components.
    /// </summary>
    public string GetFullPrompt()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Persona))
        {
            parts.Add($"You are: {Persona}");
        }

        if (!string.IsNullOrWhiteSpace(Content))
        {
            parts.Add(Content);
        }

        if (Instructions.Count > 0)
        {
            parts.Add("\nInstructions:");
            foreach (var instruction in Instructions)
            {
                parts.Add($"- {instruction}");
            }
        }

        if (Examples.Count > 0)
        {
            parts.Add("\nExamples:");
            foreach (var example in Examples)
            {
                parts.Add(example);
            }
        }

        return string.Join("\n", parts);
    }
}

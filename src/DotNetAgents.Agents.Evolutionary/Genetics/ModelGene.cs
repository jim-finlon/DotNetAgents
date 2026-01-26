using System.Text.Json;

namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Represents LLM model selection as a gene.
/// </summary>
public sealed class ModelGene : IGene
{
    /// <summary>
    /// Gets or sets the innovation number for this gene.
    /// </summary>
    public int InnovationNumber { get; set; }

    /// <summary>
    /// Gets the gene type identifier.
    /// </summary>
    public string GeneType => "Model";

    /// <summary>
    /// Gets or sets the model identifier (e.g., "gpt-4", "claude-3-opus", "llama2").
    /// </summary>
    public string ModelIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider name (e.g., "OpenAI", "Anthropic", "Ollama").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional model configuration.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelGene"/> class.
    /// </summary>
    public ModelGene()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelGene"/> class.
    /// </summary>
    /// <param name="modelIdentifier">The model identifier.</param>
    /// <param name="provider">The provider name.</param>
    /// <param name="innovationNumber">The innovation number.</param>
    public ModelGene(string modelIdentifier, string provider, int innovationNumber = 0)
    {
        ModelIdentifier = modelIdentifier;
        Provider = provider;
        InnovationNumber = innovationNumber;
    }

    /// <inheritdoc/>
    public IGene Clone()
    {
        return new ModelGene(ModelIdentifier, Provider, InnovationNumber)
        {
            Configuration = new Dictionary<string, object>(Configuration)
        };
    }

    /// <inheritdoc/>
    public void Mutate(double rate, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        // Model mutation: switch between available models
        // This would typically be constrained by available models in the system
        // For now, we'll do simple mutations on configuration
        if (random.NextDouble() < rate && Configuration.Count > 0)
        {
            var key = Configuration.Keys.ElementAt(random.Next(Configuration.Count));
            var value = Configuration[key];

            if (value is double d)
            {
                Configuration[key] = d * (0.9 + random.NextDouble() * 0.2);
            }
        }
    }

    /// <inheritdoc/>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Deserializes a ModelGene from JSON.
    /// </summary>
    public static ModelGene FromJson(string json)
    {
        return JsonSerializer.Deserialize<ModelGene>(json)
            ?? throw new InvalidOperationException("Failed to deserialize ModelGene");
    }
}

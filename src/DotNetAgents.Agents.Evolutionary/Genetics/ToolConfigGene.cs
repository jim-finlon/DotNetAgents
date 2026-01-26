using System.Text.Json;

namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Represents tool configuration as a gene, including which tools are enabled and their parameters.
/// </summary>
public sealed class ToolConfigGene : IGene
{
    /// <summary>
    /// Gets or sets the innovation number for this gene.
    /// </summary>
    public int InnovationNumber { get; set; }

    /// <summary>
    /// Gets the gene type identifier.
    /// </summary>
    public string GeneType => "ToolConfig";

    /// <summary>
    /// Gets or sets the dictionary of tool configurations.
    /// Key is tool name, value is tool settings.
    /// </summary>
    public Dictionary<string, ToolSettings> Tools { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolConfigGene"/> class.
    /// </summary>
    public ToolConfigGene()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolConfigGene"/> class.
    /// </summary>
    /// <param name="tools">Dictionary of tool configurations.</param>
    /// <param name="innovationNumber">The innovation number.</param>
    public ToolConfigGene(Dictionary<string, ToolSettings>? tools = null, int innovationNumber = 0)
    {
        Tools = tools ?? new Dictionary<string, ToolSettings>();
        InnovationNumber = innovationNumber;
    }

    /// <inheritdoc/>
    public IGene Clone()
    {
        var clonedTools = new Dictionary<string, ToolSettings>();
        foreach (var kvp in Tools)
        {
            clonedTools[kvp.Key] = kvp.Value.Clone();
        }

        return new ToolConfigGene(clonedTools, InnovationNumber);
    }

    /// <inheritdoc/>
    public void Mutate(double rate, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        foreach (var toolName in Tools.Keys.ToList())
        {
            var tool = Tools[toolName];

            // Toggle enabled state
            if (random.NextDouble() < rate)
            {
                tool.Enabled = !tool.Enabled;
            }

            // Mutate parameters
            if (tool.Enabled && random.NextDouble() < rate)
            {
                foreach (var paramKey in tool.Parameters.Keys.ToList())
                {
                    var paramValue = tool.Parameters[paramKey];
                    if (paramValue is double d)
                    {
                        // Mutate numeric parameters
                        tool.Parameters[paramKey] = d * (0.8 + random.NextDouble() * 0.4);
                    }
                    else if (paramValue is int i)
                    {
                        // Mutate integer parameters
                        tool.Parameters[paramKey] = (int)(i * (0.8 + random.NextDouble() * 0.4));
                    }
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
    /// Deserializes a ToolConfigGene from JSON.
    /// </summary>
    public static ToolConfigGene FromJson(string json)
    {
        return JsonSerializer.Deserialize<ToolConfigGene>(json)
            ?? throw new InvalidOperationException("Failed to deserialize ToolConfigGene");
    }
}

/// <summary>
/// Represents settings for a single tool.
/// </summary>
public sealed class ToolSettings
{
    /// <summary>
    /// Gets or sets whether the tool is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets tool-specific parameters.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Creates a clone of this tool settings instance.
    /// </summary>
    public ToolSettings Clone()
    {
        return new ToolSettings
        {
            Enabled = Enabled,
            Parameters = new Dictionary<string, object>(Parameters)
        };
    }
}

using System.Text.Json;

namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Represents an agent configuration as an evolvable chromosome.
/// The chromosome contains genes that define all aspects of agent behavior.
/// </summary>
public sealed class AgentChromosome
{
    /// <summary>
    /// Gets the unique identifier for this chromosome.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the ID of the first parent (null for initial population).
    /// </summary>
    public Guid? ParentAId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the second parent (null for initial population or asexual reproduction).
    /// </summary>
    public Guid? ParentBId { get; set; }

    /// <summary>
    /// Gets or sets the generation number this chromosome belongs to.
    /// </summary>
    public int Generation { get; set; }

    /// <summary>
    /// Gets or sets the fitness score (0.0 to 1.0).
    /// </summary>
    public double Fitness { get; set; }

    /// <summary>
    /// Gets or sets the adjusted fitness (for speciation).
    /// </summary>
    public double AdjustedFitness { get; set; }

    /// <summary>
    /// Gets or sets the species ID this chromosome belongs to.
    /// </summary>
    public int SpeciesId { get; set; }

    /// <summary>
    /// Gets or sets the system prompt gene.
    /// </summary>
    public PromptGene SystemPrompt { get; set; } = new();

    /// <summary>
    /// Gets or sets the tool configuration gene.
    /// </summary>
    public ToolConfigGene ToolConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets the strategy gene.
    /// </summary>
    public StrategyGene Strategies { get; set; } = new();

    /// <summary>
    /// Gets or sets the model gene.
    /// </summary>
    public ModelGene Model { get; set; } = new();

    /// <summary>
    /// Gets or sets the temperature gene.
    /// </summary>
    public NumericGene Temperature { get; set; } = new NumericGene { Name = "Temperature", Value = 0.7, Min = 0.0, Max = 2.0 };

    /// <summary>
    /// Gets or sets the max tokens gene.
    /// </summary>
    public NumericGene MaxTokens { get; set; } = new NumericGene { Name = "MaxTokens", Value = 4096, Min = 256, Max = 32768, IsInteger = true };

    /// <summary>
    /// Gets or sets the max retries gene.
    /// </summary>
    public NumericGene MaxRetries { get; set; } = new NumericGene { Name = "MaxRetries", Value = 3, Min = 0, Max = 10, IsInteger = true };

    /// <summary>
    /// Gets or sets the behavior tree gene (optional).
    /// </summary>
    public BehaviorTreeGene? BehaviorTree { get; set; }

    /// <summary>
    /// Gets or sets the state machine gene (optional).
    /// </summary>
    public StateMachineGene? StateMachine { get; set; }

    /// <summary>
    /// Gets or sets additional numeric genes.
    /// </summary>
    public Dictionary<string, NumericGene> AdditionalNumericGenes { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets all genes in this chromosome.
    /// </summary>
    public IEnumerable<IGene> AllGenes
    {
        get
        {
            yield return SystemPrompt;
            yield return ToolConfiguration;
            yield return Strategies;
            yield return Model;
            yield return Temperature;
            yield return MaxTokens;
            yield return MaxRetries;

            if (BehaviorTree != null)
                yield return BehaviorTree;

            if (StateMachine != null)
                yield return StateMachine;

            foreach (var gene in AdditionalNumericGenes.Values)
            {
                yield return gene;
            }
        }
    }

    /// <summary>
    /// Gets the innovation history (list of innovation numbers).
    /// </summary>
    public List<int> InnovationHistory => AllGenes.Select(g => g.InnovationNumber).OrderBy(i => i).ToList();

    /// <summary>
    /// Calculates compatibility distance between this chromosome and another.
    /// Used for speciation. Based on NEAT compatibility distance formula.
    /// </summary>
    /// <param name="other">The other chromosome.</param>
    /// <param name="excessCoefficient">Weight for excess genes.</param>
    /// <param name="disjointCoefficient">Weight for disjoint genes.</param>
    /// <param name="weightCoefficient">Weight for weight differences.</param>
    /// <returns>The compatibility distance.</returns>
    public double CalculateCompatibilityDistance(
        AgentChromosome other,
        double excessCoefficient = 1.0,
        double disjointCoefficient = 1.0,
        double weightCoefficient = 0.4)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisInnovations = InnovationHistory.ToHashSet();
        var otherInnovations = other.InnovationHistory.ToHashSet();

        var allInnovations = thisInnovations.Union(otherInnovations).OrderBy(i => i).ToList();
        var maxInnovation = allInnovations.LastOrDefault();

        if (maxInnovation == 0)
            return 0.0;

        var matching = thisInnovations.Intersect(otherInnovations).Count();
        var disjoint = thisInnovations.Except(otherInnovations).Count() + otherInnovations.Except(thisInnovations).Count();
        var excess = allInnovations.Count - matching - disjoint;

        var N = Math.Max(thisInnovations.Count, otherInnovations.Count);
        if (N < 20)
            N = 1; // Normalize for small genomes

        var distance = (excessCoefficient * excess + disjointCoefficient * disjoint) / N;

        // Add weight differences for matching genes
        if (matching > 0)
        {
            var weightDiff = CalculateWeightDifference(other);
            distance += weightCoefficient * weightDiff / matching;
        }

        return distance;
    }

    /// <summary>
    /// Calculates the average weight difference for matching genes.
    /// </summary>
    private double CalculateWeightDifference(AgentChromosome other)
    {
        // For now, use a simple heuristic based on gene value differences
        // This can be enhanced to compare actual gene values
        var differences = new List<double>();

        // Compare numeric genes
        if (Temperature.InnovationNumber == other.Temperature.InnovationNumber)
        {
            differences.Add(Math.Abs(Temperature.Value - other.Temperature.Value));
        }

        if (MaxTokens.InnovationNumber == other.MaxTokens.InnovationNumber)
        {
            differences.Add(Math.Abs(MaxTokens.Value - other.MaxTokens.Value) / 1000.0); // Normalize
        }

        return differences.Count > 0 ? differences.Average() : 0.0;
    }

    /// <summary>
    /// Serializes this chromosome to JSON.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    /// <summary>
    /// Deserializes an AgentChromosome from JSON.
    /// </summary>
    public static AgentChromosome FromJson(string json)
    {
        return JsonSerializer.Deserialize<AgentChromosome>(json)
            ?? throw new InvalidOperationException("Failed to deserialize AgentChromosome");
    }
}

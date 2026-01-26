using System.Text.Json;

namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Represents agent strategy parameters as a gene, including retry policies, confidence thresholds, and decision-making heuristics.
/// </summary>
public sealed class StrategyGene : IGene
{
    /// <summary>
    /// Gets or sets the innovation number for this gene.
    /// </summary>
    public int InnovationNumber { get; set; }

    /// <summary>
    /// Gets the gene type identifier.
    /// </summary>
    public string GeneType => "Strategy";

    /// <summary>
    /// Gets or sets the retry backoff multiplier.
    /// </summary>
    public double RetryBackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the confidence threshold for tool selection (0.0 to 1.0).
    /// </summary>
    public double ConfidenceThreshold { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the escalation policy when confidence is low.
    /// </summary>
    public string EscalationPolicy { get; set; } = "AskHuman";

    /// <summary>
    /// Gets or sets the maximum chain depth for recursive tool calls.
    /// </summary>
    public int MaxChainDepth { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether to use reflection (self-critique) after tool execution.
    /// </summary>
    public bool UseReflection { get; set; } = true;

    /// <summary>
    /// Gets or sets the reflection threshold (when to reflect).
    /// </summary>
    public double ReflectionThreshold { get; set; } = 0.6;

    /// <summary>
    /// Gets or sets additional strategy parameters.
    /// </summary>
    public Dictionary<string, object> AdditionalParameters { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StrategyGene"/> class.
    /// </summary>
    public StrategyGene()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StrategyGene"/> class.
    /// </summary>
    /// <param name="retryBackoffMultiplier">Retry backoff multiplier.</param>
    /// <param name="confidenceThreshold">Confidence threshold.</param>
    /// <param name="escalationPolicy">Escalation policy.</param>
    /// <param name="maxChainDepth">Maximum chain depth.</param>
    /// <param name="useReflection">Whether to use reflection.</param>
    /// <param name="reflectionThreshold">Reflection threshold.</param>
    /// <param name="innovationNumber">The innovation number.</param>
    public StrategyGene(
        double retryBackoffMultiplier = 2.0,
        double confidenceThreshold = 0.7,
        string escalationPolicy = "AskHuman",
        int maxChainDepth = 5,
        bool useReflection = true,
        double reflectionThreshold = 0.6,
        int innovationNumber = 0)
    {
        RetryBackoffMultiplier = retryBackoffMultiplier;
        ConfidenceThreshold = confidenceThreshold;
        EscalationPolicy = escalationPolicy;
        MaxChainDepth = maxChainDepth;
        UseReflection = useReflection;
        ReflectionThreshold = reflectionThreshold;
        InnovationNumber = innovationNumber;
    }

    /// <inheritdoc/>
    public IGene Clone()
    {
        return new StrategyGene(
            RetryBackoffMultiplier,
            ConfidenceThreshold,
            EscalationPolicy,
            MaxChainDepth,
            UseReflection,
            ReflectionThreshold,
            InnovationNumber)
        {
            AdditionalParameters = new Dictionary<string, object>(AdditionalParameters)
        };
    }

    /// <inheritdoc/>
    public void Mutate(double rate, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (random.NextDouble() < rate)
        {
            RetryBackoffMultiplier *= 0.8 + random.NextDouble() * 0.4; // ±20%
        }

        if (random.NextDouble() < rate)
        {
            ConfidenceThreshold = Math.Clamp(
                ConfidenceThreshold + (random.NextDouble() - 0.5) * 0.2,
                0.1,
                0.99);
        }

        if (random.NextDouble() < rate)
        {
            UseReflection = !UseReflection;
        }

        if (random.NextDouble() < rate)
        {
            ReflectionThreshold = Math.Clamp(
                ReflectionThreshold + (random.NextDouble() - 0.5) * 0.2,
                0.1,
                0.99);
        }

        if (random.NextDouble() < rate)
        {
            MaxChainDepth = Math.Max(1, MaxChainDepth + random.Next(-2, 3));
        }

        // Mutate escalation policy
        if (random.NextDouble() < rate * 0.5) // Lower probability
        {
            var policies = new[] { "AskHuman", "Retry", "Skip", "Fail" };
            EscalationPolicy = policies[random.Next(policies.Length)];
        }
    }

    /// <inheritdoc/>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Deserializes a StrategyGene from JSON.
    /// </summary>
    public static StrategyGene FromJson(string json)
    {
        return JsonSerializer.Deserialize<StrategyGene>(json)
            ?? throw new InvalidOperationException("Failed to deserialize StrategyGene");
    }
}

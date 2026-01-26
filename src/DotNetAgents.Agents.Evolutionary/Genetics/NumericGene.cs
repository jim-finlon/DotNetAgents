using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Represents a numeric gene with a value constrained within a min/max range.
/// Used for parameters like temperature, max tokens, retry counts, etc.
/// </summary>
public sealed class NumericGene : IGene
{
    /// <summary>
    /// Gets or sets the innovation number for this gene.
    /// </summary>
    public int InnovationNumber { get; set; }

    /// <summary>
    /// Gets the gene type identifier.
    /// </summary>
    public string GeneType => "Numeric";

    /// <summary>
    /// Gets or sets the name of this numeric parameter.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Gets or sets whether this is an integer value (true) or floating point (false).
    /// </summary>
    public bool IsInteger { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumericGene"/> class.
    /// </summary>
    public NumericGene()
    {
        Name = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumericGene"/> class.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The initial value.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="isInteger">Whether this is an integer value.</param>
    /// <param name="innovationNumber">The innovation number.</param>
    public NumericGene(
        string name,
        double value,
        double min,
        double max,
        bool isInteger = false,
        int innovationNumber = 0)
    {
        Name = name;
        Value = value;
        Min = min;
        Max = max;
        IsInteger = isInteger;
        InnovationNumber = innovationNumber;
    }

    /// <inheritdoc/>
    public IGene Clone()
    {
        return new NumericGene { Name = Name, Value = Value, Min = Min, Max = Max, IsInteger = IsInteger, InnovationNumber = InnovationNumber };
    }

    /// <inheritdoc/>
    public void Mutate(double rate, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (random.NextDouble() >= rate)
            return;

        var range = Max - Min;
        var mutationAmount = (random.NextDouble() - 0.5) * range * 0.2; // ±10% of range
        var newValue = Value + mutationAmount;

        // Clamp to bounds
        newValue = Math.Max(Min, Math.Min(Max, newValue));

        // Round if integer
        if (IsInteger)
        {
            newValue = Math.Round(newValue);
        }

        Value = newValue;
    }

    /// <inheritdoc/>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Deserializes a NumericGene from JSON.
    /// </summary>
    public static NumericGene FromJson(string json)
    {
        return JsonSerializer.Deserialize<NumericGene>(json)
            ?? throw new InvalidOperationException("Failed to deserialize NumericGene");
    }
}

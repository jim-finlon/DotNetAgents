namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Base interface for all genes in an agent chromosome.
/// Genes represent evolvable components of agent configuration.
/// </summary>
public interface IGene
{
    /// <summary>
    /// Gets the innovation number for this gene.
    /// Innovation numbers enable meaningful crossover between chromosomes with different structures.
    /// </summary>
    int InnovationNumber { get; }

    /// <summary>
    /// Gets the type identifier for this gene.
    /// </summary>
    string GeneType { get; }

    /// <summary>
    /// Creates a deep copy of this gene.
    /// </summary>
    /// <returns>A cloned gene instance.</returns>
    IGene Clone();

    /// <summary>
    /// Mutates this gene with the given mutation rate.
    /// </summary>
    /// <param name="rate">The mutation rate (0.0 to 1.0).</param>
    /// <param name="random">Random number generator for mutations.</param>
    void Mutate(double rate, Random random);

    /// <summary>
    /// Serializes this gene to JSON.
    /// </summary>
    /// <returns>JSON representation of the gene.</returns>
    string ToJson();

    /// <summary>
    /// Deserializes a gene from JSON.
    /// </summary>
    /// <param name="json">JSON representation of the gene.</param>
    /// <returns>Deserialized gene instance.</returns>
    static IGene FromJson(string json) => throw new NotImplementedException("Must be implemented by derived classes");
}

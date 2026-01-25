namespace DotNetAgents.Education.Models;

/// <summary>
/// Represents the context of an educational concept for dialogue generation.
/// </summary>
public record ConceptContext
{
    /// <summary>
    /// Gets the concept identifier.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the concept description or explanation.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the learning objectives for this concept.
    /// </summary>
    public IReadOnlyList<string> LearningObjectives { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the key vocabulary terms for this concept.
    /// </summary>
    public IReadOnlyList<string> KeyTerms { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets additional metadata about the concept.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
    
    /// <summary>
    /// Gets the student ID this concept context is for (optional).
    /// </summary>
    public string? StudentId { get; init; }
}

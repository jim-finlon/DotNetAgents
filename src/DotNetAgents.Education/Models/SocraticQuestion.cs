using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Models;

/// <summary>
/// Represents a Socratic question used to guide student learning.
/// </summary>
public record SocraticQuestion
{
    /// <summary>
    /// Gets the question text.
    /// </summary>
    public string QuestionText { get; init; } = string.Empty;

    /// <summary>
    /// Gets the type of Socratic question.
    /// </summary>
    public SocraticQuestionType Type { get; init; }

    /// <summary>
    /// Gets the concept this question relates to.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the expected understanding level this question targets.
    /// </summary>
    public StudentUnderstanding TargetLevel { get; init; }

    /// <summary>
    /// Gets optional hints or guidance for the question.
    /// </summary>
    public IReadOnlyList<string> Hints { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets metadata about the question.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets the timestamp when the question was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents the type of Socratic question.
/// </summary>
public enum SocraticQuestionType
{
    /// <summary>
    /// Clarifying question - "What do you mean by...?"
    /// </summary>
    Clarifying,

    /// <summary>
    /// Probing question - "Can you explain why...?"
    /// </summary>
    Probing,

    /// <summary>
    /// Assumption question - "What assumptions are you making?"
    /// </summary>
    Assumption,

    /// <summary>
    /// Implication question - "What would happen if...?"
    /// </summary>
    Implication,

    /// <summary>
    /// Viewpoint question - "How might someone else see this?"
    /// </summary>
    Viewpoint
}

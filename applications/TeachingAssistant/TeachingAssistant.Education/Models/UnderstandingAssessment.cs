using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Models;

/// <summary>
/// Represents an assessment of a student's understanding based on their response.
/// </summary>
public record UnderstandingAssessment
{
    /// <summary>
    /// Gets the assessed understanding level.
    /// </summary>
    public StudentUnderstanding AssessedLevel { get; init; }

    /// <summary>
    /// Gets a confidence score (0-1) indicating how confident the assessment is.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets feedback for the student.
    /// </summary>
    public string Feedback { get; init; } = string.Empty;

    /// <summary>
    /// Gets identified misconceptions in the student's response.
    /// </summary>
    public IReadOnlyList<string> Misconceptions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets whether the student's response indicates they need more help.
    /// </summary>
    public bool NeedsMoreHelp { get; init; }

    /// <summary>
    /// Gets whether the student's response indicates they have mastered the concept.
    /// </summary>
    public bool HasMastery { get; init; }

    /// <summary>
    /// Gets suggestions for next steps in learning.
    /// </summary>
    public IReadOnlyList<string> NextSteps { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Represents a scaffolded hint at a specific level.
/// </summary>
public record ScaffoldedHint
{
    /// <summary>
    /// Gets the hint level (1-5, where 1 is most general and 5 is most specific).
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Gets the hint text.
    /// </summary>
    public string HintText { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether this is the final hint (level 5).
    /// </summary>
    public bool IsFinalHint => Level >= 5;
}

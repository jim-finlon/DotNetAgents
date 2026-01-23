using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Extensions;

/// <summary>
/// Extension methods for <see cref="ConceptId"/>.
/// </summary>
public static class ConceptIdExtensions
{
    /// <summary>
    /// Checks if this concept is appropriate for a given grade level.
    /// </summary>
    /// <param name="conceptId">The concept identifier.</param>
    /// <param name="gradeLevel">The grade level to check.</param>
    /// <returns>True if appropriate, false otherwise.</returns>
    public static bool IsAppropriateForGrade(this ConceptId conceptId, GradeLevel gradeLevel)
    {
        // Concepts are appropriate for their target grade level or higher
        return conceptId.GradeLevel <= gradeLevel;
    }

    /// <summary>
    /// Gets a display-friendly string representation of the concept.
    /// </summary>
    /// <param name="conceptId">The concept identifier.</param>
    /// <returns>A formatted string.</returns>
    public static string ToDisplayString(this ConceptId conceptId)
    {
        return $"{conceptId.Value} ({conceptId.Subject}, {conceptId.GradeLevel})";
    }

    /// <summary>
    /// Creates a concept ID for a different grade level.
    /// </summary>
    /// <param name="conceptId">The original concept identifier.</param>
    /// <param name="newGradeLevel">The new grade level.</param>
    /// <returns>A new ConceptId with the updated grade level.</returns>
    public static ConceptId WithGradeLevel(this ConceptId conceptId, GradeLevel newGradeLevel)
    {
        return new ConceptId(conceptId.Value, conceptId.Subject, newGradeLevel);
    }
}

/// <summary>
/// Extension methods for <see cref="MasteryLevel"/>.
/// </summary>
public static class MasteryLevelExtensions
{
    /// <summary>
    /// Checks if the mastery level meets a minimum threshold.
    /// </summary>
    /// <param name="level">The mastery level.</param>
    /// <param name="minimum">The minimum required level.</param>
    /// <returns>True if the level meets or exceeds the minimum.</returns>
    public static bool MeetsMinimum(this MasteryLevel level, MasteryLevel minimum)
    {
        return level >= minimum;
    }

    /// <summary>
    /// Gets the next mastery level to work towards.
    /// </summary>
    /// <param name="level">The current mastery level.</param>
    /// <returns>The next level, or null if already at mastery.</returns>
    public static MasteryLevel? GetNextLevel(this MasteryLevel level)
    {
        return level switch
        {
            MasteryLevel.Novice => MasteryLevel.Developing,
            MasteryLevel.Developing => MasteryLevel.Proficient,
            MasteryLevel.Proficient => MasteryLevel.Advanced,
            MasteryLevel.Advanced => MasteryLevel.Mastery,
            MasteryLevel.Mastery => null,
            _ => MasteryLevel.Developing
        };
    }
}

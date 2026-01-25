namespace DotNetAgents.Education.Models;

/// <summary>
/// Represents the mastery level of a student for a concept.
/// </summary>
public enum MasteryLevel
{
    /// <summary>
    /// No understanding (0-40%).
    /// </summary>
    Novice = 0,

    /// <summary>
    /// Partial understanding (40-60%).
    /// </summary>
    Developing = 1,

    /// <summary>
    /// Good understanding (60-80%).
    /// </summary>
    Proficient = 2,

    /// <summary>
    /// Strong understanding (80-95%).
    /// </summary>
    Advanced = 3,

    /// <summary>
    /// Complete understanding (95-100%).
    /// </summary>
    Mastery = 4
}

/// <summary>
/// Extension methods for <see cref="MasteryLevel"/>.
/// </summary>
public static class MasteryLevelExtensions
{
    /// <summary>
    /// Converts a score percentage to a mastery level.
    /// </summary>
    /// <param name="score">The score as a percentage (0-100).</param>
    /// <returns>The corresponding mastery level.</returns>
    public static MasteryLevel FromScore(double score)
    {
        return score switch
        {
            >= 95 => MasteryLevel.Mastery,
            >= 80 => MasteryLevel.Advanced,
            >= 60 => MasteryLevel.Proficient,
            >= 40 => MasteryLevel.Developing,
            _ => MasteryLevel.Novice
        };
    }

    /// <summary>
    /// Converts a mastery level to a score range.
    /// </summary>
    /// <param name="level">The mastery level.</param>
    /// <returns>A tuple containing the minimum and maximum score for the level.</returns>
    public static (double Min, double Max) ToScoreRange(this MasteryLevel level)
    {
        return level switch
        {
            MasteryLevel.Novice => (0, 40),
            MasteryLevel.Developing => (40, 60),
            MasteryLevel.Proficient => (60, 80),
            MasteryLevel.Advanced => (80, 95),
            MasteryLevel.Mastery => (95, 100),
            _ => (0, 0)
        };
    }
}

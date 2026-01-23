using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Helpers;

/// <summary>
/// Helper methods for education-related operations.
/// </summary>
public static class EducationHelpers
{
    /// <summary>
    /// Gets the age range for a grade level.
    /// </summary>
    /// <param name="gradeLevel">The grade level.</param>
    /// <returns>A tuple containing the minimum and maximum ages.</returns>
    public static (int MinAge, int MaxAge) GetAgeRange(GradeLevel gradeLevel)
    {
        return gradeLevel switch
        {
            GradeLevel.K2 => (5, 8),
            GradeLevel.G3_5 => (8, 11),
            GradeLevel.G6_8 => (11, 14),
            GradeLevel.G9_10 => (14, 16),
            GradeLevel.G11_12 => (16, 18),
            GradeLevel.College => (18, 25),
            GradeLevel.Professional => (25, 100),
            _ => (0, 0)
        };
    }

    /// <summary>
    /// Determines the appropriate grade level for a given age.
    /// </summary>
    /// <param name="age">The age in years.</param>
    /// <returns>The appropriate grade level.</returns>
    public static GradeLevel GetGradeLevelForAge(int age)
    {
        if (age >= 5 && age <= 8)
            return GradeLevel.K2;
        if (age >= 9 && age <= 11)
            return GradeLevel.G3_5;
        if (age >= 12 && age <= 14)
            return GradeLevel.G6_8;
        if (age >= 15 && age <= 16)
            return GradeLevel.G9_10;
        if (age >= 17 && age <= 18)
            return GradeLevel.G11_12;
        if (age >= 19 && age <= 25)
            return GradeLevel.College;
        if (age > 25)
            return GradeLevel.Professional;
        
        return GradeLevel.G3_5; // Default
    }

    /// <summary>
    /// Validates a concept ID.
    /// </summary>
    /// <param name="conceptId">The concept ID to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValidConceptId(ConceptId? conceptId)
    {
        return conceptId != null &&
               !string.IsNullOrWhiteSpace(conceptId.Value) &&
               conceptId.GradeLevel != default;
    }

    /// <summary>
    /// Creates a concept ID from a string value.
    /// </summary>
    /// <param name="value">The concept value.</param>
    /// <param name="subject">The subject area.</param>
    /// <param name="gradeLevel">The grade level.</param>
    /// <returns>A new ConceptId instance.</returns>
    public static ConceptId CreateConceptId(
        string value,
        SubjectArea subject,
        GradeLevel gradeLevel)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Concept value cannot be null or empty.", nameof(value));

        return new ConceptId(value, subject, gradeLevel);
    }

    /// <summary>
    /// Formats a mastery score as a percentage string.
    /// </summary>
    /// <param name="score">The mastery score (0.0 to 1.0).</param>
    /// <param name="decimals">The number of decimal places.</param>
    /// <returns>A formatted percentage string.</returns>
    public static string FormatMasteryScore(double score, int decimals = 1)
    {
        var percentage = score * 100;
        var formatString = "F" + decimals;
        return percentage.ToString(formatString, System.Globalization.CultureInfo.InvariantCulture) + "%";
    }

    /// <summary>
    /// Gets a human-readable description of a mastery level.
    /// </summary>
    /// <param name="level">The mastery level.</param>
    /// <returns>A description string.</returns>
    public static string GetMasteryDescription(MasteryLevel level)
    {
        return level switch
        {
            MasteryLevel.Novice => "Novice - Beginning to learn",
            MasteryLevel.Developing => "Developing - Making progress",
            MasteryLevel.Proficient => "Proficient - Good understanding",
            MasteryLevel.Advanced => "Advanced - Strong understanding",
            MasteryLevel.Mastery => "Mastery - Complete understanding",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Calculates the average mastery score from a collection of mastery levels.
    /// </summary>
    /// <param name="masteryLevels">The mastery levels.</param>
    /// <returns>The average score (0.0 to 1.0).</returns>
    public static double CalculateAverageMastery(IEnumerable<MasteryLevel> masteryLevels)
    {
        var levels = masteryLevels.ToList();
        if (levels.Count == 0)
            return 0.0;

        var totalScore = levels.Sum(level => GetScoreForLevel(level));
        return totalScore / levels.Count;
    }

    private static double GetScoreForLevel(MasteryLevel level)
    {
        return level switch
        {
            MasteryLevel.Novice => 0.2,
            MasteryLevel.Developing => 0.5,
            MasteryLevel.Proficient => 0.7,
            MasteryLevel.Advanced => 0.875,
            MasteryLevel.Mastery => 0.975,
            _ => 0.0
        };
    }
}

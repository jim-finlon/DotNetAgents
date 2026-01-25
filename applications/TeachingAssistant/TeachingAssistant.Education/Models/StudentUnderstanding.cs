namespace DotNetAgents.Education.Models;

/// <summary>
/// Represents a student's current understanding level of a concept.
/// </summary>
public enum StudentUnderstanding
{
    /// <summary>
    /// No understanding - student has no knowledge of the concept.
    /// </summary>
    None,

    /// <summary>
    /// Beginner understanding - student has basic awareness.
    /// </summary>
    Beginner,

    /// <summary>
    /// Intermediate understanding - student has partial knowledge.
    /// </summary>
    Intermediate,

    /// <summary>
    /// Advanced understanding - student has good knowledge.
    /// </summary>
    Advanced,

    /// <summary>
    /// Expert understanding - student has mastery.
    /// </summary>
    Expert
}

using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Memory;

/// <summary>
/// Represents a student's profile information.
/// </summary>
public record StudentProfile
{
    /// <summary>
    /// Gets the unique identifier for the student.
    /// </summary>
    public string StudentId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the student's name (optional, for privacy).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the student's grade level.
    /// </summary>
    public GradeLevel GradeLevel { get; init; }

    /// <summary>
    /// Gets the student's preferred learning style.
    /// </summary>
    public string? LearningStyle { get; init; }

    /// <summary>
    /// Gets the student's interests (for personalized content).
    /// </summary>
    public IReadOnlyList<string> Interests { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the student's preferred language.
    /// </summary>
    public string? PreferredLanguage { get; init; }

    /// <summary>
    /// Gets additional metadata about the student.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets when the profile was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets when the profile was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; init; } = DateTimeOffset.UtcNow;
}

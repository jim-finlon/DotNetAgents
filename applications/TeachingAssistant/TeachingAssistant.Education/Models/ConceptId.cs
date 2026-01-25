namespace DotNetAgents.Education.Models;

/// <summary>
/// Represents a unique identifier for an educational concept.
/// </summary>
public record ConceptId
{
    /// <summary>
    /// Gets the unique identifier string.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Gets the subject area this concept belongs to.
    /// </summary>
    public SubjectArea Subject { get; init; }

    /// <summary>
    /// Gets the grade level this concept is appropriate for.
    /// </summary>
    public GradeLevel GradeLevel { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConceptId"/> class.
    /// </summary>
    /// <param name="value">The unique identifier string.</param>
    /// <param name="subject">The subject area.</param>
    /// <param name="gradeLevel">The grade level.</param>
    public ConceptId(string value, SubjectArea subject, GradeLevel gradeLevel)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Subject = subject;
        GradeLevel = gradeLevel;
    }

    /// <summary>
    /// Returns the string representation of the concept ID.
    /// </summary>
    public override string ToString() => Value;
}

/// <summary>
/// Represents subject areas in education.
/// </summary>
public enum SubjectArea
{
    /// <summary>
    /// Science subject area.
    /// </summary>
    Science,

    /// <summary>
    /// Mathematics subject area.
    /// </summary>
    Mathematics,

    /// <summary>
    /// Language Arts subject area.
    /// </summary>
    LanguageArts,

    /// <summary>
    /// Social Studies subject area.
    /// </summary>
    SocialStudies,

    /// <summary>
    /// Other subject area.
    /// </summary>
    Other
}

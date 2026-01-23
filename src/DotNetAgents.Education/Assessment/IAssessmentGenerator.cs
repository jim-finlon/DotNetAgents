using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Assessment;

/// <summary>
/// Represents the difficulty level of an assessment question.
/// </summary>
public enum DifficultyLevel
{
    /// <summary>
    /// Easy difficulty.
    /// </summary>
    Easy,

    /// <summary>
    /// Medium difficulty.
    /// </summary>
    Medium,

    /// <summary>
    /// Hard difficulty.
    /// </summary>
    Hard
}

/// <summary>
/// Represents the type of assessment question.
/// </summary>
public enum QuestionType
{
    /// <summary>
    /// Multiple choice question.
    /// </summary>
    MultipleChoice,

    /// <summary>
    /// Short answer question.
    /// </summary>
    ShortAnswer,

    /// <summary>
    /// Essay question.
    /// </summary>
    Essay,

    /// <summary>
    /// True/false question.
    /// </summary>
    TrueFalse,

    /// <summary>
    /// Matching question.
    /// </summary>
    Matching
}

/// <summary>
/// Represents a specification for generating an assessment.
/// </summary>
public record AssessmentSpecification
{
    /// <summary>
    /// Gets the concept this assessment covers.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the number of questions to generate.
    /// </summary>
    public int QuestionCount { get; init; } = 5;

    /// <summary>
    /// Gets the question types to include.
    /// </summary>
    public IReadOnlyList<QuestionType> QuestionTypes { get; init; } = new[] { QuestionType.MultipleChoice, QuestionType.ShortAnswer };

    /// <summary>
    /// Gets the difficulty distribution (Easy, Medium, Hard percentages).
    /// </summary>
    public (int Easy, int Medium, int Hard) DifficultyDistribution { get; init; } = (30, 50, 20);

    /// <summary>
    /// Gets the grade level for the assessment.
    /// </summary>
    public GradeLevel GradeLevel { get; init; }
}

/// <summary>
/// Represents an assessment question.
/// </summary>
public record AssessmentQuestion
{
    /// <summary>
    /// Gets the question identifier.
    /// </summary>
    public string QuestionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the question text.
    /// </summary>
    public string QuestionText { get; init; } = string.Empty;

    /// <summary>
    /// Gets the question type.
    /// </summary>
    public QuestionType Type { get; init; }

    /// <summary>
    /// Gets the difficulty level.
    /// </summary>
    public DifficultyLevel Difficulty { get; init; }

    /// <summary>
    /// Gets the concept this question relates to.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the correct answer(s).
    /// </summary>
    public IReadOnlyList<string> CorrectAnswers { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the distractors (for multiple choice).
    /// </summary>
    public IReadOnlyList<string> Distractors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the points/weight for this question.
    /// </summary>
    public int Points { get; init; } = 1;

    /// <summary>
    /// Gets additional metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Represents a complete assessment.
/// </summary>
public record Assessment
{
    /// <summary>
    /// Gets the assessment identifier.
    /// </summary>
    public string AssessmentId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the concept this assessment covers.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the assessment title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the assessment description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the questions in this assessment.
    /// </summary>
    public IReadOnlyList<AssessmentQuestion> Questions { get; init; } = Array.Empty<AssessmentQuestion>();

    /// <summary>
    /// Gets the total points possible.
    /// </summary>
    public int TotalPoints { get; init; }

    /// <summary>
    /// Gets the grade level.
    /// </summary>
    public GradeLevel GradeLevel { get; init; }

    /// <summary>
    /// Gets the timestamp when the assessment was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Interface for generating educational assessments.
/// </summary>
public interface IAssessmentGenerator
{
    /// <summary>
    /// Generates an assessment based on the specification.
    /// </summary>
    /// <param name="concept">The concept identifier.</param>
    /// <param name="spec">The assessment specification.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A generated assessment.</returns>
    Task<Assessment> GenerateAsync(
        ConceptId concept,
        AssessmentSpecification spec,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a single question.
    /// </summary>
    /// <param name="concept">The concept identifier.</param>
    /// <param name="type">The question type.</param>
    /// <param name="difficulty">The difficulty level.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A generated question.</returns>
    Task<AssessmentQuestion> GenerateQuestionAsync(
        ConceptId concept,
        QuestionType type,
        DifficultyLevel difficulty,
        CancellationToken cancellationToken = default);
}

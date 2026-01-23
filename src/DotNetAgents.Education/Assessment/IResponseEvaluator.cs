using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Assessment;

/// <summary>
/// Represents the result of evaluating a student's response.
/// </summary>
public record EvaluationResult
{
    /// <summary>
    /// Gets the score (0-100).
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Gets whether the answer is correct.
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// Gets the feedback for the student.
    /// </summary>
    public string Feedback { get; init; } = string.Empty;

    /// <summary>
    /// Gets the points awarded.
    /// </summary>
    public double PointsAwarded { get; init; }

    /// <summary>
    /// Gets the points possible.
    /// </summary>
    public int PointsPossible { get; init; }

    /// <summary>
    /// Gets identified misconceptions.
    /// </summary>
    public IReadOnlyList<string> Misconceptions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets suggestions for improvement.
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Represents a detected misconception.
/// </summary>
public record Misconception
{
    /// <summary>
    /// Gets the misconception identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the misconception description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the concept this misconception relates to.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the confidence level (0-1) of the detection.
    /// </summary>
    public double Confidence { get; init; }
}

/// <summary>
/// Interface for evaluating student responses to assessment questions.
/// </summary>
public interface IResponseEvaluator
{
    /// <summary>
    /// Evaluates a student's response to an assessment question.
    /// </summary>
    /// <param name="studentResponse">The student's response text.</param>
    /// <param name="question">The assessment question.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An evaluation result with scoring and feedback.</returns>
    Task<EvaluationResult> EvaluateAsync(
        string studentResponse,
        AssessmentQuestion question,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects misconceptions in a student's response.
    /// </summary>
    /// <param name="studentResponse">The student's response text.</param>
    /// <param name="concept">The concept being assessed.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of detected misconceptions.</returns>
    Task<IReadOnlyList<Misconception>> DetectMisconceptionsAsync(
        string studentResponse,
        ConceptId concept,
        CancellationToken cancellationToken = default);
}

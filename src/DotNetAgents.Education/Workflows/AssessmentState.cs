using DotNetAgents.Education.Assessment;
using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Workflows;

/// <summary>
/// Represents the state of an adaptive assessment session.
/// </summary>
public class AssessmentState
{
    /// <summary>
    /// Gets or sets the student identifier.
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assessment being taken.
    /// </summary>
    public DotNetAgents.Education.Assessment.Assessment? CurrentAssessment { get; set; }

    /// <summary>
    /// Gets or sets the current question index.
    /// </summary>
    public int CurrentQuestionIndex { get; set; }

    /// <summary>
    /// Gets or sets the current question being answered.
    /// </summary>
    public AssessmentQuestion? CurrentQuestion { get; set; }

    /// <summary>
    /// Gets or sets the student's responses.
    /// </summary>
    public Dictionary<string, string> Responses { get; set; } = new();

    /// <summary>
    /// Gets or sets the evaluation results for each question.
    /// </summary>
    public Dictionary<string, EvaluationResult> EvaluationResults { get; set; } = new();

    /// <summary>
    /// Gets or sets the current difficulty level.
    /// </summary>
    public DifficultyLevel CurrentDifficulty { get; set; } = DifficultyLevel.Medium;

    /// <summary>
    /// Gets or sets the total score so far.
    /// </summary>
    public double TotalScore { get; set; }

    /// <summary>
    /// Gets or sets the total points possible.
    /// </summary>
    public int TotalPointsPossible { get; set; }

    /// <summary>
    /// Gets or sets whether the assessment is complete.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets or sets whether mastery has been achieved.
    /// </summary>
    public bool MasteryAchieved { get; set; }

    /// <summary>
    /// Gets or sets whether early termination was triggered.
    /// </summary>
    public bool EarlyTerminated { get; set; }

    /// <summary>
    /// Gets or sets the termination reason.
    /// </summary>
    public string? TerminationReason { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive correct answers.
    /// </summary>
    public int ConsecutiveCorrect { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive incorrect answers.
    /// </summary>
    public int ConsecutiveIncorrect { get; set; }

    /// <summary>
    /// Gets or sets the assessment start time.
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the assessment end time.
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Gets or sets metadata about the assessment session.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

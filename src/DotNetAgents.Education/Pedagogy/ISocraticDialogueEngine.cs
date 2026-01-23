using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Pedagogy;

/// <summary>
/// Interface for generating Socratic dialogue questions to guide student learning.
/// </summary>
public interface ISocraticDialogueEngine
{
    /// <summary>
    /// Generates a Socratic question based on the concept context and student's current understanding level.
    /// </summary>
    /// <param name="concept">The concept context.</param>
    /// <param name="currentLevel">The student's current understanding level.</param>
    /// <param name="language">Optional language code for localization (e.g., "en", "es").</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A Socratic question.</returns>
    Task<SocraticQuestion> GenerateQuestionAsync(
        ConceptContext concept,
        StudentUnderstanding currentLevel,
        string? language = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a student's response to a Socratic question and assesses their understanding.
    /// </summary>
    /// <param name="studentResponse">The student's response text.</param>
    /// <param name="question">The original Socratic question.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An assessment of the student's understanding.</returns>
    Task<UnderstandingAssessment> EvaluateResponseAsync(
        string studentResponse,
        SocraticQuestion question,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a scaffolded hint at the specified level to help guide the student.
    /// </summary>
    /// <param name="question">The Socratic question.</param>
    /// <param name="hintLevel">The hint level (1-5, where 1 is most general and 5 is most specific).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A scaffolded hint.</returns>
    Task<ScaffoldedHint> GenerateHintAsync(
        SocraticQuestion question,
        int hintLevel,
        CancellationToken cancellationToken = default);
}

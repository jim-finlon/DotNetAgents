using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Safety;

/// <summary>
/// Represents a complexity score for text content.
/// </summary>
public record ComplexityScore
{
    /// <summary>
    /// Gets the reading level (grade level equivalent).
    /// </summary>
    public int ReadingLevel { get; init; }

    /// <summary>
    /// Gets the Flesch-Kincaid grade level score.
    /// </summary>
    public double FleschKincaid { get; init; }

    /// <summary>
    /// Gets the Flesch Reading Ease score (0-100, higher is easier).
    /// </summary>
    public double FleschReadingEase { get; init; }

    /// <summary>
    /// Gets the average sentence length.
    /// </summary>
    public double AverageSentenceLength { get; init; }

    /// <summary>
    /// Gets the average syllables per word.
    /// </summary>
    public double AverageSyllablesPerWord { get; init; }
}

/// <summary>
/// Interface for transforming content to be age-appropriate for different grade levels.
/// </summary>
public interface IAgeAdaptiveTransformer
{
    /// <summary>
    /// Transforms a prompt to be appropriate for the specified grade level.
    /// </summary>
    /// <param name="prompt">The original prompt.</param>
    /// <param name="gradeLevel">The target grade level.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The transformed prompt.</returns>
    Task<string> TransformPromptAsync(
        string prompt,
        GradeLevel gradeLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transforms a response to be appropriate for the specified grade level.
    /// </summary>
    /// <param name="response">The original response.</param>
    /// <param name="gradeLevel">The target grade level.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The transformed response.</returns>
    Task<string> TransformResponseAsync(
        string response,
        GradeLevel gradeLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assesses the complexity of text content.
    /// </summary>
    /// <param name="text">The text to assess.</param>
    /// <returns>A complexity score.</returns>
    ComplexityScore AssessComplexity(string text);
}

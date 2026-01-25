using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Safety;

/// <summary>
/// Represents the context for content filtering.
/// </summary>
public record FilterContext
{
    /// <summary>
    /// Gets the student's grade level.
    /// </summary>
    public GradeLevel GradeLevel { get; init; }

    /// <summary>
    /// Gets the student identifier.
    /// </summary>
    public string? StudentId { get; init; }

    /// <summary>
    /// Gets additional context metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Represents a content category that can be flagged.
/// </summary>
public enum ContentCategory
{
    /// <summary>
    /// Violence-related content.
    /// </summary>
    Violence,

    /// <summary>
    /// Adult content.
    /// </summary>
    Adult,

    /// <summary>
    /// Hate speech.
    /// </summary>
    HateSpeech,

    /// <summary>
    /// Self-harm references.
    /// </summary>
    SelfHarm,

    /// <summary>
    /// Personal information requests.
    /// </summary>
    PersonalInformation,

    /// <summary>
    /// Bullying content.
    /// </summary>
    Bullying
}

/// <summary>
/// Represents the result of content filtering.
/// </summary>
public record ContentFilterResult
{
    /// <summary>
    /// Gets whether the content is allowed.
    /// </summary>
    public bool IsAllowed { get; init; }

    /// <summary>
    /// Gets the filtered content (if modifications were made).
    /// </summary>
    public string? FilteredContent { get; init; }

    /// <summary>
    /// Gets the categories that were flagged.
    /// </summary>
    public IReadOnlyList<ContentCategory> FlaggedCategories { get; init; } = Array.Empty<ContentCategory>();

    /// <summary>
    /// Gets whether the content requires human review.
    /// </summary>
    public bool RequiresReview { get; init; }

    /// <summary>
    /// Gets the reason for review (if applicable).
    /// </summary>
    public string? ReviewReason { get; init; }
}

/// <summary>
/// Interface for filtering content to ensure child safety and COPPA compliance.
/// </summary>
public interface IContentFilter
{
    /// <summary>
    /// Filters input content before processing.
    /// </summary>
    /// <param name="input">The input content to filter.</param>
    /// <param name="context">The filtering context.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A filter result indicating whether the content is allowed and any modifications.</returns>
    Task<ContentFilterResult> FilterInputAsync(
        string input,
        FilterContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters output content before delivery to the student.
    /// </summary>
    /// <param name="output">The output content to filter.</param>
    /// <param name="context">The filtering context.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A filter result indicating whether the content is allowed and any modifications.</returns>
    Task<ContentFilterResult> FilterOutputAsync(
        string output,
        FilterContext context,
        CancellationToken cancellationToken = default);
}

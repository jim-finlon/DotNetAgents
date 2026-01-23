using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Pedagogy;

/// <summary>
/// Represents a performance rating for spaced repetition (SuperMemo 2 algorithm).
/// </summary>
public enum PerformanceRating
{
    /// <summary>
    /// Complete blackout - no recall (0).
    /// </summary>
    CompleteBlackout = 0,

    /// <summary>
    /// Incorrect but remembered (1).
    /// </summary>
    IncorrectButRemembered = 1,

    /// <summary>
    /// Incorrect but easy to recall (2).
    /// </summary>
    IncorrectButEasy = 2,

    /// <summary>
    /// Correct with difficulty (3).
    /// </summary>
    CorrectWithDifficulty = 3,

    /// <summary>
    /// Correct with hesitation (4).
    /// </summary>
    CorrectWithHesitation = 4,

    /// <summary>
    /// Perfect recall (5).
    /// </summary>
    Perfect = 5
}

/// <summary>
/// Represents a review item for spaced repetition.
/// </summary>
public record ReviewItem
{
    /// <summary>
    /// Gets the unique identifier for the review item.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the concept this item relates to.
    /// </summary>
    public ConceptId ConceptId { get; init; } = null!;

    /// <summary>
    /// Gets the current ease factor (default 2.5 for SM2).
    /// </summary>
    public float EaseFactor { get; init; } = 2.5f;

    /// <summary>
    /// Gets the current interval in days.
    /// </summary>
    public int Interval { get; init; }

    /// <summary>
    /// Gets the date of the last review.
    /// </summary>
    public DateTimeOffset LastReviewDate { get; init; }

    /// <summary>
    /// Gets the next review date.
    /// </summary>
    public DateTimeOffset NextReviewDate { get; init; }

    /// <summary>
    /// Gets the number of repetitions.
    /// </summary>
    public int Repetitions { get; init; }
}

/// <summary>
/// Represents a review schedule calculated by the spaced repetition scheduler.
/// </summary>
public record ReviewSchedule
{
    /// <summary>
    /// Gets the next review date.
    /// </summary>
    public DateTimeOffset NextReviewDate { get; init; }

    /// <summary>
    /// Gets the interval in days until the next review.
    /// </summary>
    public int Interval { get; init; }

    /// <summary>
    /// Gets the updated ease factor.
    /// </summary>
    public float EaseFactor { get; init; }
}

/// <summary>
/// Interface for spaced repetition schedulers that calculate optimal review intervals.
/// </summary>
public interface ISpacedRepetitionScheduler
{
    /// <summary>
    /// Calculates the next review schedule based on the item's performance rating.
    /// </summary>
    /// <param name="item">The review item.</param>
    /// <param name="rating">The performance rating (0-5).</param>
    /// <returns>A review schedule with the next review date and updated parameters.</returns>
    ReviewSchedule CalculateNextReview(ReviewItem item, PerformanceRating rating);

    /// <summary>
    /// Gets all items that are due for review at the specified time.
    /// </summary>
    /// <param name="items">The collection of review items.</param>
    /// <param name="asOf">The date/time to check against (defaults to now).</param>
    /// <returns>A list of items that are due for review.</returns>
    IReadOnlyList<ReviewItem> GetDueItems(IEnumerable<ReviewItem> items, DateTimeOffset? asOf = null);

    /// <summary>
    /// Calculates the retention probability for an item at the specified time.
    /// </summary>
    /// <param name="item">The review item.</param>
    /// <param name="asOf">The date/time to calculate retention for (defaults to now).</param>
    /// <returns>A retention probability between 0 and 1.</returns>
    float CalculateRetention(ReviewItem item, DateTimeOffset? asOf = null);
}

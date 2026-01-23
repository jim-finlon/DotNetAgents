using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Pedagogy;

/// <summary>
/// Implementation of the SuperMemo 2 (SM2) spaced repetition algorithm.
/// </summary>
public class SM2Scheduler : ISpacedRepetitionScheduler
{
    private readonly ILogger<SM2Scheduler>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SM2Scheduler"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public SM2Scheduler(ILogger<SM2Scheduler>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public ReviewSchedule CalculateNextReview(ReviewItem item, PerformanceRating rating)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var easeFactor = item.EaseFactor;
        var interval = item.Interval;
        var repetitions = item.Repetitions;

        _logger?.LogDebug(
            "Calculating next review for item {ItemId}, rating {Rating}, current interval {Interval}, ease factor {EaseFactor}",
            item.Id,
            rating,
            interval,
            easeFactor);

        // SM2 Algorithm
        if ((int)rating >= 3) // Correct response (rating 3, 4, or 5)
        {
            if (interval == 0)
            {
                // First review
                interval = 1;
            }
            else if (interval == 1)
            {
                // Second review
                interval = 6;
            }
            else
            {
                // Subsequent reviews: interval = interval * easeFactor
                interval = (int)(interval * easeFactor);
            }

            // Update ease factor
            // EF' = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02))
            var q = (int)rating;
            var efDelta = 0.1f - (5 - q) * (0.08f + (5 - q) * 0.02f);
            easeFactor = easeFactor + efDelta;

            // Minimum ease factor is 1.3
            easeFactor = Math.Max(1.3f, easeFactor);

            repetitions++;
        }
        else // Incorrect response (rating 0, 1, or 2)
        {
            // Reset interval to 1 day
            interval = 1;
            repetitions = 0;

            // Decrease ease factor
            easeFactor = Math.Max(1.3f, easeFactor - 0.2f);
        }

        var nextReviewDate = DateTimeOffset.UtcNow.AddDays(interval);

        _logger?.LogInformation(
            "Calculated review schedule: next review in {Interval} days, ease factor {EaseFactor}, repetitions {Repetitions}",
            interval,
            easeFactor,
            repetitions);

        return new ReviewSchedule
        {
            NextReviewDate = nextReviewDate,
            Interval = interval,
            EaseFactor = easeFactor
        };
    }

    /// <inheritdoc/>
    public IReadOnlyList<ReviewItem> GetDueItems(IEnumerable<ReviewItem> items, DateTimeOffset? asOf = null)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var checkDate = asOf ?? DateTimeOffset.UtcNow;

        var dueItems = items
            .Where(item => item.NextReviewDate <= checkDate)
            .OrderBy(item => item.NextReviewDate)
            .ToList();

        _logger?.LogDebug(
            "Found {Count} due items out of {Total} items",
            dueItems.Count,
            items.Count());

        return dueItems;
    }

    /// <inheritdoc/>
    public float CalculateRetention(ReviewItem item, DateTimeOffset? asOf = null)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var checkDate = asOf ?? DateTimeOffset.UtcNow;

        // If item hasn't been reviewed yet, retention is 0
        if (item.Repetitions == 0)
            return 0f;

        // Calculate days since last review
        var daysSinceReview = (checkDate - item.LastReviewDate).TotalDays;

        // If past the next review date, retention decreases
        if (checkDate > item.NextReviewDate)
        {
            var daysOverdue = (checkDate - item.NextReviewDate).TotalDays;
            // Exponential decay model: retention = e^(-decay_rate * days_overdue)
            var decayRate = 0.1f; // Adjustable parameter
            var retention = Math.Exp(-decayRate * daysOverdue);
            return Math.Max(0f, (float)retention);
        }

        // If before next review date, retention is high
        var daysUntilReview = (item.NextReviewDate - checkDate).TotalDays;
        var intervalDays = (item.NextReviewDate - item.LastReviewDate).TotalDays;

        if (intervalDays <= 0)
            return 1f;

        // Retention decreases as we approach the review date
        // Simple linear model: retention = 1 - (days_until_review / interval)
        var retentionValue = 1f - (float)(daysUntilReview / intervalDays);
        return Math.Max(0f, Math.Min(1f, retentionValue));
    }
}

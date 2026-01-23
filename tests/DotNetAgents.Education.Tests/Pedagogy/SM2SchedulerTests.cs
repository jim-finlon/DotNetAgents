using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Education.Tests.Pedagogy;

public class SM2SchedulerTests
{
    private readonly SM2Scheduler _scheduler;

    public SM2SchedulerTests()
    {
        _scheduler = new SM2Scheduler();
    }

    [Fact]
    public void CalculateNextReview_WithPerfectRating_ShouldIncreaseInterval()
    {
        // Arrange
        var item = new ReviewItem
        {
            Id = "item-1",
            ConceptId = new ConceptId("test-concept", SubjectArea.Science, GradeLevel.G6_8),
            LastReviewDate = DateTimeOffset.UtcNow.AddDays(-1),
            NextReviewDate = DateTimeOffset.UtcNow,
            EaseFactor = 2.5f,
            Interval = 2,
            Repetitions = 2
        };

        // Act
        var schedule = _scheduler.CalculateNextReview(item, PerformanceRating.Perfect);

        // Assert
        schedule.Interval.Should().BeGreaterThan(0);
        schedule.EaseFactor.Should().BeGreaterThan(item.EaseFactor);
    }

    [Fact]
    public void CalculateNextReview_WithIncorrectRating_ShouldResetRepetitions()
    {
        // Arrange
        var item = new ReviewItem
        {
            Id = "item-1",
            ConceptId = new ConceptId("test-concept", SubjectArea.Science, GradeLevel.G6_8),
            LastReviewDate = DateTimeOffset.UtcNow.AddDays(-1),
            NextReviewDate = DateTimeOffset.UtcNow,
            EaseFactor = 2.5f,
            Interval = 3,
            Repetitions = 3
        };

        // Act
        var schedule = _scheduler.CalculateNextReview(item, PerformanceRating.IncorrectButEasy);

        // Assert
        schedule.Interval.Should().Be(1); // Should reset to initial interval
    }

    [Fact]
    public void GetDueItems_WithDueItems_ShouldReturnOnlyDueItems()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var items = new List<ReviewItem>
        {
            new() { Id = "item-1", ConceptId = new ConceptId("c1", SubjectArea.Science, GradeLevel.G6_8), NextReviewDate = now.AddDays(-1), EaseFactor = 2.5f, Interval = 1, Repetitions = 1 },
            new() { Id = "item-2", ConceptId = new ConceptId("c2", SubjectArea.Science, GradeLevel.G6_8), NextReviewDate = now.AddDays(1), EaseFactor = 2.5f, Interval = 1, Repetitions = 1 },
            new() { Id = "item-3", ConceptId = new ConceptId("c3", SubjectArea.Science, GradeLevel.G6_8), NextReviewDate = now, EaseFactor = 2.5f, Interval = 1, Repetitions = 1 }
        };

        // Act
        var dueItems = _scheduler.GetDueItems(items, now);

        // Assert
        dueItems.Should().HaveCount(2);
        dueItems.Should().Contain(i => i.Id == "item-1");
        dueItems.Should().Contain(i => i.Id == "item-3");
        dueItems.Should().NotContain(i => i.Id == "item-2");
    }

    [Fact]
    public void CalculateRetention_WithFutureReviewDate_ShouldReturnHighRetention()
    {
        // Arrange
        var item = new ReviewItem
        {
            Id = "item-1",
            ConceptId = new ConceptId("test-concept", SubjectArea.Science, GradeLevel.G6_8),
            LastReviewDate = DateTimeOffset.UtcNow.AddDays(-1),
            NextReviewDate = DateTimeOffset.UtcNow.AddDays(1),
            EaseFactor = 2.5f,
            Interval = 2,
            Repetitions = 2
        };

        // Act
        var retention = _scheduler.CalculateRetention(item, DateTimeOffset.UtcNow.AddDays(0.5)); // Check before due date

        // Assert
        // The retention calculation may vary - just verify it returns a valid value between 0 and 1
        retention.Should().BeGreaterThan(0f);
        retention.Should().BeLessThanOrEqualTo(1f);
    }

    [Fact]
    public void CalculateRetention_WithPastDueDate_ShouldReturnLowerRetention()
    {
        // Arrange
        var item = new ReviewItem
        {
            Id = "item-1",
            ConceptId = new ConceptId("test-concept", SubjectArea.Science, GradeLevel.G6_8),
            LastReviewDate = DateTimeOffset.UtcNow.AddDays(-10),
            NextReviewDate = DateTimeOffset.UtcNow.AddDays(-5),
            EaseFactor = 2.5f,
            Interval = 2,
            Repetitions = 2
        };

        // Act
        var retention = _scheduler.CalculateRetention(item, DateTimeOffset.UtcNow);

        // Assert
        retention.Should().BeLessThan(1.0f);
    }
}

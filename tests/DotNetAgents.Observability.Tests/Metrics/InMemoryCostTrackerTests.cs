using DotNetAgents.Observability.Metrics;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Observability.Tests.Metrics;

public class InMemoryCostTrackerTests
{
    [Fact]
    public async Task RecordLLMCallAsync_WithValidData_RecordsSuccessfully()
    {
        // Arrange
        var tracker = new InMemoryCostTracker();

        // Act
        await tracker.RecordLLMCallAsync("gpt-4", 1000, 500, "correlation-1");

        // Assert
        var summary = await tracker.GetCostSummaryAsync(TimeSpan.FromHours(1));
        summary.TotalCalls.Should().Be(1);
        summary.TotalInputTokens.Should().Be(1000);
        summary.TotalOutputTokens.Should().Be(500);
    }

    [Fact]
    public async Task GetCostSummaryAsync_WithMultipleCalls_CalculatesCorrectly()
    {
        // Arrange
        var tracker = new InMemoryCostTracker();
        await tracker.RecordLLMCallAsync("gpt-4", 1000, 500);
        await tracker.RecordLLMCallAsync("gpt-3.5-turbo", 2000, 1000);

        // Act
        var summary = await tracker.GetCostSummaryAsync(TimeSpan.FromHours(1));

        // Assert
        summary.TotalCalls.Should().Be(2);
        summary.TotalInputTokens.Should().Be(3000);
        summary.TotalOutputTokens.Should().Be(1500);
        summary.CostByModel.Should().ContainKey("gpt-4");
        summary.CostByModel.Should().ContainKey("gpt-3.5-turbo");
    }

    [Fact]
    public async Task GetCostSummaryAsync_WithTimeFilter_FiltersCorrectly()
    {
        // Arrange
        var tracker = new InMemoryCostTracker();
        await tracker.RecordLLMCallAsync("gpt-4", 1000, 500);

        // Wait a bit to ensure different timestamps
        await Task.Delay(100);

        // Act
        var summary = await tracker.GetCostSummaryAsync(TimeSpan.FromMilliseconds(50));

        // Assert
        // Should not include the call made before the period
        summary.TotalCalls.Should().Be(0);
    }

    [Fact]
    public void EstimateCost_WithValidModel_ReturnsEstimate()
    {
        // Arrange
        var tracker = new InMemoryCostTracker();

        // Act
        var cost = tracker.EstimateCost("gpt-4", 1000, 500);

        // Assert
        cost.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCostByModelAsync_WithMultipleModels_ReturnsBreakdown()
    {
        // Arrange
        var tracker = new InMemoryCostTracker();
        await tracker.RecordLLMCallAsync("gpt-4", 1000, 500);
        await tracker.RecordLLMCallAsync("gpt-3.5-turbo", 2000, 1000);

        // Act
        var breakdown = await tracker.GetCostByModelAsync(TimeSpan.FromHours(1));

        // Assert
        breakdown.Should().ContainKey("gpt-4");
        breakdown.Should().ContainKey("gpt-3.5-turbo");
        breakdown["gpt-4"].Should().BeGreaterThan(0);
        breakdown["gpt-3.5-turbo"].Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RecordLLMCallAsync_WithInvalidInput_ThrowsException()
    {
        // Arrange
        var tracker = new InMemoryCostTracker();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => tracker.RecordLLMCallAsync(null!, 1000, 500));
        await Assert.ThrowsAsync<ArgumentException>(() => tracker.RecordLLMCallAsync("gpt-4", -1, 500));
        await Assert.ThrowsAsync<ArgumentException>(() => tracker.RecordLLMCallAsync("gpt-4", 1000, -1));
    }
}
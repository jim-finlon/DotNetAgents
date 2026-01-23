using DotNetAgents.Security.RateLimiting;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Security.Tests.RateLimiting;

public class InMemoryRateLimiterTests
{
    [Fact]
    public async Task TryAcquireAsync_WithinLimit_ReturnsTrue()
    {
        // Arrange
        var limiter = new InMemoryRateLimiter();

        // Act
        var result = await limiter.TryAcquireAsync("test-key", 5, TimeSpan.FromMinutes(1));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryAcquireAsync_ExceedsLimit_ReturnsFalse()
    {
        // Arrange
        var limiter = new InMemoryRateLimiter();

        // Act
        for (int i = 0; i < 5; i++)
        {
            await limiter.TryAcquireAsync("test-key", 5, TimeSpan.FromMinutes(1));
        }

        var result = await limiter.TryAcquireAsync("test-key", 5, TimeSpan.FromMinutes(1));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetRemainingAsync_WithNoRequests_ReturnsLimit()
    {
        // Arrange
        var limiter = new InMemoryRateLimiter();

        // Act
        var remaining = await limiter.GetRemainingAsync("test-key", 10, TimeSpan.FromMinutes(1));

        // Assert
        remaining.Should().Be(10);
    }

    [Fact]
    public async Task GetRemainingAsync_WithSomeRequests_ReturnsCorrectRemaining()
    {
        // Arrange
        var limiter = new InMemoryRateLimiter();

        // Act
        await limiter.TryAcquireAsync("test-key", 10, TimeSpan.FromMinutes(1));
        await limiter.TryAcquireAsync("test-key", 10, TimeSpan.FromMinutes(1));
        var remaining = await limiter.GetRemainingAsync("test-key", 10, TimeSpan.FromMinutes(1));

        // Assert
        remaining.Should().Be(8);
    }

    [Fact]
    public async Task ResetAsync_ClearsRateLimit()
    {
        // Arrange
        var limiter = new InMemoryRateLimiter();
        await limiter.TryAcquireAsync("test-key", 5, TimeSpan.FromMinutes(1));

        // Act
        await limiter.ResetAsync("test-key");
        var remaining = await limiter.GetRemainingAsync("test-key", 5, TimeSpan.FromMinutes(1));

        // Assert
        remaining.Should().Be(5);
    }

    [Fact]
    public async Task TryAcquireAsync_WithExpiredWindow_AllowsNewRequests()
    {
        // Arrange
        var limiter = new InMemoryRateLimiter();
        var window = TimeSpan.FromMilliseconds(100);

        // Act - Fill up the limit
        for (int i = 0; i < 3; i++)
        {
            await limiter.TryAcquireAsync("test-key", 3, window);
        }

        // Wait for window to expire
        await Task.Delay(150);

        // Should be able to acquire again
        var result = await limiter.TryAcquireAsync("test-key", 3, window);

        // Assert
        result.Should().BeTrue();
    }
}
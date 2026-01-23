using DotNetAgents.Education.Models;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Education.Tests.Models;

public class MasteryLevelTests
{
    [Theory]
    [InlineData(0, MasteryLevel.Novice)]
    [InlineData(30, MasteryLevel.Novice)]
    [InlineData(40, MasteryLevel.Developing)]
    [InlineData(50, MasteryLevel.Developing)]
    [InlineData(60, MasteryLevel.Proficient)]
    [InlineData(70, MasteryLevel.Proficient)]
    [InlineData(80, MasteryLevel.Advanced)]
    [InlineData(90, MasteryLevel.Advanced)]
    [InlineData(95, MasteryLevel.Mastery)]
    [InlineData(100, MasteryLevel.Mastery)]
    public void FromScore_WithVariousScores_ShouldReturnCorrectLevel(double score, MasteryLevel expectedLevel)
    {
        // Act
        var level = MasteryLevelExtensions.FromScore(score);

        // Assert
        level.Should().Be(expectedLevel);
    }

    [Theory]
    [InlineData(MasteryLevel.Novice, 0, 40)]
    [InlineData(MasteryLevel.Developing, 40, 60)]
    [InlineData(MasteryLevel.Proficient, 60, 80)]
    [InlineData(MasteryLevel.Advanced, 80, 95)]
    [InlineData(MasteryLevel.Mastery, 95, 100)]
    public void ToScoreRange_WithLevel_ShouldReturnCorrectRange(MasteryLevel level, double expectedMin, double expectedMax)
    {
        // Act
        var range = level.ToScoreRange();

        // Assert
        range.Min.Should().Be(expectedMin);
        range.Max.Should().Be(expectedMax);
    }
}

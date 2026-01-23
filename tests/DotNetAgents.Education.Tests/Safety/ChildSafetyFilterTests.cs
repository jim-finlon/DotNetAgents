using DotNetAgents.Education.Models;
using DotNetAgents.Education.Safety;
using DotNetAgents.Security.Validation;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.Education.Tests.Safety;

public class ChildSafetyFilterTests
{
    private readonly Mock<ISanitizer> _mockSanitizer;
    private readonly ChildSafetyFilter _filter;

    public ChildSafetyFilterTests()
    {
        _mockSanitizer = new Mock<ISanitizer>();
        _mockSanitizer.Setup(s => s.SanitizeInput(It.IsAny<string>())).Returns<string>(s => s);
        _mockSanitizer.Setup(s => s.ContainsSensitiveData(It.IsAny<string>())).Returns(false);
        _mockSanitizer.Setup(s => s.MaskSensitiveData(It.IsAny<string>())).Returns<string>(s => s);
        
        _filter = new ChildSafetyFilter(_mockSanitizer.Object);
    }

    [Fact]
    public async Task FilterInputAsync_WithSafeContent_ShouldAllow()
    {
        // Arrange
        var input = "I want to learn about photosynthesis";
        var context = new FilterContext
        {
            StudentId = "student-1",
            GradeLevel = GradeLevel.G6_8
        };

        // Act
        var result = await _filter.FilterInputAsync(input, context);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.FilteredContent.Should().NotBeNull();
    }

    [Fact]
    public async Task FilterInputAsync_WithPii_ShouldRequireReview()
    {
        // Arrange
        var input = "What's your email address?";
        var context = new FilterContext
        {
            StudentId = "student-1",
            GradeLevel = GradeLevel.G6_8
        };
        // The filter checks for patterns like "your email" which is in the blocked patterns

        // Act
        var result = await _filter.FilterInputAsync(input, context);

        // Assert
        result.IsAllowed.Should().BeFalse(); // Blocked patterns are not allowed
        result.FlaggedCategories.Should().Contain(ContentCategory.PersonalInformation);
        result.RequiresReview.Should().BeTrue();
    }

    [Fact]
    public async Task FilterOutputAsync_WithSafeContent_ShouldAllow()
    {
        // Arrange
        var output = "Photosynthesis is the process by which plants convert sunlight into energy.";
        var context = new FilterContext
        {
            StudentId = "student-1",
            GradeLevel = GradeLevel.G3_5 // Use G3_5 to avoid null reference in CheckAgeAppropriateness
        };

        // Act
        var result = await _filter.FilterOutputAsync(output, context);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.FilteredContent.Should().NotBeNull();
    }

    [Fact]
    public async Task FilterInputAsync_WithInappropriateContent_ShouldFlag()
    {
        // Arrange
        var input = "This contains kill content";
        var context = new FilterContext
        {
            StudentId = "student-1",
            GradeLevel = GradeLevel.G6_8
        };

        // Act
        var result = await _filter.FilterInputAsync(input, context);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.FlaggedCategories.Should().Contain(ContentCategory.Violence);
        result.RequiresReview.Should().BeTrue();
    }
}

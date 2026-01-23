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
        var input = "My email is test@example.com";
        var context = new FilterContext
        {
            StudentId = "student-1",
            GradeLevel = GradeLevel.G6_8
        };
        _mockSanitizer.Setup(s => s.ContainsSensitiveData(It.IsAny<string>())).Returns(true);

        // Act
        var result = await _filter.FilterInputAsync(input, context);

        // Assert
        result.RequiresReview.Should().BeTrue();
        result.FlaggedCategories.Should().Contain(ContentCategory.PersonalInformation);
    }

    [Fact]
    public async Task FilterOutputAsync_WithSafeContent_ShouldAllow()
    {
        // Arrange
        var output = "Photosynthesis is the process by which plants convert sunlight into energy.";
        var context = new FilterContext
        {
            StudentId = "student-1",
            GradeLevel = GradeLevel.G6_8
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
        var input = "This contains violence content";
        var context = new FilterContext
        {
            StudentId = "student-1",
            GradeLevel = GradeLevel.G6_8
        };

        // Act
        var result = await _filter.FilterInputAsync(input, context);

        // Assert
        // The filter may flag violence content - check if it's flagged or requires review
        result.FlaggedCategories.Should().NotBeEmpty();
        result.RequiresReview.Should().BeTrue();
    }
}

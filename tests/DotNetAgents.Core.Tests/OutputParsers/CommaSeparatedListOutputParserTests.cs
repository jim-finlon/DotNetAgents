using DotNetAgents.Core.OutputParsers;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.OutputParsers;

public class CommaSeparatedListOutputParserTests
{
    [Fact]
    public async Task ParseAsync_WithValidCommaSeparatedList_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommaSeparatedListOutputParser();
        var input = "apple, banana, cherry";

        // Act
        var result = await parser.ParseAsync(input);

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("apple");
        result[1].Should().Be("banana");
        result[2].Should().Be("cherry");
    }

    [Fact]
    public async Task ParseAsync_WithExtraSpaces_TrimsWhitespace()
    {
        // Arrange
        var parser = new CommaSeparatedListOutputParser();
        var input = "  apple  ,  banana  ,  cherry  ";

        // Act
        var result = await parser.ParseAsync(input);

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("apple");
        result[1].Should().Be("banana");
        result[2].Should().Be("cherry");
    }

    [Fact]
    public async Task ParseAsync_WithEmptyString_ReturnsEmptyList()
    {
        // Arrange
        var parser = new CommaSeparatedListOutputParser();

        // Act
        var result = await parser.ParseAsync(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseAsync_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var parser = new CommaSeparatedListOutputParser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => parser.ParseAsync(null!));
    }

    [Fact]
    public void GetFormatInstructions_ReturnsCommaSeparatedInstructions()
    {
        // Arrange
        var parser = new CommaSeparatedListOutputParser();

        // Act
        var instructions = parser.GetFormatInstructions();

        // Assert
        instructions.Should().Contain("comma-separated");
    }
}
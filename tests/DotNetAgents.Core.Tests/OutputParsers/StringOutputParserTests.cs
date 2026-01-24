using DotNetAgents.Abstractions.OutputParsers;
using DotNetAgents.Core.OutputParsers;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.OutputParsers;

public class StringOutputParserTests
{
    [Fact]
    public async Task ParseAsync_WithValidString_ReturnsString()
    {
        // Arrange
        var parser = new StringOutputParser();
        const string input = "Hello, World!";

        // Act
        var result = await parser.ParseAsync(input);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public async Task ParseAsync_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var parser = new StringOutputParser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => parser.ParseAsync(null!));
    }

    [Fact]
    public void GetFormatInstructions_ReturnsEmptyString()
    {
        // Arrange
        var parser = new StringOutputParser();

        // Act
        var instructions = parser.GetFormatInstructions();

        // Assert
        instructions.Should().BeEmpty();
    }
}
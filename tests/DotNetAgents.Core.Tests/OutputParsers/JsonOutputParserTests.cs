using DotNetAgents.Core.OutputParsers;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.OutputParsers;

public class JsonOutputParserTests
{
    [Fact]
    public async Task ParseAsync_WithValidJson_DeserializesCorrectly()
    {
        // Arrange
        var parser = new JsonOutputParser<TestData>();
        var json = "{\"name\": \"Test\", \"value\": 42}";

        // Act
        var result = await parser.ParseAsync(json);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ParseAsync_WithJsonInMarkdownCodeBlock_ExtractsJson()
    {
        // Arrange
        var parser = new JsonOutputParser<TestData>();
        var json = "```json\n{\"name\": \"Test\", \"value\": 42}\n```";

        // Act
        var result = await parser.ParseAsync(json);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ParseAsync_WithInvalidJson_ThrowsParsingException()
    {
        // Arrange
        var parser = new JsonOutputParser<TestData>();
        var invalidJson = "{invalid json}";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ParsingException>(() => parser.ParseAsync(invalidJson));
        exception.RawOutput.Should().Be(invalidJson);
    }

    [Fact]
    public async Task ParseAsync_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var parser = new JsonOutputParser<TestData>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => parser.ParseAsync(null!));
    }

    [Fact]
    public void GetFormatInstructions_ReturnsJsonInstructions()
    {
        // Arrange
        var parser = new JsonOutputParser<TestData>();

        // Act
        var instructions = parser.GetFormatInstructions();

        // Assert
        instructions.Should().Contain("JSON");
    }

    private record TestData
    {
        public string Name { get; init; } = string.Empty;
        public int Value { get; init; }
    }
}
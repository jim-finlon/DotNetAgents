using DotNetAgents.Abstractions.Prompts;
using DotNetAgents.Core.Prompts;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Prompts;

public class PromptTemplateTests
{
    [Fact]
    public void Constructor_WithValidTemplate_InitializesCorrectly()
    {
        // Arrange
        const string template = "Hello {name}!";

        // Act
        var promptTemplate = new PromptTemplate(template);

        // Assert
        promptTemplate.Template.Should().Be(template);
        promptTemplate.Variables.Should().Contain("name");
        promptTemplate.Variables.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithNullTemplate_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PromptTemplate(null!));
    }

    [Fact]
    public void Variables_WithMultipleVariables_ExtractsAll()
    {
        // Arrange
        const string template = "Hello {name}, you are {age} years old and live in {city}.";

        // Act
        var promptTemplate = new PromptTemplate(template);

        // Assert
        promptTemplate.Variables.Should().Contain("name");
        promptTemplate.Variables.Should().Contain("age");
        promptTemplate.Variables.Should().Contain("city");
        promptTemplate.Variables.Should().HaveCount(3);
    }

    [Fact]
    public async Task FormatAsync_WithValidVariables_ReturnsFormattedString()
    {
        // Arrange
        var template = new PromptTemplate("Hello {name}!");
        var variables = new Dictionary<string, object> { ["name"] = "World" };

        // Act
        var result = await template.FormatAsync(variables).ConfigureAwait(false);

        // Assert
        result.Should().Be("Hello World!");
    }

    [Fact]
    public async Task FormatAsync_WithMultipleVariables_FormatsCorrectly()
    {
        // Arrange
        var template = new PromptTemplate("Hello {name}, you are {age} years old.");
        var variables = new Dictionary<string, object>
        {
            ["name"] = "Alice",
            ["age"] = 30
        };

        // Act
        var result = await template.FormatAsync(variables).ConfigureAwait(false);

        // Assert
        result.Should().Be("Hello Alice, you are 30 years old.");
    }

    [Fact]
    public async Task FormatAsync_WithMissingVariable_ThrowsArgumentException()
    {
        // Arrange
        var template = new PromptTemplate("Hello {name}!");
        var variables = new Dictionary<string, object>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => template.FormatAsync(variables)).ConfigureAwait(false);
    }

    [Fact]
    public async Task FormatAsync_WithNullVariables_ThrowsArgumentNullException()
    {
        // Arrange
        var template = new PromptTemplate("Hello {name}!");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => template.FormatAsync(null!)).ConfigureAwait(false);
    }

    [Fact]
    public async Task FormatAsync_WithCaseInsensitiveVariables_Works()
    {
        // Arrange
        var template = new PromptTemplate("Hello {Name}!");
        var variables = new Dictionary<string, object> { ["name"] = "World" };

        // Act
        var result = await template.FormatAsync(variables).ConfigureAwait(false);

        // Assert
        result.Should().Be("Hello World!");
    }

    [Fact]
    public async Task FormatAsync_WithNullValue_ReplacesWithEmptyString()
    {
        // Arrange
        var template = new PromptTemplate("Hello {name}!");
        var variables = new Dictionary<string, object> { ["name"] = null! };

        // Act
        var result = await template.FormatAsync(variables).ConfigureAwait(false);

        // Assert
        result.Should().Be("Hello !");
    }

    [Fact]
    public async Task FormatAsync_WithRepeatedVariables_ReplacesAll()
    {
        // Arrange
        var template = new PromptTemplate("{name} says hello to {name}.");
        var variables = new Dictionary<string, object> { ["name"] = "Alice" };

        // Act
        var result = await template.FormatAsync(variables).ConfigureAwait(false);

        // Assert
        result.Should().Be("Alice says hello to Alice.");
    }
}
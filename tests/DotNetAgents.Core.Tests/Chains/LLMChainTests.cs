using DotNetAgents.Abstractions.Chains;
using DotNetAgents.Core.Chains;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.OutputParsers;
using DotNetAgents.Core.OutputParsers;
using DotNetAgents.Abstractions.Prompts;
using DotNetAgents.Core.Prompts;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.Core.Tests.Chains;

public class LLMChainTests
{
    [Fact]
    public async Task InvokeAsync_WithValidInput_ExecutesChain()
    {
        // Arrange
        var promptTemplate = new PromptTemplate("Hello {name}!");
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Hello World!");

        var chain = new LLMChain<IDictionary<string, object>, string>(
            promptTemplate,
            mockLLM.Object);

        var input = new Dictionary<string, object> { ["name"] = "World" };

        // Act
        var result = await chain.InvokeAsync(input);

        // Assert
        result.Should().Be("Hello World!");
        mockLLM.Verify(m => m.GenerateAsync(It.Is<string>(s => s.Contains("Hello World!")), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithOutputParser_ParsesOutput()
    {
        // Arrange
        var promptTemplate = new PromptTemplate("Return JSON: {request}");
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{\"value\": 42}");

        var outputParser = new JsonOutputParser<TestData>();
        var chain = new LLMChain<IDictionary<string, object>, TestData>(
            promptTemplate,
            mockLLM.Object,
            outputParser);

        var input = new Dictionary<string, object> { ["request"] = "test" };

        // Act
        var result = await chain.InvokeAsync(input);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Constructor_WithNullPromptTemplate_ThrowsArgumentNullException()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LLMChain<IDictionary<string, object>, string>(null!, mockLLM.Object));
    }

    [Fact]
    public void Constructor_WithNullLLM_ThrowsArgumentNullException()
    {
        // Arrange
        var promptTemplate = new PromptTemplate("Test");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LLMChain<IDictionary<string, object>, string>(promptTemplate, null!));
    }

    private record TestData
    {
        public int Value { get; init; }
    }
}
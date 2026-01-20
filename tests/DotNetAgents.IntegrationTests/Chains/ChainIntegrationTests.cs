using DotNetAgents.Core.Chains;
using DotNetAgents.Core.Models;
using DotNetAgents.Core.OutputParsers;
using DotNetAgents.Core.Prompts;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.IntegrationTests.Chains;

/// <summary>
/// Integration tests for chain execution scenarios.
/// These tests verify that chains work correctly end-to-end with real components.
/// </summary>
public class ChainIntegrationTests
{
    [Fact]
    public async Task LLMChain_WithPromptTemplate_ExecutesSuccessfully()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<LLMOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Hello, World!");

        var promptTemplate = new PromptTemplate("Say hello to {name}!");
        var chain = new LLMChain<IDictionary<string, object>, string>(
            promptTemplate,
            mockLLM.Object);

        var input = new Dictionary<string, object> { ["name"] = "World" };

        // Act
        var result = await chain.InvokeAsync(input);

        // Assert
        result.Should().Be("Hello, World!");
        mockLLM.Verify(m => m.GenerateAsync(
            It.Is<string>(s => s.Contains("Say hello to World!")),
            It.IsAny<LLMOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LLMChain_WithOutputParser_ParsesCorrectly()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<LLMOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("{\"value\": 42, \"name\": \"test\"}");

        var promptTemplate = new PromptTemplate("Return JSON: {request}");
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
        result.Name.Should().Be("test");
    }

    [Fact]
    public async Task SequentialChain_ComposesMultipleChains()
    {
        // Arrange
        var mockLLM1 = new Mock<ILLMModel<string, string>>();
        mockLLM1.Setup(m => m.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<LLMOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("intermediate result");

        var mockLLM2 = new Mock<ILLMModel<string, string>>();
        mockLLM2.Setup(m => m.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<LLMOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("final result");

        var chain1 = new LLMChain<string, string>(
            new PromptTemplate("{input}"),
            mockLLM1.Object);

        var chain2 = new LLMChain<string, string>(
            new PromptTemplate("{input}"),
            mockLLM2.Object);

        var sequentialChain = new SequentialChain<string, string, string>(
            chain1,
            chain2);

        // Act
        var result = await sequentialChain.InvokeAsync("start");

        // Assert
        result.Should().Be("final result");
        mockLLM1.Verify(m => m.GenerateAsync(
            It.Is<string>(s => s.Contains("start")),
            It.IsAny<LLMOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
        mockLLM2.Verify(m => m.GenerateAsync(
            It.Is<string>(s => s.Contains("intermediate result")),
            It.IsAny<LLMOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Runnable_Pipe_ComposesCorrectly()
    {
        // Arrange
        var runnable1 = new Runnable<string, string>(
            async (input, ct) => input.ToUpper());

        var runnable2 = new Runnable<string, string>(
            async (input, ct) => $"{input}!");

        // Act
        var piped = runnable1.Pipe(runnable2);
        var result = await piped.InvokeAsync("hello");

        // Assert
        result.Should().Be("HELLO!");
    }

    [Fact]
    public async Task Runnable_Map_ProcessesBatch()
    {
        // Arrange
        var runnable = new Runnable<string, string>(
            async (input, ct) => input.ToUpper());

        var inputs = new[] { "hello", "world", "test" };

        // Act
        var results = await runnable.BatchAsync(inputs);

        // Assert
        results.Should().HaveCount(3);
        results[0].Should().Be("HELLO");
        results[1].Should().Be("WORLD");
        results[2].Should().Be("TEST");
    }

    private record TestData
    {
        public int Value { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
using DotNetAgents.Abstractions.Chains;
using DotNetAgents.Core.Chains;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.OutputParsers;
using DotNetAgents.Abstractions.Prompts;
using DotNetAgents.Core.Prompts;
using DotNetAgents.Core.OutputParsers;
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
    public async Task Runnable_Pipe_ComposesChains()
    {
        // Arrange
        var runnable1 = new Runnable<string, string>(
            async (input, ct) => $"processed: {input}");

        var runnable2 = new Runnable<string, string>(
            async (input, ct) => $"{input} -> final");

        // Use Pipe for sequential composition
        var piped = runnable1.Pipe(runnable2);

        // Act
        var result = await piped.InvokeAsync("start");

        // Assert
        result.Should().Be("processed: start -> final");
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
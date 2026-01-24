using DotNetAgents.Abstractions.Agents;
using DotNetAgents.Core.Agents;
using DotNetAgents.Abstractions.Memory;
using DotNetAgents.Core.Memory.Implementations;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.Tools;
using DotNetAgents.Core.Tools;
using FluentAssertions;
using Moq;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Agents;

public class AgentExecutorTests
{
    [Fact]
    public async Task InvokeAsync_WithFinalAnswer_ReturnsAnswer()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Thought: I know the answer\nFinal Answer: 42");

        var toolRegistry = new ToolRegistry();
        var promptTemplate = new ReActPromptTemplate();
        var executor = new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate);

        // Act
        var result = await executor.InvokeAsync("What is the answer?");

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public async Task InvokeAsync_WithToolCall_ExecutesTool()
    {
        // Arrange
        var mockTool = new Mock<ITool>();
        mockTool.Setup(t => t.Name).Returns("Calculator");
        mockTool.Setup(t => t.Description).Returns("Performs calculations");
        mockTool.Setup(t => t.InputSchema).Returns(JsonSerializer.Deserialize<JsonElement>("{}"));
        mockTool.Setup(t => t.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ToolResult.Success("10"));

        var toolRegistry = new ToolRegistry();
        toolRegistry.Register(mockTool.Object);

        var mockLLM = new Mock<ILLMModel<string, string>>();
        var callCount = 0;
        mockLLM.Setup(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? "Thought: I need to calculate\nAction: Calculator\nAction Input: {\"operation\": \"add\", \"a\": 5, \"b\": 5}"
                    : "Thought: I got the result\nFinal Answer: 10";
            });

        var promptTemplate = new ReActPromptTemplate();
        var executor = new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate, maxIterations: 5);

        // Act
        var result = await executor.InvokeAsync("What is 5 + 5?");

        // Assert
        result.Should().Be("10");
        mockTool.Verify(t => t.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMemory_SavesToMemory()
    {
        // Arrange
        var memory = new InMemoryMemory();
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Thought: I know\nFinal Answer: Hello");

        var toolRegistry = new ToolRegistry();
        var promptTemplate = new ReActPromptTemplate();
        var executor = new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate, memory: memory);

        // Act
        await executor.InvokeAsync("Say hello");

        // Assert
        var messages = await memory.GetMessagesAsync(10);
        messages.Should().HaveCount(2);
        messages[0].Role.Should().Be("user");
        messages[0].Content.Should().Be("Say hello");
        messages[1].Role.Should().Be("assistant");
        messages[1].Content.Should().Be("Hello");
    }

    [Fact]
    public async Task InvokeAsync_ExceedsMaxIterations_ThrowsException()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Thought: Still thinking...");

        var toolRegistry = new ToolRegistry();
        var promptTemplate = new ReActPromptTemplate();
        var executor = new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate, maxIterations: 2);

        // Act & Assert
        await Assert.ThrowsAsync<DotNetAgents.Abstractions.Exceptions.AgentException>(() =>
            executor.InvokeAsync("Test"));
    }

    [Fact]
    public void Constructor_WithNullLLM_ThrowsArgumentNullException()
    {
        // Arrange
        var toolRegistry = new ToolRegistry();
        var promptTemplate = new ReActPromptTemplate();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AgentExecutor(null!, toolRegistry, promptTemplate));
    }

    [Fact]
    public void Constructor_WithInvalidMaxIterations_ThrowsArgumentException()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        var toolRegistry = new ToolRegistry();
        var promptTemplate = new ReActPromptTemplate();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate, maxIterations: 0));
    }
}
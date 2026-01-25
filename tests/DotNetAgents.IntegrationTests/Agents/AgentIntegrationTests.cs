using DotNetAgents.Abstractions.Agents;
using DotNetAgents.Core.Memory.Implementations;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.Prompts;
using DotNetAgents.Core.Prompts;
using DotNetAgents.Abstractions.Tools;
using DotNetAgents.Core.Tools;
using DotNetAgents.Core.Agents;
using DotNetAgents.Tools.BuiltIn;
using FluentAssertions;
using Moq;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.IntegrationTests.Agents;

/// <summary>
/// Integration tests for agent execution scenarios.
/// These tests verify that agents work correctly with tools and LLMs.
/// </summary>
public class AgentIntegrationTests
{
    [Fact]
    public async Task AgentExecutor_WithCalculatorTool_ExecutesTool()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.SetupSequence(m => m.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<LLMOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Thought: I need to calculate 2 + 2\nAction: Calculator\nAction Input: {\"expression\": \"2 + 2\"}")
            .ReturnsAsync("Thought: I got the result 4\nFinal Answer: 4");

        var toolRegistry = new ToolRegistry();
        var calculatorTool = new CalculatorTool();
        toolRegistry.Register(calculatorTool);

        var promptTemplate = new ReActPromptTemplate();
        var executor = new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate);

        // Act
        var result = await executor.InvokeAsync("What is 2 + 2?");

        // Assert
        result.Should().Contain("4");
        mockLLM.Verify(m => m.GenerateAsync(
            It.IsAny<string>(),
            It.IsAny<LLMOptions>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task AgentExecutor_WithDateTimeTool_ReturnsCurrentTime()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.SetupSequence(m => m.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<LLMOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Thought: I need to get the current time\nAction: DateTime\nAction Input: {}")
            .ReturnsAsync("Thought: I have the current time\nFinal Answer: The current time is provided above");

        var toolRegistry = new ToolRegistry();
        var dateTimeTool = new DateTimeTool();
        toolRegistry.Register(dateTimeTool);

        var promptTemplate = new ReActPromptTemplate();
        var executor = new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate);

        // Act
        var result = await executor.InvokeAsync("What time is it?");

        // Assert
        result.Should().NotBeNullOrEmpty();
        mockLLM.Verify(m => m.GenerateAsync(
            It.IsAny<string>(),
            It.IsAny<LLMOptions>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task AgentExecutor_WithMultipleTools_SelectsCorrectTool()
    {
        // Arrange
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.SetupSequence(m => m.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<LLMOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Thought: I need to calculate\nAction: Calculator\nAction Input: {\"expression\": \"10 * 5\"}")
            .ReturnsAsync("Thought: I got 50\nFinal Answer: 50");

        var toolRegistry = new ToolRegistry();
        toolRegistry.Register(new CalculatorTool());
        toolRegistry.Register(new DateTimeTool());

        var promptTemplate = new ReActPromptTemplate();
        var executor = new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate);

        // Act
        var result = await executor.InvokeAsync("Calculate 10 times 5");

        // Assert
        result.Should().Contain("50");
    }

    [Fact]
    public async Task AgentExecutor_WithMaxIterations_StopsAfterLimit()
    {
        // Arrange
        var callCount = 0;
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<LLMOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                    return "Thought: I need to calculate\nAction: Calculator\nAction Input: {\"expression\": \"1 + 1\"}";
                else
                    return "Thought: I got the result\nFinal Answer: 2";
            });

        var toolRegistry = new ToolRegistry();
        toolRegistry.Register(new CalculatorTool());

        var promptTemplate = new ReActPromptTemplate();
        var executor = new AgentExecutor(mockLLM.Object, toolRegistry, promptTemplate, maxIterations: 3);

        // Act
        var result = await executor.InvokeAsync("What is 1 + 1?");

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Should stop after maxIterations even if not finished
        mockLLM.Verify(m => m.GenerateAsync(
            It.IsAny<string>(),
            It.IsAny<LLMOptions>(),
            It.IsAny<CancellationToken>()), Times.AtMost(3));
    }
}
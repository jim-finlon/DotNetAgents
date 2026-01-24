using DotLangChain.Abstractions.Agents.Tools;
using DotLangChain.Abstractions.LLM;
using DotLangChain.Core.Agents.Tools;
using DotLangChain.Abstractions.Exceptions;
using FluentAssertions;
using System.Text.Json;

namespace DotLangChain.Tests.Unit.Agents.Tools;

public class ToolRegistryTests
{
    private class TestTools
    {
        [Tool(Description = "Adds two numbers")]
        public int Add(
            [ToolParameter(Description = "First number")] int a,
            [ToolParameter(Description = "Second number")] int b)
        {
            return a + b;
        }

        [Tool(Name = "multiply", Description = "Multiplies two numbers")]
        public Task<int> MultiplyAsync(
            [ToolParameter(Description = "First number")] int x,
            [ToolParameter(Description = "Second number")] int y)
        {
            return Task.FromResult(x * y);
        }

        [Tool(Description = "Greets someone")]
        public string Greet(
            [ToolParameter(Description = "Name to greet")] string name,
            [ToolParameter(Description = "Optional greeting", Required = false)] string? greeting = null)
        {
            return $"{greeting ?? "Hello"}, {name}!";
        }
    }

    [Fact]
    public void Register_WithInstance_RegistersTools()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tools = new TestTools();

        // Act
        registry.Register(tools);
        var executor = registry.BuildExecutor();

        // Assert
        var toolDefinitions = executor.GetToolDefinitions();
        toolDefinitions.Should().HaveCount(3);
        toolDefinitions.Should().Contain(t => t.Name == "Add");
        toolDefinitions.Should().Contain(t => t.Name == "multiply");
        toolDefinitions.Should().Contain(t => t.Name == "Greet");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidToolCall_ExecutesTool()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tools = new TestTools();
        registry.Register(tools);
        var executor = registry.BuildExecutor();

        var toolCall = new ToolCall
        {
            Id = "call1",
            Name = "Add",
            Arguments = JsonSerializer.Serialize(new { a = 5, b = 3 })
        };

        // Act
        var result = await executor.ExecuteAsync(toolCall);

        // Assert
        result.IsError.Should().BeFalse();
        result.ToolCallId.Should().Be("call1");
        var value = JsonSerializer.Deserialize<int>(result.Content);
        value.Should().Be(8);
    }

    [Fact]
    public async Task ExecuteAsync_WithAsyncTool_ExecutesAsyncTool()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tools = new TestTools();
        registry.Register(tools);
        var executor = registry.BuildExecutor();

        var toolCall = new ToolCall
        {
            Id = "call1",
            Name = "multiply",
            Arguments = JsonSerializer.Serialize(new { x = 4, y = 7 })
        };

        // Act
        var result = await executor.ExecuteAsync(toolCall);

        // Assert
        result.IsError.Should().BeFalse();
        var value = JsonSerializer.Deserialize<int>(result.Content);
        value.Should().Be(28);
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalParameter_UsesDefault()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tools = new TestTools();
        registry.Register(tools);
        var executor = registry.BuildExecutor();

        var toolCall = new ToolCall
        {
            Id = "call1",
            Name = "Greet",
            Arguments = JsonSerializer.Serialize(new { name = "Alice" })
        };

        // Act
        var result = await executor.ExecuteAsync(toolCall);

        // Assert
        result.IsError.Should().BeFalse();
        result.Content.Should().Contain("Hello, Alice!");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentTool_ThrowsToolException()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tools = new TestTools();
        registry.Register(tools);
        var executor = registry.BuildExecutor();

        var toolCall = new ToolCall
        {
            Id = "call1",
            Name = "NonExistent",
            Arguments = "{}"
        };

        // Act
        var act = async () => await executor.ExecuteAsync(toolCall);

        // Assert
        await act.Should().ThrowAsync<ToolException>()
            .Where(e => e.ToolName == "NonExistent");
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredParameter_ThrowsToolException()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tools = new TestTools();
        registry.Register(tools);
        var executor = registry.BuildExecutor();

        var toolCall = new ToolCall
        {
            Id = "call1",
            Name = "Add",
            Arguments = JsonSerializer.Serialize(new { a = 5 }) // Missing 'b'
        };

        // Act
        var act = async () => await executor.ExecuteAsync(toolCall);

        // Assert
        await act.Should().ThrowAsync<ToolException>()
            .Where(e => e.ErrorCode == "DLC006-002");
    }

    [Fact]
    public async Task ExecuteAllAsync_ExecutesMultipleTools()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tools = new TestTools();
        registry.Register(tools);
        var executor = registry.BuildExecutor();

        var toolCalls = new[]
        {
            new ToolCall
            {
                Id = "call1",
                Name = "Add",
                Arguments = JsonSerializer.Serialize(new { a = 2, b = 3 })
            },
            new ToolCall
            {
                Id = "call2",
                Name = "multiply",
                Arguments = JsonSerializer.Serialize(new { x = 4, y = 5 })
            }
        };

        // Act
        var results = await executor.ExecuteAllAsync(toolCalls);

        // Assert
        results.Should().HaveCount(2);
        JsonSerializer.Deserialize<int>(results[0].Content).Should().Be(5);
        JsonSerializer.Deserialize<int>(results[1].Content).Should().Be(20);
    }
}


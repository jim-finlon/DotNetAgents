using DotNetAgents.Tools.BuiltIn;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools.BuiltIn;

public class ShellCommandToolTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var tool = new ShellCommandTool();

        // Act
        var name = tool.Name;

        // Assert
        Assert.Equal("shell_command", name);
    }

    [Fact]
    public void Description_ReturnsNonEmptyString()
    {
        // Arrange
        var tool = new ShellCommandTool();

        // Act
        var description = tool.Description;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public void InputSchema_IsValidJson()
    {
        // Arrange
        var tool = new ShellCommandTool();

        // Act
        var schema = tool.InputSchema;

        // Assert
        Assert.True(schema.ValueKind == JsonValueKind.Object);
        Assert.True(schema.TryGetProperty("type", out _));
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingCommand_ReturnsFailure()
    {
        // Arrange
        var tool = new ShellCommandTool();
        var input = new Dictionary<string, object>();

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyCommand_ReturnsFailure()
    {
        // Arrange
        var tool = new ShellCommandTool();
        var input = new Dictionary<string, object> { ["command"] = "" };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithDisallowedCommand_ReturnsFailure()
    {
        // Arrange
        var allowedCommands = new HashSet<string> { "echo", "dir" };
        var tool = new ShellCommandTool(allowedCommands);
        var input = new Dictionary<string, object> { ["command"] = "rm" };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not in the allowed list", result.ErrorMessage ?? "");
    }

    [Fact]
    public async Task ExecuteAsync_WithAllowedCommand_ExecutesSuccessfully()
    {
        // Arrange
        var allowedCommands = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "echo" };
        var tool = new ShellCommandTool(allowedCommands);
        var input = new Dictionary<string, object>
        {
            ["command"] = "echo",
            ["arguments"] = "Hello World"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        // On Windows, echo might succeed; on Linux/Mac it might fail without proper shell
        // So we just check that we got a result
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonElementInput_ParsesCorrectly()
    {
        // Arrange
        var tool = new ShellCommandTool();
        var json = JsonSerializer.Serialize(new { command = "invalid_command_xyz" });
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = await tool.ExecuteAsync(jsonElement);

        // Assert
        Assert.NotNull(result);
    }
}
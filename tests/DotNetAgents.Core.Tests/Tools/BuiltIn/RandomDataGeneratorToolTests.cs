using DotNetAgents.Core.Tools.BuiltIn;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools.BuiltIn;

public class RandomDataGeneratorToolTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();

        // Act
        var name = tool.Name;

        // Assert
        Assert.Equal("random_data_generator", name);
    }

    [Fact]
    public void Description_ReturnsNonEmptyString()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();

        // Act
        var description = tool.Description;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public async Task ExecuteAsync_WithIntegerType_GeneratesInteger()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["type"] = "integer",
            ["min"] = 1,
            ["max"] = 100
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var value = int.Parse(result.Output as string ?? "0");
        Assert.InRange(value, 1, 99); // max is exclusive
    }

    [Fact]
    public async Task ExecuteAsync_WithDoubleType_GeneratesDouble()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["type"] = "double",
            ["min"] = 0.0,
            ["max"] = 1.0
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var value = double.Parse(result.Output as string ?? "0");
        Assert.InRange(value, 0.0, 1.0);
    }

    [Fact]
    public async Task ExecuteAsync_WithStringType_GeneratesString()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["type"] = "string",
            ["length"] = 10
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var value = result.Output as string;
        Assert.NotNull(value);
        Assert.Equal(10, value.Length);
    }

    [Fact]
    public async Task ExecuteAsync_WithUUIDType_GeneratesUUID()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["type"] = "uuid"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var value = result.Output as string;
        Assert.NotNull(value);
        Assert.True(Guid.TryParse(value, out _));
    }

    [Fact]
    public async Task ExecuteAsync_WithBooleanType_GeneratesBoolean()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["type"] = "boolean"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var value = result.Output as string;
        Assert.NotNull(value);
        Assert.True(value == "true" || value == "false");
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingType_ReturnsFailure()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var input = new Dictionary<string, object>();

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidType_ReturnsFailure()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["type"] = "invalid_type"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidMinMax_ReturnsFailure()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["type"] = "integer",
            ["min"] = 100,
            ["max"] = 1 // Invalid: min > max
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonElementInput_ParsesCorrectly()
    {
        // Arrange
        var tool = new RandomDataGeneratorTool();
        var json = JsonSerializer.Serialize(new { type = "uuid" });
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = await tool.ExecuteAsync(jsonElement);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
    }
}
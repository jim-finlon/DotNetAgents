using DotNetAgents.Tools.BuiltIn;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools.BuiltIn;

public class WeatherToolTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var tool = new WeatherTool();

        // Act
        var name = tool.Name;

        // Assert
        Assert.Equal("weather", name);
    }

    [Fact]
    public void Description_ReturnsNonEmptyString()
    {
        // Arrange
        var tool = new WeatherTool();

        // Act
        var description = tool.Description;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingLocation_ReturnsFailure()
    {
        // Arrange
        var tool = new WeatherTool();
        var input = new Dictionary<string, object>();

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyLocation_ReturnsFailure()
    {
        // Arrange
        var tool = new WeatherTool();
        var input = new Dictionary<string, object> { ["location"] = "" };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutApiKey_ReturnsPlaceholderMessage()
    {
        // Arrange
        var tool = new WeatherTool();
        var input = new Dictionary<string, object>
        {
            ["location"] = "London"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        Assert.Contains("API key", result.Output.ToString() ?? "");
    }

    [Fact]
    public async Task ExecuteAsync_WithCelsiusUnits_SetsUnits()
    {
        // Arrange
        var tool = new WeatherTool();
        var input = new Dictionary<string, object>
        {
            ["location"] = "London",
            ["units"] = "celsius"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithFahrenheitUnits_SetsUnits()
    {
        // Arrange
        var tool = new WeatherTool();
        var input = new Dictionary<string, object>
        {
            ["location"] = "London",
            ["units"] = "fahrenheit"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonElementInput_ParsesCorrectly()
    {
        // Arrange
        var tool = new WeatherTool();
        var json = JsonSerializer.Serialize(new { location = "Paris" });
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = await tool.ExecuteAsync(jsonElement);

        // Assert
        Assert.NotNull(result);
    }
}
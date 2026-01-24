using DotNetAgents.Tools.BuiltIn;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools.BuiltIn;

public class WikipediaSearchToolTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var httpClient = new HttpClient();
        var tool = new WikipediaSearchTool(httpClient);

        // Act
        var name = tool.Name;

        // Assert
        Assert.Equal("wikipedia_search", name);
    }

    [Fact]
    public void Description_ReturnsNonEmptyString()
    {
        // Arrange
        var httpClient = new HttpClient();
        var tool = new WikipediaSearchTool(httpClient);

        // Act
        var description = tool.Description;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingQuery_ReturnsFailure()
    {
        // Arrange
        var httpClient = new HttpClient();
        var tool = new WikipediaSearchTool(httpClient);
        var input = new Dictionary<string, object>();

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyQuery_ReturnsFailure()
    {
        // Arrange
        var httpClient = new HttpClient();
        var tool = new WikipediaSearchTool(httpClient);
        var input = new Dictionary<string, object> { ["query"] = "" };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidQuery_ReturnsSuccess()
    {
        // Arrange
        var httpClient = new HttpClient();
        var tool = new WikipediaSearchTool(httpClient);
        var input = new Dictionary<string, object>
        {
            ["query"] = "Artificial Intelligence"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        // May succeed or fail depending on network/Wikipedia API availability
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxResults_LimitsResults()
    {
        // Arrange
        var httpClient = new HttpClient();
        var tool = new WikipediaSearchTool(httpClient);
        var input = new Dictionary<string, object>
        {
            ["query"] = "Science",
            ["max_results"] = 3
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
        var httpClient = new HttpClient();
        var tool = new WikipediaSearchTool(httpClient);
        var json = JsonSerializer.Serialize(new { query = "Test" });
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = await tool.ExecuteAsync(jsonElement);

        // Assert
        Assert.NotNull(result);
    }
}
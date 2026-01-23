using DotNetAgents.Core.Tools.BuiltIn;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools.BuiltIn;

public class CsvReaderToolTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var tool = new CsvReaderTool();

        // Act
        var name = tool.Name;

        // Assert
        Assert.Equal("csv_reader", name);
    }

    [Fact]
    public void Description_ReturnsNonEmptyString()
    {
        // Arrange
        var tool = new CsvReaderTool();

        // Act
        var description = tool.Description;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public async Task ExecuteAsync_WithParseOperation_ParsesCSV()
    {
        // Arrange
        var tool = new CsvReaderTool();
        var csvData = "Name,Age,City\nJohn,30,New York\nJane,25,London";
        var input = new Dictionary<string, object>
        {
            ["operation"] = "parse",
            ["csv_data"] = csvData,
            ["has_header"] = true
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var output = result.Output as string;
        Assert.NotNull(output);
        Assert.Contains("John", output);
        Assert.Contains("Jane", output);
    }

    [Fact]
    public async Task ExecuteAsync_WithToJsonOperation_ConvertsToJson()
    {
        // Arrange
        var tool = new CsvReaderTool();
        var csvData = "Name,Age\nJohn,30";
        var input = new Dictionary<string, object>
        {
            ["operation"] = "to_json",
            ["csv_data"] = csvData,
            ["has_header"] = true
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var output = result.Output as string;
        Assert.NotNull(output);
        
        // Verify it's valid JSON
        var json = JsonSerializer.Deserialize<JsonElement>(output);
        Assert.True(json.ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingOperation_ReturnsFailure()
    {
        // Arrange
        var tool = new CsvReaderTool();
        var input = new Dictionary<string, object>();

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithReadOperation_NonExistentFile_ReturnsFailure()
    {
        // Arrange
        var tool = new CsvReaderTool();
        var input = new Dictionary<string, object>
        {
            ["operation"] = "read",
            ["file_path"] = "nonexistent_file.csv"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomDelimiter_ParsesCorrectly()
    {
        // Arrange
        var tool = new CsvReaderTool();
        var csvData = "Name;Age;City\nJohn;30;New York";
        var input = new Dictionary<string, object>
        {
            ["operation"] = "parse",
            ["csv_data"] = csvData,
            ["delimiter"] = ";",
            ["has_header"] = true
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonElementInput_ParsesCorrectly()
    {
        // Arrange
        var tool = new CsvReaderTool();
        var json = JsonSerializer.Serialize(new
        {
            operation = "parse",
            csv_data = "Name,Age\nJohn,30",
            has_header = true
        });
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = await tool.ExecuteAsync(jsonElement);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
    }
}
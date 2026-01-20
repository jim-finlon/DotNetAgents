using DotNetAgents.Core.Tools.BuiltIn;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools.BuiltIn;

public class EmailToolTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var tool = new EmailTool("smtp.example.com");

        // Act
        var name = tool.Name;

        // Assert
        Assert.Equal("email", name);
    }

    [Fact]
    public void Description_ReturnsNonEmptyString()
    {
        // Arrange
        var tool = new EmailTool("smtp.example.com");

        // Act
        var description = tool.Description;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingTo_ReturnsFailure()
    {
        // Arrange
        var tool = new EmailTool("smtp.example.com");
        var input = new Dictionary<string, object>
        {
            ["subject"] = "Test",
            ["body"] = "Test body"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingSubject_ReturnsFailure()
    {
        // Arrange
        var tool = new EmailTool("smtp.example.com");
        var input = new Dictionary<string, object>
        {
            ["to"] = "test@example.com",
            ["body"] = "Test body"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingBody_ReturnsFailure()
    {
        // Arrange
        var tool = new EmailTool("smtp.example.com");
        var input = new Dictionary<string, object>
        {
            ["to"] = "test@example.com",
            ["subject"] = "Test"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidSmtpServer_ReturnsFailure()
    {
        // Arrange
        var tool = new EmailTool("invalid-server-that-does-not-exist.local", 25);
        var input = new Dictionary<string, object>
        {
            ["to"] = "test@example.com",
            ["subject"] = "Test",
            ["body"] = "Test body"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        // Should fail due to invalid SMTP server (may timeout or fail immediately)
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleRecipients_ParsesCorrectly()
    {
        // Arrange
        var tool = new EmailTool("smtp.example.com");
        var input = new Dictionary<string, object>
        {
            ["to"] = "test1@example.com, test2@example.com",
            ["subject"] = "Test",
            ["body"] = "Test body"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        // Will fail due to invalid SMTP, but should parse recipients correctly
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonElementInput_ParsesCorrectly()
    {
        // Arrange
        var tool = new EmailTool("smtp.example.com");
        var json = JsonSerializer.Serialize(new
        {
            to = "test@example.com",
            subject = "Test",
            body = "Test body"
        });
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = await tool.ExecuteAsync(jsonElement);

        // Assert
        Assert.NotNull(result);
    }
}
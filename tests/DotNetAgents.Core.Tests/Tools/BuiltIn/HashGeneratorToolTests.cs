using DotNetAgents.Tools.BuiltIn;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools.BuiltIn;

public class HashGeneratorToolTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var tool = new HashGeneratorTool();

        // Act
        var name = tool.Name;

        // Assert
        Assert.Equal("hash_generator", name);
    }

    [Fact]
    public void Description_ReturnsNonEmptyString()
    {
        // Arrange
        var tool = new HashGeneratorTool();

        // Act
        var description = tool.Description;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public async Task ExecuteAsync_WithSHA256_GeneratesCorrectHash()
    {
        // Arrange
        var tool = new HashGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["input"] = "Hello World",
            ["algorithm"] = "sha256"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        // Verify hash format (hex string, lowercase)
        var hash = result.Output as string;
        Assert.NotNull(hash);
        Assert.Matches("^[a-f0-9]{64}$", hash);
    }

    [Fact]
    public async Task ExecuteAsync_WithMD5_GeneratesHash()
    {
        // Arrange
        var tool = new HashGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["input"] = "test",
            ["algorithm"] = "md5"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var hash = result.Output as string;
        Assert.NotNull(hash);
        Assert.Matches("^[a-f0-9]{32}$", hash); // MD5 produces 32 hex characters
    }

    [Fact]
    public async Task ExecuteAsync_WithBase64Format_GeneratesBase64Hash()
    {
        // Arrange
        var tool = new HashGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["input"] = "test",
            ["algorithm"] = "sha256",
            ["format"] = "base64"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
        
        var hash = result.Output as string;
        Assert.NotNull(hash);
        
        // Verify it's valid base64
        var bytes = Convert.FromBase64String(hash);
        Assert.Equal(32, bytes.Length); // SHA256 produces 32 bytes
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingInput_ReturnsFailure()
    {
        // Arrange
        var tool = new HashGeneratorTool();
        var input = new Dictionary<string, object>();

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyInput_ReturnsFailure()
    {
        // Arrange
        var tool = new HashGeneratorTool();
        var input = new Dictionary<string, object> { ["input"] = "" };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidAlgorithm_ReturnsFailure()
    {
        // Arrange
        var tool = new HashGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["input"] = "test",
            ["algorithm"] = "invalid_algorithm"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultAlgorithm_UsesSHA256()
    {
        // Arrange
        var tool = new HashGeneratorTool();
        var input = new Dictionary<string, object>
        {
            ["input"] = "Hello World"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify it's SHA256 (64 hex characters)
        var hash = result.Output as string;
        Assert.NotNull(hash);
        Assert.Matches("^[a-f0-9]{64}$", hash);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonElementInput_ParsesCorrectly()
    {
        // Arrange
        var tool = new HashGeneratorTool();
        var json = JsonSerializer.Serialize(new { input = "test", algorithm = "sha256" });
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = await tool.ExecuteAsync(jsonElement);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);
    }
}
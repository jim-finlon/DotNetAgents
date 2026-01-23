using DotNetAgents.Core.Caching;
using DotNetAgents.Core.Tools.BuiltIn;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools.BuiltIn;

public class MemoryStorageToolTests
{
    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var cache = new InMemoryCache();
        var tool = new MemoryStorageTool(cache);

        // Act
        var name = tool.Name;

        // Assert
        Assert.Equal("memory_storage", name);
    }

    [Fact]
    public void Description_ReturnsNonEmptyString()
    {
        // Arrange
        var cache = new InMemoryCache();
        var tool = new MemoryStorageTool(cache);

        // Act
        var description = tool.Description;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public async Task ExecuteAsync_WithSetOperation_StoresValue()
    {
        // Arrange
        var cache = new InMemoryCache();
        var tool = new MemoryStorageTool(cache);
        var input = new Dictionary<string, object>
        {
            ["operation"] = "set",
            ["key"] = "test_key",
            ["value"] = "test_value"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify value was stored
        var stored = await cache.GetAsync<string>("test_key");
        Assert.Equal("test_value", stored);
    }

    [Fact]
    public async Task ExecuteAsync_WithGetOperation_RetrievesValue()
    {
        // Arrange
        var cache = new InMemoryCache();
        await cache.SetAsync("test_key", "test_value");
        var tool = new MemoryStorageTool(cache);
        var input = new Dictionary<string, object>
        {
            ["operation"] = "get",
            ["key"] = "test_key"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test_value", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithGetOperation_NonExistentKey_ReturnsNotFound()
    {
        // Arrange
        var cache = new InMemoryCache();
        var tool = new MemoryStorageTool(cache);
        var input = new Dictionary<string, object>
        {
            ["operation"] = "get",
            ["key"] = "non_existent_key"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithDeleteOperation_RemovesValue()
    {
        // Arrange
        var cache = new InMemoryCache();
        await cache.SetAsync("test_key", "test_value");
        var tool = new MemoryStorageTool(cache);
        var input = new Dictionary<string, object>
        {
            ["operation"] = "delete",
            ["key"] = "test_key"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify value was deleted
        var stored = await cache.GetAsync<string>("test_key");
        Assert.Null(stored);
    }

    [Fact]
    public async Task ExecuteAsync_WithClearOperation_ClearsCache()
    {
        // Arrange
        var cache = new InMemoryCache();
        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");
        var tool = new MemoryStorageTool(cache);
        var input = new Dictionary<string, object>
        {
            ["operation"] = "clear"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify cache is cleared
        var stored1 = await cache.GetAsync<string>("key1");
        var stored2 = await cache.GetAsync<string>("key2");
        Assert.Null(stored1);
        Assert.Null(stored2);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingOperation_ReturnsFailure()
    {
        // Arrange
        var cache = new InMemoryCache();
        var tool = new MemoryStorageTool(cache);
        var input = new Dictionary<string, object>();

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidOperation_ReturnsFailure()
    {
        // Arrange
        var cache = new InMemoryCache();
        var tool = new MemoryStorageTool(cache);
        var input = new Dictionary<string, object>
        {
            ["operation"] = "invalid_operation"
        };

        // Act
        var result = await tool.ExecuteAsync(input);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
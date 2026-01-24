using DotNetAgents.Abstractions.Caching;
using DotNetAgents.Core.Caching;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Caching;

public class InMemoryCacheTests
{
    [Fact]
    public async Task GetAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var cache = new InMemoryCache();

        // Act
        var result = await cache.GetAsync<string>("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_GetAsync_WithValidData_StoresAndRetrieves()
    {
        // Arrange
        var cache = new InMemoryCache();
        var key = "test-key";
        var value = "test-value";

        // Act
        await cache.SetAsync(key, value);
        var result = await cache.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ExpiresAfterTime()
    {
        // Arrange
        var cache = new InMemoryCache();
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await cache.SetAsync(key, value, expiration);
        var beforeExpiration = await cache.GetAsync<string>(key);
        
        await Task.Delay(150);
        var afterExpiration = await cache.GetAsync<string>(key);

        // Assert
        beforeExpiration.Should().Be(value);
        afterExpiration.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_WithExistingKey_RemovesKey()
    {
        // Arrange
        var cache = new InMemoryCache();
        var key = "test-key";
        await cache.SetAsync(key, "test-value");

        // Act
        var removed = await cache.RemoveAsync(key);
        var result = await cache.GetAsync<string>(key);

        // Assert
        removed.Should().BeTrue();
        result.Should().BeNull();
    }

    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        // Arrange
        var cache = new InMemoryCache();
        await cache.SetAsync("key1", "value1");
        await cache.SetAsync("key2", "value2");

        // Act
        await cache.ClearAsync();
        var result1 = await cache.GetAsync<string>("key1");
        var result2 = await cache.GetAsync<string>("key2");

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var cache = new InMemoryCache();
        var key = "test-key";
        await cache.SetAsync(key, "test-value");

        // Act
        var exists = await cache.ExistsAsync(key);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentKey_ReturnsFalse()
    {
        // Arrange
        var cache = new InMemoryCache();

        // Act
        var exists = await cache.ExistsAsync("non-existent");

        // Assert
        exists.Should().BeFalse();
    }
}
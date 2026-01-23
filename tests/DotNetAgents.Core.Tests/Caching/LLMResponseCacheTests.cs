using DotNetAgents.Core.Caching;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Caching;

public class LLMResponseCacheTests
{
    [Fact]
    public async Task GetCachedResponseAsync_WithNonExistentInput_ReturnsNull()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new LLMResponseCache<string, string>(underlyingCache);

        // Act
        var result = await cache.GetCachedResponseAsync("test input");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CacheResponseAsync_GetCachedResponseAsync_StoresAndRetrieves()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new LLMResponseCache<string, string>(underlyingCache);
        var input = "test input";
        var output = "test output";

        // Act
        await cache.CacheResponseAsync(input, output);
        var result = await cache.GetCachedResponseAsync(input);

        // Assert
        result.Should().Be(output);
    }

    [Fact]
    public async Task GetCachedResponseAsync_WithComplexInput_WorksCorrectly()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new LLMResponseCache<TestInput, TestOutput>(underlyingCache);
        var input = new TestInput { Value = "test", Number = 42 };
        var output = new TestOutput { Result = "success" };
        await cache.CacheResponseAsync(input, output);

        // Act
        var result = await cache.GetCachedResponseAsync(input);

        // Assert
        result.Should().NotBeNull();
        result!.Result.Should().Be("success");
    }

    [Fact]
    public async Task CacheResponseAsync_WithExpiration_ExpiresAfterTime()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new LLMResponseCache<string, string>(underlyingCache);
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await cache.CacheResponseAsync("input", "output", expiration);
        var beforeExpiration = await cache.GetCachedResponseAsync("input");
        
        await Task.Delay(150);
        var afterExpiration = await cache.GetCachedResponseAsync("input");

        // Assert
        beforeExpiration.Should().Be("output");
        afterExpiration.Should().BeNull();
    }

    private class TestInput
    {
        public string Value { get; set; } = string.Empty;
        public int Number { get; set; }
    }

    private class TestOutput
    {
        public string Result { get; set; } = string.Empty;
    }
}
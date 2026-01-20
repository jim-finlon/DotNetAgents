using DotNetAgents.Core.Caching;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Caching;

public class EmbeddingCacheTests
{
    [Fact]
    public async Task GetCachedEmbeddingAsync_WithNonExistentText_ReturnsNull()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new EmbeddingCache(underlyingCache);

        // Act
        var result = await cache.GetCachedEmbeddingAsync("test text");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CacheEmbeddingAsync_GetCachedEmbeddingAsync_StoresAndRetrieves()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new EmbeddingCache(underlyingCache);
        var text = "test text";
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        await cache.CacheEmbeddingAsync(text, embedding);
        var result = await cache.GetCachedEmbeddingAsync(text);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(embedding);
    }

    [Fact]
    public async Task GetCachedEmbeddingAsync_WithSameText_ReturnsSameEmbedding()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new EmbeddingCache(underlyingCache);
        var text = "same text";
        var embedding = new float[] { 0.5f, 0.6f, 0.7f };
        await cache.CacheEmbeddingAsync(text, embedding);

        // Act - Get with same text but different reference
        var result = await cache.GetCachedEmbeddingAsync(new string(text.ToCharArray()));

        // Assert
        result.Should().BeEquivalentTo(embedding);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllEmbeddings()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new EmbeddingCache(underlyingCache);
        await cache.CacheEmbeddingAsync("text1", new float[] { 1.0f });
        await cache.CacheEmbeddingAsync("text2", new float[] { 2.0f });

        // Act
        await cache.ClearAsync();
        var result1 = await cache.GetCachedEmbeddingAsync("text1");
        var result2 = await cache.GetCachedEmbeddingAsync("text2");

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task CacheEmbeddingAsync_WithNullText_ThrowsException()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new EmbeddingCache(underlyingCache);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            cache.CacheEmbeddingAsync(null!, new float[] { 1.0f }));
    }

    [Fact]
    public async Task CacheEmbeddingAsync_WithEmptyEmbedding_ThrowsException()
    {
        // Arrange
        var underlyingCache = new InMemoryCache();
        var cache = new EmbeddingCache(underlyingCache);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            cache.CacheEmbeddingAsync("text", Array.Empty<float>()));
    }
}
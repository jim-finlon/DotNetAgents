using DotNetAgents.Core.Retrieval;
using DotNetAgents.Core.Retrieval.Implementations;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Retrieval;

public class InMemoryVectorStoreTests
{
    [Fact]
    public async Task UpsertAsync_WithValidVector_StoresVector()
    {
        // Arrange
        var store = new InMemoryVectorStore();
        var vector = new float[] { 1.0f, 2.0f, 3.0f };

        // Act
        var id = await store.UpsertAsync("test-id", vector);

        // Assert
        id.Should().Be("test-id");
    }

    [Fact]
    public async Task SearchAsync_WithSimilarVector_ReturnsHighScore()
    {
        // Arrange
        var store = new InMemoryVectorStore();
        var vector1 = new float[] { 1.0f, 0.0f, 0.0f };
        var vector2 = new float[] { 0.0f, 1.0f, 0.0f };
        var queryVector = new float[] { 1.0f, 0.0f, 0.0f };

        await store.UpsertAsync("id1", vector1);
        await store.UpsertAsync("id2", vector2);

        // Act
        var results = await store.SearchAsync(queryVector, topK: 2);

        // Assert
        results.Should().HaveCount(2);
        results[0].Id.Should().Be("id1"); // Should be most similar
        results[0].Score.Should().BeGreaterThan(results[1].Score);
    }

    [Fact]
    public async Task SearchAsync_WithMetadataFilter_FiltersResults()
    {
        // Arrange
        var store = new InMemoryVectorStore();
        var vector = new float[] { 1.0f, 2.0f, 3.0f };
        var metadata1 = new Dictionary<string, object> { ["category"] = "A" };
        var metadata2 = new Dictionary<string, object> { ["category"] = "B" };

        await store.UpsertAsync("id1", vector, metadata1);
        await store.UpsertAsync("id2", vector, metadata2);

        var filter = new Dictionary<string, object> { ["category"] = "A" };

        // Act
        var results = await store.SearchAsync(vector, topK: 10, filter: filter);

        // Assert
        results.Should().HaveCount(1);
        results[0].Id.Should().Be("id1");
    }

    [Fact]
    public async Task DeleteAsync_WithValidIds_RemovesVectors()
    {
        // Arrange
        var store = new InMemoryVectorStore();
        var vector = new float[] { 1.0f, 2.0f, 3.0f };
        await store.UpsertAsync("id1", vector);
        await store.UpsertAsync("id2", vector);

        // Act
        var deleted = await store.DeleteAsync(new[] { "id1" });

        // Assert
        deleted.Should().Be(1);
        var results = await store.SearchAsync(vector, topK: 10);
        results.Should().HaveCount(1);
        results[0].Id.Should().Be("id2");
    }

    [Fact]
    public async Task SearchAsync_WithTopK_LimitsResults()
    {
        // Arrange
        var store = new InMemoryVectorStore();
        var queryVector = new float[] { 1.0f, 0.0f, 0.0f };

        for (int i = 0; i < 10; i++)
        {
            var vector = new float[] { (float)i / 10, 0.0f, 0.0f };
            await store.UpsertAsync($"id{i}", vector);
        }

        // Act
        var results = await store.SearchAsync(queryVector, topK: 3);

        // Assert
        results.Should().HaveCount(3);
    }
}
using DotNetAgents.Workflow.Checkpoints;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Workflow.Tests.Checkpoints;

public class InMemoryCheckpointStoreTests
{
    [Fact]
    public async Task SaveAsync_WithValidCheckpoint_SavesSuccessfully()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();
        var checkpoint = new Checkpoint<TestState>
        {
            Id = "checkpoint-1",
            RunId = "run-1",
            NodeName = "node-1",
            SerializedState = "{\"value\":42}"
        };

        // Act
        var result = await store.SaveAsync(checkpoint);

        // Assert
        result.Should().Be("checkpoint-1");
        var retrieved = await store.GetAsync("checkpoint-1");
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be("checkpoint-1");
    }

    [Fact]
    public async Task GetAsync_WithExistingCheckpoint_ReturnsCheckpoint()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();
        var checkpoint = new Checkpoint<TestState>
        {
            Id = "checkpoint-1",
            RunId = "run-1",
            NodeName = "node-1",
            SerializedState = "{\"value\":42}"
        };
        await store.SaveAsync(checkpoint);

        // Act
        var result = await store.GetAsync("checkpoint-1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("checkpoint-1");
        result.RunId.Should().Be("run-1");
        result.NodeName.Should().Be("node-1");
    }

    [Fact]
    public async Task GetAsync_WithNonExistentCheckpoint_ReturnsNull()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();

        // Act
        var result = await store.GetAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestAsync_WithMultipleCheckpoints_ReturnsLatest()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();
        var checkpoint1 = new Checkpoint<TestState>
        {
            Id = "checkpoint-1",
            RunId = "run-1",
            NodeName = "node-1",
            SerializedState = "{}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        var checkpoint2 = new Checkpoint<TestState>
        {
            Id = "checkpoint-2",
            RunId = "run-1",
            NodeName = "node-2",
            SerializedState = "{}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var checkpoint3 = new Checkpoint<TestState>
        {
            Id = "checkpoint-3",
            RunId = "run-1",
            NodeName = "node-3",
            SerializedState = "{}",
            CreatedAt = DateTime.UtcNow
        };

        await store.SaveAsync(checkpoint1);
        await store.SaveAsync(checkpoint2);
        await store.SaveAsync(checkpoint3);

        // Act
        var result = await store.GetLatestAsync("run-1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("checkpoint-3");
    }

    [Fact]
    public async Task GetLatestAsync_WithNoCheckpoints_ReturnsNull()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();

        // Act
        var result = await store.GetLatestAsync("run-1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_WithMultipleCheckpoints_ReturnsAllOrdered()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();
        var checkpoint1 = new Checkpoint<TestState>
        {
            Id = "checkpoint-1",
            RunId = "run-1",
            NodeName = "node-1",
            SerializedState = "{}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        var checkpoint2 = new Checkpoint<TestState>
        {
            Id = "checkpoint-2",
            RunId = "run-1",
            NodeName = "node-2",
            SerializedState = "{}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        await store.SaveAsync(checkpoint1);
        await store.SaveAsync(checkpoint2);

        // Act
        var result = await store.ListAsync("run-1");

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("checkpoint-1");
        result[1].Id.Should().Be("checkpoint-2");
    }

    [Fact]
    public async Task ListAsync_WithNoCheckpoints_ReturnsEmptyList()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();

        // Act
        var result = await store.ListAsync("run-1");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingCheckpoint_RemovesCheckpoint()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();
        var checkpoint = new Checkpoint<TestState>
        {
            Id = "checkpoint-1",
            RunId = "run-1",
            NodeName = "node-1",
            SerializedState = "{}"
        };
        await store.SaveAsync(checkpoint);

        // Act
        await store.DeleteAsync("checkpoint-1");

        // Assert
        var result = await store.GetAsync("checkpoint-1");
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteOlderThanAsync_WithOldCheckpoints_DeletesOnlyOldOnes()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();
        var oldCheckpoint = new Checkpoint<TestState>
        {
            Id = "checkpoint-1",
            RunId = "run-1",
            NodeName = "node-1",
            SerializedState = "{}",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        var newCheckpoint = new Checkpoint<TestState>
        {
            Id = "checkpoint-2",
            RunId = "run-1",
            NodeName = "node-2",
            SerializedState = "{}",
            CreatedAt = DateTime.UtcNow
        };

        await store.SaveAsync(oldCheckpoint);
        await store.SaveAsync(newCheckpoint);

        // Act
        var deletedCount = await store.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-1));

        // Assert
        deletedCount.Should().Be(1);
        (await store.GetAsync("checkpoint-1")).Should().BeNull();
        (await store.GetAsync("checkpoint-2")).Should().NotBeNull();
    }

    [Fact]
    public async Task SaveAsync_WithNullCheckpoint_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.SaveAsync(null!));
    }

    [Fact]
    public async Task SaveAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();
        var checkpoint = new Checkpoint<TestState>
        {
            Id = string.Empty,
            RunId = "run-1",
            NodeName = "node-1",
            SerializedState = "{}"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => store.SaveAsync(checkpoint));
    }

    [Fact]
    public async Task GetAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var store = new InMemoryCheckpointStore<TestState>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => store.GetAsync(string.Empty));
    }

    private record TestState(int Value);
}
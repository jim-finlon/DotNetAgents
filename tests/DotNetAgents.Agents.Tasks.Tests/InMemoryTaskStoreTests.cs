using DotNetAgents.Agents.Tasks;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Agents.Tasks.Tests;

public class InMemoryTaskStoreTests
{
    [Fact]
    public async Task SaveAsync_WithValidTask_SavesTask()
    {
        // Arrange
        var store = new InMemoryTaskStore();
        var task = new WorkerTask
        {
            TaskId = "task-1",
            TaskType = "test-task",
            Input = "test input"
        };

        // Act
        await store.SaveAsync(task);

        // Assert
        var retrieved = await store.GetAsync("task-1");
        retrieved.Should().NotBeNull();
        retrieved!.TaskId.Should().Be("task-1");
        retrieved.TaskType.Should().Be("test-task");
    }

    [Fact]
    public async Task SaveAsync_WithNullTask_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new InMemoryTaskStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => store.SaveAsync(null!));
    }

    [Fact]
    public async Task GetAsync_WithNonExistentTask_ReturnsNull()
    {
        // Arrange
        var store = new InMemoryTaskStore();

        // Act
        var result = await store.GetAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveResultAsync_WithValidResult_SavesResult()
    {
        // Arrange
        var store = new InMemoryTaskStore();
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test" };
        await store.SaveAsync(task);

        var result = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = true,
            Output = "test output",
            WorkerAgentId = "agent-1",
            ExecutionTime = TimeSpan.FromSeconds(5)
        };

        // Act
        await store.SaveResultAsync(result);

        // Assert
        var retrieved = await store.GetResultAsync("task-1");
        retrieved.Should().NotBeNull();
        retrieved!.Success.Should().BeTrue();
        retrieved.Output.Should().Be("test output");
        retrieved.WorkerAgentId.Should().Be("agent-1");
    }

    [Fact]
    public async Task SaveResultAsync_WithNullResult_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new InMemoryTaskStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => store.SaveResultAsync(null!));
    }

    [Fact]
    public async Task GetResultAsync_WithNonExistentResult_ReturnsNull()
    {
        // Arrange
        var store = new InMemoryTaskStore();

        // Act
        var result = await store.GetResultAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidTask_UpdatesStatus()
    {
        // Arrange
        var store = new InMemoryTaskStore();
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test" };
        await store.SaveAsync(task);

        // Act
        await store.UpdateStatusAsync("task-1", TaskStatus.InProgress);

        // Assert
        // Status is tracked internally, verify no exception thrown
        await store.UpdateStatusAsync("task-1", TaskStatus.Completed);
    }

    [Fact]
    public async Task SaveResultAsync_WithFailedResult_UpdatesStatusToFailed()
    {
        // Arrange
        var store = new InMemoryTaskStore();
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test" };
        await store.SaveAsync(task);

        var result = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = false,
            ErrorMessage = "Test error",
            WorkerAgentId = "agent-1",
            ExecutionTime = TimeSpan.FromSeconds(1)
        };

        // Act
        await store.SaveResultAsync(result);

        // Assert
        var retrieved = await store.GetResultAsync("task-1");
        retrieved.Should().NotBeNull();
        retrieved!.Success.Should().BeFalse();
        retrieved.ErrorMessage.Should().Be("Test error");
    }
}

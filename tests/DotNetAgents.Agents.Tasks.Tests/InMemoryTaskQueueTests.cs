using DotNetAgents.Agents.Tasks;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Agents.Tasks.Tests;

public class InMemoryTaskQueueTests
{
    [Fact]
    public async Task EnqueueAsync_WithValidTask_EnqueuesTask()
    {
        // Arrange
        var queue = new InMemoryTaskQueue();
        var task = new WorkerTask
        {
            TaskId = "task-1",
            TaskType = "test-task",
            Input = "test input",
            Priority = 5
        };

        // Act
        await queue.EnqueueAsync(task);

        // Assert
        var pendingCount = await queue.GetPendingCountAsync();
        pendingCount.Should().Be(1);
    }

    [Fact]
    public async Task EnqueueAsync_WithNullTask_ThrowsArgumentNullException()
    {
        // Arrange
        var queue = new InMemoryTaskQueue();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => queue.EnqueueAsync(null!));
    }

    [Fact]
    public async Task DequeueAsync_WithEmptyQueue_ReturnsNull()
    {
        // Arrange
        var queue = new InMemoryTaskQueue();

        // Act
        var result = await queue.DequeueAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DequeueAsync_WithQueuedTasks_ReturnsHighestPriorityTask()
    {
        // Arrange
        var queue = new InMemoryTaskQueue();
        var task1 = new WorkerTask { TaskId = "task-1", TaskType = "test", Priority = 1 };
        var task2 = new WorkerTask { TaskId = "task-2", TaskType = "test", Priority = 10 };
        var task3 = new WorkerTask { TaskId = "task-3", TaskType = "test", Priority = 5 };

        await queue.EnqueueAsync(task1);
        await queue.EnqueueAsync(task2);
        await queue.EnqueueAsync(task3);

        // Act
        var result = await queue.DequeueAsync();

        // Assert
        result.Should().NotBeNull();
        result!.TaskId.Should().Be("task-2"); // Highest priority
    }

    [Fact]
    public async Task DequeueAsync_WithSamePriority_ReturnsOldestTask()
    {
        // Arrange
        var queue = new InMemoryTaskQueue();
        var task1 = new WorkerTask { TaskId = "task-1", TaskType = "test", Priority = 5 };
        await Task.Delay(10); // Small delay
        var task2 = new WorkerTask { TaskId = "task-2", TaskType = "test", Priority = 5 };

        await queue.EnqueueAsync(task1);
        await queue.EnqueueAsync(task2);

        // Act
        var result = await queue.DequeueAsync();

        // Assert
        result.Should().NotBeNull();
        result!.TaskId.Should().Be("task-1"); // Older task
    }

    [Fact]
    public async Task DequeueAsync_WithAgentId_ReturnsMatchingTask()
    {
        // Arrange
        var queue = new InMemoryTaskQueue();
        var task1 = new WorkerTask
        {
            TaskId = "task-1",
            TaskType = "test",
            PreferredAgentId = "agent-1",
            Priority = 5
        };
        var task2 = new WorkerTask
        {
            TaskId = "task-2",
            TaskType = "test",
            PreferredAgentId = "agent-2",
            Priority = 5
        };

        await queue.EnqueueAsync(task1);
        await queue.EnqueueAsync(task2);

        // Act
        var result = await queue.DequeueAsync("agent-1");

        // Assert
        result.Should().NotBeNull();
        result!.TaskId.Should().Be("task-1");
    }

    [Fact]
    public async Task GetPendingCountAsync_WithMultipleTasks_ReturnsCorrectCount()
    {
        // Arrange
        var queue = new InMemoryTaskQueue();
        await queue.EnqueueAsync(new WorkerTask { TaskId = "task-1", TaskType = "test" });
        await queue.EnqueueAsync(new WorkerTask { TaskId = "task-2", TaskType = "test" });
        await queue.EnqueueAsync(new WorkerTask { TaskId = "task-3", TaskType = "test" });

        // Act
        var count = await queue.GetPendingCountAsync();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task PeekAsync_WithQueuedTasks_ReturnsTaskWithoutRemoving()
    {
        // Arrange
        var queue = new InMemoryTaskQueue();
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test", Priority = 10 };
        await queue.EnqueueAsync(task);

        // Act
        var peeked = await queue.PeekAsync();
        var countAfterPeek = await queue.GetPendingCountAsync();
        var dequeued = await queue.DequeueAsync();

        // Assert
        peeked.Should().NotBeNull();
        peeked!.TaskId.Should().Be("task-1");
        countAfterPeek.Should().Be(1); // Still in queue
        dequeued.Should().NotBeNull();
        dequeued!.TaskId.Should().Be("task-1");
    }
}

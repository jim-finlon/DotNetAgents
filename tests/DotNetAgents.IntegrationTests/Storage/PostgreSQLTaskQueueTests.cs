using DotNetAgents.Agents.Tasks;
using DotNetAgents.Storage.Agents.PostgreSQL;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using TaskStatus = DotNetAgents.Agents.Tasks.TaskStatus;

namespace DotNetAgents.IntegrationTests.Storage;

/// <summary>
/// Integration tests for PostgreSQLTaskQueue.
/// Requires PostgreSQL database to be available.
/// </summary>
[Collection("PostgreSQL")]
public class PostgreSQLTaskQueueTests : IClassFixture<PostgreSQLFixture>
{
    private readonly PostgreSQLFixture _fixture;
    private readonly ILogger<PostgreSQLTaskQueue> _logger;

    public PostgreSQLTaskQueueTests(PostgreSQLFixture fixture)
    {
        _fixture = fixture;
        _logger = _fixture.LoggerFactory.CreateLogger<PostgreSQLTaskQueue>();
    }

    [Fact]
    public async Task EnqueueAsync_WithValidTask_EnqueuesTask()
    {
        // Arrange
        var queue = new PostgreSQLTaskQueue(_fixture.ConnectionString, _logger);
        var task = new WorkerTask
        {
            TaskId = "task-1",
            TaskType = "test-task",
            Input = new Dictionary<string, object> { ["data"] = "test" }
        };

        // Act
        await queue.EnqueueAsync(task);

        // Assert
        var pendingCount = await queue.GetPendingCountAsync();
        pendingCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DequeueAsync_WithEnqueuedTask_ReturnsTask()
    {
        // Arrange
        var queue = new PostgreSQLTaskQueue(_fixture.ConnectionString, _logger);
        var task = new WorkerTask
        {
            TaskId = "task-2",
            TaskType = "test-task",
            Input = new Dictionary<string, object> { ["data"] = "test" },
            Priority = 5
        };
        await queue.EnqueueAsync(task);

        // Act
        var dequeued = await queue.DequeueAsync();

        // Assert
        dequeued.Should().NotBeNull();
        dequeued!.TaskId.Should().Be("task-2");
        dequeued.TaskType.Should().Be("test-task");
    }

    [Fact]
    public async Task DequeueAsync_WithEmptyQueue_ReturnsNull()
    {
        // Arrange
        var queue = new PostgreSQLTaskQueue(_fixture.ConnectionString, _logger);

        // Act
        var dequeued = await queue.DequeueAsync();

        // Assert
        dequeued.Should().BeNull();
    }

    [Fact]
    public async Task DequeueAsync_WithPriorityOrder_ReturnsHighestPriorityFirst()
    {
        // Arrange
        var queue = new PostgreSQLTaskQueue(_fixture.ConnectionString, _logger);
        var task1 = new WorkerTask
        {
            TaskId = "task-low",
            TaskType = "test-task",
            Priority = 1
        };
        var task2 = new WorkerTask
        {
            TaskId = "task-high",
            TaskType = "test-task",
            Priority = 10
        };
        await queue.EnqueueAsync(task1);
        await queue.EnqueueAsync(task2);

        // Act
        var dequeued = await queue.DequeueAsync();

        // Assert
        dequeued.Should().NotBeNull();
        dequeued!.TaskId.Should().Be("task-high"); // Higher priority should be dequeued first
    }

    [Fact]
    public async Task GetPendingCountAsync_WithEnqueuedTasks_ReturnsCorrectCount()
    {
        // Arrange
        var queue = new PostgreSQLTaskQueue(_fixture.ConnectionString, _logger);
        var task1 = new WorkerTask
        {
            TaskId = "task-3",
            TaskType = "test-task"
        };
        var task2 = new WorkerTask
        {
            TaskId = "task-4",
            TaskType = "test-task"
        };
        await queue.EnqueueAsync(task1);
        await queue.EnqueueAsync(task2);

        // Act
        var count = await queue.GetPendingCountAsync();

        // Assert
        count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task PeekAsync_WithEnqueuedTask_ReturnsTaskWithoutRemoving()
    {
        // Arrange
        var queue = new PostgreSQLTaskQueue(_fixture.ConnectionString, _logger);
        var task = new WorkerTask
        {
            TaskId = "task-5",
            TaskType = "test-task"
        };
        await queue.EnqueueAsync(task);

        // Act
        var peeked = await queue.PeekAsync();
        var countAfterPeek = await queue.GetPendingCountAsync();
        var dequeued = await queue.DequeueAsync();

        // Assert
        peeked.Should().NotBeNull();
        peeked!.TaskId.Should().Be("task-5");
        countAfterPeek.Should().BeGreaterThan(0); // Task should still be in queue
        dequeued.Should().NotBeNull(); // Task should still be available for dequeue
    }
}

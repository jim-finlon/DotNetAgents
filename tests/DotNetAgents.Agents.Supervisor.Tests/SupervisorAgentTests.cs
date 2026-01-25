using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Supervisor;
using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.WorkerPool;
using FluentAssertions;
using Moq;
using Xunit;
using TaskStatus = DotNetAgents.Agents.Tasks.TaskStatus;

namespace DotNetAgents.Agents.Supervisor.Tests;

public class SupervisorAgentTests
{
    private readonly Mock<IAgentRegistry> _mockRegistry;
    private readonly Mock<IAgentMessageBus> _mockMessageBus;
    private readonly Mock<ITaskQueue> _mockTaskQueue;
    private readonly Mock<ITaskStore> _mockTaskStore;
    private readonly Mock<IWorkerPool> _mockWorkerPool;
    private readonly SupervisorAgent _supervisor;

    public SupervisorAgentTests()
    {
        _mockRegistry = new Mock<IAgentRegistry>();
        _mockMessageBus = new Mock<IAgentMessageBus>();
        _mockTaskQueue = new Mock<ITaskQueue>();
        _mockTaskStore = new Mock<ITaskStore>();
        _mockWorkerPool = new Mock<IWorkerPool>();

        _supervisor = new SupervisorAgent(
            _mockRegistry.Object,
            _mockMessageBus.Object,
            _mockTaskQueue.Object,
            _mockTaskStore.Object,
            _mockWorkerPool.Object);
    }

    [Fact]
    public async Task SubmitTaskAsync_WithValidTask_ReturnsTaskId()
    {
        // Arrange
        var task = new WorkerTask
        {
            TaskId = "task-1",
            TaskType = "test-task",
            Input = "test input"
        };

        _mockTaskQueue.Setup(q => q.EnqueueAsync(It.IsAny<WorkerTask>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var taskId = await _supervisor.SubmitTaskAsync(task);

        // Assert
        taskId.Should().Be("task-1");
        _mockTaskStore.Verify(s => s.SaveAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        _mockTaskQueue.Verify(q => q.EnqueueAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitTaskAsync_WithNullTask_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _supervisor.SubmitTaskAsync(null!));
    }

    [Fact]
    public async Task SubmitTasksAsync_WithMultipleTasks_ReturnsAllTaskIds()
    {
        // Arrange
        var tasks = new[]
        {
            new WorkerTask { TaskId = "task-1", TaskType = "test" },
            new WorkerTask { TaskId = "task-2", TaskType = "test" },
            new WorkerTask { TaskId = "task-3", TaskType = "test" }
        };

        _mockTaskQueue.Setup(q => q.EnqueueAsync(It.IsAny<WorkerTask>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var taskIds = await _supervisor.SubmitTasksAsync(tasks);

        // Assert
        taskIds.Should().HaveCount(3);
        taskIds.Select(t => t).Should().Contain(new[] { "task-1", "task-2", "task-3" });
    }

    [Fact]
    public async Task GetTaskStatusAsync_WithCompletedTask_ReturnsCompleted()
    {
        // Arrange
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test" };
        var result = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = true,
            Output = "result"
        };

        _mockTaskStore.Setup(s => s.GetAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskStore.Setup(s => s.GetResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var status = await _supervisor.GetTaskStatusAsync("task-1");

        // Assert
        status.Should().Be(TaskStatus.Completed);
    }

    [Fact]
    public async Task GetTaskStatusAsync_WithFailedTask_ReturnsFailed()
    {
        // Arrange
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test" };
        var result = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = false,
            ErrorMessage = "Error"
        };

        _mockTaskStore.Setup(s => s.GetAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskStore.Setup(s => s.GetResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var status = await _supervisor.GetTaskStatusAsync("task-1");

        // Assert
        status.Should().Be(TaskStatus.Failed);
    }

    [Fact]
    public async Task GetTaskStatusAsync_WithPendingTask_ReturnsPending()
    {
        // Arrange
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test" };

        _mockTaskStore.Setup(s => s.GetAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskStore.Setup(s => s.GetResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkerTaskResult?)null);

        // Act
        var status = await _supervisor.GetTaskStatusAsync("task-1");

        // Assert
        status.Should().Be(TaskStatus.Pending);
    }

    [Fact]
    public async Task GetTaskResultAsync_WithCompletedTask_ReturnsResult()
    {
        // Arrange
        var result = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = true,
            Output = "test output",
            WorkerAgentId = "agent-1"
        };

        _mockTaskStore.Setup(s => s.GetResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var retrieved = await _supervisor.GetTaskResultAsync("task-1");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Success.Should().BeTrue();
        retrieved.Output.Should().Be("test output");
    }

    [Fact]
    public async Task CancelTaskAsync_WithPendingTask_ReturnsTrue()
    {
        // Arrange
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test" };

        _mockTaskStore.Setup(s => s.GetAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskStore.Setup(s => s.GetResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkerTaskResult?)null);

        // Act
        var cancelled = await _supervisor.CancelTaskAsync("task-1");

        // Assert
        cancelled.Should().BeTrue();
        _mockTaskStore.Verify(s => s.UpdateStatusAsync("task-1", TaskStatus.Cancelled, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelTaskAsync_WithCompletedTask_ReturnsFalse()
    {
        // Arrange
        var task = new WorkerTask { TaskId = "task-1", TaskType = "test" };
        var result = new WorkerTaskResult { TaskId = "task-1", Success = true };

        _mockTaskStore.Setup(s => s.GetAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskStore.Setup(s => s.GetResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var cancelled = await _supervisor.CancelTaskAsync("task-1");

        // Assert
        cancelled.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        _mockTaskQueue.Setup(q => q.GetPendingCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var stats = await _supervisor.GetStatisticsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.TasksPending.Should().Be(5);
    }
}

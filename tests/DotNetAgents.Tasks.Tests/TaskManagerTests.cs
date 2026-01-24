using DotNetAgents.Tasks.Models;
using DotNetAgents.Tasks.Storage;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskStatus = DotNetAgents.Tasks.Models.TaskStatus;

namespace DotNetAgents.Tasks.Tests;

public class TaskManagerTests
{
    private readonly Mock<ITaskStore> _taskStoreMock;
    private readonly Mock<ILogger<TaskManager>> _loggerMock;
    private readonly TaskManager _taskManager;

    public TaskManagerTests()
    {
        _taskStoreMock = new Mock<ITaskStore>();
        _loggerMock = new Mock<ILogger<TaskManager>>();
        _taskManager = new TaskManager(_taskStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidTask_ReturnsCreatedTask()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Test task",
            Priority = TaskPriority.High
        };

        var createdTask = task with { Id = Guid.NewGuid() };
        _taskStoreMock.Setup(x => x.CreateAsync(task, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _taskManager.CreateTaskAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.SessionId.Should().Be("session-123");
        result.Content.Should().Be("Test task");
        _taskStoreMock.Verify(x => x.CreateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_WithNullTask_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _taskManager.CreateTaskAsync(null!));
    }

    [Fact]
    public async Task CreateTaskAsync_WithEmptySessionId_ThrowsArgumentException()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = string.Empty,
            Content = "Test task"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _taskManager.CreateTaskAsync(task));
    }

    [Fact]
    public async Task GetTaskAsync_WithValidId_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new WorkTask
        {
            Id = taskId,
            SessionId = "session-123",
            Content = "Test task"
        };

        _taskStoreMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await _taskManager.GetTaskAsync(taskId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
        _taskStoreMock.Verify(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTaskAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskStoreMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkTask?)null);

        // Act
        var result = await _taskManager.GetTaskAsync(taskId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTaskAsync_WithValidTask_ReturnsUpdatedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var existingTask = new WorkTask
        {
            Id = taskId,
            SessionId = "session-123",
            Content = "Original task",
            Status = TaskStatus.Pending
        };

        var updatedTask = existingTask with
        {
            Content = "Updated task",
            Status = TaskStatus.InProgress
        };

        _taskStoreMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);
        _taskStoreMock.Setup(x => x.UpdateAsync(updatedTask, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _taskManager.UpdateTaskAsync(updatedTask);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("Updated task");
        result.Status.Should().Be(TaskStatus.InProgress);
        _taskStoreMock.Verify(x => x.UpdateAsync(updatedTask, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTasksBySessionAsync_WithValidSessionId_ReturnsTasks()
    {
        // Arrange
        var sessionId = "session-123";
        var tasks = new List<WorkTask>
        {
            new WorkTask { Id = Guid.NewGuid(), SessionId = sessionId, Content = "Task 1" },
            new WorkTask { Id = Guid.NewGuid(), SessionId = sessionId, Content = "Task 2" }
        };

        _taskStoreMock.Setup(x => x.GetBySessionIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskManager.GetTasksBySessionAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        _taskStoreMock.Verify(x => x.GetBySessionIdAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTaskStatisticsAsync_WithValidSessionId_ReturnsStatistics()
    {
        // Arrange
        var sessionId = "session-123";
        var stats = new TaskStatistics
        {
            Total = 10,
            Completed = 5,
            Pending = 3,
            InProgress = 2
        };

        _taskStoreMock.Setup(x => x.GetStatisticsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _taskManager.GetTaskStatisticsAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(10);
        result.Completed.Should().Be(5);
        result.CompletionPercentage.Should().Be(50.0);
    }
}

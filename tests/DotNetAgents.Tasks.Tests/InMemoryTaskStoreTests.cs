using DotNetAgents.Tasks.Models;
using DotNetAgents.Tasks.Storage;
using FluentAssertions;
using TaskStatus = DotNetAgents.Tasks.Models.TaskStatus;

namespace DotNetAgents.Tasks.Tests;

public class InMemoryTaskStoreTests
{
    private readonly InMemoryTaskStore _store;

    public InMemoryTaskStoreTests()
    {
        _store = new InMemoryTaskStore();
    }

    [Fact]
    public async Task CreateAsync_WithValidTask_CreatesTask()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Test task",
            Priority = TaskPriority.High
        };

        // Act
        var result = await _store.CreateAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.SessionId.Should().Be("session-123");
        result.Content.Should().Be("Test task");
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_WithDefaultId_GeneratesNewId()
    {
        // Arrange
        var task = new WorkTask
        {
            Id = default,
            SessionId = "session-123",
            Content = "Test task"
        };

        // Act
        var result = await _store.CreateAsync(task);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_WithZeroOrder_AutoAssignsOrder()
    {
        // Arrange
        var task1 = new WorkTask { SessionId = "session-123", Content = "Task 1", Order = 0 };
        var task2 = new WorkTask { SessionId = "session-123", Content = "Task 2", Order = 0 };

        // Act
        var result1 = await _store.CreateAsync(task1);
        var result2 = await _store.CreateAsync(task2);

        // Assert
        result1.Order.Should().Be(0);
        result2.Order.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsTask()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Test task"
        };
        var created = await _store.CreateAsync(task);

        // Act
        var result = await _store.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Content.Should().Be("Test task");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = await _store.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidTask_UpdatesTask()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Original task",
            Status = TaskStatus.Pending
        };
        var created = await _store.CreateAsync(task);

        var updated = created with
        {
            Content = "Updated task",
            Status = TaskStatus.InProgress
        };

        // Act
        var result = await _store.UpdateAsync(updated);

        // Assert
        result.Content.Should().Be("Updated task");
        result.Status.Should().Be(TaskStatus.InProgress);
        result.UpdatedAt.Should().BeAfter(created.UpdatedAt);

        var retrieved = await _store.GetByIdAsync(created.Id);
        retrieved!.Content.Should().Be("Updated task");
    }

    [Fact]
    public async Task UpdateAsync_WhenStatusChangesToInProgress_SetsStartedAt()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Test task",
            Status = TaskStatus.Pending
        };
        var created = await _store.CreateAsync(task);

        var updated = created with { Status = TaskStatus.InProgress };

        // Act
        var result = await _store.UpdateAsync(updated);

        // Assert
        result.StartedAt.Should().NotBeNull();
        result.StartedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_WhenStatusChangesToCompleted_SetsCompletedAt()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Test task",
            Status = TaskStatus.Pending
        };
        var created = await _store.CreateAsync(task);

        var updated = created with { Status = TaskStatus.Completed };

        // Act
        var result = await _store.UpdateAsync(updated);

        // Assert
        result.CompletedAt.Should().NotBeNull();
        result.CompletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_DeletesTask()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Test task"
        };
        var created = await _store.CreateAsync(task);

        // Act
        await _store.DeleteAsync(created.Id);

        // Assert
        var result = await _store.GetByIdAsync(created.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySessionIdAsync_WithValidSessionId_ReturnsTasks()
    {
        // Arrange
        var sessionId = "session-123";
        var task1 = new WorkTask { SessionId = sessionId, Content = "Task 1", Order = 0 };
        var task2 = new WorkTask { SessionId = sessionId, Content = "Task 2", Order = 1 };
        var task3 = new WorkTask { SessionId = "session-456", Content = "Task 3", Order = 0 };

        await _store.CreateAsync(task1);
        await _store.CreateAsync(task2);
        await _store.CreateAsync(task3);

        // Act
        var result = await _store.GetBySessionIdAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Should().OnlyContain(t => t.SessionId == sessionId);
        result.Should().BeInAscendingOrder(t => t.Order);
    }

    [Fact]
    public async Task GetByStatusAsync_WithValidStatus_ReturnsFilteredTasks()
    {
        // Arrange
        var sessionId = "session-123";
        var task1 = new WorkTask { SessionId = sessionId, Content = "Task 1", Status = TaskStatus.Pending };
        var task2 = new WorkTask { SessionId = sessionId, Content = "Task 2", Status = TaskStatus.InProgress };
        var task3 = new WorkTask { SessionId = sessionId, Content = "Task 3", Status = TaskStatus.Pending };

        await _store.CreateAsync(task1);
        await _store.CreateAsync(task2);
        await _store.CreateAsync(task3);

        // Act
        var result = await _store.GetByStatusAsync(sessionId, TaskStatus.Pending);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Should().OnlyContain(t => t.Status == TaskStatus.Pending);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithValidSessionId_ReturnsStatistics()
    {
        // Arrange
        var sessionId = "session-123";
        await _store.CreateAsync(new WorkTask { SessionId = sessionId, Content = "Task 1", Status = TaskStatus.Pending });
        await _store.CreateAsync(new WorkTask { SessionId = sessionId, Content = "Task 2", Status = TaskStatus.InProgress });
        await _store.CreateAsync(new WorkTask { SessionId = sessionId, Content = "Task 3", Status = TaskStatus.Completed });
        await _store.CreateAsync(new WorkTask { SessionId = sessionId, Content = "Task 4", Status = TaskStatus.Completed });

        // Act
        var result = await _store.GetStatisticsAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(4);
        result.Pending.Should().Be(1);
        result.InProgress.Should().Be(1);
        result.Completed.Should().Be(2);
        result.CompletionPercentage.Should().Be(50.0);
    }

    [Fact]
    public async Task AreDependenciesCompleteAsync_WithNoDependencies_ReturnsTrue()
    {
        // Arrange
        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Task",
            DependsOn = Array.Empty<Guid>()
        };
        var created = await _store.CreateAsync(task);

        // Act
        var result = await _store.AreDependenciesCompleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AreDependenciesCompleteAsync_WithCompletedDependencies_ReturnsTrue()
    {
        // Arrange
        var depTask = new WorkTask
        {
            SessionId = "session-123",
            Content = "Dependency",
            Status = TaskStatus.Completed
        };
        var depCreated = await _store.CreateAsync(depTask);

        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Task",
            DependsOn = new[] { depCreated.Id }
        };
        var created = await _store.CreateAsync(task);

        // Act
        var result = await _store.AreDependenciesCompleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AreDependenciesCompleteAsync_WithIncompleteDependencies_ReturnsFalse()
    {
        // Arrange
        var depTask = new WorkTask
        {
            SessionId = "session-123",
            Content = "Dependency",
            Status = TaskStatus.Pending
        };
        var depCreated = await _store.CreateAsync(depTask);

        var task = new WorkTask
        {
            SessionId = "session-123",
            Content = "Task",
            DependsOn = new[] { depCreated.Id }
        };
        var created = await _store.CreateAsync(task);

        // Act
        var result = await _store.AreDependenciesCompleteAsync(created.Id);

        // Assert
        result.Should().BeFalse();
    }
}

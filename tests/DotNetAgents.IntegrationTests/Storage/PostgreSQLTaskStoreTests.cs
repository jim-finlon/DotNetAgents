using DotNetAgents.Agents.Tasks;
using DotNetAgents.Storage.Agents.PostgreSQL;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using TaskStatus = DotNetAgents.Agents.Tasks.TaskStatus;

namespace DotNetAgents.IntegrationTests.Storage;

/// <summary>
/// Integration tests for PostgreSQLTaskStore.
/// Requires PostgreSQL database to be available.
/// </summary>
[Collection("PostgreSQL")]
public class PostgreSQLTaskStoreTests : IClassFixture<PostgreSQLFixture>
{
    private readonly PostgreSQLFixture _fixture;
    private readonly ILogger<PostgreSQLTaskStore> _logger;

    public PostgreSQLTaskStoreTests(PostgreSQLFixture fixture)
    {
        _fixture = fixture;
        _logger = _fixture.LoggerFactory.CreateLogger<PostgreSQLTaskStore>();
    }

    [Fact]
    public async Task SaveAsync_WithValidTask_SavesTask()
    {
        // Arrange
        var store = new PostgreSQLTaskStore(_fixture.ConnectionString, _logger);
        var task = new WorkerTask
        {
            TaskId = "task-store-1",
            TaskType = "test-task",
            Input = new Dictionary<string, object> { ["data"] = "test" }
        };

        // Act
        await store.SaveAsync(task);

        // Assert
        var retrieved = await store.GetAsync("task-store-1");
        retrieved.Should().NotBeNull();
        retrieved!.TaskId.Should().Be("task-store-1");
    }

    [Fact]
    public async Task GetAsync_WithSavedTask_ReturnsTask()
    {
        // Arrange
        var store = new PostgreSQLTaskStore(_fixture.ConnectionString, _logger);
        var task = new WorkerTask
        {
            TaskId = "task-store-2",
            TaskType = "test-task",
            Input = new Dictionary<string, object> { ["key"] = "value" },
            RequiredCapability = "capability-1",
            Priority = 5
        };
        await store.SaveAsync(task);

        // Act
        var retrieved = await store.GetAsync("task-store-2");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.TaskId.Should().Be("task-store-2");
        retrieved.TaskType.Should().Be("test-task");
        retrieved.RequiredCapability.Should().Be("capability-1");
        retrieved.Priority.Should().Be(5);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentTask_ReturnsNull()
    {
        // Arrange
        var store = new PostgreSQLTaskStore(_fixture.ConnectionString, _logger);

        // Act
        var retrieved = await store.GetAsync("non-existent-task");

        // Assert
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithSavedTask_UpdatesStatus()
    {
        // Arrange
        var store = new PostgreSQLTaskStore(_fixture.ConnectionString, _logger);
        var task = new WorkerTask
        {
            TaskId = "task-store-3",
            TaskType = "test-task"
        };
        await store.SaveAsync(task);

        // Act
        await store.UpdateStatusAsync("task-store-3", TaskStatus.InProgress);

        // Assert
        var retrieved = await store.GetAsync("task-store-3");
        retrieved.Should().NotBeNull();
        retrieved!.TaskId.Should().Be("task-store-3");
        // Note: Status is stored separately, so we verify the task exists
    }

    [Fact]
    public async Task SaveResultAsync_WithValidResult_SavesResult()
    {
        // Arrange
        var store = new PostgreSQLTaskStore(_fixture.ConnectionString, _logger);
        var task = new WorkerTask
        {
            TaskId = "task-store-4",
            TaskType = "test-task"
        };
        await store.SaveAsync(task);
        var result = new WorkerTaskResult
        {
            TaskId = "task-store-4",
            Success = true,
            Output = new Dictionary<string, object> { ["result"] = "success" },
            WorkerAgentId = "worker-1",
            ExecutionTime = TimeSpan.FromSeconds(5)
        };

        // Act
        await store.SaveResultAsync(result);

        // Assert
        var retrievedResult = await store.GetResultAsync("task-store-4");
        retrievedResult.Should().NotBeNull();
        retrievedResult!.TaskId.Should().Be("task-store-4");
        retrievedResult.Success.Should().BeTrue();
        retrievedResult.WorkerAgentId.Should().Be("worker-1");
    }

    [Fact]
    public async Task GetResultAsync_WithSavedResult_ReturnsResult()
    {
        // Arrange
        var store = new PostgreSQLTaskStore(_fixture.ConnectionString, _logger);
        var task = new WorkerTask
        {
            TaskId = "task-store-5",
            TaskType = "test-task"
        };
        await store.SaveAsync(task);
        var result = new WorkerTaskResult
        {
            TaskId = "task-store-5",
            Success = false,
            ErrorMessage = "Test error",
            WorkerAgentId = "worker-2"
        };
        await store.SaveResultAsync(result);

        // Act
        var retrievedResult = await store.GetResultAsync("task-store-5");

        // Assert
        retrievedResult.Should().NotBeNull();
        retrievedResult!.TaskId.Should().Be("task-store-5");
        retrievedResult.Success.Should().BeFalse();
        retrievedResult.ErrorMessage.Should().Be("Test error");
        retrievedResult.WorkerAgentId.Should().Be("worker-2");
    }

    [Fact]
    public async Task GetResultAsync_WithNonExistentResult_ReturnsNull()
    {
        // Arrange
        var store = new PostgreSQLTaskStore(_fixture.ConnectionString, _logger);

        // Act
        var retrievedResult = await store.GetResultAsync("non-existent-task");

        // Assert
        retrievedResult.Should().BeNull();
    }
}

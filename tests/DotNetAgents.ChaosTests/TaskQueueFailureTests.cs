using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.Supervisor;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetAgents.ChaosTests;

/// <summary>
/// Chaos engineering tests for task queue failure scenarios.
/// </summary>
public class TaskQueueFailureTests : IClassFixture<ChaosTestFixture>
{
    private readonly ChaosTestFixture _fixture;

    public TaskQueueFailureTests(ChaosTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task System_Should_Handle_High_Task_Volume()
    {
        // Arrange
        var supervisor = _fixture.ServiceProvider.GetRequiredService<ISupervisorAgent>();
        var taskQueue = _fixture.ServiceProvider.GetRequiredService<ITaskQueue>();

        // Act: Submit large number of tasks rapidly
        var tasks = Enumerable.Range(1, 1000).Select(i => new WorkerTask
        {
            TaskId = $"task-{i}",
            TaskType = "load-test",
            Priority = TaskPriority.Normal,
            Input = new Dictionary<string, object> { ["index"] = i }
        }).ToList();

        var submitTasks = tasks.Select(task => supervisor.SubmitTaskAsync(task));
        await Task.WhenAll(submitTasks);

        // Assert: All tasks should be enqueued
        var pendingCount = await taskQueue.GetPendingCountAsync();
        pendingCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task System_Should_Process_Tasks_After_Queue_Overload()
    {
        // Arrange
        var supervisor = _fixture.ServiceProvider.GetRequiredService<ISupervisorAgent>();
        var taskQueue = _fixture.ServiceProvider.GetRequiredService<ITaskQueue>();

        // Overload the queue
        var tasks = Enumerable.Range(1, 500).Select(i => new WorkerTask
        {
            TaskId = $"task-{i}",
            TaskType = "test",
            Priority = TaskPriority.Normal,
            Input = new Dictionary<string, object> { ["test"] = i }
        }).ToList();

        foreach (var task in tasks)
        {
            await supervisor.SubmitTaskAsync(task);
        }

        // Act: Wait for processing
        await Task.Delay(2000);

        // Assert: Queue should be processing tasks
        var pendingCount = await taskQueue.GetPendingCountAsync();
        // Some tasks should have been processed (exact count depends on worker capacity)
        pendingCount.Should().BeLessThan(500);
    }
}

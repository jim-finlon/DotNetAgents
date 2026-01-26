using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.Supervisor;
using DotNetAgents.Agents.WorkerPool;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DotNetAgents.ChaosTests;

/// <summary>
/// Chaos engineering tests for agent failure scenarios.
/// </summary>
public class AgentFailureTests : IClassFixture<ChaosTestFixture>
{
    private readonly ChaosTestFixture _fixture;

    public AgentFailureTests(ChaosTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task System_Should_Continue_When_Agent_Fails()
    {
        // Arrange
        var supervisor = _fixture.ServiceProvider.GetRequiredService<ISupervisorAgent>();
        var workerPool = _fixture.ServiceProvider.GetRequiredService<IWorkerPool>();

        // Submit initial tasks
        var tasks = Enumerable.Range(1, 10).Select(i => new WorkerTask
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

        // Act: Simulate agent failure by removing worker
        await workerPool.RemoveWorkerAsync("worker-1");

        // Assert: System should continue processing with remaining workers
        var stats = await workerPool.GetStatisticsAsync();
        stats.AvailableWorkers.Should().BeGreaterThan(0);
        stats.TotalWorkers.Should().Be(9); // One less than initial 10
    }

    [Fact]
    public async Task System_Should_Recover_When_Agent_Rejoins()
    {
        // Arrange
        var registry = _fixture.ServiceProvider.GetRequiredService<IAgentRegistry>();
        var workerPool = _fixture.ServiceProvider.GetRequiredService<IWorkerPool>();

        // Remove an agent
        await workerPool.RemoveWorkerAsync("worker-1");

        // Act: Re-register the agent
        var capabilities = new AgentCapabilities
        {
            AgentId = "worker-1",
            AgentType = "test-worker",
            SupportedTools = new[] { "tool1" },
            MaxConcurrentTasks = 5
        };

        await registry.RegisterAsync(capabilities);
        await workerPool.AddWorkerAsync("worker-1");

        // Assert: Agent should be available again
        var stats = await workerPool.GetStatisticsAsync();
        stats.TotalWorkers.Should().Be(10);
        stats.AvailableWorkers.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task System_Should_Handle_Multiple_Agent_Failures()
    {
        // Arrange
        var workerPool = _fixture.ServiceProvider.GetRequiredService<IWorkerPool>();

        // Act: Remove multiple agents
        await workerPool.RemoveWorkerAsync("worker-1");
        await workerPool.RemoveWorkerAsync("worker-2");
        await workerPool.RemoveWorkerAsync("worker-3");

        // Assert: System should still have available workers
        var stats = await workerPool.GetStatisticsAsync();
        stats.TotalWorkers.Should().Be(7);
        stats.AvailableWorkers.Should().BeGreaterThan(0);
    }
}

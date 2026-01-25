using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.WorkerPool;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.Agents.WorkerPool.Tests;

public class WorkerPoolTests
{
    private readonly Mock<IAgentRegistry> _mockRegistry;
    private readonly WorkerPool _workerPool;

    public WorkerPoolTests()
    {
        _mockRegistry = new Mock<IAgentRegistry>();
        _workerPool = new WorkerPool(_mockRegistry.Object);
    }

    [Fact]
    public async Task AddWorkerAsync_WithRegisteredAgent_AddsWorker()
    {
        // Arrange
        var agentInfo = new AgentInfo
        {
            AgentId = "agent-1",
            AgentType = "test",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities { AgentId = "agent-1", AgentType = "test" },
            LastHeartbeat = DateTimeOffset.UtcNow
        };

        _mockRegistry.Setup(r => r.GetByIdAsync("agent-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(agentInfo);

        // Act
        await _workerPool.AddWorkerAsync("agent-1");

        // Assert
        _workerPool.WorkerCount.Should().Be(1);
    }

    [Fact]
    public async Task AddWorkerAsync_WithNonExistentAgent_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockRegistry.Setup(r => r.GetByIdAsync("agent-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgentInfo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workerPool.AddWorkerAsync("agent-1"));
    }

    [Fact]
    public async Task RemoveWorkerAsync_WithAddedWorker_RemovesWorker()
    {
        // Arrange
        var agentInfo = new AgentInfo
        {
            AgentId = "agent-1",
            AgentType = "test",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities { AgentId = "agent-1", AgentType = "test" },
            LastHeartbeat = DateTimeOffset.UtcNow
        };

        _mockRegistry.Setup(r => r.GetByIdAsync("agent-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(agentInfo);

        await _workerPool.AddWorkerAsync("agent-1");

        // Act
        await _workerPool.RemoveWorkerAsync("agent-1");

        // Assert
        _workerPool.WorkerCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAvailableWorkerAsync_WithAvailableWorker_ReturnsWorker()
    {
        // Arrange
        var agentInfo = new AgentInfo
        {
            AgentId = "agent-1",
            AgentType = "test",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities
            {
                AgentId = "agent-1",
                AgentType = "test",
                MaxConcurrentTasks = 5
            },
            CurrentTaskCount = 0,
            LastHeartbeat = DateTimeOffset.UtcNow
        };

        _mockRegistry.Setup(r => r.GetByIdAsync("agent-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(agentInfo);
        _mockRegistry.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { agentInfo });

        await _workerPool.AddWorkerAsync("agent-1");

        // Act
        IWorkerPool pool = _workerPool;
        var worker = await pool.GetAvailableWorkerAsync(requiredCapability: null, CancellationToken.None);

        // Assert
        worker.Should().NotBeNull();
        worker!.AgentId.Should().Be("agent-1");
    }

    [Fact]
    public async Task GetAvailableWorkerAsync_WithNoAvailableWorkers_ReturnsNull()
    {
        // Arrange
        var agentInfo = new AgentInfo
        {
            AgentId = "agent-1",
            AgentType = "test",
            Status = AgentStatus.Busy,
            Capabilities = new AgentCapabilities
            {
                AgentId = "agent-1",
                AgentType = "test",
                MaxConcurrentTasks = 1
            },
            CurrentTaskCount = 1,
            LastHeartbeat = DateTimeOffset.UtcNow
        };

        _mockRegistry.Setup(r => r.GetByIdAsync("agent-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(agentInfo);
        _mockRegistry.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { agentInfo });

        await _workerPool.AddWorkerAsync("agent-1");

        // Act
        IWorkerPool pool = _workerPool;
        var worker = await pool.GetAvailableWorkerAsync(requiredCapability: null, CancellationToken.None);

        // Assert
        worker.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableWorkerAsync_WithRequiredCapability_FiltersByCapability()
    {
        // Arrange
        var agent1 = new AgentInfo
        {
            AgentId = "agent-1",
            AgentType = "test",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities
            {
                AgentId = "agent-1",
                AgentType = "test",
                SupportedTools = new[] { "tool1" },
                MaxConcurrentTasks = 5
            },
            CurrentTaskCount = 0,
            LastHeartbeat = DateTimeOffset.UtcNow
        };
        var agent2 = new AgentInfo
        {
            AgentId = "agent-2",
            AgentType = "test",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities
            {
                AgentId = "agent-2",
                AgentType = "test",
                SupportedTools = new[] { "tool2" },
                MaxConcurrentTasks = 5
            },
            CurrentTaskCount = 0,
            LastHeartbeat = DateTimeOffset.UtcNow
        };

        _mockRegistry.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id) => id == "agent-1" ? agent1 : agent2);
        _mockRegistry.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { agent1, agent2 });

        await _workerPool.AddWorkerAsync("agent-1");
        await _workerPool.AddWorkerAsync("agent-2");

        // Act
        var worker = await _workerPool.GetAvailableWorkerAsync("tool1");

        // Assert
        worker.Should().NotBeNull();
        worker!.AgentId.Should().Be("agent-1");
    }

    [Fact]
    public async Task GetStatisticsAsync_WithWorkers_ReturnsStatistics()
    {
        // Arrange
        var agentInfo = new AgentInfo
        {
            AgentId = "agent-1",
            AgentType = "test",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities
            {
                AgentId = "agent-1",
                AgentType = "test",
                MaxConcurrentTasks = 5
            },
            CurrentTaskCount = 0,
            LastHeartbeat = DateTimeOffset.UtcNow
        };

        _mockRegistry.Setup(r => r.GetByIdAsync("agent-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(agentInfo);
        _mockRegistry.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { agentInfo });

        await _workerPool.AddWorkerAsync("agent-1");

        // Act
        var stats = await _workerPool.GetStatisticsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.TotalWorkers.Should().Be(1);
        stats.AvailableWorkers.Should().Be(1);
    }
}

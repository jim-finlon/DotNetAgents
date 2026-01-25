using DotNetAgents.Agents.Registry;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Agents.Registry.Tests;

public class InMemoryAgentRegistryTests
{
    [Fact]
    public async Task RegisterAsync_WithValidCapabilities_RegistersAgent()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities = new AgentCapabilities
        {
            AgentId = "agent-1",
            AgentType = "test-agent",
            SupportedTools = new[] { "tool1", "tool2" },
            SupportedIntents = new[] { "intent1" },
            MaxConcurrentTasks = 5
        };

        // Act
        await registry.RegisterAsync(capabilities);

        // Assert
        var agent = await registry.GetByIdAsync("agent-1");
        agent.Should().NotBeNull();
        agent!.AgentId.Should().Be("agent-1");
        agent.AgentType.Should().Be("test-agent");
        agent.Status.Should().Be(AgentStatus.Available);
        agent.Capabilities.SupportedTools.Should().HaveCount(2);
        agent.Capabilities.MaxConcurrentTasks.Should().Be(5);
    }

    [Fact]
    public async Task RegisterAsync_WithNullCapabilities_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => registry.RegisterAsync(null!));
    }

    [Fact]
    public async Task UnregisterAsync_WithRegisteredAgent_RemovesAgent()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities = new AgentCapabilities { AgentId = "agent-1", AgentType = "test" };
        await registry.RegisterAsync(capabilities);

        // Act
        await registry.UnregisterAsync("agent-1");

        // Assert
        var agent = await registry.GetByIdAsync("agent-1");
        agent.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithRegisteredAgent_UpdatesStatus()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities = new AgentCapabilities { AgentId = "agent-1", AgentType = "test" };
        await registry.RegisterAsync(capabilities);

        // Act
        await registry.UpdateStatusAsync("agent-1", AgentStatus.Busy);

        // Assert
        var agent = await registry.GetByIdAsync("agent-1");
        agent!.Status.Should().Be(AgentStatus.Busy);
    }

    [Fact]
    public async Task UpdateTaskCountAsync_WithRegisteredAgent_UpdatesTaskCount()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities = new AgentCapabilities { AgentId = "agent-1", AgentType = "test" };
        await registry.RegisterAsync(capabilities);

        // Act
        await registry.UpdateTaskCountAsync("agent-1", 3);

        // Assert
        var agent = await registry.GetByIdAsync("agent-1");
        agent!.CurrentTaskCount.Should().Be(3);
    }

    [Fact]
    public async Task UpdateTaskCountAsync_WithNegativeCount_ThrowsArgumentException()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities = new AgentCapabilities { AgentId = "agent-1", AgentType = "test" };
        await registry.RegisterAsync(capabilities);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => registry.UpdateTaskCountAsync("agent-1", -1));
    }

    [Fact]
    public async Task FindByCapabilityAsync_WithMatchingTool_ReturnsAgents()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities1 = new AgentCapabilities
        {
            AgentId = "agent-1",
            AgentType = "test",
            SupportedTools = new[] { "tool1", "tool2" }
        };
        var capabilities2 = new AgentCapabilities
        {
            AgentId = "agent-2",
            AgentType = "test",
            SupportedTools = new[] { "tool2", "tool3" }
        };
        await registry.RegisterAsync(capabilities1);
        await registry.RegisterAsync(capabilities2);

        // Act
        var agents = await registry.FindByCapabilityAsync("tool2");

        // Assert
        agents.Should().HaveCount(2);
        agents.Select(a => a.AgentId).Should().Contain(new[] { "agent-1", "agent-2" });
    }

    [Fact]
    public async Task FindByCapabilityAsync_WithMatchingIntent_ReturnsAgents()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities = new AgentCapabilities
        {
            AgentId = "agent-1",
            AgentType = "test",
            SupportedIntents = new[] { "intent1", "intent2" }
        };
        await registry.RegisterAsync(capabilities);

        // Act
        var agents = await registry.FindByCapabilityAsync("intent1");

        // Assert
        agents.Should().HaveCount(1);
        agents[0].AgentId.Should().Be("agent-1");
    }

    [Fact]
    public async Task FindByTypeAsync_WithMatchingType_ReturnsAgents()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities1 = new AgentCapabilities { AgentId = "agent-1", AgentType = "type-a" };
        var capabilities2 = new AgentCapabilities { AgentId = "agent-2", AgentType = "type-b" };
        var capabilities3 = new AgentCapabilities { AgentId = "agent-3", AgentType = "type-a" };
        await registry.RegisterAsync(capabilities1);
        await registry.RegisterAsync(capabilities2);
        await registry.RegisterAsync(capabilities3);

        // Act
        var agents = await registry.FindByTypeAsync("type-a");

        // Assert
        agents.Should().HaveCount(2);
        agents.Select(a => a.AgentId).Should().Contain(new[] { "agent-1", "agent-3" });
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleAgents_ReturnsAllAgents()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities1 = new AgentCapabilities { AgentId = "agent-1", AgentType = "test" };
        var capabilities2 = new AgentCapabilities { AgentId = "agent-2", AgentType = "test" };
        await registry.RegisterAsync(capabilities1);
        await registry.RegisterAsync(capabilities2);

        // Act
        var agents = await registry.GetAllAsync();

        // Assert
        agents.Should().HaveCount(2);
    }

    [Fact]
    public async Task RecordHeartbeatAsync_WithRegisteredAgent_UpdatesLastHeartbeat()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();
        var capabilities = new AgentCapabilities { AgentId = "agent-1", AgentType = "test" };
        await registry.RegisterAsync(capabilities);
        var initialAgent = await registry.GetByIdAsync("agent-1");
        var initialHeartbeat = initialAgent!.LastHeartbeat;

        await Task.Delay(100); // Small delay

        // Act
        await registry.RecordHeartbeatAsync("agent-1");

        // Assert
        var updatedAgent = await registry.GetByIdAsync("agent-1");
        updatedAgent!.LastHeartbeat.Should().BeAfter(initialHeartbeat);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentAgent_ReturnsNull()
    {
        // Arrange
        var registry = new InMemoryAgentRegistry();

        // Act
        var agent = await registry.GetByIdAsync("non-existent");

        // Assert
        agent.Should().BeNull();
    }
}

using DotNetAgents.Agents.Registry;
using DotNetAgents.Storage.Agents.PostgreSQL;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DotNetAgents.IntegrationTests.Storage;

/// <summary>
/// Integration tests for PostgreSQLAgentRegistry.
/// Requires PostgreSQL database to be available.
/// </summary>
[Collection("PostgreSQL")]
public class PostgreSQLAgentRegistryTests : IClassFixture<PostgreSQLFixture>
{
    private readonly PostgreSQLFixture _fixture;
    private readonly ILogger<PostgreSQLAgentRegistry> _logger;

    public PostgreSQLAgentRegistryTests(PostgreSQLFixture fixture)
    {
        _fixture = fixture;
        _logger = _fixture.LoggerFactory.CreateLogger<PostgreSQLAgentRegistry>();
    }

    [Fact]
    public async Task RegisterAsync_WithValidCapabilities_RegistersAgent()
    {
        // Arrange
        var registry = new PostgreSQLAgentRegistry(_fixture.ConnectionString, _logger);
        var capabilities = new AgentCapabilities
        {
            AgentId = "test-agent-1",
            AgentType = "test-type",
            SupportedTools = new[] { "tool1", "tool2" },
            SupportedIntents = new[] { "intent1" },
            MaxConcurrentTasks = 5
        };

        // Act
        await registry.RegisterAsync(capabilities);

        // Assert
        var agent = await registry.GetByIdAsync("test-agent-1");
        agent.Should().NotBeNull();
        agent!.AgentId.Should().Be("test-agent-1");
        agent.AgentType.Should().Be("test-type");
        agent.Status.Should().Be(AgentStatus.Available);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithRegisteredAgent_UpdatesStatus()
    {
        // Arrange
        var registry = new PostgreSQLAgentRegistry(_fixture.ConnectionString, _logger);
        var capabilities = new AgentCapabilities
        {
            AgentId = "test-agent-2",
            AgentType = "test-type"
        };
        await registry.RegisterAsync(capabilities);

        // Act
        await registry.UpdateStatusAsync("test-agent-2", AgentStatus.Busy);

        // Assert
        var agent = await registry.GetByIdAsync("test-agent-2");
        agent.Should().NotBeNull();
        agent!.Status.Should().Be(AgentStatus.Busy);
    }

    [Fact]
    public async Task UpdateTaskCountAsync_WithRegisteredAgent_UpdatesTaskCount()
    {
        // Arrange
        var registry = new PostgreSQLAgentRegistry(_fixture.ConnectionString, _logger);
        var capabilities = new AgentCapabilities
        {
            AgentId = "test-agent-3",
            AgentType = "test-type",
            MaxConcurrentTasks = 10
        };
        await registry.RegisterAsync(capabilities);

        // Act
        await registry.UpdateTaskCountAsync("test-agent-3", 3);

        // Assert
        var agent = await registry.GetByIdAsync("test-agent-3");
        agent.Should().NotBeNull();
        agent!.CurrentTaskCount.Should().Be(3);
    }

    [Fact]
    public async Task FindByCapabilityAsync_WithMatchingCapability_ReturnsAgents()
    {
        // Arrange
        var registry = new PostgreSQLAgentRegistry(_fixture.ConnectionString, _logger);
        var capabilities1 = new AgentCapabilities
        {
            AgentId = "test-agent-4",
            AgentType = "test-type",
            SupportedTools = new[] { "tool1", "tool2" }
        };
        var capabilities2 = new AgentCapabilities
        {
            AgentId = "test-agent-5",
            AgentType = "test-type",
            SupportedTools = new[] { "tool3" }
        };
        await registry.RegisterAsync(capabilities1);
        await registry.RegisterAsync(capabilities2);

        // Act
        var agents = await registry.FindByCapabilityAsync("tool1");

        // Assert
        agents.Should().NotBeNull();
        agents.Should().Contain(a => a.AgentId == "test-agent-4");
        agents.Should().NotContain(a => a.AgentId == "test-agent-5");
    }

    [Fact]
    public async Task FindByTypeAsync_WithMatchingType_ReturnsAgents()
    {
        // Arrange
        var registry = new PostgreSQLAgentRegistry(_fixture.ConnectionString, _logger);
        var capabilities1 = new AgentCapabilities
        {
            AgentId = "test-agent-6",
            AgentType = "type-a"
        };
        var capabilities2 = new AgentCapabilities
        {
            AgentId = "test-agent-7",
            AgentType = "type-b"
        };
        await registry.RegisterAsync(capabilities1);
        await registry.RegisterAsync(capabilities2);

        // Act
        var agents = await registry.FindByTypeAsync("type-a");

        // Assert
        agents.Should().NotBeNull();
        agents.Should().Contain(a => a.AgentId == "test-agent-6");
        agents.Should().NotContain(a => a.AgentId == "test-agent-7");
    }

    [Fact]
    public async Task UnregisterAsync_WithRegisteredAgent_RemovesAgent()
    {
        // Arrange
        var registry = new PostgreSQLAgentRegistry(_fixture.ConnectionString, _logger);
        var capabilities = new AgentCapabilities
        {
            AgentId = "test-agent-8",
            AgentType = "test-type"
        };
        await registry.RegisterAsync(capabilities);

        // Act
        await registry.UnregisterAsync("test-agent-8");

        // Assert
        var agent = await registry.GetByIdAsync("test-agent-8");
        agent.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleAgents_ReturnsAllAgents()
    {
        // Arrange
        var registry = new PostgreSQLAgentRegistry(_fixture.ConnectionString, _logger);
        var capabilities1 = new AgentCapabilities
        {
            AgentId = "test-agent-9",
            AgentType = "test-type"
        };
        var capabilities2 = new AgentCapabilities
        {
            AgentId = "test-agent-10",
            AgentType = "test-type"
        };
        await registry.RegisterAsync(capabilities1);
        await registry.RegisterAsync(capabilities2);

        // Act
        var agents = await registry.GetAllAsync();

        // Assert
        agents.Should().NotBeNull();
        agents.Should().Contain(a => a.AgentId == "test-agent-9");
        agents.Should().Contain(a => a.AgentId == "test-agent-10");
    }
}

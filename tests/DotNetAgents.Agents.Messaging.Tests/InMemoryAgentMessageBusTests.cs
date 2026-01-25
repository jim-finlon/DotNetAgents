using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.Registry;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.Agents.Messaging.Tests;

public class InMemoryAgentMessageBusTests
{
    private readonly Mock<IAgentRegistry> _mockRegistry;
    private readonly InMemoryAgentMessageBus _messageBus;

    public InMemoryAgentMessageBusTests()
    {
        _mockRegistry = new Mock<IAgentRegistry>();
        _messageBus = new InMemoryAgentMessageBus(_mockRegistry.Object);
    }

    [Fact]
    public async Task SendAsync_WithValidMessage_SendsSuccessfully()
    {
        // Arrange
        var message = new AgentMessage
        {
            FromAgentId = "agent-1",
            ToAgentId = "agent-2",
            MessageType = "test-message",
            Payload = "test payload"
        };

        var received = false;
        await _messageBus.SubscribeAsync("agent-2", async (msg, ct) =>
        {
            received = true;
            await Task.CompletedTask;
        });

        // Act
        var result = await _messageBus.SendAsync(message);

        // Assert
        result.Success.Should().BeTrue();
        result.MessageId.Should().Be(message.MessageId);
        
        // Wait a bit for async delivery
        await Task.Delay(100);
        received.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithEmptyToAgentId_ReturnsFailure()
    {
        // Arrange
        var message = new AgentMessage
        {
            FromAgentId = "agent-1",
            ToAgentId = string.Empty,
            MessageType = "test-message"
        };

        // Act
        var result = await _messageBus.SendAsync(message);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ToAgentId cannot be empty");
    }

    [Fact]
    public async Task SendAsync_WithBroadcastToAgentId_BroadcastsMessage()
    {
        // Arrange
        var message = new AgentMessage
        {
            FromAgentId = "agent-1",
            ToAgentId = "*",
            MessageType = "broadcast-message"
        };

        var agent1 = new AgentInfo
        {
            AgentId = "agent-2",
            AgentType = "test",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities { AgentId = "agent-2", AgentType = "test" },
            LastHeartbeat = DateTimeOffset.UtcNow
        };
        var agent2 = new AgentInfo
        {
            AgentId = "agent-3",
            AgentType = "test",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities { AgentId = "agent-3", AgentType = "test" },
            LastHeartbeat = DateTimeOffset.UtcNow
        };

        _mockRegistry.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { agent1, agent2 });

        var receivedCount = 0;
        await _messageBus.SubscribeAsync("agent-2", async (msg, ct) => { receivedCount++; await Task.CompletedTask; });
        await _messageBus.SubscribeAsync("agent-3", async (msg, ct) => { receivedCount++; await Task.CompletedTask; });

        // Act
        var result = await _messageBus.SendAsync(message);

        // Assert
        result.Success.Should().BeTrue();
        
        // Wait a bit for async delivery (with retry)
        for (int i = 0; i < 10 && receivedCount < 2; i++)
        {
            await Task.Delay(50);
        }
        receivedCount.Should().Be(2);
    }

    [Fact]
    public async Task SendAsync_WithExpiredMessage_ReturnsFailure()
    {
        // Arrange
        var message = new AgentMessage
        {
            FromAgentId = "agent-1",
            ToAgentId = "agent-2",
            MessageType = "test-message",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10),
            TimeToLive = TimeSpan.FromMinutes(5)
        };

        // Act
        var result = await _messageBus.SendAsync(message);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("expired");
    }

    [Fact]
    public async Task BroadcastAsync_WithFilter_FiltersAgents()
    {
        // Arrange
        var message = new AgentMessage
        {
            FromAgentId = "agent-1",
            ToAgentId = "*",
            MessageType = "broadcast-message"
        };

        var agent1 = new AgentInfo
        {
            AgentId = "agent-2",
            AgentType = "type-a",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities { AgentId = "agent-2", AgentType = "type-a" },
            LastHeartbeat = DateTimeOffset.UtcNow
        };
        var agent2 = new AgentInfo
        {
            AgentId = "agent-3",
            AgentType = "type-b",
            Status = AgentStatus.Available,
            Capabilities = new AgentCapabilities { AgentId = "agent-3", AgentType = "type-b" },
            LastHeartbeat = DateTimeOffset.UtcNow
        };

        _mockRegistry.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { agent1, agent2 });

        var receivedCount = 0;
        await _messageBus.SubscribeAsync("agent-2", async (msg, ct) => { receivedCount++; await Task.CompletedTask; });
        await _messageBus.SubscribeAsync("agent-3", async (msg, ct) => { receivedCount++; await Task.CompletedTask; });

        // Act
        var result = await _messageBus.BroadcastAsync(message, agent => agent.AgentType == "type-a");

        // Assert
        result.Success.Should().BeTrue();
        
        // Wait a bit for async delivery (with retry)
        for (int i = 0; i < 10 && receivedCount < 1; i++)
        {
            await Task.Delay(50);
        }
        receivedCount.Should().Be(1); // Only agent-2 should receive
    }

    [Fact]
    public async Task SubscribeAsync_WithHandler_ReceivesMessages()
    {
        // Arrange
        var receivedMessages = new List<AgentMessage>();
        await _messageBus.SubscribeAsync("agent-1", async (msg, ct) =>
        {
            receivedMessages.Add(msg);
            await Task.CompletedTask;
        });

        var message = new AgentMessage
        {
            FromAgentId = "agent-2",
            ToAgentId = "agent-1",
            MessageType = "test-message"
        };

        // Act
        await _messageBus.SendAsync(message);

        // Assert
        for (int i = 0; i < 10 && receivedMessages.Count == 0; i++)
        {
            await Task.Delay(50);
        }
        receivedMessages.Should().HaveCount(1);
        receivedMessages[0].MessageId.Should().Be(message.MessageId);
    }

    [Fact]
    public async Task SubscribeByTypeAsync_WithHandler_ReceivesMessagesByType()
    {
        // Arrange
        var receivedMessages = new List<AgentMessage>();
        await _messageBus.SubscribeByTypeAsync("test-message", async (msg, ct) =>
        {
            receivedMessages.Add(msg);
            await Task.CompletedTask;
        });

        var message1 = new AgentMessage
        {
            FromAgentId = "agent-1",
            ToAgentId = "agent-2",
            MessageType = "test-message"
        };
        var message2 = new AgentMessage
        {
            FromAgentId = "agent-1",
            ToAgentId = "agent-2",
            MessageType = "other-message"
        };

        // Act
        await _messageBus.SendAsync(message1);
        await _messageBus.SendAsync(message2);

        // Assert
        for (int i = 0; i < 10 && receivedMessages.Count < 1; i++)
        {
            await Task.Delay(50);
        }
        receivedMessages.Should().HaveCount(1);
        receivedMessages[0].MessageType.Should().Be("test-message");
    }

    [Fact]
    public async Task SubscribeAsync_WithDispose_Unsubscribes()
    {
        // Arrange
        var receivedCount = 0;
        var subscription = await _messageBus.SubscribeAsync("agent-1", async (msg, ct) =>
        {
            receivedCount++;
            await Task.CompletedTask;
        });

        var message = new AgentMessage
        {
            FromAgentId = "agent-2",
            ToAgentId = "agent-1",
            MessageType = "test-message"
        };

        // Act
        await _messageBus.SendAsync(message);
        await Task.Delay(100);
        var countBeforeDispose = receivedCount;

        subscription.Dispose();
        await _messageBus.SendAsync(message);
        await Task.Delay(100);

        // Assert
        receivedCount.Should().Be(countBeforeDispose); // Should not increase after dispose
    }

    [Fact]
    public async Task SendAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _messageBus.SendAsync(null!));
    }
}

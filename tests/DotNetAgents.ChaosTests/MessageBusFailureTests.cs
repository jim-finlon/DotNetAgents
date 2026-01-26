using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.Messaging.InMemory;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotNetAgents.ChaosTests;

/// <summary>
/// Chaos engineering tests for message bus failure scenarios.
/// </summary>
public class MessageBusFailureTests : IClassFixture<ChaosTestFixture>
{
    private readonly ChaosTestFixture _fixture;

    public MessageBusFailureTests(ChaosTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task System_Should_Handle_Message_Bus_Overload()
    {
        // Arrange
        var messageBus = _fixture.ServiceProvider.GetRequiredService<IAgentMessageBus>();

        // Act: Send large number of messages rapidly
        var messages = Enumerable.Range(1, 1000).Select(i => new AgentMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageType = "test-message",
            FromAgentId = "agent-1",
            ToAgentId = "agent-2",
            Payload = new Dictionary<string, object> { ["index"] = i }
        }).ToList();

        var sendTasks = messages.Select(msg => messageBus.SendAsync(msg));
        var results = await Task.WhenAll(sendTasks);

        // Assert: All messages should be sent successfully
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
    }

    [Fact]
    public async Task System_Should_Continue_When_Message_Delivery_Fails()
    {
        // Arrange
        var messageBus = _fixture.ServiceProvider.GetRequiredService<IAgentMessageBus>();

        // Act: Send message to non-existent agent
        var message = new AgentMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageType = "test",
            FromAgentId = "agent-1",
            ToAgentId = "non-existent-agent",
            Payload = new Dictionary<string, object>()
        };

        var result = await messageBus.SendAsync(message);

        // Assert: System should handle gracefully (may fail but shouldn't crash)
        // The exact behavior depends on implementation
        result.Should().NotBeNull();
    }
}

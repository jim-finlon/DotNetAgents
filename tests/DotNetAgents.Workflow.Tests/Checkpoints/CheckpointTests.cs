using DotNetAgents.Workflow.Checkpoints;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Workflow.Tests.Checkpoints;

public class CheckpointTests
{
    [Fact]
    public void Checkpoint_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var checkpoint = new Checkpoint<TestState>
        {
            Id = "test-id",
            RunId = "run-id",
            NodeName = "test-node",
            SerializedState = "{\"value\":42}",
            CreatedAt = DateTime.UtcNow,
            StateVersion = 1,
            Metadata = new Dictionary<string, object> { ["key"] = "value" }
        };

        // Act & Assert
        checkpoint.Id.Should().Be("test-id");
        checkpoint.RunId.Should().Be("run-id");
        checkpoint.NodeName.Should().Be("test-node");
        checkpoint.SerializedState.Should().Be("{\"value\":42}");
        checkpoint.StateVersion.Should().Be(1);
        checkpoint.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void Checkpoint_WithDefaultValues_HasExpectedDefaults()
    {
        // Arrange & Act
        var checkpoint = new Checkpoint<TestState>
        {
            Id = "test-id",
            RunId = "run-id",
            NodeName = "test-node",
            SerializedState = "{}"
        };

        // Assert
        checkpoint.StateVersion.Should().Be(1);
        checkpoint.Metadata.Should().NotBeNull();
        checkpoint.Metadata.Should().BeEmpty();
        checkpoint.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private record TestState(int Value);
}
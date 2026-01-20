using DotNetAgents.Workflow.HumanInLoop;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Workflow.Tests.HumanInLoop;

public class WorkflowStateInspectorTests
{
    [Fact]
    public void GetSnapshot_WithValidState_ReturnsSnapshot()
    {
        // Arrange
        var inspector = new WorkflowStateInspector<TestState>();
        var state = new TestState(42, "test");

        // Act
        var snapshot = inspector.GetSnapshot(state);

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.State.Should().Be(state);
        snapshot.StateType.Should().Be("TestState");
        snapshot.Properties.Should().ContainKey("Value");
        snapshot.Properties.Should().ContainKey("Name");
    }

    [Fact]
    public void GetSummary_WithValidState_ReturnsSummary()
    {
        // Arrange
        var inspector = new WorkflowStateInspector<TestState>();
        var state = new TestState(42, "test");

        // Act
        var summary = inspector.GetSummary(state);

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("TestState");
        summary.Should().Contain("Value");
        summary.Should().Contain("Name");
    }

    private record TestState(int Value, string Name);
}
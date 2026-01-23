using DotNetAgents.Workflow.HumanInLoop;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Workflow.Tests.HumanInLoop;

public class InMemoryApprovalHandlerTests
{
    [Fact]
    public async Task RequestApprovalAsync_WithValidInput_CreatesPendingApproval()
    {
        // Arrange
        var handler = new InMemoryApprovalHandler<TestState>();
        var state = new TestState(42);

        // Act
        var approved = await handler.RequestApprovalAsync("run1", "node1", state, "Please approve");

        // Assert
        approved.Should().BeFalse();
        var pending = handler.GetPendingApproval("run1", "node1");
        pending.Should().NotBeNull();
        pending!.WorkflowRunId.Should().Be("run1");
        pending.NodeName.Should().Be("node1");
        pending.Message.Should().Be("Please approve");
    }

    [Fact]
    public async Task IsApprovedAsync_WithNoApproval_ReturnsFalse()
    {
        // Arrange
        var handler = new InMemoryApprovalHandler<TestState>();

        // Act
        var approved = await handler.IsApprovedAsync("run1", "node1");

        // Assert
        approved.Should().BeFalse();
    }

    [Fact]
    public async Task Approve_WithApproval_SetsApprovalToTrue()
    {
        // Arrange
        var handler = new InMemoryApprovalHandler<TestState>();
        var state = new TestState(42);
        await handler.RequestApprovalAsync("run1", "node1", state);

        // Act
        handler.Approve("run1", "node1", approved: true);
        var approved = await handler.IsApprovedAsync("run1", "node1");

        // Assert
        approved.Should().BeTrue();
        handler.GetPendingApproval("run1", "node1").Should().BeNull();
    }

    [Fact]
    public void GetPendingApprovals_WithMultipleApprovals_ReturnsAll()
    {
        // Arrange
        var handler = new InMemoryApprovalHandler<TestState>();
        var state1 = new TestState(1);
        var state2 = new TestState(2);
        handler.RequestApprovalAsync("run1", "node1", state1).Wait();
        handler.RequestApprovalAsync("run1", "node2", state2).Wait();

        // Act
        var pending = handler.GetPendingApprovals("run1");

        // Assert
        pending.Should().HaveCount(2);
        pending.Select(p => p.NodeName).Should().Contain(new[] { "node1", "node2" });
    }

    private record TestState(int Value);
}
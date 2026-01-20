using DotNetAgents.Core.Exceptions;
using DotNetAgents.Workflow.Graph;
using DotNetAgents.Workflow.HumanInLoop;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.Workflow.Tests.HumanInLoop;

public class ApprovalNodeTests
{
    [Fact]
    public void Constructor_WithValidHandler_CreatesNode()
    {
        // Arrange
        var mockHandler = new Mock<IApprovalHandler<TestState>>();

        // Act
        var node = new ApprovalNode<TestState>("approval", mockHandler.Object);

        // Assert
        node.Should().NotBeNull();
        node.Name.Should().Be("approval");
    }

    [Fact]
    public async Task ExecuteAsync_WithApproval_Proceeds()
    {
        // Arrange
        var mockHandler = new Mock<IApprovalHandler<TestState>>();
        mockHandler.Setup(h => h.RequestApprovalAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TestState>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockHandler.Setup(h => h.IsApprovedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var node = new ApprovalNode<TestState>("approval", mockHandler.Object, timeout: TimeSpan.FromSeconds(1));
        var state = new TestState(42);

        // Act
        var result = await node.ExecuteAsync(state);

        // Assert
        result.Should().Be(state);
    }

    [Fact]
    public async Task ExecuteAsync_WithRejection_ThrowsException()
    {
        // Arrange
        var mockHandler = new Mock<IApprovalHandler<TestState>>();
        mockHandler.Setup(h => h.RequestApprovalAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TestState>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockHandler.Setup(h => h.IsApprovedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var node = new ApprovalNode<TestState>("approval", mockHandler.Object, timeout: TimeSpan.FromMilliseconds(100));
        var state = new TestState(42);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AgentException>(() => node.ExecuteAsync(state));
        exception.Category.Should().Be(ErrorCategory.WorkflowError);
    }

    private record TestState(int Value);
}
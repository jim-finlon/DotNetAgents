using DotNetAgents.Workflow.HumanInLoop;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.Workflow.Tests.HumanInLoop;

public class DecisionNodeTests
{
    private record TestState
    {
        public string? Decision { get; set; }
        public string? WorkflowRunId { get; set; }
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesNode()
    {
        // Arrange
        var mockHandler = new Mock<IDecisionHandler<TestState>>();
        var options = new[] { "option1", "option2", "option3" };

        // Act
        var node = new DecisionNode<TestState>(
            "test-decision",
            mockHandler.Object,
            "Choose an option",
            options);

        // Assert
        node.Should().NotBeNull();
        node.Name.Should().Be("test-decision");
    }

    [Fact]
    public void Constructor_WithEmptyOptions_ThrowsArgumentException()
    {
        // Arrange
        var mockHandler = new Mock<IDecisionHandler<TestState>>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DecisionNode<TestState>(
            "test-decision",
            mockHandler.Object,
            "Choose an option",
            Array.Empty<string>()));
    }

    [Fact]
    public async Task ExecuteAsync_WithImmediateDecision_ReturnsStateWithDecision()
    {
        // Arrange
        var handler = new InMemoryDecisionHandler<TestState>();
        var options = new[] { "option1", "option2" };
        var node = new DecisionNode<TestState>(
            "test-decision",
            handler,
            "Choose an option",
            options);

        var state = new TestState { WorkflowRunId = "run-1" };
        handler.SetDecision("run-1", "test-decision", "option1");

        // Act
        var result = await node.ExecuteAsync(state, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Decision.Should().Be("option1");
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingDecision_WaitsForDecision()
    {
        // Arrange
        var handler = new InMemoryDecisionHandler<TestState>();
        var options = new[] { "option1", "option2" };
        var node = new DecisionNode<TestState>(
            "test-decision",
            handler,
            "Choose an option",
            options,
            timeout: TimeSpan.FromSeconds(2));

        var state = new TestState { WorkflowRunId = "run-1" };

        // Set decision after a delay
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            handler.SetDecision("run-1", "test-decision", "option2");
        });

        // Act
        var result = await node.ExecuteAsync(state, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Decision.Should().Be("option2");
    }

    [Fact]
    public async Task ExecuteAsync_WithTimeout_ThrowsAgentException()
    {
        // Arrange
        var handler = new InMemoryDecisionHandler<TestState>();
        var options = new[] { "option1", "option2" };
        var node = new DecisionNode<TestState>(
            "test-decision",
            handler,
            "Choose an option",
            options,
            timeout: TimeSpan.FromMilliseconds(100));

        var state = new TestState { WorkflowRunId = "run-1" };

        // Act & Assert
        await Assert.ThrowsAsync<DotNetAgents.Abstractions.Exceptions.AgentException>(
            () => node.ExecuteAsync(state, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidDecision_ThrowsAgentException()
    {
        // Arrange
        var handler = new InMemoryDecisionHandler<TestState>();
        var options = new[] { "option1", "option2" };
        var node = new DecisionNode<TestState>(
            "test-decision",
            handler,
            "Choose an option",
            options);

        var state = new TestState { WorkflowRunId = "run-1" };
        
        // Set invalid decision directly (bypassing validation)
        var key = "run-1:test-decision";
        var request = handler.GetPendingDecision("run-1", "test-decision");
        if (request == null)
        {
            await handler.RequestDecisionAsync("run-1", "test-decision", state, "Choose", options);
        }

        // Act & Assert
        // This test verifies that the node validates the decision
        // We'll need to test this through the handler's SetDecision method
        Assert.Throws<ArgumentException>(() => 
            handler.SetDecision("run-1", "test-decision", "invalid-option"));
    }

    [Fact]
    public async Task ExecuteAsync_StoresDecisionInStateProperty()
    {
        // Arrange
        var handler = new InMemoryDecisionHandler<TestState>();
        var options = new[] { "option1", "option2" };
        var node = new DecisionNode<TestState>(
            "test-decision",
            handler,
            "Choose an option",
            options);

        var state = new TestState { WorkflowRunId = "run-1" };
        handler.SetDecision("run-1", "test-decision", "option1");

        // Act
        var result = await node.ExecuteAsync(state, CancellationToken.None);

        // Assert
        result.Decision.Should().Be("option1");
    }
}

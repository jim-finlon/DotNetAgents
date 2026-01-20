using DotNetAgents.Core.Exceptions;
using DotNetAgents.Workflow.Graph;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Workflow.Tests.Graph;

public class StateGraphTests
{
    public record TestState(string Value, int Count);

    [Fact]
    public void AddNode_WithValidNode_AddsNode()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        var node = new GraphNode<TestState>("node1", (state, ct) => Task.FromResult(state));

        // Act
        graph.AddNode(node);

        // Assert
        graph.Nodes.Should().ContainKey("node1");
        graph.Nodes["node1"].Should().Be(node);
    }

    [Fact]
    public void AddNode_WithDuplicateName_ThrowsException()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("node1", (state, ct) => Task.FromResult(state));

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            graph.AddNode("node1", (state, ct) => Task.FromResult(state)));
    }

    [Fact]
    public void AddEdge_WithValidNodes_AddsEdge()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("node1", (state, ct) => Task.FromResult(state));
        graph.AddNode("node2", (state, ct) => Task.FromResult(state));

        // Act
        graph.AddEdge("node1", "node2");

        // Assert
        graph.Edges.Should().HaveCount(1);
        graph.Edges[0].From.Should().Be("node1");
        graph.Edges[0].To.Should().Be("node2");
    }

    [Fact]
    public void AddEdge_WithNonExistentSource_ThrowsException()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("node2", (state, ct) => Task.FromResult(state));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => graph.AddEdge("node1", "node2"));
    }

    [Fact]
    public void SetEntryPoint_WithValidNode_SetsEntryPoint()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("node1", (state, ct) => Task.FromResult(state));

        // Act
        graph.SetEntryPoint("node1");

        // Assert
        graph.EntryPoint.Should().Be("node1");
    }

    [Fact]
    public void Validate_WithValidGraph_DoesNotThrow()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("start", (state, ct) => Task.FromResult(state));
        graph.AddNode("end", (state, ct) => Task.FromResult(state));
        graph.AddEdge("start", "end");
        graph.SetEntryPoint("start");
        graph.AddExitPoint("end");

        // Act & Assert
        graph.Invoking(g => g.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithoutEntryPoint_ThrowsException()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("node1", (state, ct) => Task.FromResult(state));
        graph.AddExitPoint("node1");

        // Act & Assert
        graph.Invoking(g => g.Validate())
            .Should()
            .Throw<AgentException>()
            .WithMessage("*entry point*");
    }

    [Fact]
    public void Validate_WithoutExitPoint_ThrowsException()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("node1", (state, ct) => Task.FromResult(state));
        graph.SetEntryPoint("node1");

        // Act & Assert
        graph.Invoking(g => g.Validate())
            .Should()
            .Throw<AgentException>()
            .WithMessage("*exit point*");
    }

    [Fact]
    public void GetOutgoingEdges_WithConditionalEdge_ReturnsMatchingEdges()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("node1", (state, ct) => Task.FromResult(state));
        graph.AddNode("node2", (state, ct) => Task.FromResult(state));
        graph.AddNode("node3", (state, ct) => Task.FromResult(state));
        graph.AddEdge("node1", "node2", state => state.Count > 0);
        graph.AddEdge("node1", "node3", state => state.Count <= 0);

        var state1 = new TestState("test", 1);
        var state2 = new TestState("test", 0);

        // Act
        var edges1 = graph.GetOutgoingEdges("node1", state1);
        var edges2 = graph.GetOutgoingEdges("node1", state2);

        // Assert
        edges1.Should().HaveCount(1);
        edges1[0].To.Should().Be("node2");
        edges2.Should().HaveCount(1);
        edges2[0].To.Should().Be("node3");
    }
}
using DotNetAgents.Core.Exceptions;
using DotNetAgents.Workflow.Graph;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Workflow.Tests.Graph;

public class WorkflowBuilderTests
{
    [Fact]
    public void Create_ReturnsNewBuilder()
    {
        // Act
        var builder = WorkflowBuilder<TestState>.Create();

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void AddNode_WithHandler_AddsNode()
    {
        // Arrange
        var builder = WorkflowBuilder<TestState>.Create();

        // Act
        var result = builder.AddNode("test-node", async (state, ct) => state);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddEdge_AddsEdge()
    {
        // Arrange
        var builder = WorkflowBuilder<TestState>.Create();
        builder.AddNode("node1", async (state, ct) => state);
        builder.AddNode("node2", async (state, ct) => state);

        // Act
        var result = builder.AddEdge("node1", "node2");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void SetEntryPoint_SetsEntryPoint()
    {
        // Arrange
        var builder = WorkflowBuilder<TestState>.Create();
        builder.AddNode("start", async (state, ct) => state);

        // Act
        var result = builder.SetEntryPoint("start");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddExitPoint_AddsExitPoint()
    {
        // Arrange
        var builder = WorkflowBuilder<TestState>.Create();
        builder.AddNode("end", async (state, ct) => state);

        // Act
        var result = builder.AddExitPoint("end");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void Build_WithValidWorkflow_BuildsGraph()
    {
        // Arrange
        var builder = WorkflowBuilder<TestState>.Create();
        builder.AddNode("start", async (state, ct) => new TestState(state.Value + 1));
        builder.AddNode("end", async (state, ct) => state);
        builder.AddEdge("start", "end");
        builder.SetEntryPoint("start");
        builder.AddExitPoint("end");

        // Act
        var graph = builder.Build();

        // Assert
        graph.Should().NotBeNull();
        graph.EntryPoint.Should().Be("start");
    }

    [Fact]
    public void Build_WithoutEntryPoint_ThrowsException()
    {
        // Arrange
        var builder = WorkflowBuilder<TestState>.Create();
        builder.AddNode("node1", async (state, ct) => state);

        // Act & Assert
        var exception = Assert.Throws<AgentException>(() => builder.Build());
        exception.Category.Should().Be(ErrorCategory.ConfigurationError);
    }

    [Fact]
    public void Build_WithConditionalEdge_IncludesCondition()
    {
        // Arrange
        var builder = WorkflowBuilder<TestState>.Create();
        builder.AddNode("start", async (state, ct) => state);
        builder.AddNode("even", async (state, ct) => state);
        builder.AddNode("odd", async (state, ct) => state);
        builder.AddEdge("start", "even", state => state.Value % 2 == 0);
        builder.AddEdge("start", "odd", state => state.Value % 2 != 0);
        builder.SetEntryPoint("start");
        builder.AddExitPoint("even");
        builder.AddExitPoint("odd");

        // Act
        var graph = builder.Build();

        // Assert
        graph.Should().NotBeNull();
        var edges = graph.GetOutgoingEdges("start", new TestState(2));
        edges.Should().HaveCount(1);
        edges[0].To.Should().Be("even");
    }

    private record TestState(int Value);
}
using DotLangChain.Abstractions.Agents;
using DotLangChain.Core.Agents.Graph;
using DotLangChain.Core.Exceptions;
using FluentAssertions;

namespace DotLangChain.Tests.Unit.Agents.Graph;

public class GraphBuilderTests
{
    private class TestState : AgentState
    {
        public string Value { get; set; } = "";
        public int Counter { get; set; }
    }

    [Fact]
    public void AddNode_WithValidNode_AddsNode()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();

        // Act
        builder.AddNode("node1", async (state, ct) =>
        {
            state.Value = "processed";
            return state;
        });

        // Assert
        var graph = builder.SetEntryPoint("node1").Compile();
        graph.Should().NotBeNull();
    }

    [Fact]
    public void AddEdge_WithValidNodes_AddsEdge()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();

        // Act
        builder
            .AddNode("start", async (s, ct) => s)
            .AddNode("end", async (s, ct) => s)
            .AddEdge("start", "end")
            .SetEntryPoint("start");

        // Assert
        var graph = builder.Compile();
        graph.Should().NotBeNull();
    }

    [Fact]
    public void AddConditionalEdge_WithCondition_AddsConditionalEdge()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();

        // Act
        builder
            .AddNode("node1", async (s, ct) => { s.Value = "test"; return s; })
            .AddNode("node2", async (s, ct) => s)
            .AddNode("node3", async (s, ct) => s)
            .AddConditionalEdge("node1", state => state.Value == "test"
                ? EdgeDecision.To("node2")
                : EdgeDecision.To("node3"))
            .SetEntryPoint("node1");

        // Assert
        var graph = builder.Compile();
        graph.Should().NotBeNull();
    }

    [Fact]
    public void Compile_WithoutEntryPoint_ThrowsException()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();
        builder.AddNode("node1", async (s, ct) => s);

        // Act
        var act = () => builder.Compile();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*entry point*");
    }

    [Fact]
    public void Compile_WithInvalidEntryPoint_ThrowsGraphException()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();
        builder.AddNode("node1", async (s, ct) => s);

        // Act
        var act = () => builder.SetEntryPoint("nonexistent").Compile();

        // Assert
        act.Should().Throw<GraphException>();
    }

    [Fact]
    public async Task CompiledGraph_ExecutesLinearGraph()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();
        builder
            .AddNode("start", async (s, ct) => { s.Value = "started"; return s; })
            .AddNode("middle", async (s, ct) => { s.Value = "middle"; return s; })
            .AddNode("end", async (s, ct) => { s.Value = "ended"; return s; })
            .AddEdge("start", "middle")
            .AddEdge("middle", "end")
            .SetEntryPoint("start");

        var graph = builder.Compile();
        var initialState = new TestState();

        // Act
        var finalState = await graph.InvokeAsync(initialState);

        // Assert
        finalState.Value.Should().Be("ended");
        finalState.StepCount.Should().Be(3);
    }

    [Fact]
    public async Task CompiledGraph_ExecutesConditionalBranch()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();
        builder
            .AddNode("start", async (s, ct) => { s.Counter = 5; return s; })
            .AddNode("path1", async (s, ct) => { s.Value = "path1"; return s; })
            .AddNode("path2", async (s, ct) => { s.Value = "path2"; return s; })
            .AddConditionalEdge("start", state => state.Counter > 3
                ? EdgeDecision.To("path1")
                : EdgeDecision.To("path2"))
            .SetEntryPoint("start");

        var graph = builder.Compile();
        var initialState = new TestState();

        // Act
        var finalState = await graph.InvokeAsync(initialState);

        // Assert
        finalState.Value.Should().Be("path1");
    }

    [Fact]
    public async Task CompiledGraph_StreamsEvents()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();
        builder
            .AddNode("node1", async (s, ct) => s)
            .SetEntryPoint("node1");

        var graph = builder.Compile();
        var initialState = new TestState();
        var events = new List<GraphEvent<TestState>>();

        // Act
        await foreach (var evt in graph.StreamAsync(initialState))
        {
            events.Add(evt);
        }

        // Assert
        events.Should().NotBeEmpty();
        events.Should().Contain(e => e.EventType == GraphEventType.NodeStarted);
        events.Should().Contain(e => e.EventType == GraphEventType.NodeCompleted);
        events.Should().Contain(e => e.EventType == GraphEventType.GraphCompleted);
    }

    [Fact]
    public async Task CompiledGraph_RespectsMaxSteps()
    {
        // Arrange
        var builder = new GraphBuilder<TestState>();
        builder
            .AddNode("loop", async (s, ct) => { s.Counter++; return s; })
            .AddEdge("loop", "loop") // Create infinite loop
            .SetEntryPoint("loop");

        var graph = builder.Compile();
        var options = new GraphExecutionOptions { MaxSteps = 5 };
        var initialState = new TestState();

        // Act
        var act = async () => await graph.InvokeAsync(initialState, options);

        // Assert
        await act.Should().ThrowAsync<GraphException>()
            .Where(e => e.ErrorCode == "DLC005-001");
    }
}


using DotNetAgents.Abstractions.Exceptions;
using DotNetAgents.Workflow.Execution;
using DotNetAgents.Workflow.Graph;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetAgents.Workflow.Tests.Execution;

public class GraphExecutorTests
{
    public record TestState(string Value, int Count);

    [Fact]
    public async Task ExecuteAsync_WithSimpleGraph_ExecutesSuccessfully()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("start", (state, ct) => Task.FromResult(state with { Count = state.Count + 1 }));
        graph.AddNode("end", (state, ct) => Task.FromResult(state));
        graph.AddEdge("start", "end");
        graph.SetEntryPoint("start");
        graph.AddExitPoint("end");

        var executor = new GraphExecutor<TestState>(graph);
        var initialState = new TestState("test", 0);

        // Act
        var result = await executor.ExecuteAsync(initialState);

        // Assert
        result.Count.Should().Be(1);
        result.Value.Should().Be("test");
    }

    [Fact]
    public async Task ExecuteAsync_WithConditionalEdge_TakesCorrectPath()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("start", (state, ct) => Task.FromResult(state with { Count = state.Count + 1 }));
        graph.AddNode("even", (state, ct) => Task.FromResult(state with { Value = "even" }));
        graph.AddNode("odd", (state, ct) => Task.FromResult(state with { Value = "odd" }));
        graph.AddNode("end", (state, ct) => Task.FromResult(state));
        
        // After increment, count will be 1 (odd), so we check the incremented value
        graph.AddEdge("start", "even", state => state.Count % 2 == 0);
        graph.AddEdge("start", "odd", state => state.Count % 2 != 0);
        graph.AddEdge("even", "end");
        graph.AddEdge("odd", "end");
        
        graph.SetEntryPoint("start");
        graph.AddExitPoint("end");

        var executor = new GraphExecutor<TestState>(graph);
        var initialState = new TestState("test", 0);

        // Act
        var result = await executor.ExecuteAsync(initialState);

        // Assert
        // After increment, count is 1 (odd)
        result.Value.Should().Be("odd");
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaxIterations_ThrowsException()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("loop", (state, ct) => Task.FromResult(state with { Count = state.Count + 1 }));
        graph.AddNode("end", (state, ct) => Task.FromResult(state));
        graph.AddEdge("loop", "loop"); // Loop back to itself
        graph.SetEntryPoint("loop");
        graph.AddExitPoint("end"); // Note: loop is NOT an exit point

        var executor = new GraphExecutor<TestState>(graph);
        var initialState = new TestState("test", 0);
        var options = new GraphExecutionOptions { MaxIterations = 5 };

        // Act & Assert
        await Assert.ThrowsAsync<AgentException>(() =>
            executor.ExecuteAsync(initialState, options));
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_CancelsExecution()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("start", async (state, ct) =>
        {
            await Task.Delay(1000, ct);
            return state;
        });
        graph.AddNode("end", (state, ct) => Task.FromResult(state));
        graph.AddEdge("start", "end");
        graph.SetEntryPoint("start");
        graph.AddExitPoint("end");

        var executor = new GraphExecutor<TestState>(graph);
        var initialState = new TestState("test", 0);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AgentException>(() =>
            executor.ExecuteAsync(initialState, cancellationToken: cts.Token));
        exception.InnerException.Should().BeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNodeError_ThrowsException()
    {
        // Arrange
        var graph = new StateGraph<TestState>();
        graph.AddNode("start", (state, ct) => throw new InvalidOperationException("Test error"));
        graph.AddNode("end", (state, ct) => Task.FromResult(state));
        graph.AddEdge("start", "end");
        graph.SetEntryPoint("start");
        graph.AddExitPoint("end");

        var executor = new GraphExecutor<TestState>(graph);
        var initialState = new TestState("test", 0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AgentException>(() =>
            executor.ExecuteAsync(initialState));
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }
}
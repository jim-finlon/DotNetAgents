using DotNetAgents.Agents.StateMachines;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetAgents.Agents.StateMachines.Tests;

public class StateMachineBuilderTests
{
    private readonly Mock<ILogger<AgentStateMachine<TestState>>> _loggerMock;

    public StateMachineBuilderTests()
    {
        _loggerMock = new Mock<ILogger<AgentStateMachine<TestState>>>();
    }

    [Fact]
    public void Build_WithValidConfiguration_BuildsStateMachine()
    {
        // Arrange
        var builder = new StateMachineBuilder<TestState>(_loggerMock.Object);

        // Act
        var stateMachine = builder
            .AddState("Idle")
            .AddState("Working")
            .AddTransition("Idle", "Working")
            .SetInitialState("Idle")
            .Build();

        // Assert
        stateMachine.Should().NotBeNull();
        stateMachine.CurrentState.Should().Be("Idle");
    }

    [Fact]
    public void Build_WithoutInitialState_ThrowsException()
    {
        // Arrange
        var builder = new StateMachineBuilder<TestState>(_loggerMock.Object);

        // Act & Assert
        builder.Invoking(b => b
            .AddState("Idle")
            .AddState("Working")
            .Build())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*initial state*");
    }

    [Fact]
    public void AddState_WithActions_ExecutesActions()
    {
        // Arrange
        var entryCalled = false;
        var exitCalled = false;
        var context = new TestState { Value = 1 };
        var builder = new StateMachineBuilder<TestState>(_loggerMock.Object);

        // Act
        var stateMachine = builder
            .AddState("Idle", exitAction: _ => exitCalled = true)
            .AddState("Working", entryAction: _ => entryCalled = true)
            .AddTransition("Idle", "Working")
            .SetInitialState("Idle")
            .Build();

        stateMachine.TransitionAsync("Working", context).Wait();

        // Assert
        entryCalled.Should().BeTrue();
        exitCalled.Should().BeTrue();
    }

    [Fact]
    public void AddTransition_WithGuard_RespectsGuard()
    {
        // Arrange
        var context = new TestState { Value = 5 };
        var builder = new StateMachineBuilder<TestState>(_loggerMock.Object);

        // Act
        var stateMachine = builder
            .AddState("Idle")
            .AddState("Working")
            .AddTransition("Idle", "Working", guard: s => s.Value > 10)
            .SetInitialState("Idle")
            .Build();

        // Assert
        stateMachine.CanTransition("Idle", "Working", context).Should().BeFalse();
    }

    private class TestState
    {
        public int Value { get; set; }
    }
}

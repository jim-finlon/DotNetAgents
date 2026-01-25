using DotNetAgents.Agents.StateMachines;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetAgents.Agents.StateMachines.Tests;

public class AgentStateMachineTests
{
    private readonly Mock<ILogger<AgentStateMachine<TestState>>> _loggerMock;
    private readonly AgentStateMachine<TestState> _stateMachine;

    public AgentStateMachineTests()
    {
        _loggerMock = new Mock<ILogger<AgentStateMachine<TestState>>>();
        _stateMachine = new AgentStateMachine<TestState>(_loggerMock.Object);
    }

    [Fact]
    public void AddState_WithValidName_AddsState()
    {
        // Act
        _stateMachine.AddState("Idle");

        // Assert
        _stateMachine.CurrentState.Should().BeEmpty(); // No initial state set yet
    }

    [Fact]
    public void AddState_WithDuplicateName_ThrowsException()
    {
        // Arrange
        _stateMachine.AddState("Idle");

        // Act & Assert
        _stateMachine.Invoking(sm => sm.AddState("Idle"))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void SetInitialState_WithValidState_SetsInitialState()
    {
        // Arrange
        _stateMachine.AddState("Idle");

        // Act
        _stateMachine.SetInitialState("Idle");

        // Assert
        _stateMachine.CurrentState.Should().Be("Idle");
    }

    [Fact]
    public void SetInitialState_WithInvalidState_ThrowsException()
    {
        // Act & Assert
        _stateMachine.Invoking(sm => sm.SetInitialState("NonExistent"))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void AddTransition_WithValidStates_AddsTransition()
    {
        // Arrange
        _stateMachine.AddState("Idle");
        _stateMachine.AddState("Working");

        // Act
        _stateMachine.AddTransition("Idle", "Working");

        // Assert - no exception thrown
    }

    [Fact]
    public void AddTransition_WithInvalidFromState_ThrowsException()
    {
        // Arrange
        _stateMachine.AddState("Working");

        // Act & Assert
        _stateMachine.Invoking(sm => sm.AddTransition("NonExistent", "Working"))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void CanTransition_WithValidTransition_ReturnsTrue()
    {
        // Arrange
        var context = new TestState { Value = 1 };
        _stateMachine.AddState("Idle");
        _stateMachine.AddState("Working");
        _stateMachine.AddTransition("Idle", "Working");
        _stateMachine.SetInitialState("Idle");

        // Act
        var result = _stateMachine.CanTransition("Idle", "Working", context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanTransition_WithGuardCondition_RespectsGuard()
    {
        // Arrange
        var context = new TestState { Value = 5 };
        _stateMachine.AddState("Idle");
        _stateMachine.AddState("Working");
        _stateMachine.AddTransition("Idle", "Working", guard: s => s.Value > 10);
        _stateMachine.SetInitialState("Idle");

        // Act
        var result = _stateMachine.CanTransition("Idle", "Working", context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TransitionAsync_WithValidTransition_TransitionsSuccessfully()
    {
        // Arrange
        var context = new TestState { Value = 1 };
        var entryActionCalled = false;
        var exitActionCalled = false;

        _stateMachine.AddState("Idle", exitAction: _ => exitActionCalled = true);
        _stateMachine.AddState("Working", entryAction: _ => entryActionCalled = true);
        _stateMachine.AddTransition("Idle", "Working");
        _stateMachine.SetInitialState("Idle");

        // Act
        await _stateMachine.TransitionAsync("Working", context);

        // Assert
        _stateMachine.CurrentState.Should().Be("Working");
        exitActionCalled.Should().BeTrue();
        entryActionCalled.Should().BeTrue();
    }

    [Fact]
    public async Task TransitionAsync_WithInvalidTransition_ThrowsException()
    {
        // Arrange
        var context = new TestState { Value = 1 };
        _stateMachine.AddState("Idle");
        _stateMachine.AddState("Working");
        _stateMachine.SetInitialState("Idle");

        // Act & Assert
        await _stateMachine.Invoking(sm => sm.TransitionAsync("Working", context))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not allowed*");
    }

    [Fact]
    public void GetAvailableTransitions_ReturnsValidTransitions()
    {
        // Arrange
        var context = new TestState { Value = 1 };
        _stateMachine.AddState("Idle");
        _stateMachine.AddState("Working");
        _stateMachine.AddState("Error");
        _stateMachine.AddTransition("Idle", "Working");
        _stateMachine.AddTransition("Idle", "Error");
        _stateMachine.SetInitialState("Idle");

        // Act
        var transitions = _stateMachine.GetAvailableTransitions(context);

        // Assert
        transitions.Should().Contain("Working");
        transitions.Should().Contain("Error");
        transitions.Should().HaveCount(2);
    }

    [Fact]
    public void Reset_ResetsToInitialState()
    {
        // Arrange
        var context = new TestState { Value = 1 };
        _stateMachine.AddState("Idle");
        _stateMachine.AddState("Working");
        _stateMachine.AddTransition("Idle", "Working");
        _stateMachine.SetInitialState("Idle");
        _stateMachine.TransitionAsync("Working", context).Wait();

        // Act
        _stateMachine.Reset(context);

        // Assert
        _stateMachine.CurrentState.Should().Be("Idle");
    }

    [Fact]
    public void StateTransitioned_Event_RaisedOnTransition()
    {
        // Arrange
        var context = new TestState { Value = 1 };
        StateTransitionEventArgs<TestState>? eventArgs = null;
        _stateMachine.AddState("Idle");
        _stateMachine.AddState("Working");
        _stateMachine.AddTransition("Idle", "Working");
        _stateMachine.SetInitialState("Idle");
        _stateMachine.StateTransitioned += (sender, e) => eventArgs = e;

        // Act
        _stateMachine.TransitionAsync("Working", context).Wait();

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.FromState.Should().Be("Idle");
        eventArgs.ToState.Should().Be("Working");
        eventArgs.Context.Should().Be(context);
    }

    private class TestState
    {
        public int Value { get; set; }
    }
}

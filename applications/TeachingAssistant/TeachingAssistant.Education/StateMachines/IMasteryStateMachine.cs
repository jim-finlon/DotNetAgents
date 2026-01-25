namespace DotNetAgents.Education.StateMachines;

/// <summary>
/// Interface for mastery state machine operations.
/// This interface is defined in Education to avoid circular dependencies.
/// </summary>
/// <typeparam name="TState">The type of the state context.</typeparam>
public interface IMasteryStateMachine<TState> where TState : class
{
    /// <summary>
    /// Gets the current mastery state.
    /// </summary>
    string? CurrentState { get; }

    /// <summary>
    /// Transitions the state machine to a new state based on the mastery score.
    /// </summary>
    /// <param name="toState">The target state.</param>
    /// <param name="context">The state context.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task TransitionAsync(string toState, TState context, CancellationToken cancellationToken = default);
}

namespace DotNetAgents.Education.StateMachines;

/// <summary>
/// Interface for learning session state machine operations.
/// This interface is defined in Education to avoid circular dependencies.
/// </summary>
/// <typeparam name="TState">The type of the state context.</typeparam>
public interface ILearningSessionStateMachine<TState> where TState : class
{
    /// <summary>
    /// Gets the current state of the learning session.
    /// </summary>
    string? CurrentState { get; }

    /// <summary>
    /// Transitions the state machine to a new state.
    /// </summary>
    /// <param name="toState">The target state.</param>
    /// <param name="context">The state context.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task TransitionAsync(string toState, TState context, CancellationToken cancellationToken = default);
}

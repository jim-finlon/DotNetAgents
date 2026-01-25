using DotNetAgents.Agents.StateMachines;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.StateMachines;

/// <summary>
/// State machine patterns for learning session lifecycle management.
/// </summary>
public static class LearningSessionStateMachinePattern
{
    /// <summary>
    /// Creates a Learning Session state machine pattern.
    /// Pattern: Initialized → Learning → Assessment → Review → Learning/Completed
    /// Includes pause/resume: Any → Paused → Previous state
    /// </summary>
    /// <typeparam name="TState">The type of the state context.</typeparam>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="sessionTimeout">Timeout duration for session before transitioning to Completed (default: 2 hours).</param>
    /// <param name="learningTimeout">Timeout duration for Learning state before transitioning to Assessment (default: 30 minutes).</param>
    /// <returns>A configured state machine.</returns>
    public static IStateMachine<TState> CreateLearningSessionPattern<TState>(
        ILogger<AgentStateMachine<TState>>? logger = null,
        TimeSpan? sessionTimeout = null,
        TimeSpan? learningTimeout = null)
        where TState : class
    {
        var builder = new StateMachineBuilder<TState>(logger);

        var sessionTimeoutDuration = sessionTimeout ?? TimeSpan.FromHours(2);
        var learningTimeoutDuration = learningTimeout ?? TimeSpan.FromMinutes(30);

        var stateMachine = builder
            .AddState("Initialized",
                entryAction: ctx => { /* Log entering initialized state */ })
            .AddState("Learning",
                entryAction: ctx => { /* Log entering learning state */ })
            .AddState("Assessment",
                entryAction: ctx => { /* Log entering assessment state */ })
            .AddState("Review",
                entryAction: ctx => { /* Log entering review state */ })
            .AddState("Completed",
                entryAction: ctx => { /* Log entering completed state */ })
            .AddState("Paused",
                entryAction: ctx => { /* Log entering paused state */ })
            .AddState("Error",
                entryAction: ctx => { /* Log error state entry */ })
            .AddTransition("Initialized", "Learning",
                guard: ctx => true) // On session start
            .AddTransition("Learning", "Assessment",
                guard: ctx => true) // On assessment trigger
            .AddTransition("Assessment", "Review",
                guard: ctx => true) // On assessment complete
            .AddTransition("Review", "Learning",
                guard: ctx => true) // On review complete, continue learning
            .AddTransition("Review", "Completed",
                guard: ctx => true) // On mastery achieved
            .AddTransition("Learning", "Completed",
                guard: ctx => true) // Direct completion from learning (optional path)
            // Pause transitions from any active state
            .AddTransition("Initialized", "Paused",
                guard: ctx => true) // On pause request
            .AddTransition("Learning", "Paused",
                guard: ctx => true) // On pause request
            .AddTransition("Assessment", "Paused",
                guard: ctx => true) // On pause request
            .AddTransition("Review", "Paused",
                guard: ctx => true) // On pause request
            // Resume transitions (Paused → Previous state)
            // Note: Actual previous state tracking would be handled by context
            .AddTransition("Paused", "Learning",
                guard: ctx => true) // On resume (default to Learning)
            .AddTransition("Paused", "Assessment",
                guard: ctx => true) // On resume to Assessment
            .AddTransition("Paused", "Review",
                guard: ctx => true) // On resume to Review
            // Error transitions from any state
            .AddTransition("Initialized", "Error",
                guard: ctx => true) // On exception
            .AddTransition("Learning", "Error",
                guard: ctx => true) // On exception
            .AddTransition("Assessment", "Error",
                guard: ctx => true) // On exception
            .AddTransition("Review", "Error",
                guard: ctx => true) // On exception
            .AddTransition("Paused", "Error",
                guard: ctx => true) // On exception
            // Recovery from error
            .AddTransition("Error", "Learning",
                guard: ctx => true) // After recovery, return to Learning
            .SetInitialState("Initialized")
            .Build();

        // Add timeout transitions
        if (stateMachine is AgentStateMachine<TState> agentStateMachine)
        {
            // Learning timeout: Learning → Assessment (trigger assessment)
            agentStateMachine.AddTimeoutTransition("Learning", "Assessment", learningTimeoutDuration);
            
            // Session timeout: Any active state → Completed (after full session timeout)
            // Note: This would typically be handled at a higher level, but we can add it to Learning as a safety
            agentStateMachine.AddTimeoutTransition("Learning", "Completed", sessionTimeoutDuration);
        }

        return stateMachine;
    }
}

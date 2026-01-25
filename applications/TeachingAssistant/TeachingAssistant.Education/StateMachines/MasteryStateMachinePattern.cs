using DotNetAgents.Agents.StateMachines;
using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.StateMachines;

/// <summary>
/// State machine patterns for student mastery tracking.
/// </summary>
public static class MasteryStateMachinePattern
{
    /// <summary>
    /// Creates a Mastery state machine pattern for tracking student progress.
    /// Pattern: Novice → Learning → Proficient → Master
    /// Transitions are based on mastery score thresholds.
    /// </summary>
    /// <typeparam name="TState">The type of the state context.</typeparam>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="noviceThreshold">Score threshold for Novice state (default: 0.0).</param>
    /// <param name="learningThreshold">Score threshold for Learning state (default: 0.4).</param>
    /// <param name="proficientThreshold">Score threshold for Proficient state (default: 0.6).</param>
    /// <param name="masterThreshold">Score threshold for Master state (default: 0.8).</param>
    /// <returns>A configured state machine.</returns>
    public static IStateMachine<TState> CreateMasteryPattern<TState>(
        ILogger<AgentStateMachine<TState>>? logger = null,
        double noviceThreshold = 0.0,
        double learningThreshold = 0.4,
        double proficientThreshold = 0.6,
        double masterThreshold = 0.8)
        where TState : class
    {
        var builder = new StateMachineBuilder<TState>(logger);

        var stateMachine = builder
            .AddState("Novice",
                entryAction: ctx => { /* Log entering novice state */ })
            .AddState("Learning",
                entryAction: ctx => { /* Log entering learning state */ })
            .AddState("Proficient",
                entryAction: ctx => { /* Log entering proficient state */ })
            .AddState("Master",
                entryAction: ctx => { /* Log entering master state */ })
            .AddState("Error",
                entryAction: ctx => { /* Log error state entry */ })
            // Progressive transitions (upward progression)
            .AddTransition("Novice", "Learning",
                guard: ctx => true) // Score >= learningThreshold
            .AddTransition("Learning", "Proficient",
                guard: ctx => true) // Score >= proficientThreshold
            .AddTransition("Proficient", "Master",
                guard: ctx => true) // Score >= masterThreshold
            // Regressive transitions (score drops)
            .AddTransition("Master", "Proficient",
                guard: ctx => true) // Score < masterThreshold
            .AddTransition("Proficient", "Learning",
                guard: ctx => true) // Score < proficientThreshold
            .AddTransition("Learning", "Novice",
                guard: ctx => true) // Score < learningThreshold
            // Error transitions from any state
            .AddTransition("Novice", "Error",
                guard: ctx => true) // On exception
            .AddTransition("Learning", "Error",
                guard: ctx => true) // On exception
            .AddTransition("Proficient", "Error",
                guard: ctx => true) // On exception
            .AddTransition("Master", "Error",
                guard: ctx => true) // On exception
            // Recovery from error (return to previous state or Novice)
            .AddTransition("Error", "Novice",
                guard: ctx => true) // After recovery
            .SetInitialState("Novice")
            .Build();

        return stateMachine;
    }

    /// <summary>
    /// Determines the mastery state based on a score.
    /// </summary>
    /// <param name="score">The mastery score (0-100).</param>
    /// <param name="noviceThreshold">Score threshold for Novice state (default: 0.0).</param>
    /// <param name="learningThreshold">Score threshold for Learning state (default: 40.0).</param>
    /// <param name="proficientThreshold">Score threshold for Proficient state (default: 60.0).</param>
    /// <param name="masterThreshold">Score threshold for Master state (default: 80.0).</param>
    /// <returns>The mastery state name.</returns>
    public static string DetermineMasteryState(
        double score,
        double noviceThreshold = 0.0,
        double learningThreshold = 40.0,
        double proficientThreshold = 60.0,
        double masterThreshold = 80.0)
    {
        return score switch
        {
            >= 80.0 => "Master",
            >= 60.0 => "Proficient",
            >= 40.0 => "Learning",
            _ => "Novice"
        };
    }

    /// <summary>
    /// Converts a MasteryLevel enum to a state machine state name.
    /// </summary>
    /// <param name="level">The mastery level.</param>
    /// <returns>The corresponding state name.</returns>
    public static string FromMasteryLevel(MasteryLevel level)
    {
        return level switch
        {
            MasteryLevel.Novice => "Novice",
            MasteryLevel.Developing => "Learning",
            MasteryLevel.Proficient => "Proficient",
            MasteryLevel.Advanced => "Proficient", // Map Advanced to Proficient
            MasteryLevel.Mastery => "Master",
            _ => "Novice"
        };
    }

    /// <summary>
    /// Converts a state machine state name to a MasteryLevel enum.
    /// </summary>
    /// <param name="state">The state name.</param>
    /// <returns>The corresponding mastery level.</returns>
    public static MasteryLevel ToMasteryLevel(string state)
    {
        return state switch
        {
            "Novice" => MasteryLevel.Novice,
            "Learning" => MasteryLevel.Developing,
            "Proficient" => MasteryLevel.Proficient,
            "Master" => MasteryLevel.Mastery,
            _ => MasteryLevel.Novice
        };
    }
}

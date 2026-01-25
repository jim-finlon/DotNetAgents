using DotNetAgents.Abstractions.Memory;
using DotNetAgents.Core.Memory.Implementations;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.StateMachines;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Memory;

/// <summary>
/// Memory store for tracking student mastery levels across concepts.
/// </summary>
public class MasteryStateMemory : InMemoryMemory, IMemoryStore
{
    private readonly Dictionary<string, Dictionary<ConceptId, ConceptMastery>> _masteryStates = new();
    private readonly Dictionary<string, Dictionary<ConceptId, MasteryContext>> _masteryContexts = new(); // studentId -> conceptId -> context
    private readonly object _masteryLock = new();
    private readonly ILogger<MasteryStateMemory>? _logger;
    private readonly IMasteryStateMachine<MasteryContext>? _stateMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="MasteryStateMemory"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="stateMachine">Optional state machine for tracking mastery progression.</param>
    public MasteryStateMemory(
        ILogger<MasteryStateMemory>? logger = null,
        IMasteryStateMachine<MasteryContext>? stateMachine = null)
    {
        _logger = logger;
        _stateMachine = stateMachine;
    }

    /// <summary>
    /// Gets the mastery level for a specific concept and student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="conceptId">The concept identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The concept mastery, or null if not found.</returns>
    public Task<ConceptMastery?> GetMasteryAsync(
        string studentId,
        ConceptId conceptId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (conceptId == null)
            throw new ArgumentNullException(nameof(conceptId));

        lock (_masteryLock)
        {
            if (_masteryStates.TryGetValue(studentId, out var studentMastery))
            {
                studentMastery.TryGetValue(conceptId, out var mastery);
                return Task.FromResult<ConceptMastery?>(mastery);
            }

            return Task.FromResult<ConceptMastery?>(null);
        }
    }

    /// <summary>
    /// Gets all mastery levels for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A dictionary mapping concept IDs to concept mastery.</returns>
    public Task<IReadOnlyDictionary<ConceptId, ConceptMastery>> GetAllMasteryAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_masteryLock)
        {
            if (_masteryStates.TryGetValue(studentId, out var studentMastery))
            {
                return Task.FromResult<IReadOnlyDictionary<ConceptId, ConceptMastery>>(
                    new Dictionary<ConceptId, ConceptMastery>(studentMastery));
            }

            return Task.FromResult<IReadOnlyDictionary<ConceptId, ConceptMastery>>(
                new Dictionary<ConceptId, ConceptMastery>());
        }
    }

    /// <summary>
    /// Updates the mastery level for a concept and student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="mastery">The concept mastery to save.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task UpdateMasteryAsync(
        string studentId,
        ConceptMastery mastery,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (mastery == null)
            throw new ArgumentNullException(nameof(mastery));

        lock (_masteryLock)
        {
            if (!_masteryStates.TryGetValue(studentId, out var studentMastery))
            {
                studentMastery = new Dictionary<ConceptId, ConceptMastery>();
                _masteryStates[studentId] = studentMastery;
            }

            // Get previous mastery to determine if state transition is needed
            var previousMastery = studentMastery.TryGetValue(mastery.ConceptId, out var prev) ? prev : null;
            studentMastery[mastery.ConceptId] = mastery;

            // Update state machine if available
            if (_stateMachine != null)
            {
                // Get or create mastery context
                if (!_masteryContexts.TryGetValue(studentId, out var studentContexts))
                {
                    studentContexts = new Dictionary<ConceptId, MasteryContext>();
                    _masteryContexts[studentId] = studentContexts;
                }

                if (!studentContexts.TryGetValue(mastery.ConceptId, out var context))
                {
                    context = new MasteryContext
                    {
                        StudentId = studentId,
                        ConceptId = mastery.ConceptId,
                        Score = mastery.Score,
                        Level = mastery.Level,
                        LastUpdated = mastery.LastUpdated
                    };
                    studentContexts[mastery.ConceptId] = context;
                }
                else
                {
                    // Update context
                    context.Score = mastery.Score;
                    context.Level = mastery.Level;
                    context.LastUpdated = mastery.LastUpdated;
                }

                // Determine target state based on score
                var targetState = MasteryStateMachinePattern.DetermineMasteryState(mastery.Score);
                var currentState = _stateMachine.CurrentState;

                // Only transition if state changed
                if (currentState != targetState)
                {
                    try
                    {
                        _stateMachine.TransitionAsync(targetState, context, cancellationToken).Wait(cancellationToken);
                        _logger?.LogDebug(
                            "Mastery state transitioned for student {StudentId}, concept {ConceptId}: {PreviousState} → {NewState}",
                            studentId,
                            mastery.ConceptId.Value,
                            currentState ?? "null",
                            targetState);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex,
                            "Failed to transition mastery state for student {StudentId}, concept {ConceptId}",
                            studentId,
                            mastery.ConceptId.Value);
                    }
                }
            }

            _logger?.LogDebug(
                "Updated mastery for student {StudentId}, concept {ConceptId}, level {Level}",
                studentId,
                mastery.ConceptId.Value,
                mastery.Level);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates multiple mastery levels for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="masteryLevels">The concept mastery levels to update.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task UpdateMasteryBatchAsync(
        string studentId,
        IReadOnlyList<ConceptMastery> masteryLevels,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (masteryLevels == null)
            throw new ArgumentNullException(nameof(masteryLevels));

        lock (_masteryLock)
        {
            if (!_masteryStates.TryGetValue(studentId, out var studentMastery))
            {
                studentMastery = new Dictionary<ConceptId, ConceptMastery>();
                _masteryStates[studentId] = studentMastery;
            }

            foreach (var mastery in masteryLevels)
            {
                studentMastery[mastery.ConceptId] = mastery;
            }

            _logger?.LogDebug(
                "Updated {Count} mastery levels for student {StudentId}",
                masteryLevels.Count,
                studentId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes mastery data for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task DeleteMasteryAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_masteryLock)
        {
            var removed = _masteryStates.Remove(studentId);
            if (removed)
            {
                _logger?.LogDebug("Deleted all mastery data for student {StudentId}", studentId);
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation - actual persistence would be handled by a persistent store
        lock (_masteryLock)
        {
            _logger?.LogDebug("SaveAsync called (in-memory, no-op)");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation - actual loading would be handled by a persistent store
        lock (_masteryLock)
        {
            _logger?.LogDebug("LoadAsync called (in-memory, no-op)");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the current mastery state for a student and concept.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="conceptId">The concept identifier.</param>
    /// <returns>The current mastery state, or null if state machine is not configured or not found.</returns>
    public string? GetMasteryState(string studentId, ConceptId conceptId)
    {
        if (_stateMachine == null)
            return null;

        lock (_masteryLock)
        {
            if (!_masteryContexts.TryGetValue(studentId, out var studentContexts))
                return null;

            if (!studentContexts.TryGetValue(conceptId, out _))
                return null;

            return _stateMachine.CurrentState;
        }
    }

    /// <summary>
    /// Gets the mastery context for a student and concept.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="conceptId">The concept identifier.</param>
    /// <returns>The mastery context, or null if not found.</returns>
    public MasteryContext? GetMasteryContext(string studentId, ConceptId conceptId)
    {
        lock (_masteryLock)
        {
            if (!_masteryContexts.TryGetValue(studentId, out var studentContexts))
                return null;

            studentContexts.TryGetValue(conceptId, out var context);
            return context;
        }
    }
}

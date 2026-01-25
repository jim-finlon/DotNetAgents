using DotNetAgents.Abstractions.Memory;
using DotNetAgents.Core.Memory.Implementations;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.StateMachines;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Memory;

/// <summary>
/// Memory store for learning sessions with resume capability.
/// </summary>
public class LearningSessionMemory : InMemoryMemory, IMemoryStore
{
    private readonly Dictionary<string, LearningSession> _sessions = new();
    private readonly Dictionary<string, string> _activeSessionsByStudent = new(); // studentId -> sessionId
    private readonly Dictionary<string, LearningSessionContext> _sessionContexts = new(); // sessionId -> context
    private readonly object _sessionLock = new();
    private readonly ILogger<LearningSessionMemory>? _logger;
    private readonly ILearningSessionStateMachine<LearningSessionContext>? _stateMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="LearningSessionMemory"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="stateMachine">Optional state machine for tracking session lifecycle.</param>
    public LearningSessionMemory(
        ILogger<LearningSessionMemory>? logger = null,
        ILearningSessionStateMachine<LearningSessionContext>? stateMachine = null)
    {
        _logger = logger;
        _stateMachine = stateMachine;
    }

    /// <summary>
    /// Creates a new learning session.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="conceptId">The concept being learned.</param>
    /// <param name="initialState">Optional initial state data.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The created learning session.</returns>
    public Task<LearningSession> CreateSessionAsync(
        string studentId,
        ConceptId conceptId,
        IDictionary<string, object>? initialState = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (conceptId == null)
            throw new ArgumentNullException(nameof(conceptId));

        lock (_sessionLock)
        {
            // End any existing active session for this student
            if (_activeSessionsByStudent.TryGetValue(studentId, out var existingSessionId))
            {
                if (_sessions.TryGetValue(existingSessionId, out var existingSession))
                {
                    var ended = existingSession with { CompletedAt = DateTimeOffset.UtcNow };
                    _sessions[existingSessionId] = ended;
                    _logger?.LogDebug(
                        "Ended existing session {SessionId} for student {StudentId}",
                        existingSessionId,
                        studentId);
                }
            }

            var session = new LearningSession
            {
                SessionId = Guid.NewGuid().ToString(),
                StudentId = studentId,
                ConceptId = conceptId,
                CurrentStep = 0,
                StateData = initialState ?? new Dictionary<string, object>(),
                StartedAt = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow
            };

            _sessions[session.SessionId] = session;
            _activeSessionsByStudent[studentId] = session.SessionId;

            // Initialize state machine context if available
            if (_stateMachine != null)
            {
                var context = new LearningSessionContext
                {
                    SessionId = session.SessionId,
                    StudentId = studentId,
                    ConceptId = conceptId,
                    CurrentStep = 0,
                    InitializedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };
                _sessionContexts[session.SessionId] = context;

                // Transition to Initialized state
                try
                {
                    _stateMachine.TransitionAsync("Initialized", context, cancellationToken).Wait(cancellationToken);
                    _logger?.LogDebug("Learning session {SessionId} transitioned to Initialized state", session.SessionId);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to transition learning session to Initialized state");
                }
            }

            _logger?.LogInformation(
                "Created learning session {SessionId} for student {StudentId}, concept {ConceptId}",
                session.SessionId,
                studentId,
                conceptId.Value);

            return Task.FromResult(session);
        }
    }

    /// <summary>
    /// Gets a learning session by ID.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The learning session, or null if not found.</returns>
    public Task<LearningSession?> GetSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

        lock (_sessionLock)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return Task.FromResult(session);
        }
    }

    /// <summary>
    /// Gets the active session for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The active learning session, or null if none exists.</returns>
    public Task<LearningSession?> GetActiveSessionAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_sessionLock)
        {
            if (_activeSessionsByStudent.TryGetValue(studentId, out var sessionId))
            {
                _sessions.TryGetValue(sessionId, out var session);
                // Verify it's still active
                if (session != null && session.IsActive)
                {
                    return Task.FromResult<LearningSession?>(session);
                }
            }

            return Task.FromResult<LearningSession?>(null);
        }
    }

    /// <summary>
    /// Updates a learning session (e.g., checkpoint, state change).
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="updateAction">Action to update the session.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The updated session.</returns>
    public Task<LearningSession> UpdateSessionAsync(
        string sessionId,
        Func<LearningSession, LearningSession> updateAction,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));
        if (updateAction == null)
            throw new ArgumentNullException(nameof(updateAction));

        lock (_sessionLock)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                throw new InvalidOperationException($"Session {sessionId} not found.");
            }

            var updated = updateAction(session) with { LastUpdated = DateTimeOffset.UtcNow };
            _sessions[sessionId] = updated;

            // If session was completed, remove from active sessions
            if (updated.CompletedAt != null && _activeSessionsByStudent.TryGetValue(updated.StudentId, out var activeSessionId) && activeSessionId == sessionId)
            {
                _activeSessionsByStudent.Remove(updated.StudentId);
            }

            _logger?.LogDebug(
                "Updated session {SessionId}, step {Step}",
                sessionId,
                updated.CurrentStep);

            return Task.FromResult(updated);
        }
    }


    /// <summary>
    /// Completes a learning session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The completed session.</returns>
    public Task<LearningSession> CompleteSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

        return UpdateSessionAsync(
            sessionId,
            session =>
            {
                if (session.CompletedAt != null)
                {
                    return session; // Already completed
                }

                // Transition to Completed state if state machine is available
                if (_stateMachine != null && _sessionContexts.TryGetValue(sessionId, out var context))
                {
                    try
                    {
                        context.CompletedAt = DateTimeOffset.UtcNow;
                        context.IsActive = false;
                        _stateMachine.TransitionAsync("Completed", context, cancellationToken).Wait(cancellationToken);
                        _logger?.LogDebug("Learning session {SessionId} transitioned to Completed state", sessionId);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to transition learning session to Completed state");
                    }
                }

                var completed = session with { CompletedAt = DateTimeOffset.UtcNow };
                _activeSessionsByStudent.Remove(completed.StudentId);

                _logger?.LogInformation(
                    "Completed session {SessionId} for student {StudentId}",
                    sessionId,
                    completed.StudentId);

                return completed;
            },
            cancellationToken);
    }

    /// <summary>
    /// Transitions a learning session to a specific state.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="state">The target state (e.g., "Learning", "Assessment", "Review").</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The updated session.</returns>
    public Task<LearningSession> TransitionToStateAsync(
        string sessionId,
        string state,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be null or empty.", nameof(state));

        if (_stateMachine == null)
        {
            throw new InvalidOperationException("State machine is not configured. Cannot transition states.");
        }

        lock (_sessionLock)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                throw new InvalidOperationException($"Session {sessionId} not found.");
            }

            if (!_sessionContexts.TryGetValue(sessionId, out var context))
            {
                throw new InvalidOperationException($"Session context {sessionId} not found.");
            }

            // Update context based on state
            switch (state)
            {
                case "Learning":
                    context.LearningStartedAt ??= DateTimeOffset.UtcNow;
                    break;
                case "Assessment":
                    context.AssessmentStartedAt ??= DateTimeOffset.UtcNow;
                    break;
                case "Review":
                    context.ReviewStartedAt ??= DateTimeOffset.UtcNow;
                    break;
                case "Paused":
                    context.PausedAt = DateTimeOffset.UtcNow;
                    context.PreviousStateBeforePause = _stateMachine.CurrentState;
                    break;
            }

            try
            {
                _stateMachine.TransitionAsync(state, context, cancellationToken).Wait(cancellationToken);
                _logger?.LogDebug("Learning session {SessionId} transitioned to {State}", sessionId, state);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to transition learning session {SessionId} to {State}", sessionId, state);
                throw;
            }

            return Task.FromResult(session);
        }
    }

    /// <summary>
    /// Pauses a learning session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The paused session.</returns>
    public Task<LearningSession> PauseSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return TransitionToStateAsync(sessionId, "Paused", cancellationToken);
    }

    /// <summary>
    /// Resumes a paused learning session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="resumeToState">Optional state to resume to. If null, resumes to previous state or Learning.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The resumed session.</returns>
    public Task<LearningSession> ResumeSessionAsync(
        string sessionId,
        string? resumeToState = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

        if (_stateMachine == null)
        {
            // Fallback to original resume logic if no state machine
            return ResumeSessionAsync(sessionId, cancellationToken);
        }

        lock (_sessionLock)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                throw new InvalidOperationException($"Session {sessionId} not found.");
            }

            if (session.CompletedAt != null)
            {
                throw new InvalidOperationException($"Cannot resume completed session {sessionId}.");
            }

            if (!_sessionContexts.TryGetValue(sessionId, out var context))
            {
                throw new InvalidOperationException($"Session context {sessionId} not found.");
            }

            // Determine resume state
            var targetState = resumeToState ?? context.PreviousStateBeforePause ?? "Learning";

            // Update context
            context.PausedAt = null;
            context.PreviousStateBeforePause = null;

            try
            {
                _stateMachine.TransitionAsync(targetState, context, cancellationToken).Wait(cancellationToken);
                _logger?.LogDebug("Learning session {SessionId} resumed to {State}", sessionId, targetState);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to resume learning session {SessionId} to {State}", sessionId, targetState);
                throw;
            }

            // Reactivate if needed
            if (!_activeSessionsByStudent.ContainsKey(session.StudentId))
            {
                _activeSessionsByStudent[session.StudentId] = sessionId;
            }

            return Task.FromResult(session);
        }
    }

    /// <summary>
    /// Gets the current state machine state for a learning session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The current state, or null if state machine is not configured or session not found.</returns>
    public string? GetSessionState(string sessionId)
    {
        if (_stateMachine == null)
            return null;

        lock (_sessionLock)
        {
            if (!_sessionContexts.ContainsKey(sessionId))
                return null;

            return _stateMachine.CurrentState;
        }
    }

    /// <summary>
    /// Gets the session context for a learning session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The session context, or null if not found.</returns>
    public LearningSessionContext? GetSessionContext(string sessionId)
    {
        lock (_sessionLock)
        {
            _sessionContexts.TryGetValue(sessionId, out var context);
            return context;
        }
    }

    /// <summary>
    /// Gets all sessions for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="includeCompleted">Whether to include completed sessions.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of learning sessions.</returns>
    public Task<IReadOnlyList<LearningSession>> GetSessionsAsync(
        string studentId,
        bool includeCompleted = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_sessionLock)
        {
            var sessions = _sessions.Values
                .Where(s => s.StudentId == studentId)
                .Where(s => includeCompleted || s.IsActive)
                .OrderByDescending(s => s.LastUpdated)
                .ToList();

            return Task.FromResult<IReadOnlyList<LearningSession>>(sessions);
        }
    }

    /// <inheritdoc/>
    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation - actual persistence would be handled by a persistent store
        lock (_sessionLock)
        {
            _logger?.LogDebug("SaveAsync called (in-memory, no-op)");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation - actual loading would be handled by a persistent store
        lock (_sessionLock)
        {
            _logger?.LogDebug("LoadAsync called (in-memory, no-op)");
        }

        return Task.CompletedTask;
    }
}

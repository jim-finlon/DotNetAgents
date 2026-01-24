using DotNetAgents.Abstractions.Memory;
using DotNetAgents.Core.Memory.Implementations;
using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Memory;

/// <summary>
/// Memory store for learning sessions with resume capability.
/// </summary>
public class LearningSessionMemory : InMemoryMemory, IMemoryStore
{
    private readonly Dictionary<string, LearningSession> _sessions = new();
    private readonly Dictionary<string, string> _activeSessionsByStudent = new(); // studentId -> sessionId
    private readonly object _sessionLock = new();
    private readonly ILogger<LearningSessionMemory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LearningSessionMemory"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public LearningSessionMemory(ILogger<LearningSessionMemory>? logger = null)
    {
        _logger = logger;
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
    /// Resumes a learning session from a checkpoint.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The resumed session.</returns>
    public Task<LearningSession> ResumeSessionAsync(
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
                    throw new InvalidOperationException($"Cannot resume completed session {sessionId}.");
                }

                // Reactivate if needed
                if (!_activeSessionsByStudent.ContainsKey(session.StudentId))
                {
                    _activeSessionsByStudent[session.StudentId] = sessionId;
                }

                return session;
            },
            cancellationToken);
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

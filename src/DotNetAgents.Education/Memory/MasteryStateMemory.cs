using DotNetAgents.Abstractions.Memory;
using DotNetAgents.Core.Memory.Implementations;
using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Memory;

/// <summary>
/// Memory store for tracking student mastery levels across concepts.
/// </summary>
public class MasteryStateMemory : InMemoryMemory, IMemoryStore
{
    private readonly Dictionary<string, Dictionary<ConceptId, ConceptMastery>> _masteryStates = new();
    private readonly object _masteryLock = new();
    private readonly ILogger<MasteryStateMemory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MasteryStateMemory"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public MasteryStateMemory(ILogger<MasteryStateMemory>? logger = null)
    {
        _logger = logger;
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

            studentMastery[mastery.ConceptId] = mastery;

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
}

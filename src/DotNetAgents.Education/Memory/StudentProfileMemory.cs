using DotNetAgents.Abstractions.Memory;
using DotNetAgents.Core.Memory.Implementations;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Memory;

/// <summary>
/// Memory store for student profile information, extending DotNetAgents IMemory.
/// </summary>
public class StudentProfileMemory : InMemoryMemory, IMemoryStore
{
    private readonly Dictionary<string, StudentProfile> _profiles = new();
    private readonly object _profileLock = new();
    private readonly ILogger<StudentProfileMemory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudentProfileMemory"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public StudentProfileMemory(ILogger<StudentProfileMemory>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates a student profile.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The student profile, or null if not found.</returns>
    public Task<StudentProfile?> GetProfileAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_profileLock)
        {
            _profiles.TryGetValue(studentId, out var profile);
            return Task.FromResult(profile);
        }
    }

    /// <summary>
    /// Saves or updates a student profile.
    /// </summary>
    /// <param name="profile">The student profile to save.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task SaveProfileAsync(
        StudentProfile profile,
        CancellationToken cancellationToken = default)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        lock (_profileLock)
        {
            var updated = profile with { LastUpdated = DateTimeOffset.UtcNow };
            _profiles[profile.StudentId] = updated;

            _logger?.LogDebug(
                "Saved profile for student {StudentId}",
                profile.StudentId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a student profile.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task DeleteProfileAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_profileLock)
        {
            var removed = _profiles.Remove(studentId);
            if (removed)
            {
                _logger?.LogDebug("Deleted profile for student {StudentId}", studentId);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all student profiles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of all student profiles.</returns>
    public Task<IReadOnlyList<StudentProfile>> GetAllProfilesAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_profileLock)
        {
            return Task.FromResult<IReadOnlyList<StudentProfile>>(
                _profiles.Values.ToList());
        }
    }

    /// <inheritdoc/>
    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation - actual persistence would be handled by a persistent store
        lock (_profileLock)
        {
            _logger?.LogDebug("SaveAsync called (in-memory, no-op)");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation - actual loading would be handled by a persistent store
        lock (_profileLock)
        {
            _logger?.LogDebug("LoadAsync called (in-memory, no-op)");
        }

        return Task.CompletedTask;
    }
}

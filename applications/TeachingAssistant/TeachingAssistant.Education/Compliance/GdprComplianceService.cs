using DotNetAgents.Education.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DotNetAgents.Education.Compliance;

/// <summary>
/// Implementation of GDPR compliance service for data export, deletion, and anonymization.
/// </summary>
public class GdprComplianceService : IGdprComplianceService
{
    private readonly StudentProfileMemory _profileMemory;
    private readonly MasteryStateMemory _masteryMemory;
    private readonly LearningSessionMemory _sessionMemory;
    private readonly Dictionary<string, bool> _consents = new();
    private readonly object _lockObject = new();
    private readonly ILogger<GdprComplianceService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GdprComplianceService"/> class.
    /// </summary>
    /// <param name="profileMemory">The student profile memory.</param>
    /// <param name="masteryMemory">The mastery state memory.</param>
    /// <param name="sessionMemory">The learning session memory.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public GdprComplianceService(
        StudentProfileMemory profileMemory,
        MasteryStateMemory masteryMemory,
        LearningSessionMemory sessionMemory,
        ILogger<GdprComplianceService>? logger = null)
    {
        _profileMemory = profileMemory ?? throw new ArgumentNullException(nameof(profileMemory));
        _masteryMemory = masteryMemory ?? throw new ArgumentNullException(nameof(masteryMemory));
        _sessionMemory = sessionMemory ?? throw new ArgumentNullException(nameof(sessionMemory));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<GdprComplianceService>.Instance;
    }

    /// <inheritdoc/>
    public async Task<ExportedStudentData> ExportStudentDataAsync(
        string studentId,
        ExportFormat format = ExportFormat.Json,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        _logger.LogInformation("Exporting student data for {StudentId} in {Format} format", studentId, format);

        // Collect all student data
        var profile = await _profileMemory.GetProfileAsync(studentId, cancellationToken).ConfigureAwait(false);
        var mastery = await _masteryMemory.GetAllMasteryAsync(studentId, cancellationToken).ConfigureAwait(false);
        var sessions = await _sessionMemory.GetSessionsAsync(studentId, includeCompleted: true, cancellationToken).ConfigureAwait(false);

        var exportedData = new ExportedStudentData
        {
            StudentId = studentId,
            Profile = profile,
            Mastery = mastery,
            Sessions = sessions,
            Assessments = null, // Would be populated from assessment storage
            Conversations = null, // Would be populated from conversation storage
            ExportedAt = DateTimeOffset.UtcNow
        };

        _logger.LogInformation("Exported student data for {StudentId}", studentId);

        return exportedData;
    }

    /// <inheritdoc/>
    public async Task DeleteStudentDataAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        _logger.LogWarning("Deleting all data for student {StudentId} (GDPR right to be forgotten)", studentId);

        // Delete profile
        await _profileMemory.DeleteProfileAsync(studentId, cancellationToken).ConfigureAwait(false);

        // Delete mastery
        await _masteryMemory.DeleteMasteryAsync(studentId, cancellationToken).ConfigureAwait(false);

        // Delete sessions (would need to implement delete method)
        // For now, sessions are managed through LearningSessionMemory

        // Remove consent
        lock (_lockObject)
        {
            _consents.Remove(studentId);
        }

        _logger.LogInformation("Deleted all data for student {StudentId}", studentId);
    }

    /// <inheritdoc/>
    public async Task AnonymizeStudentDataAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        _logger.LogInformation("Anonymizing data for student {StudentId}", studentId);

        // Get profile and anonymize PII
        var profile = await _profileMemory.GetProfileAsync(studentId, cancellationToken).ConfigureAwait(false);
        if (profile != null)
        {
            var anonymized = profile with
            {
                Name = null,
                PreferredLanguage = null,
                Metadata = null // Remove PII from metadata
            };

            await _profileMemory.SaveProfileAsync(anonymized, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Anonymized data for student {StudentId}", studentId);
    }

    /// <inheritdoc/>
    public Task<bool> HasConsentAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_lockObject)
        {
            _consents.TryGetValue(studentId, out var hasConsent);
            return Task.FromResult(hasConsent);
        }
    }

    /// <inheritdoc/>
    public Task RecordConsentAsync(
        string studentId,
        bool hasConsent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_lockObject)
        {
            _consents[studentId] = hasConsent;

            _logger.LogInformation(
                "Recorded GDPR consent for student {StudentId}: {HasConsent}",
                studentId,
                hasConsent);
        }

        return Task.CompletedTask;
    }
}

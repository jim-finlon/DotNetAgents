using DotNetAgents.Security.Audit;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Compliance;

/// <summary>
/// Implementation of education-specific audit logging.
/// </summary>
public class EducationAuditLogger : IEducationAuditLogger
{
    private readonly List<EducationAuditEntry> _auditEntries = new();
    private readonly object _lockObject = new();
    private readonly ILogger<EducationAuditLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EducationAuditLogger"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public EducationAuditLogger(ILogger<EducationAuditLogger>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EducationAuditLogger>.Instance;
    }

    /// <inheritdoc/>
    public Task LogAuditEventAsync(
        AuditEventType eventType,
        string message,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));

        lock (_lockObject)
        {
            var entry = new EducationAuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                EventType = MapToEducationEventType(eventType),
                Message = message,
                Timestamp = DateTimeOffset.UtcNow,
                Metadata = metadata
            };

            _auditEntries.Add(entry);

            _logger.LogInformation(
                "Audit event logged: Type {Type}, Message {Message}",
                eventType,
                message);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task LogStudentInteractionAsync(
        string studentId,
        string interactionType,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (string.IsNullOrWhiteSpace(interactionType))
            throw new ArgumentException("Interaction type cannot be null or empty.", nameof(interactionType));

        var enhancedMetadata = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>())
        {
            ["interaction_type"] = interactionType
        };

        return LogEducationEventAsync(
            EducationAuditEventType.StudentInteraction,
            studentId,
            $"Student interaction: {interactionType}",
            enhancedMetadata,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task LogContentAccessAsync(
        string studentId,
        string contentId,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentException("Content ID cannot be null or empty.", nameof(contentId));

        var enhancedMetadata = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>())
        {
            ["content_id"] = contentId
        };

        return LogEducationEventAsync(
            EducationAuditEventType.ContentAccess,
            studentId,
            $"Content accessed: {contentId}",
            enhancedMetadata,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task LogAssessmentSubmissionAsync(
        string studentId,
        string assessmentId,
        double score,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (string.IsNullOrWhiteSpace(assessmentId))
            throw new ArgumentException("Assessment ID cannot be null or empty.", nameof(assessmentId));

        var enhancedMetadata = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>())
        {
            ["assessment_id"] = assessmentId,
            ["score"] = score
        };

        return LogEducationEventAsync(
            EducationAuditEventType.AssessmentSubmission,
            studentId,
            $"Assessment submitted: {assessmentId}, Score: {score}",
            enhancedMetadata,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task LogSafetyAlertAsync(
        string studentId,
        string alertType,
        string severity,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (string.IsNullOrWhiteSpace(alertType))
            throw new ArgumentException("Alert type cannot be null or empty.", nameof(alertType));
        if (string.IsNullOrWhiteSpace(severity))
            throw new ArgumentException("Severity cannot be null or empty.", nameof(severity));

        var enhancedMetadata = new Dictionary<string, object>(metadata ?? new Dictionary<string, object>())
        {
            ["alert_type"] = alertType,
            ["severity"] = severity
        };

        return LogEducationEventAsync(
            EducationAuditEventType.SafetyAlert,
            studentId,
            $"Safety alert: {alertType}, Severity: {severity}",
            enhancedMetadata,
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<EducationAuditEntry>> GetAuditEntriesAsync(
        string studentId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_lockObject)
        {
            var entries = _auditEntries
                .Where(e => e.StudentId == studentId)
                .Where(e => startDate == null || e.Timestamp >= startDate.Value)
                .Where(e => endDate == null || e.Timestamp <= endDate.Value)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            _logger.LogDebug(
                "Retrieved {Count} audit entries for student {StudentId}",
                entries.Count,
                studentId);

            return Task.FromResult<IReadOnlyList<EducationAuditEntry>>(entries);
        }
    }

    private Task LogEducationEventAsync(
        EducationAuditEventType eventType,
        string? studentId,
        string message,
        IDictionary<string, object>? metadata,
        CancellationToken cancellationToken)
    {
        lock (_lockObject)
        {
            var entry = new EducationAuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                EventType = eventType,
                StudentId = studentId,
                Message = message,
                Timestamp = DateTimeOffset.UtcNow,
                Metadata = metadata
            };

            _auditEntries.Add(entry);

            _logger.LogInformation(
                "Education audit event logged: Type {Type}, Student {StudentId}, Message {Message}",
                eventType,
                studentId ?? "N/A",
                message);
        }

        return Task.CompletedTask;
    }

    private static EducationAuditEventType MapToEducationEventType(AuditEventType eventType)
    {
        return eventType switch
        {
            AuditEventType.SecurityEvent => EducationAuditEventType.SafetyAlert,
            AuditEventType.StateModification => EducationAuditEventType.StudentInteraction,
            _ => EducationAuditEventType.StudentInteraction
        };
    }
}

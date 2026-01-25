using DotNetAgents.Security.Audit;

namespace DotNetAgents.Education.Compliance;

/// <summary>
/// Represents an education-specific audit event.
/// </summary>
public enum EducationAuditEventType
{
    /// <summary>
    /// Student interaction event.
    /// </summary>
    StudentInteraction = 100,

    /// <summary>
    /// Content access event.
    /// </summary>
    ContentAccess = 101,

    /// <summary>
    /// Assessment submission event.
    /// </summary>
    AssessmentSubmission = 102,

    /// <summary>
    /// Safety alert event.
    /// </summary>
    SafetyAlert = 103,

    /// <summary>
    /// Mastery achievement event.
    /// </summary>
    MasteryAchieved = 104,

    /// <summary>
    /// Data export event.
    /// </summary>
    DataExport = 105,

    /// <summary>
    /// Data deletion event.
    /// </summary>
    DataDeletion = 106,

    /// <summary>
    /// Parent consent event.
    /// </summary>
    ParentConsent = 107
}

/// <summary>
/// Represents an education audit log entry.
/// </summary>
public record EducationAuditEntry
{
    /// <summary>
    /// Gets the log entry identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the event type.
    /// </summary>
    public EducationAuditEventType EventType { get; init; }

    /// <summary>
    /// Gets the student identifier (if applicable).
    /// </summary>
    public string? StudentId { get; init; }

    /// <summary>
    /// Gets the user identifier who triggered the event.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the event message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp of the event.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets additional metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Interface for education-specific audit logging.
/// </summary>
public interface IEducationAuditLogger : IAuditLogger
{
    /// <summary>
    /// Logs a student interaction event.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="interactionType">The type of interaction.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task LogStudentInteractionAsync(
        string studentId,
        string interactionType,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a content access event.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="contentId">The content identifier.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task LogContentAccessAsync(
        string studentId,
        string contentId,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an assessment submission event.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="assessmentId">The assessment identifier.</param>
    /// <param name="score">The score achieved.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task LogAssessmentSubmissionAsync(
        string studentId,
        string assessmentId,
        double score,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a safety alert event.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="alertType">The alert type.</param>
    /// <param name="severity">The alert severity.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task LogSafetyAlertAsync(
        string studentId,
        string alertType,
        string severity,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit entries for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="startDate">The start date for the query.</param>
    /// <param name="endDate">The end date for the query.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of audit entries.</returns>
    Task<IReadOnlyList<EducationAuditEntry>> GetAuditEntriesAsync(
        string studentId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);
}

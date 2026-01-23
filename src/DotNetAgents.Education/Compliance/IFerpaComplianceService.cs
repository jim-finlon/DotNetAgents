namespace DotNetAgents.Education.Compliance;

/// <summary>
/// Represents an access log entry for FERPA compliance.
/// </summary>
public record AccessLogEntry
{
    /// <summary>
    /// Gets the log entry identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the student identifier.
    /// </summary>
    public string StudentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user identifier who accessed the data.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the type of access (view, modify, delete, export).
    /// </summary>
    public AccessType AccessType { get; init; }

    /// <summary>
    /// Gets the resource that was accessed.
    /// </summary>
    public string Resource { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp of the access.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the reason for access.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Gets whether parent consent was obtained.
    /// </summary>
    public bool HasParentConsent { get; init; }
}

/// <summary>
/// Represents the type of access to student data.
/// </summary>
public enum AccessType
{
    /// <summary>
    /// View access.
    /// </summary>
    View,

    /// <summary>
    /// Modify access.
    /// </summary>
    Modify,

    /// <summary>
    /// Delete access.
    /// </summary>
    Delete,

    /// <summary>
    /// Export access.
    /// </summary>
    Export
}

/// <summary>
/// Interface for FERPA compliance services.
/// </summary>
public interface IFerpaComplianceService
{
    /// <summary>
    /// Checks if a user has permission to access student data.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="accessType">The type of access requested.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if access is permitted, false otherwise.</returns>
    Task<bool> CheckAccessPermissionAsync(
        string userId,
        string studentId,
        AccessType accessType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an access to student data.
    /// </summary>
    /// <param name="entry">The access log entry.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task LogAccessAsync(
        AccessLogEntry entry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets access logs for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="startDate">The start date for the query.</param>
    /// <param name="endDate">The end date for the query.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of access log entries.</returns>
    Task<IReadOnlyList<AccessLogEntry>> GetAccessLogsAsync(
        string studentId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if parent consent has been obtained for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if consent has been obtained, false otherwise.</returns>
    Task<bool> HasParentConsentAsync(
        string studentId,
        CancellationToken cancellationToken = default);
}

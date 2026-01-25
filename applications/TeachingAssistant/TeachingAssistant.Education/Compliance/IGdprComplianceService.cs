namespace DotNetAgents.Education.Compliance;

/// <summary>
/// Represents a data export request for GDPR compliance.
/// </summary>
public record DataExportRequest
{
    /// <summary>
    /// Gets the request identifier.
    /// </summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the student identifier.
    /// </summary>
    public string StudentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the requested format (JSON, CSV, XML).
    /// </summary>
    public ExportFormat Format { get; init; } = ExportFormat.Json;

    /// <summary>
    /// Gets when the request was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets when the export was completed (null if pending).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Gets the status of the export.
    /// </summary>
    public ExportStatus Status { get; init; } = ExportStatus.Pending;

    /// <summary>
    /// Gets the download URL for the exported data (if completed).
    /// </summary>
    public string? DownloadUrl { get; init; }
}

/// <summary>
/// Represents the format for data export.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// JSON format.
    /// </summary>
    Json,

    /// <summary>
    /// CSV format.
    /// </summary>
    Csv,

    /// <summary>
    /// XML format.
    /// </summary>
    Xml
}

/// <summary>
/// Represents the status of a data export.
/// </summary>
public enum ExportStatus
{
    /// <summary>
    /// Export is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// Export is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Export completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Export failed.
    /// </summary>
    Failed
}

/// <summary>
/// Represents exported student data.
/// </summary>
public record ExportedStudentData
{
    /// <summary>
    /// Gets the student identifier.
    /// </summary>
    public string StudentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the profile data.
    /// </summary>
    public object? Profile { get; init; }

    /// <summary>
    /// Gets the mastery data.
    /// </summary>
    public object? Mastery { get; init; }

    /// <summary>
    /// Gets the session data.
    /// </summary>
    public object? Sessions { get; init; }

    /// <summary>
    /// Gets the assessment data.
    /// </summary>
    public object? Assessments { get; init; }

    /// <summary>
    /// Gets the conversation data.
    /// </summary>
    public object? Conversations { get; init; }

    /// <summary>
    /// Gets the export timestamp.
    /// </summary>
    public DateTimeOffset ExportedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Interface for GDPR compliance services.
/// </summary>
public interface IGdprComplianceService
{
    /// <summary>
    /// Exports all data for a student in the requested format.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="format">The export format.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The exported data.</returns>
    Task<ExportedStudentData> ExportStudentDataAsync(
        string studentId,
        ExportFormat format = ExportFormat.Json,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all data for a student (right to be forgotten).
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task DeleteStudentDataAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Anonymizes student data (removes PII while preserving analytics).
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task AnonymizeStudentDataAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if consent has been obtained for data processing.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if consent has been obtained, false otherwise.</returns>
    Task<bool> HasConsentAsync(
        string studentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records consent for data processing.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="hasConsent">Whether consent has been obtained.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task RecordConsentAsync(
        string studentId,
        bool hasConsent,
        CancellationToken cancellationToken = default);
}

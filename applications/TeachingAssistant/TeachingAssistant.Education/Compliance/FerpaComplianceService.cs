using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Compliance;

/// <summary>
/// Implementation of FERPA compliance service for educational data access control.
/// </summary>
public class FerpaComplianceService : IFerpaComplianceService
{
    private readonly IEducationAuthorizationService _authorizationService;
    private readonly List<AccessLogEntry> _accessLogs = new();
    private readonly Dictionary<string, bool> _parentConsents = new();
    private readonly object _lockObject = new();
    private readonly ILogger<FerpaComplianceService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FerpaComplianceService"/> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service for permission checking.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public FerpaComplianceService(
        IEducationAuthorizationService authorizationService,
        ILogger<FerpaComplianceService>? logger = null)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<FerpaComplianceService>.Instance;
    }

    /// <inheritdoc/>
    public async Task<bool> CheckAccessPermissionAsync(
        string userId,
        string studentId,
        AccessType accessType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        _logger.LogDebug(
            "Checking FERPA access permission: User {UserId}, Student {StudentId}, Type {AccessType}",
            userId,
            studentId,
            accessType);

        // Check authorization service for permission
        var hasPermission = await _authorizationService.CheckPermissionAsync(
            userId,
            $"ferpa:{accessType.ToString().ToLowerInvariant()}:{studentId}",
            cancellationToken).ConfigureAwait(false);

        // For export and delete, also check parent consent
        if (hasPermission && (accessType == AccessType.Export || accessType == AccessType.Delete))
        {
            var hasConsent = await HasParentConsentAsync(studentId, cancellationToken).ConfigureAwait(false);
            if (!hasConsent)
            {
                _logger.LogWarning(
                    "Access denied: Parent consent not obtained for student {StudentId}",
                    studentId);
                return false;
            }
        }

        _logger.LogInformation(
            "FERPA access permission: User {UserId}, Student {StudentId}, Type {AccessType}, Result {Result}",
            userId,
            studentId,
            accessType,
            hasPermission ? "GRANTED" : "DENIED");

        return hasPermission;
    }

    /// <inheritdoc/>
    public Task LogAccessAsync(
        AccessLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        lock (_lockObject)
        {
            _accessLogs.Add(entry);

            _logger.LogInformation(
                "Logged FERPA access: User {UserId}, Student {StudentId}, Type {AccessType}, Resource {Resource}",
                entry.UserId,
                entry.StudentId,
                entry.AccessType,
                entry.Resource);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<AccessLogEntry>> GetAccessLogsAsync(
        string studentId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_lockObject)
        {
            var logs = _accessLogs
                .Where(log => log.StudentId == studentId)
                .Where(log => startDate == null || log.Timestamp >= startDate.Value)
                .Where(log => endDate == null || log.Timestamp <= endDate.Value)
                .OrderByDescending(log => log.Timestamp)
                .ToList();

            _logger.LogDebug(
                "Retrieved {Count} access logs for student {StudentId}",
                logs.Count,
                studentId);

            return Task.FromResult<IReadOnlyList<AccessLogEntry>>(logs);
        }
    }

    /// <inheritdoc/>
    public Task<bool> HasParentConsentAsync(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_lockObject)
        {
            _parentConsents.TryGetValue(studentId, out var hasConsent);
            return Task.FromResult(hasConsent);
        }
    }

    /// <summary>
    /// Records parent consent for a student.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="hasConsent">Whether consent has been obtained.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task RecordParentConsentAsync(
        string studentId,
        bool hasConsent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));

        lock (_lockObject)
        {
            _parentConsents[studentId] = hasConsent;

            _logger.LogInformation(
                "Recorded parent consent for student {StudentId}: {HasConsent}",
                studentId,
                hasConsent);
        }

        return Task.CompletedTask;
    }
}

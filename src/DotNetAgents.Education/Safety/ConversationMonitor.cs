using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Safety;

/// <summary>
/// Represents a turn in a conversation for monitoring.
/// </summary>
public record ConversationTurn
{
    /// <summary>
    /// Gets the student's message.
    /// </summary>
    public string StudentMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets the assistant's response.
    /// </summary>
    public string AssistantResponse { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp of the turn.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents the result of conversation monitoring.
/// </summary>
public record MonitoringResult
{
    /// <summary>
    /// Gets whether any alerts were generated.
    /// </summary>
    public bool HasAlerts { get; init; }

    /// <summary>
    /// Gets the alerts generated from this monitoring check.
    /// </summary>
    public IReadOnlyList<MonitoringAlert> Alerts { get; init; } = Array.Empty<MonitoringAlert>();
}

/// <summary>
/// Represents a monitoring alert for concerning content.
/// </summary>
public record MonitoringAlert
{
    /// <summary>
    /// Gets the alert identifier.
    /// </summary>
    public string AlertId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the student identifier.
    /// </summary>
    public string StudentId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the conversation identifier.
    /// </summary>
    public string ConversationId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the severity level of the alert.
    /// </summary>
    public AlertSeverity Severity { get; init; }

    /// <summary>
    /// Gets the alert type.
    /// </summary>
    public AlertType Type { get; init; }

    /// <summary>
    /// Gets the alert message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the flagged content.
    /// </summary>
    public string FlaggedContent { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when the alert was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets additional metadata about the alert.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Represents the severity level of a monitoring alert.
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Low severity - informational.
    /// </summary>
    Low,

    /// <summary>
    /// Medium severity - requires attention.
    /// </summary>
    Medium,

    /// <summary>
    /// High severity - requires immediate attention.
    /// </summary>
    High,

    /// <summary>
    /// Critical severity - requires immediate intervention.
    /// </summary>
    Critical
}

/// <summary>
/// Represents the type of monitoring alert.
/// </summary>
public enum AlertType
{
    /// <summary>
    /// Potential bullying detected.
    /// </summary>
    Bullying,

    /// <summary>
    /// Self-harm indicators detected.
    /// </summary>
    SelfHarm,

    /// <summary>
    /// Abuse indicators detected.
    /// </summary>
    Abuse,

    /// <summary>
    /// Distress signals detected.
    /// </summary>
    Distress,

    /// <summary>
    /// Other concerning content.
    /// </summary>
    Other
}

/// <summary>
/// Represents a date range for querying alerts.
/// </summary>
public record DateRange
{
    /// <summary>
    /// Gets the start date.
    /// </summary>
    public DateTimeOffset Start { get; init; }

    /// <summary>
    /// Gets the end date.
    /// </summary>
    public DateTimeOffset End { get; init; }
}

/// <summary>
/// Interface for monitoring conversations for concerning content.
/// </summary>
public interface IConversationMonitor
{
    /// <summary>
    /// Monitors a conversation turn for concerning content.
    /// </summary>
    /// <param name="conversationId">The conversation identifier.</param>
    /// <param name="turn">The conversation turn to monitor.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A monitoring result with any alerts.</returns>
    Task<MonitoringResult> MonitorAsync(
        string conversationId,
        ConversationTurn turn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets alerts for a student within a date range.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="range">The date range to query.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of monitoring alerts.</returns>
    Task<IReadOnlyList<MonitoringAlert>> GetAlertsAsync(
        string studentId,
        DateRange range,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of conversation monitoring for detecting concerning content.
/// </summary>
public class ConversationMonitor : IConversationMonitor
{
    private readonly ILogger<ConversationMonitor> _logger;
    private readonly List<MonitoringAlert> _alertHistory = new();
    private readonly object _lockObject = new();

    // Distress signal patterns
    private readonly IReadOnlySet<string> _distressPatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "want to die", "kill myself", "end it all", "no reason to live",
        "hurt myself", "cut myself", "suicide", "self-harm",
        "nobody cares", "everyone hates me", "worthless", "hopeless"
    };

    private readonly IReadOnlySet<string> _bullyingPatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "hate you", "you're stupid", "you're dumb", "nobody likes you",
        "everyone laughs at you", "you're weird", "you're a loser"
    };

    private readonly IReadOnlySet<string> _abusePatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "hurt me", "hit me", "abuse", "scared of", "afraid of",
        "don't tell", "keep secret", "threatened"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ConversationMonitor"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public ConversationMonitor(ILogger<ConversationMonitor>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ConversationMonitor>.Instance;
    }

    /// <inheritdoc/>
    public Task<MonitoringResult> MonitorAsync(
        string conversationId,
        ConversationTurn turn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation ID cannot be null or empty.", nameof(conversationId));
        if (turn == null)
            throw new ArgumentNullException(nameof(turn));

        _logger.LogDebug("Monitoring conversation {ConversationId}", conversationId);

        var alerts = new List<MonitoringAlert>();
        var lowerMessage = turn.StudentMessage.ToLowerInvariant();

        // Check for self-harm indicators (CRITICAL)
        if (_distressPatterns.Any(pattern => lowerMessage.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            var alert = new MonitoringAlert
            {
                StudentId = string.Empty, // Would be populated from context
                ConversationId = conversationId,
                Severity = AlertSeverity.Critical,
                Type = AlertType.SelfHarm,
                Message = "Potential self-harm indicators detected in student message",
                FlaggedContent = turn.StudentMessage,
                Timestamp = turn.Timestamp
            };
            alerts.Add(alert);
            StoreAlert(alert);
        }

        // Check for bullying indicators (HIGH)
        if (_bullyingPatterns.Any(pattern => lowerMessage.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            var alert = new MonitoringAlert
            {
                StudentId = string.Empty,
                ConversationId = conversationId,
                Severity = AlertSeverity.High,
                Type = AlertType.Bullying,
                Message = "Potential bullying language detected",
                FlaggedContent = turn.StudentMessage,
                Timestamp = turn.Timestamp
            };
            alerts.Add(alert);
            StoreAlert(alert);
        }

        // Check for abuse indicators (CRITICAL)
        if (_abusePatterns.Any(pattern => lowerMessage.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            var alert = new MonitoringAlert
            {
                StudentId = string.Empty,
                ConversationId = conversationId,
                Severity = AlertSeverity.Critical,
                Type = AlertType.Abuse,
                Message = "Potential abuse indicators detected",
                FlaggedContent = turn.StudentMessage,
                Timestamp = turn.Timestamp
            };
            alerts.Add(alert);
            StoreAlert(alert);
        }

        // Check for general distress (MEDIUM)
        if (ContainsDistressKeywords(lowerMessage))
        {
            var alert = new MonitoringAlert
            {
                StudentId = string.Empty,
                ConversationId = conversationId,
                Severity = AlertSeverity.Medium,
                Type = AlertType.Distress,
                Message = "Student appears to be in distress",
                FlaggedContent = turn.StudentMessage,
                Timestamp = turn.Timestamp
            };
            alerts.Add(alert);
            StoreAlert(alert);
        }

        if (alerts.Count > 0)
        {
            _logger.LogWarning(
                "Generated {Count} alerts for conversation {ConversationId}",
                alerts.Count,
                conversationId);
        }

        return Task.FromResult(new MonitoringResult
        {
            HasAlerts = alerts.Count > 0,
            Alerts = alerts
        });
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<MonitoringAlert>> GetAlertsAsync(
        string studentId,
        DateRange range,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
        if (range == null)
            throw new ArgumentNullException(nameof(range));

        lock (_lockObject)
        {
            var alerts = _alertHistory
                .Where(a => a.StudentId == studentId || string.IsNullOrEmpty(a.StudentId))
                .Where(a => a.Timestamp >= range.Start && a.Timestamp <= range.End)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            _logger.LogDebug(
                "Retrieved {Count} alerts for student {StudentId} in date range",
                alerts.Count,
                studentId);

            return Task.FromResult<IReadOnlyList<MonitoringAlert>>(alerts);
        }
    }

    private void StoreAlert(MonitoringAlert alert)
    {
        lock (_lockObject)
        {
            _alertHistory.Add(alert);
        }
    }

    private static bool ContainsDistressKeywords(string text)
    {
        var distressKeywords = new[] { "sad", "lonely", "scared", "worried", "anxious", "stressed", "upset" };
        return distressKeywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}

namespace TeachingAssistant.AdminUI.Models;

public record SystemMetricsDto(
    int ActiveStudents,
    int ActiveSessions,
    int PendingAssessments,
    decimal AverageResponseTime,
    Dictionary<string, object>? AdditionalMetrics,
    DateTimeOffset Timestamp);

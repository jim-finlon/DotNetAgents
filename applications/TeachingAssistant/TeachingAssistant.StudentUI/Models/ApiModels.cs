using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.StudentUI.Models;

public record StudentDto(
    Guid Id,
    Guid FamilyId,
    string Name,
    DateOnly? Birthdate,
    int? GradeLevel,
    string? AvatarId,
    Dictionary<string, object> Preferences,
    Dictionary<string, object> AccessibilitySettings,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record SubjectProgressDto(
    Guid StudentId,
    Subject Subject,
    Guid? CurrentUnitId,
    decimal OverallProgress,
    int TotalTimeMinutes,
    DateTimeOffset? LastActivityAt,
    int StreakDays,
    int LongestStreak,
    Dictionary<string, object> Settings);

public record WorkflowSessionDto(
    Guid SessionId,
    string WorkflowType,
    string StudentId,
    Guid ContentUnitId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public record WorkflowStateDto(
    Guid SessionId,
    string CurrentNode,
    string Status,
    Dictionary<string, object> State,
    bool IsComplete,
    DateTimeOffset LastUpdated);

public record StartWorkflowRequest(
    string StudentId,
    Guid ContentUnitId);

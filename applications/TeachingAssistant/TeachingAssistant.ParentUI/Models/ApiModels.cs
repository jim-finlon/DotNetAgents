using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.ParentUI.Models;

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

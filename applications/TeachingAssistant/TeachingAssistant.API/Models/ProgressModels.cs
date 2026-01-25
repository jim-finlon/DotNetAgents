using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Models;

/// <summary>
/// Subject progress data transfer object.
/// </summary>
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

/// <summary>
/// Request model for updating progress.
/// </summary>
public record UpdateProgressRequest(
    Guid? CurrentUnitId,
    int? TimeSpentMinutes);

/// <summary>
/// Content mastery data transfer object.
/// </summary>
public record ContentMasteryDto(
    Guid Id,
    Guid StudentId,
    Guid ContentUnitId,
    MasteryLevel MasteryLevel,
    decimal? MasteryScore,
    int Attempts,
    int CorrectAttempts,
    int TotalTimeSeconds,
    DateTimeOffset? FirstSeenAt,
    DateTimeOffset? LastReviewedAt,
    DateTimeOffset? NextReviewAt);

/// <summary>
/// Request model for updating mastery.
/// </summary>
public record UpdateMasteryRequest(
    MasteryLevel? MasteryLevel,
    decimal? MasteryScore,
    int? Attempts,
    int? CorrectAttempts,
    int? TimeSpentSeconds);

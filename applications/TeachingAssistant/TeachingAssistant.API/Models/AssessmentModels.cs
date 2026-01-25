using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Models;

/// <summary>
/// Assessment data transfer object.
/// </summary>
public record AssessmentDto(
    Guid Id,
    Guid ContentUnitId,
    string Title,
    string? Description,
    AssessmentType Type,
    int? TimeLimitMinutes,
    int TotalPoints,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>
/// Request model for creating an assessment.
/// </summary>
public record CreateAssessmentRequest(
    Guid ContentUnitId,
    string Title,
    string? Description,
    AssessmentType Type,
    int? TimeLimitMinutes,
    int TotalPoints);

/// <summary>
/// Request model for submitting an assessment response.
/// </summary>
public record SubmitAssessmentRequest(
    Dictionary<Guid, string> Responses); // AssessmentItemId -> Response text

/// <summary>
/// Assessment result data transfer object.
/// </summary>
public record AssessmentResultDto(
    Guid Id,
    Guid AssessmentId,
    Guid StudentId,
    decimal Score,
    decimal Percentage,
    int TotalQuestions,
    int CorrectAnswers,
    DateTimeOffset CompletedAt,
    Dictionary<string, object>? Feedback);

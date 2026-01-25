namespace DotNetAgents.Education.Models;

/// <summary>
/// Tutor response model for tutoring interactions.
/// </summary>
public record TutorResponse(
    string StudentId,
    string Message,
    string? QuestionId,
    string? Hint,
    Dictionary<string, object>? Metadata,
    DateTimeOffset Timestamp);

using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Models;

/// <summary>
/// Student data transfer object.
/// </summary>
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

/// <summary>
/// Request model for creating a student.
/// </summary>
public record CreateStudentRequest(
    Guid FamilyId,
    string Name,
    DateOnly? Birthdate,
    int? GradeLevel,
    string? AvatarId,
    Dictionary<string, object>? Preferences,
    Dictionary<string, object>? AccessibilitySettings);

/// <summary>
/// Request model for updating a student.
/// </summary>
public record UpdateStudentRequest(
    string? Name,
    DateOnly? Birthdate,
    int? GradeLevel,
    string? AvatarId,
    Dictionary<string, object>? Preferences,
    Dictionary<string, object>? AccessibilitySettings);

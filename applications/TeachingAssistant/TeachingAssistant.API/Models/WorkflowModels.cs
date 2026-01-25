namespace TeachingAssistant.API.Models;

/// <summary>
/// Workflow session data transfer object.
/// </summary>
public record WorkflowSessionDto(
    Guid SessionId,
    string WorkflowType,
    string StudentId,
    Guid ContentUnitId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

/// <summary>
/// Workflow state data transfer object.
/// </summary>
public record WorkflowStateDto(
    Guid SessionId,
    string CurrentNode,
    string Status,
    Dictionary<string, object> State,
    bool IsComplete,
    DateTimeOffset LastUpdated);

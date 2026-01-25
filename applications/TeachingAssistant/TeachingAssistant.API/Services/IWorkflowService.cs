using TeachingAssistant.API.Models;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service interface for workflow execution.
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Starts a Socratic tutoring session.
    /// </summary>
    Task<WorkflowSessionDto> StartSocraticTutoringAsync(string studentId, Guid contentUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a lesson delivery session.
    /// </summary>
    Task<WorkflowSessionDto> StartLessonDeliveryAsync(string studentId, Guid contentUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Continues a workflow session from checkpoint.
    /// </summary>
    Task<WorkflowStateDto> ContinueWorkflowAsync(Guid sessionId, string? input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflow session state.
    /// </summary>
    Task<WorkflowStateDto?> GetWorkflowStateAsync(Guid sessionId, CancellationToken cancellationToken = default);
}

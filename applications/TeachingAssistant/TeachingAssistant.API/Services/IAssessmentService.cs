using TeachingAssistant.API.Models;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service interface for assessment operations.
/// </summary>
public interface IAssessmentService
{
    /// <summary>
    /// Gets an assessment by ID.
    /// </summary>
    Task<AssessmentDto?> GetAssessmentByIdAsync(Guid assessmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assessments for a content unit.
    /// </summary>
    Task<IEnumerable<AssessmentDto>> GetAssessmentsByContentUnitAsync(Guid contentUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new assessment.
    /// </summary>
    Task<AssessmentDto> CreateAssessmentAsync(CreateAssessmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits a student response to an assessment.
    /// </summary>
    Task<AssessmentResultDto> SubmitAssessmentResponseAsync(Guid assessmentId, Guid studentId, SubmitAssessmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assessment results for a student.
    /// </summary>
    Task<IEnumerable<AssessmentResultDto>> GetAssessmentResultsAsync(Guid studentId, Guid? assessmentId = null, CancellationToken cancellationToken = default);
}

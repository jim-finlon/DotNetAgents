using TeachingAssistant.API.Models;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service interface for student progress tracking operations.
/// </summary>
public interface IProgressService
{
    /// <summary>
    /// Gets progress for a student in a specific subject.
    /// </summary>
    Task<SubjectProgressDto?> GetSubjectProgressAsync(Guid studentId, Subject subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all subject progress for a student.
    /// </summary>
    Task<IEnumerable<SubjectProgressDto>> GetAllSubjectProgressAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates progress for a student in a subject.
    /// </summary>
    Task<SubjectProgressDto> UpdateSubjectProgressAsync(Guid studentId, Subject subject, UpdateProgressRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mastery records for a student.
    /// </summary>
    Task<IEnumerable<ContentMasteryDto>> GetContentMasteryAsync(Guid studentId, Guid? contentUnitId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates mastery for a content unit.
    /// </summary>
    Task<ContentMasteryDto> UpdateContentMasteryAsync(Guid studentId, Guid contentUnitId, UpdateMasteryRequest request, CancellationToken cancellationToken = default);
}

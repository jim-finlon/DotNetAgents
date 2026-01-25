using TeachingAssistant.API.Models;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service interface for student management operations.
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Gets a student by ID.
    /// </summary>
    Task<StudentDto?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all students for a family.
    /// </summary>
    Task<IEnumerable<StudentDto>> GetStudentsByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new student.
    /// </summary>
    Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing student.
    /// </summary>
    Task<StudentDto> UpdateStudentAsync(Guid studentId, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a student (soft delete for COPPA compliance).
    /// </summary>
    Task<bool> DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
}

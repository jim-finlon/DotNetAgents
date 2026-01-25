using Microsoft.EntityFrameworkCore;
using TeachingAssistant.API.Models;
using TeachingAssistant.Data;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service implementation for student management operations.
/// </summary>
public class StudentService : IStudentService
{
    private readonly TeachingAssistantDbContext _dbContext;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        TeachingAssistantDbContext dbContext,
        ILogger<StudentService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StudentDto?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student == null)
        {
            return null;
        }

        return MapToDto(student);
    }

    public async Task<IEnumerable<StudentDto>> GetStudentsByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        var students = await _dbContext.Students
            .Where(s => s.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        return students.Select(MapToDto);
    }

    public async Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FamilyId = request.FamilyId,
            Name = request.Name,
            Birthdate = request.Birthdate,
            GradeLevel = request.GradeLevel,
            AvatarId = request.AvatarId,
            Preferences = request.Preferences ?? new Dictionary<string, object>(),
            AccessibilitySettings = request.AccessibilitySettings ?? new Dictionary<string, object>(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created student {StudentId} for family {FamilyId}", student.Id, request.FamilyId);

        return MapToDto(student);
    }

    public async Task<StudentDto> UpdateStudentAsync(Guid studentId, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student == null)
        {
            throw new InvalidOperationException($"Student with ID {studentId} not found.");
        }

        if (request.Name != null)
            student.Name = request.Name;
        if (request.Birthdate.HasValue)
            student.Birthdate = request.Birthdate;
        if (request.GradeLevel.HasValue)
            student.GradeLevel = request.GradeLevel;
        if (request.AvatarId != null)
            student.AvatarId = request.AvatarId;
        if (request.Preferences != null)
            student.Preferences = request.Preferences;
        if (request.AccessibilitySettings != null)
            student.AccessibilitySettings = request.AccessibilitySettings;

        student.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated student {StudentId}", studentId);

        return MapToDto(student);
    }

    public async Task<bool> DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student == null)
        {
            return false;
        }

        // Soft delete for COPPA compliance
        student.DeletedAt = DateTimeOffset.UtcNow;
        student.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Soft deleted student {StudentId}", studentId);

        return true;
    }

    private static StudentDto MapToDto(Student student)
    {
        return new StudentDto(
            student.Id,
            student.FamilyId,
            student.Name,
            student.Birthdate,
            student.GradeLevel,
            student.AvatarId,
            student.Preferences,
            student.AccessibilitySettings,
            student.CreatedAt,
            student.UpdatedAt);
    }
}

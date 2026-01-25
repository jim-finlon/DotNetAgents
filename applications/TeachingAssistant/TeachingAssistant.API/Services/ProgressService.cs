using Microsoft.EntityFrameworkCore;
using TeachingAssistant.API.Models;
using TeachingAssistant.Data;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service implementation for student progress tracking operations.
/// </summary>
public class ProgressService : IProgressService
{
    private readonly TeachingAssistantDbContext _dbContext;
    private readonly ILogger<ProgressService> _logger;

    public ProgressService(
        TeachingAssistantDbContext dbContext,
        ILogger<ProgressService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SubjectProgressDto?> GetSubjectProgressAsync(Guid studentId, Subject subject, CancellationToken cancellationToken = default)
    {
        var progress = await _dbContext.StudentSubjectProgress
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Subject == subject, cancellationToken);

        if (progress == null)
        {
            return null;
        }

        return MapToDto(progress);
    }

    public async Task<IEnumerable<SubjectProgressDto>> GetAllSubjectProgressAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var progressList = await _dbContext.StudentSubjectProgress
            .Where(p => p.StudentId == studentId)
            .ToListAsync(cancellationToken);

        return progressList.Select(MapToDto);
    }

    public async Task<SubjectProgressDto> UpdateSubjectProgressAsync(Guid studentId, Subject subject, UpdateProgressRequest request, CancellationToken cancellationToken = default)
    {
        var progress = await _dbContext.StudentSubjectProgress
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Subject == subject, cancellationToken);

        if (progress == null)
        {
            // Create new progress record
            progress = new StudentSubjectProgress
            {
                StudentId = studentId,
                Subject = subject,
                CurrentUnitId = request.CurrentUnitId,
                OverallProgress = 0,
                TotalTimeMinutes = request.TimeSpentMinutes ?? 0,
                LastActivityAt = DateTimeOffset.UtcNow,
                StreakDays = 0,
                LongestStreak = 0,
                Settings = new Dictionary<string, object>()
            };

            _dbContext.StudentSubjectProgress.Add(progress);
        }
        else
        {
            if (request.CurrentUnitId.HasValue)
                progress.CurrentUnitId = request.CurrentUnitId;
            if (request.TimeSpentMinutes.HasValue)
                progress.TotalTimeMinutes += request.TimeSpentMinutes.Value;
            
            progress.LastActivityAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated progress for student {StudentId} in subject {Subject}", studentId, subject);

        return MapToDto(progress);
    }

    public async Task<IEnumerable<ContentMasteryDto>> GetContentMasteryAsync(Guid studentId, Guid? contentUnitId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ContentMastery
            .Where(m => m.StudentId == studentId);

        if (contentUnitId.HasValue)
        {
            query = query.Where(m => m.ContentUnitId == contentUnitId.Value);
        }

        var masteryRecords = await query.ToListAsync(cancellationToken);

        return masteryRecords.Select(MapToDto);
    }

    public async Task<ContentMasteryDto> UpdateContentMasteryAsync(Guid studentId, Guid contentUnitId, UpdateMasteryRequest request, CancellationToken cancellationToken = default)
    {
        var mastery = await _dbContext.ContentMastery
            .FirstOrDefaultAsync(m => m.StudentId == studentId && m.ContentUnitId == contentUnitId, cancellationToken);

        if (mastery == null)
        {
            mastery = new ContentMastery
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                ContentUnitId = contentUnitId,
                MasteryLevel = request.MasteryLevel ?? MasteryLevel.NotStarted,
                MasteryScore = request.MasteryScore,
                Attempts = request.Attempts ?? 0,
                CorrectAttempts = request.CorrectAttempts ?? 0,
                TotalTimeSeconds = request.TimeSpentSeconds ?? 0,
                FirstSeenAt = DateTimeOffset.UtcNow,
                LastReviewedAt = DateTimeOffset.UtcNow,
                EaseFactor = 2.5m,
                IntervalDays = 1,
                Metadata = new Dictionary<string, object>()
            };

            _dbContext.ContentMastery.Add(mastery);
        }
        else
        {
            if (request.MasteryLevel.HasValue)
                mastery.MasteryLevel = request.MasteryLevel.Value;
            if (request.MasteryScore.HasValue)
                mastery.MasteryScore = request.MasteryScore;
            if (request.Attempts.HasValue)
                mastery.Attempts = request.Attempts.Value;
            if (request.CorrectAttempts.HasValue)
                mastery.CorrectAttempts = request.CorrectAttempts.Value;
            if (request.TimeSpentSeconds.HasValue)
                mastery.TotalTimeSeconds += request.TimeSpentSeconds.Value;

            mastery.LastReviewedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated mastery for student {StudentId} and content unit {ContentUnitId}", studentId, contentUnitId);

        return MapToDto(mastery);
    }

    private static SubjectProgressDto MapToDto(StudentSubjectProgress progress)
    {
        return new SubjectProgressDto(
            progress.StudentId,
            progress.Subject,
            progress.CurrentUnitId,
            progress.OverallProgress,
            progress.TotalTimeMinutes,
            progress.LastActivityAt,
            progress.StreakDays,
            progress.LongestStreak,
            progress.Settings);
    }

    private static ContentMasteryDto MapToDto(ContentMastery mastery)
    {
        return new ContentMasteryDto(
            mastery.Id,
            mastery.StudentId,
            mastery.ContentUnitId,
            mastery.MasteryLevel,
            mastery.MasteryScore,
            mastery.Attempts,
            mastery.CorrectAttempts,
            mastery.TotalTimeSeconds,
            mastery.FirstSeenAt,
            mastery.LastReviewedAt,
            mastery.NextReviewAt);
    }
}

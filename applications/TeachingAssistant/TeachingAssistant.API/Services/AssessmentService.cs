using Microsoft.EntityFrameworkCore;
using TeachingAssistant.API.Models;
using TeachingAssistant.Data;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service implementation for assessment operations.
/// </summary>
public class AssessmentService : IAssessmentService
{
    private readonly TeachingAssistantDbContext _dbContext;
    private readonly ILogger<AssessmentService> _logger;

    public AssessmentService(
        TeachingAssistantDbContext dbContext,
        ILogger<AssessmentService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AssessmentDto?> GetAssessmentByIdAsync(Guid assessmentId, CancellationToken cancellationToken = default)
    {
        var assessment = await _dbContext.Assessments
            .FirstOrDefaultAsync(a => a.Id == assessmentId, cancellationToken);

        if (assessment == null)
        {
            return null;
        }

        return MapToDto(assessment);
    }

    public async Task<IEnumerable<AssessmentDto>> GetAssessmentsByContentUnitAsync(Guid contentUnitId, CancellationToken cancellationToken = default)
    {
        var assessments = await _dbContext.Assessments
            .Where(a => a.ContentUnitId == contentUnitId && a.IsActive)
            .ToListAsync(cancellationToken);

        return assessments.Select(MapToDto);
    }

    public async Task<AssessmentDto> CreateAssessmentAsync(CreateAssessmentRequest request, CancellationToken cancellationToken = default)
    {
        var assessment = new Assessment
        {
            Id = Guid.NewGuid(),
            ContentUnitId = request.ContentUnitId,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            TimeLimitMinutes = request.TimeLimitMinutes,
            TotalPoints = request.TotalPoints,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Assessments.Add(assessment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created assessment {AssessmentId} for content unit {ContentUnitId}", assessment.Id, request.ContentUnitId);

        return MapToDto(assessment);
    }

    public async Task<AssessmentResultDto> SubmitAssessmentResponseAsync(Guid assessmentId, Guid studentId, SubmitAssessmentRequest request, CancellationToken cancellationToken = default)
    {
        var assessment = await _dbContext.Assessments
            .Include(a => a.Items)
            .FirstOrDefaultAsync(a => a.Id == assessmentId, cancellationToken);

        if (assessment == null)
        {
            throw new InvalidOperationException($"Assessment with ID {assessmentId} not found.");
        }

        // Calculate score
        int correctAnswers = 0;
        int totalQuestions = assessment.Items.Count;
        decimal totalScore = 0;

        foreach (var item in assessment.Items)
        {
            if (request.Responses.TryGetValue(item.Id, out var response))
            {
                // Extract correct answer from question data JSON
                // In production, this would use the ResponseEvaluator for more sophisticated scoring
                var isCorrect = false;
                if (item.QuestionData.TryGetValue("correct_answer", out var correctAnswerObj))
                {
                    var correctAnswer = correctAnswerObj?.ToString() ?? string.Empty;
                    isCorrect = string.Equals(correctAnswer, response, StringComparison.OrdinalIgnoreCase);
                }

                if (isCorrect)
                {
                    correctAnswers++;
                    totalScore += item.Points;
                }
            }
        }

        var percentage = totalQuestions > 0 ? (totalScore / assessment.TotalPoints) * 100 : 0;

        // Create student response records
        foreach (var response in request.Responses)
        {
            var item = assessment.Items.FirstOrDefault(i => i.Id == response.Key);
            if (item != null)
            {
                var isCorrect = false;
                if (item.QuestionData.TryGetValue("correct_answer", out var correctAnswerObj))
                {
                    var correctAnswer = correctAnswerObj?.ToString() ?? string.Empty;
                    isCorrect = string.Equals(correctAnswer, response.Value, StringComparison.OrdinalIgnoreCase);
                }
                var studentResponse = new StudentResponse
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    AssessmentItemId = response.Key,
                    Response = new Dictionary<string, object> { ["text"] = response.Value },
                    IsCorrect = isCorrect,
                    PartialCredit = isCorrect ? 100 : 0,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _dbContext.StudentResponses.Add(studentResponse);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var resultDto = new AssessmentResultDto(
            Guid.NewGuid(), // Result ID - in production, this would be stored
            assessmentId,
            studentId,
            totalScore,
            percentage,
            totalQuestions,
            correctAnswers,
            DateTimeOffset.UtcNow,
            new Dictionary<string, object>
            {
                ["feedback"] = $"You scored {correctAnswers} out of {totalQuestions} questions correctly.",
                ["percentage"] = percentage
            });

        _logger.LogInformation("Student {StudentId} submitted assessment {AssessmentId} with score {Score}%", studentId, assessmentId, percentage);

        return resultDto;
    }

    public async Task<IEnumerable<AssessmentResultDto>> GetAssessmentResultsAsync(Guid studentId, Guid? assessmentId = null, CancellationToken cancellationToken = default)
    {
        // In production, this would query a dedicated AssessmentResult table
        // For now, we'll calculate from StudentResponses
        var query = _dbContext.StudentResponses
            .Include(sr => sr.AssessmentItem)
                .ThenInclude(ai => ai!.Assessment)
            .Where(sr => sr.StudentId == studentId);

        if (assessmentId.HasValue)
        {
            query = query.Where(sr => sr.AssessmentItem != null && sr.AssessmentItem.AssessmentId == assessmentId.Value);
        }

        var responses = await query.ToListAsync(cancellationToken);

        // Group by assessment and calculate results
        var results = responses
            .Where(sr => sr.AssessmentItem != null)
            .GroupBy(sr => sr.AssessmentItem!.AssessmentId)
            .Select(g =>
            {
                var assessment = g.First().AssessmentItem!.Assessment;
                var totalScore = g.Sum(sr => (double)(sr.PartialCredit ?? 0) * assessment.TotalPoints / 100);
                var correctAnswers = g.Count(sr => sr.IsCorrect == true);
                var totalQuestions = assessment.Items.Count;
                var percentage = assessment.TotalPoints > 0 ? (totalScore / assessment.TotalPoints) * 100 : 0;

                return new AssessmentResultDto(
                    Guid.NewGuid(),
                    assessment.Id,
                    studentId,
                    (decimal)totalScore,
                    (decimal)percentage,
                    totalQuestions,
                    correctAnswers,
                    g.Max(sr => sr.CreatedAt),
                    new Dictionary<string, object>
                    {
                        ["feedback"] = $"You scored {correctAnswers} out of {totalQuestions} questions correctly."
                    });
            })
            .ToList();

        return results;
    }

    private static AssessmentDto MapToDto(Assessment assessment)
    {
        return new AssessmentDto(
            assessment.Id,
            assessment.ContentUnitId,
            assessment.Title,
            assessment.Description,
            assessment.Type,
            assessment.TimeLimitMinutes,
            assessment.TotalPoints,
            assessment.IsActive,
            assessment.CreatedAt);
    }
}

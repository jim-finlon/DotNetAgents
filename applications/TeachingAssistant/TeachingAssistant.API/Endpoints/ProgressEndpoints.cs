using Microsoft.AspNetCore.Mvc;
using TeachingAssistant.API.Models;
using TeachingAssistant.API.Services;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Endpoints;

/// <summary>
/// Endpoints for student progress tracking.
/// </summary>
public static class ProgressEndpoints
{
    public static void MapProgressEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/progress")
            .WithTags("Progress")
            .RequireAuthorization();

        group.MapGet("/students/{studentId:guid}/subjects/{subject}", GetSubjectProgressAsync)
            .WithName("GetSubjectProgress")
            .WithSummary("Get progress for a student in a specific subject")
            .Produces<SubjectProgressDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/students/{studentId:guid}/subjects", GetAllSubjectProgressAsync)
            .WithName("GetAllSubjectProgress")
            .WithSummary("Get all subject progress for a student")
            .Produces<IEnumerable<SubjectProgressDto>>(StatusCodes.Status200OK);

        group.MapPut("/students/{studentId:guid}/subjects/{subject}", UpdateSubjectProgressAsync)
            .WithName("UpdateSubjectProgress")
            .WithSummary("Update progress for a student in a subject")
            .Produces<SubjectProgressDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapGet("/students/{studentId:guid}/mastery", GetContentMasteryAsync)
            .WithName("GetContentMastery")
            .WithSummary("Get mastery records for a student")
            .Produces<IEnumerable<ContentMasteryDto>>(StatusCodes.Status200OK);

        group.MapPut("/students/{studentId:guid}/mastery/{contentUnitId:guid}", UpdateContentMasteryAsync)
            .WithName("UpdateContentMastery")
            .WithSummary("Update mastery for a content unit")
            .Produces<ContentMasteryDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> GetSubjectProgressAsync(
        Guid studentId,
        Subject subject,
        IProgressService progressService,
        CancellationToken cancellationToken)
    {
        var progress = await progressService.GetSubjectProgressAsync(studentId, subject, cancellationToken);
        return progress == null ? Results.NotFound() : Results.Ok(progress);
    }

    private static async Task<IResult> GetAllSubjectProgressAsync(
        Guid studentId,
        IProgressService progressService,
        CancellationToken cancellationToken)
    {
        var progressList = await progressService.GetAllSubjectProgressAsync(studentId, cancellationToken);
        return Results.Ok(progressList);
    }

    private static async Task<IResult> UpdateSubjectProgressAsync(
        Guid studentId,
        Subject subject,
        [FromBody] UpdateProgressRequest request,
        IProgressService progressService,
        CancellationToken cancellationToken)
    {
        var progress = await progressService.UpdateSubjectProgressAsync(studentId, subject, request, cancellationToken);
        return Results.Ok(progress);
    }

    private static async Task<IResult> GetContentMasteryAsync(
        Guid studentId,
        [FromQuery] Guid? contentUnitId,
        IProgressService progressService,
        CancellationToken cancellationToken)
    {
        var mastery = await progressService.GetContentMasteryAsync(studentId, contentUnitId, cancellationToken);
        return Results.Ok(mastery);
    }

    private static async Task<IResult> UpdateContentMasteryAsync(
        Guid studentId,
        Guid contentUnitId,
        [FromBody] UpdateMasteryRequest request,
        IProgressService progressService,
        CancellationToken cancellationToken)
    {
        var mastery = await progressService.UpdateContentMasteryAsync(studentId, contentUnitId, request, cancellationToken);
        return Results.Ok(mastery);
    }
}

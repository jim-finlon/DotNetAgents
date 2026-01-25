using Microsoft.AspNetCore.Mvc;
using TeachingAssistant.API.Models;
using TeachingAssistant.API.Services;

namespace TeachingAssistant.API.Endpoints;

/// <summary>
/// Endpoints for assessment operations.
/// </summary>
public static class AssessmentEndpoints
{
    public static void MapAssessmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/assessments")
            .WithTags("Assessments")
            .RequireAuthorization();

        group.MapGet("/{assessmentId:guid}", GetAssessmentByIdAsync)
            .WithName("GetAssessmentById")
            .WithSummary("Get an assessment by ID")
            .Produces<AssessmentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/content-unit/{contentUnitId:guid}", GetAssessmentsByContentUnitAsync)
            .WithName("GetAssessmentsByContentUnit")
            .WithSummary("Get assessments for a content unit")
            .Produces<IEnumerable<AssessmentDto>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateAssessmentAsync)
            .WithName("CreateAssessment")
            .WithSummary("Create a new assessment")
            .Produces<AssessmentDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/{assessmentId:guid}/submit", SubmitAssessmentResponseAsync)
            .WithName("SubmitAssessmentResponse")
            .WithSummary("Submit a student response to an assessment")
            .Produces<AssessmentResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapGet("/students/{studentId:guid}/results", GetAssessmentResultsAsync)
            .WithName("GetAssessmentResults")
            .WithSummary("Get assessment results for a student")
            .Produces<IEnumerable<AssessmentResultDto>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAssessmentByIdAsync(
        Guid assessmentId,
        IAssessmentService assessmentService,
        CancellationToken cancellationToken)
    {
        var assessment = await assessmentService.GetAssessmentByIdAsync(assessmentId, cancellationToken);
        return assessment == null ? Results.NotFound() : Results.Ok(assessment);
    }

    private static async Task<IResult> GetAssessmentsByContentUnitAsync(
        Guid contentUnitId,
        IAssessmentService assessmentService,
        CancellationToken cancellationToken)
    {
        var assessments = await assessmentService.GetAssessmentsByContentUnitAsync(contentUnitId, cancellationToken);
        return Results.Ok(assessments);
    }

    private static async Task<IResult> CreateAssessmentAsync(
        [FromBody] CreateAssessmentRequest request,
        IAssessmentService assessmentService,
        CancellationToken cancellationToken)
    {
        var assessment = await assessmentService.CreateAssessmentAsync(request, cancellationToken);
        return Results.Created($"/api/assessments/{assessment.Id}", assessment);
    }

    private static async Task<IResult> SubmitAssessmentResponseAsync(
        Guid assessmentId,
        [FromQuery] Guid studentId,
        [FromBody] SubmitAssessmentRequest request,
        IAssessmentService assessmentService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await assessmentService.SubmitAssessmentResponseAsync(assessmentId, studentId, request, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetAssessmentResultsAsync(
        Guid studentId,
        [FromQuery] Guid? assessmentId,
        IAssessmentService assessmentService,
        CancellationToken cancellationToken)
    {
        var results = await assessmentService.GetAssessmentResultsAsync(studentId, assessmentId, cancellationToken);
        return Results.Ok(results);
    }
}

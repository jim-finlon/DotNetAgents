using Microsoft.AspNetCore.Mvc;
using TeachingAssistant.API.Models;
using TeachingAssistant.API.Services;

namespace TeachingAssistant.API.Endpoints;

/// <summary>
/// Endpoints for workflow execution.
/// </summary>
public static class WorkflowEndpoints
{
    public static void MapWorkflowEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/workflows")
            .WithTags("Workflows")
            .RequireAuthorization();

        group.MapPost("/socratic-tutoring/start", StartSocraticTutoringAsync)
            .WithName("StartSocraticTutoring")
            .WithSummary("Start a Socratic tutoring session")
            .Produces<WorkflowSessionDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/lesson-delivery/start", StartLessonDeliveryAsync)
            .WithName("StartLessonDelivery")
            .WithSummary("Start a lesson delivery session")
            .Produces<WorkflowSessionDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/sessions/{sessionId:guid}/continue", ContinueWorkflowAsync)
            .WithName("ContinueWorkflow")
            .WithSummary("Continue a workflow session")
            .Produces<WorkflowStateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapGet("/sessions/{sessionId:guid}/state", GetWorkflowStateAsync)
            .WithName("GetWorkflowState")
            .WithSummary("Get workflow session state")
            .Produces<WorkflowStateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> StartSocraticTutoringAsync(
        [FromBody] StartWorkflowRequest request,
        IWorkflowService workflowService,
        CancellationToken cancellationToken)
    {
        var session = await workflowService.StartSocraticTutoringAsync(
            request.StudentId,
            request.ContentUnitId,
            cancellationToken);

        return Results.Created($"/api/workflows/sessions/{session.SessionId}/state", session);
    }

    private static async Task<IResult> StartLessonDeliveryAsync(
        [FromBody] StartWorkflowRequest request,
        IWorkflowService workflowService,
        CancellationToken cancellationToken)
    {
        var session = await workflowService.StartLessonDeliveryAsync(
            request.StudentId,
            request.ContentUnitId,
            cancellationToken);

        return Results.Created($"/api/workflows/sessions/{session.SessionId}/state", session);
    }

    private static async Task<IResult> ContinueWorkflowAsync(
        Guid sessionId,
        [FromBody] ContinueWorkflowRequest? request,
        IWorkflowService workflowService,
        CancellationToken cancellationToken)
    {
        try
        {
            var state = await workflowService.ContinueWorkflowAsync(
                sessionId,
                request?.Input,
                cancellationToken);

            return Results.Ok(state);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetWorkflowStateAsync(
        Guid sessionId,
        IWorkflowService workflowService,
        CancellationToken cancellationToken)
    {
        var state = await workflowService.GetWorkflowStateAsync(sessionId, cancellationToken);
        return state == null ? Results.NotFound() : Results.Ok(state);
    }
}

/// <summary>
/// Request model for starting a workflow.
/// </summary>
public record StartWorkflowRequest(
    string StudentId,
    Guid ContentUnitId);

/// <summary>
/// Request model for continuing a workflow.
/// </summary>
public record ContinueWorkflowRequest(
    string? Input);

using Microsoft.AspNetCore.Mvc;
using TeachingAssistant.API.Models;
using TeachingAssistant.API.Services;

namespace TeachingAssistant.API.Endpoints;

/// <summary>
/// Endpoints for student management.
/// </summary>
public static class StudentEndpoints
{
    public static void MapStudentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/students")
            .WithTags("Students")
            .RequireAuthorization();

        group.MapGet("/{studentId:guid}", GetStudentByIdAsync)
            .WithName("GetStudentById")
            .WithSummary("Get a student by ID")
            .Produces<StudentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/family/{familyId:guid}", GetStudentsByFamilyIdAsync)
            .WithName("GetStudentsByFamilyId")
            .WithSummary("Get all students for a family")
            .Produces<IEnumerable<StudentDto>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateStudentAsync)
            .WithName("CreateStudent")
            .WithSummary("Create a new student")
            .Produces<StudentDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{studentId:guid}", UpdateStudentAsync)
            .WithName("UpdateStudent")
            .WithSummary("Update an existing student")
            .Produces<StudentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{studentId:guid}", DeleteStudentAsync)
            .WithName("DeleteStudent")
            .WithSummary("Delete a student (soft delete)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetStudentByIdAsync(
        Guid studentId,
        IStudentService studentService,
        CancellationToken cancellationToken)
    {
        var student = await studentService.GetStudentByIdAsync(studentId, cancellationToken);
        return student == null ? Results.NotFound() : Results.Ok(student);
    }

    private static async Task<IResult> GetStudentsByFamilyIdAsync(
        Guid familyId,
        IStudentService studentService,
        CancellationToken cancellationToken)
    {
        var students = await studentService.GetStudentsByFamilyIdAsync(familyId, cancellationToken);
        return Results.Ok(students);
    }

    private static async Task<IResult> CreateStudentAsync(
        [FromBody] CreateStudentRequest request,
        IStudentService studentService,
        CancellationToken cancellationToken)
    {
        var student = await studentService.CreateStudentAsync(request, cancellationToken);
        return Results.Created($"/api/students/{student.Id}", student);
    }

    private static async Task<IResult> UpdateStudentAsync(
        Guid studentId,
        [FromBody] UpdateStudentRequest request,
        IStudentService studentService,
        CancellationToken cancellationToken)
    {
        try
        {
            var student = await studentService.UpdateStudentAsync(studentId, request, cancellationToken);
            return Results.Ok(student);
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> DeleteStudentAsync(
        Guid studentId,
        IStudentService studentService,
        CancellationToken cancellationToken)
    {
        var deleted = await studentService.DeleteStudentAsync(studentId, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}

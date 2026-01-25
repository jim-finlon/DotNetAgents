using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using DotNetAgents.Education.Workflows;
using DotNetAgents.Workflow.Execution;
using Microsoft.EntityFrameworkCore;
using TeachingAssistant.API.Hubs;
using TeachingAssistant.API.Models;
using TeachingAssistant.Data;
using TeachingAssistant.Data.Entities;
using Microsoft.AspNetCore.SignalR;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service implementation for workflow execution.
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly TeachingAssistantDbContext _dbContext;
    private readonly ISocraticDialogueEngine _dialogueEngine;
    private readonly IHubContext<TutorHub> _tutorHub;
    private readonly ILogger<WorkflowService> _logger;
    private readonly Dictionary<Guid, GraphExecutor<SocraticDialogueState>> _socraticExecutors = new();
    private readonly Dictionary<Guid, GraphExecutor<LessonState>> _lessonExecutors = new();

    public WorkflowService(
        TeachingAssistantDbContext dbContext,
        ISocraticDialogueEngine dialogueEngine,
        IHubContext<TutorHub> tutorHub,
        ILogger<WorkflowService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dialogueEngine = dialogueEngine ?? throw new ArgumentNullException(nameof(dialogueEngine));
        _tutorHub = tutorHub ?? throw new ArgumentNullException(nameof(tutorHub));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WorkflowSessionDto> StartSocraticTutoringAsync(string studentId, Guid contentUnitId, CancellationToken cancellationToken = default)
    {
        var contentUnit = await _dbContext.ContentUnits
            .FirstOrDefaultAsync(cu => cu.Id == contentUnitId, cancellationToken);

        if (contentUnit == null)
        {
            throw new InvalidOperationException($"Content unit {contentUnitId} not found.");
        }

        var sessionId = Guid.NewGuid();
        var concept = new Concept(new ConceptId(contentUnit.Title, contentUnit.GradeBand.GetGradeLevel()));

        var initialState = new SocraticDialogueState
        {
            StudentId = studentId,
            Concept = concept,
            ConversationHistory = new List<DialogueTurn>(),
            CurrentPhase = DialoguePhase.Initializing
        };

        var graph = new SocraticTutorGraph(_dialogueEngine, _logger).Build();
        var executor = new GraphExecutor<SocraticDialogueState>(graph);
        _socraticExecutors[sessionId] = executor;

        // Create learning session record
        var learningSession = new LearningSession
        {
            Id = sessionId,
            StudentId = Guid.Parse(studentId),
            ContentUnitId = contentUnitId,
            SessionType = "SocraticTutoring",
            State = new Dictionary<string, object> { ["workflow"] = "SocraticTutor" },
            StartedAt = DateTimeOffset.UtcNow
        };

        _dbContext.LearningSessions.Add(learningSession);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Execute first node
        var result = await executor.ExecuteAsync(initialState, cancellationToken);

        // Notify via SignalR
        await _tutorHub.Clients.Group($"student_{studentId}").SendAsync("WorkflowStarted", new
        {
            SessionId = sessionId,
            WorkflowType = "SocraticTutoring",
            CurrentPhase = result.State.CurrentPhase.ToString()
        }, cancellationToken);

        return new WorkflowSessionDto(
            sessionId,
            "SocraticTutoring",
            studentId,
            contentUnitId,
            "Running",
            DateTimeOffset.UtcNow,
            null);
    }

    public async Task<WorkflowSessionDto> StartLessonDeliveryAsync(string studentId, Guid contentUnitId, CancellationToken cancellationToken = default)
    {
        // Similar implementation for lesson delivery
        // This would require IAssessmentGenerator, IResponseEvaluator, IMasteryCalculator
        throw new NotImplementedException("Lesson delivery workflow integration pending");
    }

    public async Task<WorkflowStateDto> ContinueWorkflowAsync(Guid sessionId, string? input, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.LearningSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found.");
        }

        // Load state from checkpoint
        // For now, simplified - in production would deserialize from session.State
        if (_socraticExecutors.TryGetValue(sessionId, out var executor))
        {
            // Continue execution with input
            // This is simplified - real implementation would restore state from checkpoint
            throw new NotImplementedException("Checkpoint restoration pending");
        }

        throw new InvalidOperationException($"No executor found for session {sessionId}");
    }

    public async Task<WorkflowStateDto?> GetWorkflowStateAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.LearningSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null)
        {
            return null;
        }

        var status = session.EndedAt.HasValue ? "Completed" : (session.Summary?.TryGetValue("status", out var statusObj) == true ? statusObj.ToString() ?? "Unknown" : "Running");
        var currentNode = session.Summary?.TryGetValue("currentNode", out var nodeObj) == true ? nodeObj.ToString() ?? "unknown" : "unknown";

        return new WorkflowStateDto(
            sessionId,
            currentNode,
            status,
            session.Summary ?? new Dictionary<string, object>(),
            session.EndedAt.HasValue,
            session.StartedAt);
    }
}

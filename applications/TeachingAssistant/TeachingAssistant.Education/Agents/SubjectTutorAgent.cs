using DotNetAgents.Education.Pedagogy;
using DotNetAgents.Education.RAG;
using DotNetAgents.Education.Models;
using TeachingAssistant.AI;
using TeachingAssistant.Data.Entities;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Agents;

/// <summary>
/// Specialized tutor agent for a specific subject.
/// </summary>
public class SubjectTutorAgent
{
    private readonly Subject _subject;
    private readonly ISocraticDialogueEngine _dialogueEngine;
    private readonly CurriculumRagService _ragService;
    private readonly IModelRouter _modelRouter;
    private readonly ILogger<SubjectTutorAgent> _logger;

    public SubjectTutorAgent(
        Subject subject,
        ISocraticDialogueEngine dialogueEngine,
        CurriculumRagService ragService,
        IModelRouter modelRouter,
        ILogger<SubjectTutorAgent> logger)
    {
        _subject = subject;
        _dialogueEngine = dialogueEngine ?? throw new ArgumentNullException(nameof(dialogueEngine));
        _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));
        _modelRouter = modelRouter ?? throw new ArgumentNullException(nameof(modelRouter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Subject Subject => _subject;

    /// <summary>
    /// Handles a student query for this subject.
    /// </summary>
    public async Task<TutorResponse> HandleQueryAsync(
        string studentId,
        string query,
        GradeBand? gradeBand = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Subject tutor {Subject} handling query for student {StudentId}",
            _subject,
            studentId);

        // Retrieve relevant curriculum content
        var ragContext = await _ragService.RetrieveContextAsync(
            query,
            studentId,
            _subject,
            gradeBand,
            topK: 5,
            cancellationToken);

        // Generate response using model router (selects appropriate LLM)
        var llm = await _modelRouter.GetModelAsync(
            TaskComplexity.Standard,
            cancellationToken);

        if (llm == null)
        {
            return new TutorResponse(
                studentId,
                "I'm sorry, I'm having trouble connecting right now. Please try again later.",
                null,
                null,
                null,
                DateTimeOffset.UtcNow);
        }

        // Map enums for ConceptId
        var subjectArea = _subject switch
        {
            Subject.Biology or Subject.Chemistry or Subject.Physics or Subject.EarthScience 
                or Subject.Astronomy or Subject.EnvironmentalScience => Models.SubjectArea.Science,
            Subject.Mathematics => Models.SubjectArea.Mathematics,
            _ => Models.SubjectArea.Other
        };
        
        var gradeLevel = gradeBand switch
        {
            GradeBand.K2 => Models.GradeLevel.K2,
            GradeBand.G3_5 => Models.GradeLevel.G3_5,
            GradeBand.G6_8 => Models.GradeLevel.G6_8,
            GradeBand.G9_10 => Models.GradeLevel.G9_10,
            GradeBand.G11_12 => Models.GradeLevel.G11_12,
            _ => Models.GradeLevel.G6_8 // Default
        };
        
        // Create concept context
        var conceptContext = new Models.ConceptContext
        {
            ConceptId = new Models.ConceptId(query, subjectArea, gradeLevel),
            Description = query,
            LearningObjectives = new List<string>(),
            KeyTerms = new List<string>(),
            StudentId = studentId
        };
        
        // Use Socratic dialogue engine to generate response
        var response = await _dialogueEngine.GenerateQuestionAsync(
            conceptContext,
            Models.StudentUnderstanding.Beginner,
            cancellationToken: cancellationToken);

        return new TutorResponse(
            studentId,
            response.QuestionText,
            response.ConceptId?.Value ?? Guid.NewGuid().ToString(),
            null,
            new Dictionary<string, object> { ["rag_context"] = ragContext.RetrievedChunks.Count },
            DateTimeOffset.UtcNow);
    }
}

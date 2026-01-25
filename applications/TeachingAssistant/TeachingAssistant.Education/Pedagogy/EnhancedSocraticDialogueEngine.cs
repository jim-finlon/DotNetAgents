using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.Memory;
using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.RAG;
using Microsoft.Extensions.Logging;
using TeachingAssistant.AI;
using TeachingAssistant.Data.Entities;

namespace DotNetAgents.Education.Pedagogy;

/// <summary>
/// Enhanced Socratic dialogue engine with RAG integration for curriculum-aware question generation.
/// </summary>
public class EnhancedSocraticDialogueEngine : ISocraticDialogueEngine
{
    private readonly ISocraticDialogueEngine _baseEngine;
    private readonly CurriculumRagService _ragService;
    private readonly IModelRouter _modelRouter;
    private readonly IMemory? _memory;
    private readonly ILogger<EnhancedSocraticDialogueEngine> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedSocraticDialogueEngine"/> class.
    /// </summary>
    public EnhancedSocraticDialogueEngine(
        ILLMModel<ChatMessage[], ChatMessage> llmModel,
        CurriculumRagService ragService,
        IModelRouter modelRouter,
        IMemory? memory = null,
        ILogger<EnhancedSocraticDialogueEngine>? logger = null)
    {
        _baseEngine = new SocraticDialogueEngine(
            llmModel ?? throw new ArgumentNullException(nameof(llmModel)),
            memory,
            logger);
        _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));
        _modelRouter = modelRouter ?? throw new ArgumentNullException(nameof(modelRouter));
        _memory = memory;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EnhancedSocraticDialogueEngine>.Instance;
    }

    /// <inheritdoc />
    public async Task<SocraticQuestion> GenerateQuestionAsync(
        ConceptContext concept,
        StudentUnderstanding currentLevel,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        // Retrieve relevant curriculum content using RAG
        var ragContext = await _ragService.RetrieveContextAsync(
            query: $"Explain {concept.ConceptId.Value}",
            studentId: concept.StudentId ?? "unknown",
            subject: MapSubjectArea(concept.ConceptId.Subject),
            gradeBand: MapGradeLevel(concept.ConceptId.GradeLevel),
            topK: 3,
            cancellationToken: cancellationToken);

        // Select appropriate model based on complexity
        var complexity = DetermineComplexity(currentLevel, ragContext);
        var model = await _modelRouter.GetModelAsync(complexity, requiresSafetyCheck: true, cancellationToken);

        // Build prompt with curriculum context
        var prompt = BuildSocraticPrompt(concept, currentLevel, ragContext, language);

        // Generate question
        var response = await model.GenerateAsync(prompt, options: null, cancellationToken);

        return new SocraticQuestion
        {
            QuestionText = response,
            ConceptId = concept.ConceptId,
            TargetLevel = currentLevel,
            Type = SocraticQuestionType.Probing,
            Hints = new List<string>(), // Will be generated on demand
            Metadata = new Dictionary<string, object> { ["rag_context"] = ragContext.AssembledContext }
        };
    }

    /// <inheritdoc />
    public async Task<UnderstandingAssessment> EvaluateResponseAsync(
        string studentResponse,
        SocraticQuestion question,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Evaluating response with RAG context for question {QuestionId}",
            question.ConceptId.Value);

        // Retrieve curriculum context for evaluation
        var ragContext = await _ragService.RetrieveContextAsync(
            query: question.QuestionText,
            studentId: "unknown", // Would need student ID from context
            subject: MapSubjectArea(question.ConceptId.Subject),
            gradeBand: MapGradeLevel(question.ConceptId.GradeLevel),
            topK: 2,
            cancellationToken: cancellationToken);

        // Use base engine evaluation (it will use the enhanced context through memory if available)
        var assessment = await _baseEngine.EvaluateResponseAsync(
            studentResponse,
            question,
            cancellationToken);

        // Enhance assessment with RAG context
        return EnhanceAssessmentWithRag(assessment, ragContext);
    }

    /// <inheritdoc />
    public async Task<ScaffoldedHint> GenerateHintAsync(
        SocraticQuestion question,
        int hintLevel,
        CancellationToken cancellationToken = default)
    {
        // Use base engine for hint generation
        return await _baseEngine.GenerateHintAsync(question, hintLevel, cancellationToken);
    }

    private ConceptContext EnhanceConceptWithRag(ConceptContext concept, RagContext ragContext)
    {
        // Enhance concept description with RAG content
        var enhancedDescription = concept.Description;
        if (!string.IsNullOrWhiteSpace(ragContext.AssembledContext))
        {
            enhancedDescription += $"\n\nRelevant Curriculum Content:\n{ragContext.AssembledContext}";
        }

        // Create enhanced concept context
        var enhancedMetadata = new Dictionary<string, object>(concept.Metadata ?? new Dictionary<string, object>())
        {
            ["rag_context"] = ragContext.AssembledContext,
            ["rag_chunks_count"] = ragContext.RetrievedChunks.Count,
            ["rag_source_ids"] = ragContext.SourceContentIds.Select(id => id.ToString()).ToList()
        };

        return new ConceptContext
        {
            ConceptId = concept.ConceptId,
            Description = enhancedDescription,
            LearningObjectives = concept.LearningObjectives,
            KeyTerms = concept.KeyTerms,
            Metadata = enhancedMetadata
        };
    }

    private UnderstandingAssessment EnhanceAssessmentWithRag(
        UnderstandingAssessment assessment,
        RagContext ragContext)
    {
        // Enhance feedback with curriculum context if needed
        var enhancedFeedback = assessment.Feedback;
        if (!string.IsNullOrWhiteSpace(ragContext.AssembledContext) && assessment.NeedsMoreHelp)
        {
            enhancedFeedback += $"\n\nConsider reviewing: {ragContext.AssembledContext.Substring(0, Math.Min(200, ragContext.AssembledContext.Length))}...";
        }

        return new UnderstandingAssessment
        {
            AssessedLevel = assessment.AssessedLevel,
            Confidence = assessment.Confidence,
            Feedback = enhancedFeedback,
            Misconceptions = assessment.Misconceptions,
            NeedsMoreHelp = assessment.NeedsMoreHelp,
            HasMastery = assessment.HasMastery,
            NextSteps = assessment.NextSteps
        };
    }

    private static Subject? MapSubjectArea(SubjectArea subjectArea)
    {
        return subjectArea switch
        {
            SubjectArea.Science => Subject.Biology, // Default to Biology for Science
            SubjectArea.Mathematics => Subject.Mathematics,
            _ => null
        };
    }

    private static GradeBand? MapGradeLevel(GradeLevel gradeLevel)
    {
        return gradeLevel switch
        {
            GradeLevel.K2 => GradeBand.K2,
            GradeLevel.G3_5 => GradeBand.G3_5,
            GradeLevel.G6_8 => GradeBand.G6_8,
            GradeLevel.G9_10 => GradeBand.G9_10,
            GradeLevel.G11_12 => GradeBand.G11_12,
            _ => null
        };
    }

    private static TaskComplexity DetermineComplexity(StudentUnderstanding understanding, RagContext ragContext)
    {
        // Simple complexity determination based on understanding level and context size
        if (understanding.ConfidenceLevel >= 0.8)
            return TaskComplexity.Simple;
        
        if (ragContext.RetrievedChunks.Count > 5 || understanding.ConfidenceLevel < 0.3)
            return TaskComplexity.Complex;
        
        return TaskComplexity.Standard;
    }

    private static string BuildSocraticPrompt(
        ConceptContext concept,
        StudentUnderstanding currentLevel,
        RagContext ragContext,
        string? language)
    {
        var langNote = !string.IsNullOrEmpty(language) ? $" (Respond in {language})" : "";
        
        return $"""
            Generate a Socratic question to help a student understand: {concept.ConceptId.Value}
            
            Student's current understanding level: {currentLevel.ConfidenceLevel:F2}
            Student's current knowledge: {currentLevel.KnownConcepts.Count} concepts
            
            Relevant curriculum content:
            {ragContext.AssembledContext}
            
            Create a question that guides the student to discover the answer themselves, 
            building on what they already know. The question should be appropriate for 
            their current level and use the curriculum content as context.{langNote}
            
            Question:
            """;
    }
}

/// <summary>
/// Scaffolded hint for Socratic questions.
/// </summary>
public class ScaffoldedHint
{
    public string HintText { get; set; } = string.Empty;
    public int Level { get; set; }
}

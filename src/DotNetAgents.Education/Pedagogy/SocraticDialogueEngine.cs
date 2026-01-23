using DotNetAgents.Core.Memory;
using DotNetAgents.Core.Models;
using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Pedagogy;

/// <summary>
/// Implementation of the Socratic dialogue engine for generating educational questions.
/// </summary>
public class SocraticDialogueEngine : ISocraticDialogueEngine
{
    private readonly ILLMModel<ChatMessage[], ChatMessage> _llmModel;
    private readonly IMemory? _memory;
    private readonly ILogger<SocraticDialogueEngine> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocraticDialogueEngine"/> class.
    /// </summary>
    /// <param name="llmModel">The LLM model to use for question generation.</param>
    /// <param name="memory">Optional memory store for conversation history.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when llmModel is null.</exception>
    public SocraticDialogueEngine(
        ILLMModel<ChatMessage[], ChatMessage> llmModel,
        IMemory? memory = null,
        ILogger<SocraticDialogueEngine>? logger = null)
    {
        _llmModel = llmModel ?? throw new ArgumentNullException(nameof(llmModel));
        _memory = memory;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<SocraticDialogueEngine>.Instance;
    }

    /// <inheritdoc/>
    public async Task<SocraticQuestion> GenerateQuestionAsync(
        ConceptContext concept,
        StudentUnderstanding currentLevel,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        if (concept == null)
            throw new ArgumentNullException(nameof(concept));

        _logger.LogDebug(
            "Generating Socratic question for concept {ConceptId}, understanding level {Level}",
            concept.ConceptId.Value,
            currentLevel);

        // Build the prompt for question generation
        var systemPrompt = BuildSystemPrompt(concept, currentLevel, language);
        var userPrompt = BuildUserPrompt(concept, currentLevel);

        // Get conversation history if memory is available
        var history = new List<ChatMessage>();
        if (_memory != null)
        {
            var messages = await _memory.GetMessagesAsync(10, cancellationToken).ConfigureAwait(false);
            foreach (var msg in messages)
            {
                history.Add(new ChatMessage
                {
                    Role = msg.Role,
                    Content = msg.Content
                });
            }
        }

        // Build chat messages
        var chatMessages = new List<ChatMessage>
        {
            ChatMessage.System(systemPrompt)
        };
        chatMessages.AddRange(history);
        chatMessages.Add(ChatMessage.User(userPrompt));

        // Generate question using LLM
        var options = new LLMOptions
        {
            Temperature = 0.7, // Creative but focused
            MaxTokens = 200
        };

        var response = await _llmModel.GenerateAsync(
            chatMessages.ToArray(),
            options,
            cancellationToken).ConfigureAwait(false);

        // Parse the response to extract question
        var question = ParseQuestion(response.Content, concept, currentLevel);

        _logger.LogInformation(
            "Generated Socratic question type {Type} for concept {ConceptId}",
            question.Type,
            concept.ConceptId.Value);

        return question;
    }

    /// <inheritdoc/>
    public async Task<UnderstandingAssessment> EvaluateResponseAsync(
        string studentResponse,
        SocraticQuestion question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentResponse))
            throw new ArgumentException("Student response cannot be null or empty.", nameof(studentResponse));
        if (question == null)
            throw new ArgumentNullException(nameof(question));

        _logger.LogDebug(
            "Evaluating student response for question {QuestionId}",
            question.ConceptId.Value);

        // Build evaluation prompt
        var systemPrompt = BuildEvaluationSystemPrompt();
        var userPrompt = BuildEvaluationUserPrompt(question, studentResponse);

        var chatMessages = new[]
        {
            ChatMessage.System(systemPrompt),
            ChatMessage.User(userPrompt)
        };

        var options = new LLMOptions
        {
            Temperature = 0.3, // More deterministic for evaluation
            MaxTokens = 300
        };

        var response = await _llmModel.GenerateAsync(
            chatMessages,
            options,
            cancellationToken).ConfigureAwait(false);

        // Parse assessment from response
        var assessment = ParseAssessment(response.Content, question);

        _logger.LogInformation(
            "Evaluated response: Level {Level}, Confidence {Confidence}, Mastery {Mastery}",
            assessment.AssessedLevel,
            assessment.Confidence,
            assessment.HasMastery);

        return assessment;
    }

    /// <inheritdoc/>
    public async Task<ScaffoldedHint> GenerateHintAsync(
        SocraticQuestion question,
        int hintLevel,
        CancellationToken cancellationToken = default)
    {
        if (question == null)
            throw new ArgumentNullException(nameof(question));
        if (hintLevel < 1 || hintLevel > 5)
            throw new ArgumentOutOfRangeException(nameof(hintLevel), "Hint level must be between 1 and 5.");

        _logger.LogDebug(
            "Generating hint level {Level} for question {QuestionId}",
            hintLevel,
            question.ConceptId.Value);

        // Build hint generation prompt
        var systemPrompt = BuildHintSystemPrompt();
        var userPrompt = BuildHintUserPrompt(question, hintLevel);

        var chatMessages = new[]
        {
            ChatMessage.System(systemPrompt),
            ChatMessage.User(userPrompt)
        };

        var options = new LLMOptions
        {
            Temperature = 0.5,
            MaxTokens = 150
        };

        var response = await _llmModel.GenerateAsync(
            chatMessages,
            options,
            cancellationToken).ConfigureAwait(false);

        var hint = new ScaffoldedHint
        {
            Level = hintLevel,
            HintText = response.Content.Trim()
        };

        _logger.LogInformation(
            "Generated hint level {Level} for question {QuestionId}",
            hintLevel,
            question.ConceptId.Value);

        return hint;
    }

    private string BuildSystemPrompt(ConceptContext concept, StudentUnderstanding currentLevel, string? language)
    {
        var langNote = string.IsNullOrEmpty(language) ? string.Empty : $" Respond in {language}.";
        
        return $@"You are a Socratic tutor helping a student learn about {concept.ConceptId.Value}.
The student's current understanding level is: {currentLevel}.
Concept description: {concept.Description}
Learning objectives: {string.Join(", ", concept.LearningObjectives)}
Key terms: {string.Join(", ", concept.KeyTerms)}

Your role is to ask thought-provoking questions that guide the student toward understanding, NOT to give direct answers.
Generate ONE Socratic question that:
- Is appropriate for the student's current level ({currentLevel})
- Guides them to discover the answer themselves
- Uses one of these question types: Clarifying, Probing, Assumption, Implication, or Viewpoint
- Is age-appropriate and clear
- Does NOT reveal the answer directly{langNote}

Respond with ONLY the question text, nothing else.";
    }

    private string BuildUserPrompt(ConceptContext concept, StudentUnderstanding currentLevel)
    {
        return $"Generate a Socratic question about {concept.ConceptId.Value} for a student at {currentLevel} understanding level.";
    }

    private string BuildEvaluationSystemPrompt()
    {
        return @"You are an educational assessment system evaluating a student's response to a Socratic question.
Analyze the response and provide:
1. Understanding level: None, Beginner, Intermediate, Advanced, or Expert
2. Confidence score (0-1): How confident you are in this assessment
3. Feedback: Constructive, encouraging feedback
4. Misconceptions: List any misconceptions identified
5. Needs more help: Boolean indicating if student needs additional support
6. Has mastery: Boolean indicating if student demonstrates mastery
7. Next steps: Suggestions for what to learn next

Respond in JSON format with these fields.";
    }

    private string BuildEvaluationUserPrompt(SocraticQuestion question, string studentResponse)
    {
        return $@"Question: {question.QuestionText}
Question Type: {question.Type}
Concept: {question.ConceptId.Value}

Student Response: {studentResponse}

Evaluate this response and provide assessment.";
    }

    private string BuildHintSystemPrompt()
    {
        return @"You are a Socratic tutor providing scaffolded hints.
Provide hints at 5 levels:
- Level 1: Very general direction (most subtle)
- Level 2: Slightly more specific guidance
- Level 3: Partial information
- Level 4: Most of the answer
- Level 5: Complete answer (last resort)

Respond with ONLY the hint text for the requested level, nothing else.";
    }

    private string BuildHintUserPrompt(SocraticQuestion question, int hintLevel)
    {
        return $@"Question: {question.QuestionText}
Concept: {question.ConceptId.Value}

Provide a Level {hintLevel} hint for this question.";
    }

    private SocraticQuestion ParseQuestion(string response, ConceptContext concept, StudentUnderstanding currentLevel)
    {
        // Determine question type based on keywords in the response
        var questionType = DetermineQuestionType(response);

        return new SocraticQuestion
        {
            QuestionText = response.Trim(),
            Type = questionType,
            ConceptId = concept.ConceptId,
            TargetLevel = currentLevel,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private SocraticQuestionType DetermineQuestionType(string question)
    {
        var lowerQuestion = question.ToLowerInvariant();

        if (lowerQuestion.Contains("what do you mean") || lowerQuestion.Contains("can you clarify"))
            return SocraticQuestionType.Clarifying;

        if (lowerQuestion.Contains("why") || lowerQuestion.Contains("how") || lowerQuestion.Contains("can you explain"))
            return SocraticQuestionType.Probing;

        if (lowerQuestion.Contains("assumption") || lowerQuestion.Contains("assuming"))
            return SocraticQuestionType.Assumption;

        if (lowerQuestion.Contains("what would happen") || lowerQuestion.Contains("what if") || lowerQuestion.Contains("implication"))
            return SocraticQuestionType.Implication;

        if (lowerQuestion.Contains("how might") || lowerQuestion.Contains("viewpoint") || lowerQuestion.Contains("perspective"))
            return SocraticQuestionType.Viewpoint;

        // Default to probing if unclear
        return SocraticQuestionType.Probing;
    }

    private UnderstandingAssessment ParseAssessment(string response, SocraticQuestion question)
    {
        // Try to parse JSON response
        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            if (json != null)
            {
                return new UnderstandingAssessment
                {
                    AssessedLevel = ParseUnderstandingLevel(json.GetValueOrDefault("understanding_level")?.ToString()),
                    Confidence = ParseDouble(json.GetValueOrDefault("confidence")?.ToString()) ?? 0.5,
                    Feedback = json.GetValueOrDefault("feedback")?.ToString() ?? "Good effort!",
                    Misconceptions = ParseList(json.GetValueOrDefault("misconceptions")),
                    NeedsMoreHelp = ParseBool(json.GetValueOrDefault("needs_more_help")?.ToString()) ?? false,
                    HasMastery = ParseBool(json.GetValueOrDefault("has_mastery")?.ToString()) ?? false,
                    NextSteps = ParseList(json.GetValueOrDefault("next_steps"))
                };
            }
        }
        catch
        {
            // Fall through to default parsing
        }

        // Fallback: simple text-based parsing
        return ParseAssessmentFromText(response);
    }

    private UnderstandingAssessment ParseAssessmentFromText(string response)
    {
        var lowerResponse = response.ToLowerInvariant();
        var level = StudentUnderstanding.Intermediate;
        var confidence = 0.5;
        var hasMastery = false;
        var needsHelp = false;

        // Simple heuristics
        if (lowerResponse.Contains("excellent") || lowerResponse.Contains("perfect") || lowerResponse.Contains("mastery"))
        {
            level = StudentUnderstanding.Expert;
            hasMastery = true;
            confidence = 0.9;
        }
        else if (lowerResponse.Contains("good") || lowerResponse.Contains("correct"))
        {
            level = StudentUnderstanding.Advanced;
            confidence = 0.7;
        }
        else if (lowerResponse.Contains("incorrect") || lowerResponse.Contains("wrong") || lowerResponse.Contains("misunderstanding"))
        {
            level = StudentUnderstanding.Beginner;
            needsHelp = true;
            confidence = 0.6;
        }

        return new UnderstandingAssessment
        {
            AssessedLevel = level,
            Confidence = confidence,
            Feedback = response.Trim(),
            Misconceptions = Array.Empty<string>(),
            NeedsMoreHelp = needsHelp,
            HasMastery = hasMastery,
            NextSteps = Array.Empty<string>()
        };
    }

    private StudentUnderstanding ParseUnderstandingLevel(string? level)
    {
        if (string.IsNullOrEmpty(level))
            return StudentUnderstanding.Intermediate;

        return level.ToLowerInvariant() switch
        {
            "none" => StudentUnderstanding.None,
            "beginner" => StudentUnderstanding.Beginner,
            "intermediate" => StudentUnderstanding.Intermediate,
            "advanced" => StudentUnderstanding.Advanced,
            "expert" => StudentUnderstanding.Expert,
            _ => StudentUnderstanding.Intermediate
        };
    }

    private double? ParseDouble(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (double.TryParse(value, out var result))
            return result;

        return null;
    }

    private bool? ParseBool(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (bool.TryParse(value, out var result))
            return result;

        if (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1")
            return true;

        if (value.Equals("false", StringComparison.OrdinalIgnoreCase) || value == "0")
            return false;

        return null;
    }

    private IReadOnlyList<string> ParseList(object? value)
    {
        if (value == null)
            return Array.Empty<string>();

        if (value is string str)
        {
            // Try to parse as JSON array
            try
            {
                var array = System.Text.Json.JsonSerializer.Deserialize<string[]>(str);
                return array ?? Array.Empty<string>();
            }
            catch
            {
                // Return as single-item list
                return new[] { str };
            }
        }

        if (value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            return jsonElement.EnumerateArray()
                .Select(e => e.GetString() ?? string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        return Array.Empty<string>();
    }
}

using DotNetAgents.Core.Models;
using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Assessment;

/// <summary>
/// Implementation of assessment generation using LLM.
/// </summary>
public class AssessmentGenerator : IAssessmentGenerator
{
    private readonly ILLMModel<ChatMessage[], ChatMessage> _llmModel;
    private readonly ILogger<AssessmentGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssessmentGenerator"/> class.
    /// </summary>
    /// <param name="llmModel">The LLM model to use for question generation.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when llmModel is null.</exception>
    public AssessmentGenerator(
        ILLMModel<ChatMessage[], ChatMessage> llmModel,
        ILogger<AssessmentGenerator>? logger = null)
    {
        _llmModel = llmModel ?? throw new ArgumentNullException(nameof(llmModel));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AssessmentGenerator>.Instance;
    }

    /// <inheritdoc/>
    public async Task<Assessment> GenerateAsync(
        ConceptId concept,
        AssessmentSpecification spec,
        CancellationToken cancellationToken = default)
    {
        if (concept == null)
            throw new ArgumentNullException(nameof(concept));
        if (spec == null)
            throw new ArgumentNullException(nameof(spec));

        _logger.LogInformation(
            "Generating assessment for concept {ConceptId} with {Count} questions",
            concept.Value,
            spec.QuestionCount);

        var questions = new List<AssessmentQuestion>();

        // Generate questions based on specification
        var questionTypes = spec.QuestionTypes.Count > 0
            ? spec.QuestionTypes
            : new[] { QuestionType.MultipleChoice, QuestionType.ShortAnswer };

        var difficultyCounts = CalculateDifficultyCounts(spec.QuestionCount, spec.DifficultyDistribution);

        int questionIndex = 0;
        foreach (var questionType in questionTypes)
        {
            var typeCount = spec.QuestionCount / questionTypes.Count;
            var remainder = spec.QuestionCount % questionTypes.Count;
            var actualCount = typeCount + (questionIndex < remainder ? 1 : 0);

            for (int i = 0; i < actualCount; i++)
            {
                // Determine difficulty based on distribution
                var difficulty = DetermineDifficulty(i, actualCount, difficultyCounts);

                var question = await GenerateQuestionAsync(concept, questionType, difficulty, cancellationToken)
                    .ConfigureAwait(false);
                questions.Add(question);
            }

            questionIndex++;
        }

        var totalPoints = questions.Sum(q => q.Points);

        var assessment = new Assessment
        {
            AssessmentId = Guid.NewGuid().ToString(),
            ConceptId = concept,
            Title = $"Assessment: {concept.Value}",
            Description = $"Assessment covering {concept.Value}",
            Questions = questions,
            TotalPoints = totalPoints,
            GradeLevel = spec.GradeLevel,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _logger.LogInformation(
            "Generated assessment {AssessmentId} with {Count} questions, {Points} total points",
            assessment.AssessmentId,
            questions.Count,
            totalPoints);

        return assessment;
    }

    /// <inheritdoc/>
    public async Task<AssessmentQuestion> GenerateQuestionAsync(
        ConceptId concept,
        QuestionType type,
        DifficultyLevel difficulty,
        CancellationToken cancellationToken = default)
    {
        if (concept == null)
            throw new ArgumentNullException(nameof(concept));

        _logger.LogDebug(
            "Generating {Type} question for concept {ConceptId} at {Difficulty} difficulty",
            type,
            concept.Value,
            difficulty);

        var systemPrompt = BuildQuestionSystemPrompt(concept, type, difficulty);
        var userPrompt = BuildQuestionUserPrompt(concept, type, difficulty);

        var chatMessages = new[]
        {
            ChatMessage.System(systemPrompt),
            ChatMessage.User(userPrompt)
        };

        var options = new LLMOptions
        {
            Temperature = 0.7,
            MaxTokens = 400
        };

        var response = await _llmModel.GenerateAsync(
            chatMessages,
            options,
            cancellationToken).ConfigureAwait(false);

        var question = ParseQuestion(response.Content, concept, type, difficulty);

        _logger.LogInformation(
            "Generated {Type} question {QuestionId} for concept {ConceptId}",
            type,
            question.QuestionId,
            concept.Value);

        return question;
    }

    private string BuildQuestionSystemPrompt(ConceptId concept, QuestionType type, DifficultyLevel difficulty)
    {
        var typeDescription = type switch
        {
            QuestionType.MultipleChoice => "multiple choice question with 4 options (1 correct, 3 distractors)",
            QuestionType.ShortAnswer => "short answer question (1-2 sentences expected)",
            QuestionType.Essay => "essay question (paragraph response expected)",
            QuestionType.TrueFalse => "true/false question",
            QuestionType.Matching => "matching question with 5 pairs",
            _ => "question"
        };

        var difficultyDescription = difficulty switch
        {
            DifficultyLevel.Easy => "easy (basic recall)",
            DifficultyLevel.Medium => "medium (application of knowledge)",
            DifficultyLevel.Hard => "hard (analysis and synthesis)",
            _ => "medium"
        };

        return $@"You are an educational assessment generator creating {typeDescription} for concept: {concept.Value}
Difficulty level: {difficultyDescription}
Grade level: {concept.GradeLevel}

Generate a question that:
- Tests understanding of the concept
- Is age-appropriate for {concept.GradeLevel}
- Matches the difficulty level ({difficultyDescription})
- Has clear, unambiguous correct answer(s)
- For multiple choice: includes plausible but incorrect distractors

Respond in JSON format:
{{
  ""question"": ""question text"",
  ""correctAnswer"": ""correct answer"",
  ""distractors"": [""distractor1"", ""distractor2"", ""distractor3""],
  ""points"": 1
}}";
    }

    private string BuildQuestionUserPrompt(ConceptId concept, QuestionType type, DifficultyLevel difficulty)
    {
        return $"Generate a {type} question about {concept.Value} at {difficulty} difficulty level for {concept.GradeLevel}.";
    }

    private AssessmentQuestion ParseQuestion(string response, ConceptId concept, QuestionType type, DifficultyLevel difficulty)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            if (json != null)
            {
                var questionText = json.GetValueOrDefault("question")?.ToString() ?? "Question";
                var correctAnswer = json.GetValueOrDefault("correctAnswer")?.ToString() ?? string.Empty;
                var distractors = ParseStringArray(json.GetValueOrDefault("distractors"));
                var points = ParseInt(json.GetValueOrDefault("points")?.ToString()) ?? 1;

                return new AssessmentQuestion
                {
                    QuestionId = Guid.NewGuid().ToString(),
                    QuestionText = questionText,
                    Type = type,
                    Difficulty = difficulty,
                    ConceptId = concept,
                    CorrectAnswers = new[] { correctAnswer },
                    Distractors = distractors,
                    Points = points
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON response, using fallback parsing");
        }

        // Fallback: simple text parsing
        return new AssessmentQuestion
        {
            QuestionId = Guid.NewGuid().ToString(),
            QuestionText = response.Trim(),
            Type = type,
            Difficulty = difficulty,
            ConceptId = concept,
            CorrectAnswers = Array.Empty<string>(),
            Distractors = Array.Empty<string>(),
            Points = 1
        };
    }

    private IReadOnlyList<string> ParseStringArray(object? value)
    {
        if (value == null)
            return Array.Empty<string>();

        if (value is string str)
        {
            try
            {
                var array = System.Text.Json.JsonSerializer.Deserialize<string[]>(str);
                return array ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
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

    private int? ParseInt(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (int.TryParse(value, out var result))
            return result;

        return null;
    }

    private (int Easy, int Medium, int Hard) CalculateDifficultyCounts(int total, (int Easy, int Medium, int Hard) distribution)
    {
        var totalPercent = distribution.Easy + distribution.Medium + distribution.Hard;
        if (totalPercent == 0)
            return (total / 3, total / 3, total - (total / 3) * 2);

        var easy = (int)Math.Round(total * distribution.Easy / 100.0);
        var medium = (int)Math.Round(total * distribution.Medium / 100.0);
        var hard = total - easy - medium;

        return (easy, medium, hard);
    }

    private DifficultyLevel DetermineDifficulty(int index, int total, (int Easy, int Medium, int Hard) counts)
    {
        if (index < counts.Easy)
            return DifficultyLevel.Easy;
        if (index < counts.Easy + counts.Medium)
            return DifficultyLevel.Medium;
        return DifficultyLevel.Hard;
    }
}

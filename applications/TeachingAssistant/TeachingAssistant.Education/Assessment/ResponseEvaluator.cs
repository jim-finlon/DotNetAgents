using DotNetAgents.Abstractions.Models;
using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Assessment;

/// <summary>
/// Implementation of response evaluation with scoring and misconception detection.
/// </summary>
public class ResponseEvaluator : IResponseEvaluator
{
    private readonly ILLMModel<ChatMessage[], ChatMessage> _llmModel;
    private readonly ILogger<ResponseEvaluator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseEvaluator"/> class.
    /// </summary>
    /// <param name="llmModel">The LLM model to use for evaluation.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when llmModel is null.</exception>
    public ResponseEvaluator(
        ILLMModel<ChatMessage[], ChatMessage> llmModel,
        ILogger<ResponseEvaluator>? logger = null)
    {
        _llmModel = llmModel ?? throw new ArgumentNullException(nameof(llmModel));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ResponseEvaluator>.Instance;
    }

    /// <inheritdoc/>
    public async Task<EvaluationResult> EvaluateAsync(
        string studentResponse,
        AssessmentQuestion question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentResponse))
            throw new ArgumentException("Student response cannot be null or empty.", nameof(studentResponse));
        if (question == null)
            throw new ArgumentNullException(nameof(question));

        _logger.LogDebug(
            "Evaluating response for question {QuestionId}, type {Type}",
            question.QuestionId,
            question.Type);

        // Build evaluation prompt
        var systemPrompt = BuildEvaluationSystemPrompt(question);
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

        // Parse evaluation result
        var result = ParseEvaluationResult(response.Content, question, studentResponse);

        _logger.LogInformation(
            "Evaluated response: Score {Score}%, Correct {IsCorrect}, Points {Points}/{Total}",
            result.Score,
            result.IsCorrect,
            result.PointsAwarded,
            result.PointsPossible);

        return result;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Misconception>> DetectMisconceptionsAsync(
        string studentResponse,
        ConceptId concept,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentResponse))
            throw new ArgumentException("Student response cannot be null or empty.", nameof(studentResponse));
        if (concept == null)
            throw new ArgumentNullException(nameof(concept));

        _logger.LogDebug("Detecting misconceptions for concept {ConceptId}", concept.Value);

        var systemPrompt = BuildMisconceptionSystemPrompt();
        var userPrompt = BuildMisconceptionUserPrompt(concept, studentResponse);

        var chatMessages = new[]
        {
            ChatMessage.System(systemPrompt),
            ChatMessage.User(userPrompt)
        };

        var options = new LLMOptions
        {
            Temperature = 0.3,
            MaxTokens = 400
        };

        var response = await _llmModel.GenerateAsync(
            chatMessages,
            options,
            cancellationToken).ConfigureAwait(false);

        var misconceptions = ParseMisconceptions(response.Content, concept);

        _logger.LogInformation(
            "Detected {Count} misconceptions for concept {ConceptId}",
            misconceptions.Count,
            concept.Value);

        return misconceptions;
    }

    private string BuildEvaluationSystemPrompt(AssessmentQuestion question)
    {
        var evaluationCriteria = question.Type switch
        {
            QuestionType.MultipleChoice => "Check if the answer matches the correct option exactly.",
            QuestionType.TrueFalse => "Check if the answer matches 'true' or 'false'.",
            QuestionType.ShortAnswer => "Evaluate based on key concepts mentioned. Award partial credit for partially correct answers.",
            QuestionType.Essay => "Evaluate based on: 1) Correctness of key concepts, 2) Completeness, 3) Clarity. Award partial credit.",
            QuestionType.Matching => "Check if all pairs are correctly matched.",
            _ => "Evaluate the response for correctness."
        };

        return $@"You are an educational assessment evaluator.
Question Type: {question.Type}
Question: {question.QuestionText}
Correct Answer(s): {string.Join(", ", question.CorrectAnswers)}
Points Possible: {question.Points}

Evaluation Criteria:
{evaluationCriteria}

Respond in JSON format:
{{
  ""score"": 0-100,
  ""isCorrect"": true/false,
  ""pointsAwarded"": 0-{question.Points},
  ""feedback"": ""constructive feedback for the student"",
  ""misconceptions"": [""misconception1"", ""misconception2""],
  ""suggestions"": [""suggestion1"", ""suggestion2""]
}}";
    }

    private string BuildEvaluationUserPrompt(AssessmentQuestion question, string studentResponse)
    {
        return $@"Question: {question.QuestionText}
Correct Answer(s): {string.Join(", ", question.CorrectAnswers)}

Student Response: {studentResponse}

Evaluate this response and provide scoring and feedback.";
    }

    private string BuildMisconceptionSystemPrompt()
    {
        return @"You are an educational misconception detector.
Analyze student responses to identify common misconceptions about concepts.

Respond in JSON format with an array of misconceptions:
[
  {{
    ""description"": ""misconception description"",
    ""confidence"": 0.0-1.0
  }}
]";
    }

    private string BuildMisconceptionUserPrompt(ConceptId concept, string studentResponse)
    {
        return $@"Concept: {concept.Value}
Student Response: {studentResponse}

Identify any misconceptions in this response.";
    }

    private EvaluationResult ParseEvaluationResult(string response, AssessmentQuestion question, string studentResponse)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            if (json != null)
            {
                var score = ParseDouble(json.GetValueOrDefault("score")?.ToString()) ?? 0.0;
                var isCorrect = ParseBool(json.GetValueOrDefault("isCorrect")?.ToString()) ?? false;
                var pointsAwarded = ParseDouble(json.GetValueOrDefault("pointsAwarded")?.ToString()) ?? 0.0;
                var feedback = json.GetValueOrDefault("feedback")?.ToString() ?? "Good effort!";
                var misconceptions = ParseStringArray(json.GetValueOrDefault("misconceptions"));
                var suggestions = ParseStringArray(json.GetValueOrDefault("suggestions"));

                return new EvaluationResult
                {
                    Score = score,
                    IsCorrect = isCorrect,
                    PointsAwarded = pointsAwarded,
                    PointsPossible = question.Points,
                    Feedback = feedback,
                    Misconceptions = misconceptions,
                    Suggestions = suggestions
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON evaluation result, using fallback");
        }

        // Fallback: simple text-based evaluation
        return EvaluateSimple(studentResponse, question);
    }

    private EvaluationResult EvaluateSimple(string studentResponse, AssessmentQuestion question)
    {
        var lowerResponse = studentResponse.ToLowerInvariant().Trim();
        var lowerCorrect = question.CorrectAnswers
            .Select(a => a.ToLowerInvariant().Trim())
            .ToList();

        // Check for exact match
        var isExactMatch = lowerCorrect.Any(correct => lowerResponse == correct);
        if (isExactMatch)
        {
            return new EvaluationResult
            {
                Score = 100,
                IsCorrect = true,
                PointsAwarded = question.Points,
                PointsPossible = question.Points,
                Feedback = "Correct! Well done.",
                Misconceptions = Array.Empty<string>(),
                Suggestions = Array.Empty<string>()
            };
        }

        // Check for partial match (contains correct answer)
        var isPartialMatch = lowerCorrect.Any(correct => lowerResponse.Contains(correct, StringComparison.OrdinalIgnoreCase));
        if (isPartialMatch)
        {
            return new EvaluationResult
            {
                Score = 70,
                IsCorrect = false,
                PointsAwarded = (int)(question.Points * 0.7),
                PointsPossible = question.Points,
                Feedback = "Partially correct. Try to be more specific.",
                Misconceptions = Array.Empty<string>(),
                Suggestions = new[] { "Review the key concepts", "Be more specific in your answer" }
            };
        }

        return new EvaluationResult
        {
            Score = 0,
            IsCorrect = false,
            PointsAwarded = 0,
            PointsPossible = question.Points,
            Feedback = "Incorrect. Review the material and try again.",
            Misconceptions = Array.Empty<string>(),
            Suggestions = new[] { "Review the concept", "Ask for help if needed" }
        };
    }

    private IReadOnlyList<Misconception> ParseMisconceptions(string response, ConceptId concept)
    {
        var misconceptions = new List<Misconception>();

        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(response);
            
            if (json.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in json.EnumerateArray())
                {
                    var description = item.GetProperty("description").GetString() ?? string.Empty;
                    var confidence = item.TryGetProperty("confidence", out var confElement)
                        ? confElement.GetDouble()
                        : 0.5;

                    if (!string.IsNullOrEmpty(description))
                    {
                        misconceptions.Add(new Misconception
                        {
                            Id = Guid.NewGuid().ToString(),
                            Description = description,
                            ConceptId = concept,
                            Confidence = confidence
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse misconceptions JSON, returning empty list");
        }

        return misconceptions;
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
}

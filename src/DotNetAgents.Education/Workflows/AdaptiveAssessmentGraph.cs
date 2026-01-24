using DotNetAgents.Education.Assessment;
using DotNetAgents.Education.Models;
using DotNetAgents.Workflow.Graph;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Workflows;

/// <summary>
/// Pre-built workflow graph for adaptive assessments with difficulty adjustment.
/// </summary>
public class AdaptiveAssessmentGraph
{
    private readonly IAssessmentGenerator _assessmentGenerator;
    private readonly IResponseEvaluator _responseEvaluator;
    private readonly ILogger<AdaptiveAssessmentGraph>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveAssessmentGraph"/> class.
    /// </summary>
    /// <param name="assessmentGenerator">The assessment generator.</param>
    /// <param name="responseEvaluator">The response evaluator.</param>
    /// <param name="logger">Optional logger.</param>
    public AdaptiveAssessmentGraph(
        IAssessmentGenerator assessmentGenerator,
        IResponseEvaluator responseEvaluator,
        ILogger<AdaptiveAssessmentGraph>? logger = null)
    {
        _assessmentGenerator = assessmentGenerator ?? throw new ArgumentNullException(nameof(assessmentGenerator));
        _responseEvaluator = responseEvaluator ?? throw new ArgumentNullException(nameof(responseEvaluator));
        _logger = logger;
    }

    /// <summary>
    /// Builds the adaptive assessment workflow graph.
    /// </summary>
    /// <returns>The configured state graph.</returns>
    public StateGraph<AssessmentState> Build()
    {
        var graph = new StateGraph<AssessmentState>();

        // Add nodes
        graph.AddNode("initialize", InitializeNode)
              .AddNode("present_question", PresentQuestionNode)
              .AddNode("evaluate_response", EvaluateResponseNode)
              .AddNode("adjust_difficulty", AdjustDifficultyNode)
              .AddNode("check_termination", CheckTerminationNode)
              .AddNode("finalize", FinalizeNode)
              .SetEntryPoint("initialize")
              .AddExitPoint("finalize");

        // Add edges
        graph.AddEdge("initialize", "present_question")
              .AddEdge("present_question", "evaluate_response")
              .AddEdge("evaluate_response", "adjust_difficulty")
              .AddEdge("adjust_difficulty", "check_termination")
              .AddEdge(new GraphEdge<AssessmentState>("check_termination", "finalize", state => state.IsComplete || state.EarlyTerminated))
              .AddEdge(new GraphEdge<AssessmentState>("check_termination", "present_question", state => !state.IsComplete && !state.EarlyTerminated));

        return graph;
    }

    private Task<AssessmentState> InitializeNode(
        AssessmentState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Initializing adaptive assessment for student {StudentId}",
            state.StudentId);

        state.StartTime = DateTimeOffset.UtcNow;
        state.CurrentDifficulty = DifficultyLevel.Medium;
        state.CurrentQuestionIndex = 0;

        return Task.FromResult(state);
    }

    private async Task<AssessmentState> PresentQuestionNode(
        AssessmentState state,
        CancellationToken cancellationToken)
    {
        if (state.CurrentAssessment == null)
        {
            _logger?.LogWarning("Cannot present question without assessment");
            return state;
        }

        if (state.CurrentQuestionIndex >= state.CurrentAssessment.Questions.Count)
        {
            state.IsComplete = true;
            return state;
        }

        var question = state.CurrentAssessment.Questions[state.CurrentQuestionIndex];
        state.CurrentQuestion = question;

        _logger?.LogInformation(
            "Presenting question {QuestionIndex} ({Difficulty}) to student {StudentId}",
            state.CurrentQuestionIndex + 1,
            question.Difficulty,
            state.StudentId);

        return state;
    }

    private async Task<AssessmentState> EvaluateResponseNode(
        AssessmentState state,
        CancellationToken cancellationToken)
    {
        if (state.CurrentQuestion == null)
        {
            _logger?.LogWarning("Cannot evaluate without a question");
            return state;
        }

        if (!state.Responses.TryGetValue(state.CurrentQuestion.QuestionId, out var response))
        {
            _logger?.LogWarning("No response found for question {QuestionId}", state.CurrentQuestion.QuestionId);
            return state;
        }

        _logger?.LogInformation(
            "Evaluating response for question {QuestionId}",
            state.CurrentQuestion.QuestionId);

        var evaluationResult = await _responseEvaluator.EvaluateAsync(
            response,
            state.CurrentQuestion,
            cancellationToken).ConfigureAwait(false);

        state.EvaluationResults[state.CurrentQuestion.QuestionId] = evaluationResult;
        state.TotalScore += evaluationResult.PointsAwarded;
        state.TotalPointsPossible += evaluationResult.PointsPossible;

        // Update consecutive counters
        if (evaluationResult.IsCorrect)
        {
            state.ConsecutiveCorrect++;
            state.ConsecutiveIncorrect = 0;
        }
        else
        {
            state.ConsecutiveIncorrect++;
            state.ConsecutiveCorrect = 0;
        }

        return state;
    }

    private Task<AssessmentState> AdjustDifficultyNode(
        AssessmentState state,
        CancellationToken cancellationToken)
    {
        if (state.CurrentQuestion == null)
        {
            return Task.FromResult(state);
        }

        var evaluationResult = state.EvaluationResults.GetValueOrDefault(state.CurrentQuestion.QuestionId);
        if (evaluationResult == null)
        {
            return Task.FromResult(state);
        }

        // Adaptive difficulty adjustment
        if (evaluationResult.IsCorrect && state.ConsecutiveCorrect >= 2)
        {
            // Increase difficulty after 2 consecutive correct answers
            state.CurrentDifficulty = state.CurrentDifficulty switch
            {
                DifficultyLevel.Easy => DifficultyLevel.Medium,
                DifficultyLevel.Medium => DifficultyLevel.Hard,
                DifficultyLevel.Hard => DifficultyLevel.Hard,
                _ => state.CurrentDifficulty
            };

            _logger?.LogInformation(
                "Increasing difficulty to {Difficulty} for student {StudentId}",
                state.CurrentDifficulty,
                state.StudentId);
        }
        else if (!evaluationResult.IsCorrect && state.ConsecutiveIncorrect >= 2)
        {
            // Decrease difficulty after 2 consecutive incorrect answers
            state.CurrentDifficulty = state.CurrentDifficulty switch
            {
                DifficultyLevel.Hard => DifficultyLevel.Medium,
                DifficultyLevel.Medium => DifficultyLevel.Easy,
                DifficultyLevel.Easy => DifficultyLevel.Easy,
                _ => state.CurrentDifficulty
            };

            _logger?.LogInformation(
                "Decreasing difficulty to {Difficulty} for student {StudentId}",
                state.CurrentDifficulty,
                state.StudentId);
        }

        return Task.FromResult(state);
    }

    private Task<AssessmentState> CheckTerminationNode(
        AssessmentState state,
        CancellationToken cancellationToken)
    {
        // Check if all questions answered
        if (state.CurrentAssessment != null &&
            state.CurrentQuestionIndex >= state.CurrentAssessment.Questions.Count - 1)
        {
            state.IsComplete = true;
            _logger?.LogInformation("Assessment complete for student {StudentId}", state.StudentId);
            return Task.FromResult(state);
        }

        // Early termination: Mastery achieved (90%+ score)
        var scorePercentage = state.TotalPointsPossible > 0
            ? (state.TotalScore / state.TotalPointsPossible) * 100
            : 0;

        if (scorePercentage >= 90 && state.CurrentQuestionIndex >= 3)
        {
            state.EarlyTerminated = true;
            state.TerminationReason = "Mastery achieved";
            state.MasteryAchieved = true;
            state.IsComplete = true;
            _logger?.LogInformation(
                "Early termination: Mastery achieved ({Score}%) for student {StudentId}",
                scorePercentage,
                state.StudentId);
            return Task.FromResult(state);
        }

        // Early termination: Struggling (below 40% after 5 questions)
        if (scorePercentage < 40 && state.CurrentQuestionIndex >= 4)
        {
            state.EarlyTerminated = true;
            state.TerminationReason = "Student struggling - needs additional support";
            state.IsComplete = true;
            _logger?.LogInformation(
                "Early termination: Student struggling ({Score}%) for student {StudentId}",
                scorePercentage,
                state.StudentId);
            return Task.FromResult(state);
        }

        // Move to next question
        state.CurrentQuestionIndex++;

        return Task.FromResult(state);
    }

    private Task<AssessmentState> FinalizeNode(
        AssessmentState state,
        CancellationToken cancellationToken)
    {
        state.EndTime = DateTimeOffset.UtcNow;
        state.IsComplete = true;

        var scorePercentage = state.TotalPointsPossible > 0
            ? (state.TotalScore / state.TotalPointsPossible) * 100
            : 0;

        if (scorePercentage >= 95)
        {
            state.MasteryAchieved = true;
        }

        _logger?.LogInformation(
            "Assessment finalized for student {StudentId}: Score {Score}/{Total} ({Percentage}%)",
            state.StudentId,
            state.TotalScore,
            state.TotalPointsPossible,
            scorePercentage);

        return Task.FromResult(state);
    }
}

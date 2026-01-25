using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using DotNetAgents.Workflow.Graph;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Workflows;

/// <summary>
/// Pre-built workflow graph for Socratic tutoring sessions.
/// </summary>
public class SocraticTutorGraph
{
    private readonly ISocraticDialogueEngine _dialogueEngine;
    private readonly ILogger<SocraticTutorGraph>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocraticTutorGraph"/> class.
    /// </summary>
    /// <param name="dialogueEngine">The Socratic dialogue engine.</param>
    /// <param name="logger">Optional logger.</param>
    public SocraticTutorGraph(
        ISocraticDialogueEngine dialogueEngine,
        ILogger<SocraticTutorGraph>? logger = null)
    {
        _dialogueEngine = dialogueEngine ?? throw new ArgumentNullException(nameof(dialogueEngine));
        _logger = logger;
    }

    /// <summary>
    /// Builds the Socratic tutor workflow graph.
    /// </summary>
    /// <returns>The configured state graph.</returns>
    public StateGraph<SocraticDialogueState> Build()
    {
        var graph = new StateGraph<SocraticDialogueState>();

        // Add nodes
        graph.AddNode("assess", AssessNode)
              .AddNode("question", QuestionNode)
              .AddNode("evaluate", EvaluateNode)
              .AddNode("hint", HintNode)
              .AddNode("celebrate", CelebrateNode)
              .SetEntryPoint("assess")
              .AddExitPoint("celebrate");

        // Add edges with conditions
        graph.AddEdge("assess", "question")
              .AddEdge("question", "evaluate")
              .AddEdge("evaluate", "celebrate", state => state.MasteryAchieved)
              .AddEdge("evaluate", "hint", state => state.LastAssessment?.NeedsMoreHelp == true && !state.MasteryAchieved)
              .AddEdge("evaluate", "question", state => !state.MasteryAchieved && state.LastAssessment?.NeedsMoreHelp != true)
              .AddEdge("hint", "question", state => state.CurrentHintLevel < 5)
              .AddEdge("hint", "celebrate", state => state.CurrentHintLevel >= 5 && state.MasteryAchieved);

        return graph;
    }

    private async Task<SocraticDialogueState> AssessNode(
        SocraticDialogueState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Assessing student {StudentId} understanding of concept {ConceptId}",
            state.StudentId,
            state.Concept.ConceptId.Value);

        state.CurrentPhase = DialoguePhase.Assessing;
        state.CurrentLevel = StudentUnderstanding.Beginner; // Initial assessment

        state.ConversationHistory.Add(new DialogueTurn
        {
            TurnNumber = state.ConversationHistory.Count + 1,
            Speaker = "Tutor",
            Message = $"Let's explore {state.Concept.ConceptId.Value}. What do you already know about this concept?",
            Timestamp = DateTimeOffset.UtcNow
        });

        return state;
    }

    private async Task<SocraticDialogueState> QuestionNode(
        SocraticDialogueState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Generating Socratic question for student {StudentId}",
            state.StudentId);

        state.CurrentPhase = DialoguePhase.Questioning;
        state.QuestionCount++;

        var question = await _dialogueEngine.GenerateQuestionAsync(
            state.Concept,
            state.CurrentLevel,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        state.CurrentQuestion = question;

        state.ConversationHistory.Add(new DialogueTurn
        {
            TurnNumber = state.ConversationHistory.Count + 1,
            Speaker = "Tutor",
            Message = question.QuestionText,
            Timestamp = DateTimeOffset.UtcNow
        });

        return state;
    }

    private async Task<SocraticDialogueState> EvaluateNode(
        SocraticDialogueState state,
        CancellationToken cancellationToken)
    {
        if (state.CurrentQuestion == null || string.IsNullOrWhiteSpace(state.LastResponse))
        {
            _logger?.LogWarning("Cannot evaluate without question and response");
            return state;
        }

        _logger?.LogInformation(
            "Evaluating response from student {StudentId}",
            state.StudentId);

        state.CurrentPhase = DialoguePhase.Evaluating;

        var assessment = await _dialogueEngine.EvaluateResponseAsync(
            state.LastResponse,
            state.CurrentQuestion,
            cancellationToken).ConfigureAwait(false);

        state.LastAssessment = assessment;
        state.CurrentLevel = assessment.AssessedLevel;

        if (assessment.HasMastery)
        {
            state.MasteryAchieved = true;
        }

        state.ConversationHistory.Add(new DialogueTurn
        {
            TurnNumber = state.ConversationHistory.Count + 1,
            Speaker = "Tutor",
            Message = assessment.Feedback,
            Timestamp = DateTimeOffset.UtcNow
        });

        return state;
    }

    private async Task<SocraticDialogueState> HintNode(
        SocraticDialogueState state,
        CancellationToken cancellationToken)
    {
        if (state.CurrentQuestion == null)
        {
            _logger?.LogWarning("Cannot provide hint without a question");
            return state;
        }

        _logger?.LogInformation(
            "Providing hint level {HintLevel} to student {StudentId}",
            state.CurrentHintLevel + 1,
            state.StudentId);

        state.CurrentPhase = DialoguePhase.Hinting;
        state.CurrentHintLevel = Math.Min(state.CurrentHintLevel + 1, 5);
        state.HintCount++;

        var hint = await _dialogueEngine.GenerateHintAsync(
            state.CurrentQuestion,
            state.CurrentHintLevel,
            cancellationToken).ConfigureAwait(false);

        state.CurrentHint = hint;

        state.ConversationHistory.Add(new DialogueTurn
        {
            TurnNumber = state.ConversationHistory.Count + 1,
            Speaker = "Tutor",
            Message = $"Hint: {hint.HintText}",
            Timestamp = DateTimeOffset.UtcNow
        });

        return state;
    }

    private Task<SocraticDialogueState> CelebrateNode(
        SocraticDialogueState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Celebrating mastery achievement for student {StudentId}",
            state.StudentId);

        state.CurrentPhase = DialoguePhase.Celebrating;
        state.MasteryAchieved = true;

        state.ConversationHistory.Add(new DialogueTurn
        {
            TurnNumber = state.ConversationHistory.Count + 1,
            Speaker = "Tutor",
            Message = "Excellent work! You've demonstrated a strong understanding of this concept. Let's move on to the next topic!",
            Timestamp = DateTimeOffset.UtcNow
        });

        state.CurrentPhase = DialoguePhase.Completed;

        return Task.FromResult(state);
    }
}

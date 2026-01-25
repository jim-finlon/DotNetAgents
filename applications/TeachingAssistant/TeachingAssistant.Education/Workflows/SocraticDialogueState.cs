using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Workflows;

/// <summary>
/// Represents the state of a Socratic dialogue tutoring session.
/// </summary>
public class SocraticDialogueState
{
    /// <summary>
    /// Gets or sets the student identifier.
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the concept being taught.
    /// </summary>
    public ConceptContext Concept { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current understanding level.
    /// </summary>
    public StudentUnderstanding CurrentLevel { get; set; }

    /// <summary>
    /// Gets or sets the current Socratic question.
    /// </summary>
    public SocraticQuestion? CurrentQuestion { get; set; }

    /// <summary>
    /// Gets or sets the student's last response.
    /// </summary>
    public string? LastResponse { get; set; }

    /// <summary>
    /// Gets or sets the last assessment of understanding.
    /// </summary>
    public UnderstandingAssessment? LastAssessment { get; set; }

    /// <summary>
    /// Gets or sets the current hint level (1-5).
    /// </summary>
    public int CurrentHintLevel { get; set; }

    /// <summary>
    /// Gets or sets the current scaffolded hint.
    /// </summary>
    public ScaffoldedHint? CurrentHint { get; set; }

    /// <summary>
    /// Gets or sets the number of questions asked.
    /// </summary>
    public int QuestionCount { get; set; }

    /// <summary>
    /// Gets or sets the number of hints provided.
    /// </summary>
    public int HintCount { get; set; }

    /// <summary>
    /// Gets or sets whether mastery has been achieved.
    /// </summary>
    public bool MasteryAchieved { get; set; }

    /// <summary>
    /// Gets or sets the conversation history.
    /// </summary>
    public List<DialogueTurn> ConversationHistory { get; set; } = new();

    /// <summary>
    /// Gets or sets the current phase of the dialogue.
    /// </summary>
    public DialoguePhase CurrentPhase { get; set; } = DialoguePhase.Assessing;

    /// <summary>
    /// Gets or sets metadata about the session.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a turn in the dialogue conversation.
/// </summary>
public record DialogueTurn
{
    /// <summary>
    /// Gets the turn number.
    /// </summary>
    public int TurnNumber { get; init; }

    /// <summary>
    /// Gets the speaker (Tutor or Student).
    /// </summary>
    public string Speaker { get; init; } = string.Empty;

    /// <summary>
    /// Gets the message content.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents the current phase of the Socratic dialogue.
/// </summary>
public enum DialoguePhase
{
    /// <summary>
    /// Assessing the student's initial understanding.
    /// </summary>
    Assessing,

    /// <summary>
    /// Asking a Socratic question.
    /// </summary>
    Questioning,

    /// <summary>
    /// Evaluating the student's response.
    /// </summary>
    Evaluating,

    /// <summary>
    /// Providing a hint.
    /// </summary>
    Hinting,

    /// <summary>
    /// Celebrating mastery achievement.
    /// </summary>
    Celebrating,

    /// <summary>
    /// Dialogue completed.
    /// </summary>
    Completed
}

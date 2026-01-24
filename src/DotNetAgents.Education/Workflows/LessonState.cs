using DotNetAgents.Education.Assessment;
using DotNetAgents.Education.Models;

namespace DotNetAgents.Education.Workflows;

/// <summary>
/// Represents the state of a lesson delivery session.
/// </summary>
public class LessonState
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
    /// Gets or sets the current lesson phase.
    /// </summary>
    public LessonPhase CurrentPhase { get; set; } = LessonPhase.Introduction;

    /// <summary>
    /// Gets or sets the lesson content sections.
    /// </summary>
    public List<LessonSection> Sections { get; set; } = new();

    /// <summary>
    /// Gets or sets the current section index.
    /// </summary>
    public int CurrentSectionIndex { get; set; }

    /// <summary>
    /// Gets or sets the current section.
    /// </summary>
    public LessonSection? CurrentSection { get; set; }

    /// <summary>
    /// Gets or sets the practice problems.
    /// </summary>
    public List<AssessmentQuestion> PracticeProblems { get; set; } = new();

    /// <summary>
    /// Gets or sets the current practice problem index.
    /// </summary>
    public int CurrentPracticeIndex { get; set; }

    /// <summary>
    /// Gets or sets the current practice problem.
    /// </summary>
    public AssessmentQuestion? CurrentPracticeProblem { get; set; }

    /// <summary>
    /// Gets or sets the practice problem responses.
    /// </summary>
    public Dictionary<string, string> PracticeResponses { get; set; } = new();

    /// <summary>
    /// Gets or sets the practice problem evaluation results.
    /// </summary>
    public Dictionary<string, EvaluationResult> PracticeResults { get; set; } = new();

    /// <summary>
    /// Gets or sets the mastery check assessment.
    /// </summary>
    public DotNetAgents.Education.Assessment.Assessment? MasteryCheckAssessment { get; set; }

    /// <summary>
    /// Gets or sets whether mastery has been achieved.
    /// </summary>
    public bool MasteryAchieved { get; set; }

    /// <summary>
    /// Gets or sets the mastery score (0-100).
    /// </summary>
    public double MasteryScore { get; set; }

    /// <summary>
    /// Gets or sets whether the lesson is complete.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets or sets the lesson start time.
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the lesson end time.
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Gets or sets metadata about the lesson session.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a section of a lesson.
/// </summary>
public record LessonSection
{
    /// <summary>
    /// Gets the section identifier.
    /// </summary>
    public string SectionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the section title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the section content.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the section order.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets whether this section has been completed.
    /// </summary>
    public bool IsCompleted { get; init; }
}

/// <summary>
/// Represents the current phase of a lesson.
/// </summary>
public enum LessonPhase
{
    /// <summary>
    /// Introduction phase - introducing the concept.
    /// </summary>
    Introduction,

    /// <summary>
    /// Content delivery phase - teaching the concept.
    /// </summary>
    ContentDelivery,

    /// <summary>
    /// Practice phase - students practice with problems.
    /// </summary>
    Practice,

    /// <summary>
    /// Mastery check phase - assessing understanding.
    /// </summary>
    MasteryCheck,

    /// <summary>
    /// Review phase - reviewing and reinforcing.
    /// </summary>
    Review,

    /// <summary>
    /// Lesson completed.
    /// </summary>
    Completed
}

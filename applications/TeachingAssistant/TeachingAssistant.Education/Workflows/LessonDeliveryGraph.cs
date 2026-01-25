using DotNetAgents.Education.Assessment;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using DotNetAgents.Workflow.Graph;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Workflows;

/// <summary>
/// Pre-built workflow graph for lesson delivery with mastery check gates.
/// </summary>
public class LessonDeliveryGraph
{
    private readonly IAssessmentGenerator _assessmentGenerator;
    private readonly IResponseEvaluator _responseEvaluator;
    private readonly IMasteryCalculator _masteryCalculator;
    private readonly ILogger<LessonDeliveryGraph>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LessonDeliveryGraph"/> class.
    /// </summary>
    /// <param name="assessmentGenerator">The assessment generator.</param>
    /// <param name="responseEvaluator">The response evaluator.</param>
    /// <param name="masteryCalculator">The mastery calculator.</param>
    /// <param name="logger">Optional logger.</param>
    public LessonDeliveryGraph(
        IAssessmentGenerator assessmentGenerator,
        IResponseEvaluator responseEvaluator,
        IMasteryCalculator masteryCalculator,
        ILogger<LessonDeliveryGraph>? logger = null)
    {
        _assessmentGenerator = assessmentGenerator ?? throw new ArgumentNullException(nameof(assessmentGenerator));
        _responseEvaluator = responseEvaluator ?? throw new ArgumentNullException(nameof(responseEvaluator));
        _masteryCalculator = masteryCalculator ?? throw new ArgumentNullException(nameof(masteryCalculator));
        _logger = logger;
    }

    /// <summary>
    /// Builds the lesson delivery workflow graph.
    /// </summary>
    /// <returns>The configured state graph.</returns>
    public StateGraph<LessonState> Build()
    {
        var graph = new StateGraph<LessonState>();

        // Add nodes
        graph.AddNode("introduce", IntroduceNode)
              .AddNode("deliver_content", DeliverContentNode)
              .AddNode("practice", PracticeNode)
              .AddNode("check_mastery", CheckMasteryNode)
              .AddNode("review", ReviewNode)
              .AddNode("complete", CompleteNode)
              .SetEntryPoint("introduce")
              .AddExitPoint("complete");

        // Add edges with conditions
        graph.AddEdge("introduce", "deliver_content")
              .AddEdge("deliver_content", "practice")
              .AddEdge("practice", "check_mastery")
              .AddEdge(new GraphEdge<LessonState>("check_mastery", "complete", state => state.MasteryAchieved))
              .AddEdge(new GraphEdge<LessonState>("check_mastery", "review", state => !state.MasteryAchieved && state.MasteryScore >= 60))
              .AddEdge(new GraphEdge<LessonState>("check_mastery", "deliver_content", state => !state.MasteryAchieved && state.MasteryScore < 60))
              .AddEdge("review", "check_mastery");

        return graph;
    }

    private Task<LessonState> IntroduceNode(
        LessonState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Introducing concept {ConceptId} to student {StudentId}",
            state.Concept.ConceptId.Value,
            state.StudentId);

        state.CurrentPhase = LessonPhase.Introduction;
        state.StartTime = DateTimeOffset.UtcNow;

        // Add introduction section
        if (state.Sections.Count == 0)
        {
            state.Sections.Add(new LessonSection
            {
                SectionId = "intro",
                Title = "Introduction",
                Content = $"Welcome! Today we'll learn about {state.Concept.ConceptId.Value}.",
                Order = 0
            });
        }

        state.CurrentSectionIndex = 0;
        state.CurrentSection = state.Sections[0];

        return Task.FromResult(state);
    }

    private Task<LessonState> DeliverContentNode(
        LessonState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Delivering content section {SectionIndex} to student {StudentId}",
            state.CurrentSectionIndex,
            state.StudentId);

        state.CurrentPhase = LessonPhase.ContentDelivery;

        // Move to next section if available
        if (state.CurrentSectionIndex < state.Sections.Count - 1)
        {
            state.CurrentSectionIndex++;
            state.CurrentSection = state.Sections[state.CurrentSectionIndex];
        }
        else if (state.Sections.Count == 1)
        {
            // Add content sections if not already present
            state.Sections.Add(new LessonSection
            {
                SectionId = "content-1",
                Title = "Main Content",
                Content = state.Concept.Description,
                Order = 1
            });
            state.CurrentSectionIndex = 1;
            state.CurrentSection = state.Sections[1];
        }

        return Task.FromResult(state);
    }

    private async Task<LessonState> PracticeNode(
        LessonState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Starting practice phase for student {StudentId}",
            state.StudentId);

        state.CurrentPhase = LessonPhase.Practice;

        // Generate practice problems if not already present
        if (state.PracticeProblems.Count == 0)
        {
            var practiceSpec = new AssessmentSpecification
            {
                ConceptId = state.Concept.ConceptId,
                QuestionCount = 3,
                QuestionTypes = new[] { QuestionType.ShortAnswer, QuestionType.MultipleChoice },
                DifficultyDistribution = (50, 40, 10),
                GradeLevel = state.Concept.ConceptId.GradeLevel
            };

            var practiceAssessment = await _assessmentGenerator.GenerateAsync(
                state.Concept.ConceptId,
                practiceSpec,
                cancellationToken).ConfigureAwait(false);

            state.PracticeProblems = practiceAssessment.Questions.ToList();
        }

        // Present first practice problem if not already started
        if (state.CurrentPracticeIndex == 0 && state.CurrentPracticeProblem == null)
        {
            state.CurrentPracticeProblem = state.PracticeProblems[0];
        }

        return state;
    }

    private async Task<LessonState> CheckMasteryNode(
        LessonState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Checking mastery for student {StudentId}",
            state.StudentId);

        state.CurrentPhase = LessonPhase.MasteryCheck;

        // Generate mastery check assessment if not already present
        if (state.MasteryCheckAssessment == null)
        {
            var masterySpec = new AssessmentSpecification
            {
                ConceptId = state.Concept.ConceptId,
                QuestionCount = 5,
                QuestionTypes = new[] { QuestionType.MultipleChoice, QuestionType.ShortAnswer },
                DifficultyDistribution = (20, 50, 30),
                GradeLevel = state.Concept.ConceptId.GradeLevel
            };

            state.MasteryCheckAssessment = await _assessmentGenerator.GenerateAsync(
                state.Concept.ConceptId,
                masterySpec,
                cancellationToken).ConfigureAwait(false);
        }

        // Evaluate all practice responses if not already evaluated
        if (state.PracticeResults.Count < state.PracticeResponses.Count)
        {
            foreach (var (questionId, response) in state.PracticeResponses)
            {
                if (!state.PracticeResults.ContainsKey(questionId))
                {
                    var question = state.PracticeProblems.FirstOrDefault(q => q.QuestionId == questionId);
                    if (question != null)
                    {
                        var result = await _responseEvaluator.EvaluateAsync(
                            response,
                            question,
                            cancellationToken).ConfigureAwait(false);

                        state.PracticeResults[questionId] = result;
                    }
                }
            }
        }

        // Calculate mastery score
        var totalScore = state.PracticeResults.Values.Sum(r => r.PointsAwarded);
        var totalPossible = state.PracticeResults.Values.Sum(r => r.PointsPossible);
        state.MasteryScore = totalPossible > 0 ? (totalScore / totalPossible) * 100 : 0;

        // Check if mastery achieved (95%+)
        state.MasteryAchieved = state.MasteryScore >= 95;

        // Calculate mastery level using mastery calculator
        var assessmentResults = state.PracticeResults.Values.Select(r => new AssessmentResult
        {
            Score = state.MasteryScore,
            Timestamp = DateTimeOffset.UtcNow,
            AssessmentId = "mastery-check"
        }).ToList();

        var masteryLevel = _masteryCalculator.CalculateMastery(
            state.Concept.ConceptId,
            assessmentResults);

        state.MasteryAchieved = masteryLevel >= MasteryLevel.Mastery;

        _logger?.LogInformation(
            "Mastery check complete for student {StudentId}: Score {Score}%, Mastery: {Mastery}",
            state.StudentId,
            state.MasteryScore,
            state.MasteryAchieved);

        return state;
    }

    private Task<LessonState> ReviewNode(
        LessonState state,
        CancellationToken cancellationToken)
    {
        _logger?.LogInformation(
            "Reviewing concept with student {StudentId}",
            state.StudentId);

        state.CurrentPhase = LessonPhase.Review;

        // Add review section
        state.Sections.Add(new LessonSection
        {
            SectionId = "review",
            Title = "Review",
            Content = $"Let's review what we've learned about {state.Concept.ConceptId.Value}. " +
                     $"You scored {state.MasteryScore:F1}% on the practice problems. " +
                     "Let's go over the key points again.",
            Order = state.Sections.Count
        });

        return Task.FromResult(state);
    }

    private Task<LessonState> CompleteNode(
        LessonState state,
        CancellationToken cancellationToken)
    {
        state.CurrentPhase = LessonPhase.Completed;
        state.IsComplete = true;
        state.EndTime = DateTimeOffset.UtcNow;

        _logger?.LogInformation(
            "Lesson completed for student {StudentId}: Mastery {Mastery}, Score {Score}%",
            state.StudentId,
            state.MasteryAchieved,
            state.MasteryScore);

        return Task.FromResult(state);
    }
}

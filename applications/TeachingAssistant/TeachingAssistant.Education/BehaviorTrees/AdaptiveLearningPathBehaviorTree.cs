using DotNetAgents.Agents.BehaviorTrees;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Pedagogy;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.BehaviorTrees;

/// <summary>
/// Context object for adaptive learning path behavior tree operations.
/// </summary>
public class AdaptiveLearningPathContext
{
    /// <summary>
    /// Gets or sets the student identifier.
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the available concepts to choose from.
    /// </summary>
    public IReadOnlyList<ConceptId> AvailableConcepts { get; set; } = Array.Empty<ConceptId>();

    /// <summary>
    /// Gets or sets the student's current mastery levels.
    /// </summary>
    public IReadOnlyDictionary<ConceptId, ConceptMastery> StudentMastery { get; set; } = new Dictionary<ConceptId, ConceptMastery>();

    /// <summary>
    /// Gets or sets the selected concept for the learning path.
    /// </summary>
    public ConceptId? SelectedConcept { get; set; }

    /// <summary>
    /// Gets or sets the learning path strategy used.
    /// </summary>
    public LearningPathStrategy Strategy { get; set; } = LearningPathStrategy.Unknown;

    /// <summary>
    /// Gets or sets whether prerequisites are met for the selected concept.
    /// </summary>
    public bool PrerequisitesMet { get; set; }

    /// <summary>
    /// Gets or sets the reason for selecting this concept.
    /// </summary>
    public string? SelectionReason { get; set; }
}

/// <summary>
/// Represents the learning path selection strategy.
/// </summary>
public enum LearningPathStrategy
{
    Unknown,
    PrerequisiteBased,
    MasteryGap,
    DifficultyProgression,
    InterestBased,
    ReviewNeeded
}

/// <summary>
/// Behavior tree for adaptive learning path selection that determines the next concept
/// a student should learn based on mastery, prerequisites, and learning goals.
/// </summary>
public class AdaptiveLearningPathBehaviorTree
{
    private readonly BehaviorTree<AdaptiveLearningPathContext> _tree;
    private readonly BehaviorTreeExecutor<AdaptiveLearningPathContext> _executor;
    private readonly ILogger<AdaptiveLearningPathBehaviorTree>? _logger;
    private readonly IMasteryCalculator? _masteryCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveLearningPathBehaviorTree"/> class.
    /// </summary>
    /// <param name="masteryCalculator">Optional mastery calculator for prerequisite checking.</param>
    /// <param name="logger">Optional logger instance.</param>
    public AdaptiveLearningPathBehaviorTree(
        IMasteryCalculator? masteryCalculator = null,
        ILogger<AdaptiveLearningPathBehaviorTree>? logger = null)
    {
        _logger = logger;
        _masteryCalculator = masteryCalculator;

        // Create logger for behavior tree nodes
        ILogger<BehaviorTreeNode<AdaptiveLearningPathContext>>? behaviorTreeLogger = null;
        ILogger<BehaviorTreeExecutor<AdaptiveLearningPathContext>>? executorLogger = null;
        if (logger != null)
        {
            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug));
            behaviorTreeLogger = loggerFactory.CreateLogger<BehaviorTreeNode<AdaptiveLearningPathContext>>();
            executorLogger = loggerFactory.CreateLogger<BehaviorTreeExecutor<AdaptiveLearningPathContext>>();
        }

        // Build behavior tree
        // Root: Selector (try strategies in order)
        var root = new SelectorNode<AdaptiveLearningPathContext>("LearningPathSelector", behaviorTreeLogger)
            // Strategy 1: Review Needed (concepts that need review based on spaced repetition)
            .AddChild(new SequenceNode<AdaptiveLearningPathContext>("ReviewNeededSequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<AdaptiveLearningPathContext>(
                    "HasReviewNeeded",
                    ctx => HasReviewNeededConcepts(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<AdaptiveLearningPathContext>(
                    "SelectReviewConcept",
                    ctx => SelectReviewConcept(ctx),
                    behaviorTreeLogger)))
            // Strategy 2: Mastery Gap (concepts with low mastery that have prerequisites met)
            .AddChild(new SequenceNode<AdaptiveLearningPathContext>("MasteryGapSequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<AdaptiveLearningPathContext>(
                    "HasMasteryGaps",
                    ctx => HasMasteryGaps(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<AdaptiveLearningPathContext>(
                    "SelectMasteryGapConcept",
                    ctx => SelectMasteryGapConcept(ctx),
                    behaviorTreeLogger)))
            // Strategy 3: Prerequisite-Based (concepts ready to learn based on prerequisites)
            .AddChild(new SequenceNode<AdaptiveLearningPathContext>("PrerequisiteSequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<AdaptiveLearningPathContext>(
                    "HasReadyConcepts",
                    ctx => HasReadyConcepts(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<AdaptiveLearningPathContext>(
                    "SelectPrerequisiteConcept",
                    ctx => SelectPrerequisiteConcept(ctx),
                    behaviorTreeLogger)))
            // Strategy 4: Difficulty Progression (concepts at appropriate difficulty level)
            .AddChild(new SequenceNode<AdaptiveLearningPathContext>("DifficultySequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<AdaptiveLearningPathContext>(
                    "HasAppropriateDifficulty",
                    ctx => HasAppropriateDifficulty(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<AdaptiveLearningPathContext>(
                    "SelectDifficultyConcept",
                    ctx => SelectDifficultyConcept(ctx),
                    behaviorTreeLogger)))
            // Fallback: Select first available concept
            .AddChild(new ActionNode<AdaptiveLearningPathContext>(
                "SelectFirstAvailable",
                ctx => SelectFirstAvailable(ctx),
                behaviorTreeLogger));

        _tree = new BehaviorTree<AdaptiveLearningPathContext>("AdaptiveLearningPathTree", root);
        _executor = new BehaviorTreeExecutor<AdaptiveLearningPathContext>(executorLogger);
    }

    /// <summary>
    /// Determines the next concept for a student's learning path.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="availableConcepts">The available concepts to choose from.</param>
    /// <param name="studentMastery">The student's current mastery levels.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The learning path context with selected concept and strategy.</returns>
    public async Task<AdaptiveLearningPathContext> DetermineLearningPathAsync(
        string studentId,
        IReadOnlyList<ConceptId> availableConcepts,
        IReadOnlyDictionary<ConceptId, ConceptMastery> studentMastery,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studentId);
        ArgumentNullException.ThrowIfNull(availableConcepts);
        ArgumentNullException.ThrowIfNull(studentMastery);

        var context = new AdaptiveLearningPathContext
        {
            StudentId = studentId,
            AvailableConcepts = availableConcepts,
            StudentMastery = studentMastery
        };

        var result = await _executor.ExecuteAsync(_tree, context, cancellationToken).ConfigureAwait(false);

        if (result.Status == BehaviorTreeNodeStatus.Success)
        {
            _logger?.LogInformation(
                "Learning path determined for student {StudentId}: Concept {ConceptId}, Strategy {Strategy}",
                studentId,
                context.SelectedConcept?.Value ?? "none",
                context.Strategy);
        }
        else
        {
            _logger?.LogWarning(
                "Learning path determination failed for student {StudentId} - no concept selected",
                studentId);
        }

        return context;
    }

    private bool HasReviewNeededConcepts(AdaptiveLearningPathContext context)
    {
        // Check if any concepts need review based on spaced repetition
        // This would typically check last review date and spaced repetition schedule
        // For now, simplified check: concepts with mastery but not recently reviewed
        foreach (var concept in context.AvailableConcepts)
        {
            if (context.StudentMastery.TryGetValue(concept, out var mastery))
            {
                // If mastered but last updated more than 7 days ago, needs review
                var daysSinceUpdate = (DateTimeOffset.UtcNow - mastery.LastUpdated).TotalDays;
                if (mastery.Level >= MasteryLevel.Proficient && daysSinceUpdate > 7)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private BehaviorTreeNodeStatus SelectReviewConcept(AdaptiveLearningPathContext context)
    {
        // Select concept that needs review (oldest last update)
        ConceptId? selectedConcept = null;
        DateTimeOffset? oldestUpdate = null;

        foreach (var concept in context.AvailableConcepts)
        {
            if (context.StudentMastery.TryGetValue(concept, out var mastery))
            {
                var daysSinceUpdate = (DateTimeOffset.UtcNow - mastery.LastUpdated).TotalDays;
                if (mastery.Level >= MasteryLevel.Proficient && daysSinceUpdate > 7)
                {
                    if (oldestUpdate == null || mastery.LastUpdated < oldestUpdate)
                    {
                        oldestUpdate = mastery.LastUpdated;
                        selectedConcept = concept;
                    }
                }
            }
        }

        if (selectedConcept != null)
        {
            context.SelectedConcept = selectedConcept;
            context.Strategy = LearningPathStrategy.ReviewNeeded;
            context.SelectionReason = $"Concept needs review (last reviewed {oldestUpdate})";
            return BehaviorTreeNodeStatus.Success;
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private bool HasMasteryGaps(AdaptiveLearningPathContext context)
    {
        // Check for concepts with low mastery that have prerequisites met
        if (_masteryCalculator == null)
            return false;

        var masteryLevels = context.StudentMastery.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Level);

        foreach (var concept in context.AvailableConcepts)
        {
            // Skip if already mastered
            if (masteryLevels.TryGetValue(concept, out var level) && level >= MasteryLevel.Mastery)
                continue;

            // Check if prerequisites are met
            if (_masteryCalculator.MeetsPrerequisites(concept, masteryLevels))
            {
                // Check if mastery is low (below Proficient)
                if (!masteryLevels.TryGetValue(concept, out var currentLevel) || currentLevel < MasteryLevel.Proficient)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private BehaviorTreeNodeStatus SelectMasteryGapConcept(AdaptiveLearningPathContext context)
    {
        if (_masteryCalculator == null)
            return BehaviorTreeNodeStatus.Failure;

        var masteryLevels = context.StudentMastery.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Level);

        ConceptId? selectedConcept = null;
        double lowestScore = double.MaxValue;

        foreach (var concept in context.AvailableConcepts)
        {
            // Skip if already mastered
            if (masteryLevels.TryGetValue(concept, out var level) && level >= MasteryLevel.Mastery)
                continue;

            // Check if prerequisites are met
            if (_masteryCalculator.MeetsPrerequisites(concept, masteryLevels))
            {
                // Get current score (or 0 if not assessed)
                var score = context.StudentMastery.TryGetValue(concept, out var mastery) ? mastery.Score : 0.0;
                if (score < lowestScore)
                {
                    lowestScore = score;
                    selectedConcept = concept;
                }
            }
        }

        if (selectedConcept != null)
        {
            context.SelectedConcept = selectedConcept;
            context.Strategy = LearningPathStrategy.MasteryGap;
            context.PrerequisitesMet = true;
            context.SelectionReason = $"Concept has mastery gap (score: {lowestScore:F1}%)";
            return BehaviorTreeNodeStatus.Success;
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private bool HasReadyConcepts(AdaptiveLearningPathContext context)
    {
        if (_masteryCalculator == null)
            return false;

        var masteryLevels = context.StudentMastery.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Level);

        var readyConcepts = _masteryCalculator.GetReadyConcepts(
            context.AvailableConcepts.ToList(),
            masteryLevels);

        return readyConcepts.Count > 0;
    }

    private BehaviorTreeNodeStatus SelectPrerequisiteConcept(AdaptiveLearningPathContext context)
    {
        if (_masteryCalculator == null)
            return BehaviorTreeNodeStatus.Failure;

        var masteryLevels = context.StudentMastery.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Level);

        var readyConcepts = _masteryCalculator.GetReadyConcepts(
            context.AvailableConcepts.ToList(),
            masteryLevels);

        if (readyConcepts.Count > 0)
        {
            // Select first ready concept (could be enhanced with priority scoring)
            context.SelectedConcept = readyConcepts[0];
            context.Strategy = LearningPathStrategy.PrerequisiteBased;
            context.PrerequisitesMet = true;
            context.SelectionReason = "Concept has prerequisites met";
            return BehaviorTreeNodeStatus.Success;
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private bool HasAppropriateDifficulty(AdaptiveLearningPathContext context)
    {
        // Check if there are concepts at appropriate difficulty level
        // This is a simplified check - in practice, would consider grade level, complexity, etc.
        return context.AvailableConcepts.Count > 0;
    }

    private BehaviorTreeNodeStatus SelectDifficultyConcept(AdaptiveLearningPathContext context)
    {
        // Select concept based on difficulty progression
        // For now, select first unmastered concept
        foreach (var concept in context.AvailableConcepts)
        {
            if (!context.StudentMastery.TryGetValue(concept, out var mastery) || mastery.Level < MasteryLevel.Mastery)
            {
                context.SelectedConcept = concept;
                context.Strategy = LearningPathStrategy.DifficultyProgression;
                context.SelectionReason = "Concept at appropriate difficulty level";
                return BehaviorTreeNodeStatus.Success;
            }
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private BehaviorTreeNodeStatus SelectFirstAvailable(AdaptiveLearningPathContext context)
    {
        // Fallback: select first available concept
        if (context.AvailableConcepts.Count > 0)
        {
            context.SelectedConcept = context.AvailableConcepts[0];
            context.Strategy = LearningPathStrategy.Unknown;
            context.SelectionReason = "Fallback: first available concept";
            return BehaviorTreeNodeStatus.Success;
        }

        return BehaviorTreeNodeStatus.Failure;
    }
}

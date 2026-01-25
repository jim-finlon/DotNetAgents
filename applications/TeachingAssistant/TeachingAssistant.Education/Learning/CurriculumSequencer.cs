using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetAgents.Agents.BehaviorTrees;
using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeachingAssistant.Data;
using TeachingAssistant.Data.Entities;

namespace DotNetAgents.Education.Learning;

/// <summary>
/// Context object for curriculum sequencing behavior tree operations.
/// </summary>
public class CurriculumSequencingContext
{
    /// <summary>
    /// Gets or sets the student identifier.
    /// </summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject to sequence content for.
    /// </summary>
    public Subject Subject { get; set; }

    /// <summary>
    /// Gets or sets the grade band for the student.
    /// </summary>
    public GradeBand GradeBand { get; set; }

    /// <summary>
    /// Gets or sets the available content units to choose from.
    /// </summary>
    public IReadOnlyList<ContentUnit> AvailableContentUnits { get; set; } = Array.Empty<ContentUnit>();

    /// <summary>
    /// Gets or sets the student's mastery levels for content units (keyed by ContentUnit.Id).
    /// </summary>
    public IReadOnlyDictionary<Guid, TeachingAssistant.Data.Entities.MasteryLevel> ContentMastery { get; set; } = new Dictionary<Guid, TeachingAssistant.Data.Entities.MasteryLevel>();

    /// <summary>
    /// Gets or sets the selected content unit for the learning path.
    /// </summary>
    public ContentUnit? SelectedContentUnit { get; set; }

    /// <summary>
    /// Gets or sets the sequencing strategy used.
    /// </summary>
    public SequencingStrategy Strategy { get; set; } = SequencingStrategy.Unknown;

    /// <summary>
    /// Gets or sets whether prerequisites are met for the selected content unit.
    /// </summary>
    public bool PrerequisitesMet { get; set; }

    /// <summary>
    /// Gets or sets the reason for selecting this content unit.
    /// </summary>
    public string? SelectionReason { get; set; }

    /// <summary>
    /// Gets or sets the generated learning path sequence.
    /// </summary>
    public List<ContentUnit> LearningPath { get; set; } = new();
}

/// <summary>
/// Represents the sequencing strategy used to select content units.
/// </summary>
public enum SequencingStrategy
{
    Unknown,
    PrerequisiteBased,
    MasteryGap,
    DifficultyProgression,
    ReviewNeeded,
    InterestBased,
    TopicContinuity
}

/// <summary>
/// Service for sequencing curriculum content units into adaptive learning paths using behavior trees.
/// </summary>
public class CurriculumSequencer
{
    private readonly BehaviorTree<CurriculumSequencingContext> _tree;
    private readonly BehaviorTreeExecutor<CurriculumSequencingContext> _executor;
    private readonly TeachingAssistantDbContext _dbContext;
    private readonly MasteryStateMemory _masteryMemory;
    private readonly ILogger<CurriculumSequencer>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurriculumSequencer"/> class.
    /// </summary>
    /// <param name="dbContext">The database context for accessing content units.</param>
    /// <param name="masteryMemory">The mastery memory for checking student mastery levels.</param>
    /// <param name="logger">Optional logger instance.</param>
    public CurriculumSequencer(
        TeachingAssistantDbContext dbContext,
        MasteryStateMemory masteryMemory,
        ILogger<CurriculumSequencer>? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _masteryMemory = masteryMemory ?? throw new ArgumentNullException(nameof(masteryMemory));
        _logger = logger;

        // Create logger for behavior tree nodes
        ILogger<BehaviorTreeNode<CurriculumSequencingContext>>? behaviorTreeLogger = null;
        ILogger<BehaviorTreeExecutor<CurriculumSequencingContext>>? executorLogger = null;
        if (logger != null)
        {
            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug));
            behaviorTreeLogger = loggerFactory.CreateLogger<BehaviorTreeNode<CurriculumSequencingContext>>();
            executorLogger = loggerFactory.CreateLogger<BehaviorTreeExecutor<CurriculumSequencingContext>>();
        }

        // Build behavior tree
        // Root: Selector (try strategies in order)
        var root = new SelectorNode<CurriculumSequencingContext>("SequencingSelector", behaviorTreeLogger)
            // Strategy 1: Review Needed (content units that need review based on spaced repetition)
            .AddChild(new SequenceNode<CurriculumSequencingContext>("ReviewNeededSequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<CurriculumSequencingContext>(
                    "HasReviewNeeded",
                    ctx => HasReviewNeededContent(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<CurriculumSequencingContext>(
                    "SelectReviewContent",
                    ctx => SelectReviewContent(ctx),
                    behaviorTreeLogger)))
            // Strategy 2: Mastery Gap (content units with low mastery that have prerequisites met)
            .AddChild(new SequenceNode<CurriculumSequencingContext>("MasteryGapSequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<CurriculumSequencingContext>(
                    "HasMasteryGaps",
                    ctx => HasMasteryGaps(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<CurriculumSequencingContext>(
                    "SelectMasteryGapContent",
                    ctx => SelectMasteryGapContent(ctx),
                    behaviorTreeLogger)))
            // Strategy 3: Prerequisite-Based (content units ready to learn based on prerequisites)
            .AddChild(new SequenceNode<CurriculumSequencingContext>("PrerequisiteSequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<CurriculumSequencingContext>(
                    "HasReadyContent",
                    ctx => HasReadyContent(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<CurriculumSequencingContext>(
                    "SelectPrerequisiteContent",
                    ctx => SelectPrerequisiteContent(ctx),
                    behaviorTreeLogger)))
            // Strategy 4: Topic Continuity (continue with related topics)
            .AddChild(new SequenceNode<CurriculumSequencingContext>("TopicContinuitySequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<CurriculumSequencingContext>(
                    "HasTopicContinuity",
                    ctx => HasTopicContinuity(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<CurriculumSequencingContext>(
                    "SelectTopicContinuityContent",
                    ctx => SelectTopicContinuityContent(ctx),
                    behaviorTreeLogger)))
            // Strategy 5: Difficulty Progression (content units at appropriate difficulty level)
            .AddChild(new SequenceNode<CurriculumSequencingContext>("DifficultySequence", behaviorTreeLogger)
                .AddChild(new ConditionNode<CurriculumSequencingContext>(
                    "HasAppropriateDifficulty",
                    ctx => HasAppropriateDifficulty(ctx),
                    behaviorTreeLogger))
                .AddChild(new ActionNode<CurriculumSequencingContext>(
                    "SelectDifficultyContent",
                    ctx => SelectDifficultyContent(ctx),
                    behaviorTreeLogger)))
            // Fallback: Select first available content unit
            .AddChild(new ActionNode<CurriculumSequencingContext>(
                "SelectFirstAvailable",
                ctx => SelectFirstAvailable(ctx),
                behaviorTreeLogger));

        _tree = new BehaviorTree<CurriculumSequencingContext>("CurriculumSequencingTree", root);
        _executor = new BehaviorTreeExecutor<CurriculumSequencingContext>(executorLogger);
    }

    /// <summary>
    /// Generates a learning path sequence for a student in a specific subject.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="subject">The subject to sequence content for.</param>
    /// <param name="gradeBand">The grade band for the student.</param>
    /// <param name="maxUnits">Maximum number of content units to include in the path.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of content units in the recommended learning order.</returns>
    public async Task<List<ContentUnit>> GenerateLearningPathAsync(
        string studentId,
        Subject subject,
        GradeBand gradeBand,
        int maxUnits = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(studentId);

        _logger?.LogInformation(
            "Generating learning path for student {StudentId}, subject {Subject}, grade band {GradeBand}",
            studentId,
            subject,
            gradeBand);

        // Load available content units for the subject and grade band
        var availableUnits = await _dbContext.ContentUnits
            .Include(cu => cu.Prerequisites)
                .ThenInclude(p => p.PrerequisiteUnit)
            .Include(cu => cu.LearningObjectives)
            .Where(cu => cu.Subject == subject
                && cu.GradeBand == gradeBand
                && cu.IsActive)
            .ToListAsync(cancellationToken);

        if (availableUnits.Count == 0)
        {
            _logger?.LogWarning(
                "No active content units found for subject {Subject}, grade band {GradeBand}",
                subject,
                gradeBand);
            return new List<ContentUnit>();
        }

        // Load student mastery levels for content units
        var studentMastery = await _masteryMemory.GetAllMasteryAsync(studentId, cancellationToken);
        var contentMastery = await LoadContentMasteryAsync(studentId, availableUnits, cancellationToken);

        var learningPath = new List<ContentUnit>();
        var remainingUnits = availableUnits.ToList();
        var selectedUnitIds = new HashSet<Guid>();

        // Generate sequence by repeatedly selecting next unit
        for (int i = 0; i < maxUnits && remainingUnits.Count > 0; i++)
        {
            var context = new CurriculumSequencingContext
            {
                StudentId = studentId,
                Subject = subject,
                GradeBand = gradeBand,
                AvailableContentUnits = remainingUnits,
                ContentMastery = contentMastery,
                LearningPath = learningPath
            };

            var result = await _executor.ExecuteAsync(_tree, context, cancellationToken).ConfigureAwait(false);

            if (result.Status == BehaviorTreeNodeStatus.Success && context.SelectedContentUnit != null)
            {
                var selectedUnit = context.SelectedContentUnit;
                learningPath.Add(selectedUnit);
                selectedUnitIds.Add(selectedUnit.Id);
                remainingUnits.Remove(selectedUnit);

                _logger?.LogInformation(
                    "Selected content unit {ContentUnitId} ({Title}) for student {StudentId} using strategy {Strategy}: {Reason}",
                    selectedUnit.Id,
                    selectedUnit.Title,
                    studentId,
                    context.Strategy,
                    context.SelectionReason ?? "No reason provided");
            }
            else
            {
                _logger?.LogWarning(
                    "Failed to select next content unit for student {StudentId} at step {Step}",
                    studentId,
                    i + 1);
                break;
            }
        }

        _logger?.LogInformation(
            "Generated learning path with {Count} content units for student {StudentId}",
            learningPath.Count,
            studentId);

        return learningPath;
    }

    /// <summary>
    /// Determines the next content unit for a student's learning path.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="subject">The subject to sequence content for.</param>
    /// <param name="gradeBand">The grade band for the student.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The selected content unit or null if none available.</returns>
    public async Task<ContentUnit?> GetNextContentUnitAsync(
        string studentId,
        Subject subject,
        GradeBand gradeBand,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(studentId);

        // Load available content units
        var availableUnits = await _dbContext.ContentUnits
            .Include(cu => cu.Prerequisites)
                .ThenInclude(p => p.PrerequisiteUnit)
            .Include(cu => cu.LearningObjectives)
            .Where(cu => cu.Subject == subject
                && cu.GradeBand == gradeBand
                && cu.IsActive)
            .ToListAsync(cancellationToken);

        if (availableUnits.Count == 0)
        {
            return null;
        }

        // Load student mastery
        var contentMastery = await LoadContentMasteryAsync(studentId, availableUnits, cancellationToken);

        var context = new CurriculumSequencingContext
        {
            StudentId = studentId,
            Subject = subject,
            GradeBand = gradeBand,
            AvailableContentUnits = availableUnits,
            ContentMastery = contentMastery
        };

        var result = await _executor.ExecuteAsync(_tree, context, cancellationToken).ConfigureAwait(false);

        if (result.Status == BehaviorTreeNodeStatus.Success && context.SelectedContentUnit != null)
        {
            _logger?.LogInformation(
                "Selected next content unit {ContentUnitId} ({Title}) for student {StudentId} using strategy {Strategy}",
                context.SelectedContentUnit.Id,
                context.SelectedContentUnit.Title,
                studentId,
                context.Strategy);
        }

        return context.SelectedContentUnit;
    }

    private async Task<Dictionary<Guid, TeachingAssistant.Data.Entities.MasteryLevel>> LoadContentMasteryAsync(
        string studentId,
        List<ContentUnit> contentUnits,
        CancellationToken cancellationToken)
    {
        var masteryDict = new Dictionary<Guid, TeachingAssistant.Data.Entities.MasteryLevel>();

        // Load mastery records from database
        var unitIds = contentUnits.Select(cu => cu.Id).ToList();
        var masteryRecords = await _dbContext.ContentMastery
            .Where(cm => cm.StudentId.ToString() == studentId && unitIds.Contains(cm.ContentUnitId))
            .ToListAsync(cancellationToken);

        foreach (var record in masteryRecords)
        {
            masteryDict[record.ContentUnitId] = record.MasteryLevel;
        }

        return masteryDict;
    }

    private bool HasReviewNeededContent(CurriculumSequencingContext context)
    {
        // Check if any content units need review based on spaced repetition
        // Use pre-loaded mastery data from context
        foreach (var unit in context.AvailableContentUnits)
        {
            if (context.ContentMastery.TryGetValue(unit.Id, out var level) && level >= MasteryLevel.Proficient)
            {
                // For review check, we'd need to load last reviewed date
                // For now, simplified: assume proficient+ units may need review
                // In production, would check NextReviewAt from ContentMastery entity
                return true;
            }
        }
        return false;
    }

    private BehaviorTreeNodeStatus SelectReviewContent(CurriculumSequencingContext context)
    {
        // Select content unit that needs review (prefer proficient+ units)
        // In production, would check NextReviewAt from ContentMastery
        foreach (var unit in context.AvailableContentUnits)
        {
            if (context.ContentMastery.TryGetValue(unit.Id, out var level) && level >= MasteryLevel.Proficient)
            {
                context.SelectedContentUnit = unit;
                context.Strategy = SequencingStrategy.ReviewNeeded;
                context.SelectionReason = "Content needs review based on spaced repetition";
                return BehaviorTreeNodeStatus.Success;
            }
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private bool HasMasteryGaps(CurriculumSequencingContext context)
    {
        // Check for content units with low mastery that have prerequisites met
        foreach (var unit in context.AvailableContentUnits)
        {
            // Skip if already mastered
            if (context.ContentMastery.TryGetValue(unit.Id, out var level) && level >= TeachingAssistant.Data.Entities.MasteryLevel.Mastery)
                continue;

            // Check if prerequisites are met
            if (MeetsPrerequisites(unit, context.ContentMastery))
            {
                // Check if mastery is low (below Proficient) or not assessed
                if (!context.ContentMastery.TryGetValue(unit.Id, out var currentLevel) || currentLevel < MasteryLevel.Proficient)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private BehaviorTreeNodeStatus SelectMasteryGapContent(CurriculumSequencingContext context)
    {
        ContentUnit? selectedUnit = null;
        TeachingAssistant.Data.Entities.MasteryLevel lowestLevel = TeachingAssistant.Data.Entities.MasteryLevel.Mastery; // Start high, find lowest

        foreach (var unit in context.AvailableContentUnits)
        {
            // Skip if already mastered
            if (context.ContentMastery.TryGetValue(unit.Id, out var level) && level >= TeachingAssistant.Data.Entities.MasteryLevel.Mastery)
                continue;

            // Check if prerequisites are met
            if (MeetsPrerequisites(unit, context.ContentMastery))
            {
                // Get current mastery level (or NotStarted if not assessed)
                var currentLevel = context.ContentMastery.TryGetValue(unit.Id, out var existingLevel) 
                    ? existingLevel 
                    : MasteryLevel.NotStarted;

                // Select unit with lowest mastery level
                if (currentLevel < lowestLevel)
                {
                    lowestLevel = currentLevel;
                    selectedUnit = unit;
                }
            }
        }

        if (selectedUnit != null)
        {
            context.SelectedContentUnit = selectedUnit;
            context.Strategy = SequencingStrategy.MasteryGap;
            context.PrerequisitesMet = true;
            context.SelectionReason = $"Content has mastery gap (level: {lowestLevel})";
            return BehaviorTreeNodeStatus.Success;
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private bool HasReadyContent(CurriculumSequencingContext context)
    {
        // Check for content units with prerequisites met
        foreach (var unit in context.AvailableContentUnits)
        {
            if (MeetsPrerequisites(unit, context.ContentMastery))
            {
                // Check if not already mastered
                if (!context.ContentMastery.TryGetValue(unit.Id, out var level) || level < TeachingAssistant.Data.Entities.MasteryLevel.Mastery)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private BehaviorTreeNodeStatus SelectPrerequisiteContent(CurriculumSequencingContext context)
    {
        // Select content unit with prerequisites met (prefer units with fewer prerequisites)
        ContentUnit? selectedUnit = null;
        int minPrerequisiteCount = int.MaxValue;

        foreach (var unit in context.AvailableContentUnits)
        {
            // Skip if already mastered
            if (context.ContentMastery.TryGetValue(unit.Id, out var level) && level >= TeachingAssistant.Data.Entities.MasteryLevel.Mastery)
                continue;

            if (MeetsPrerequisites(unit, context.ContentMastery))
            {
                var prereqCount = unit.Prerequisites.Count(p => p.Strength >= 2); // Only count strong prerequisites
                if (prereqCount < minPrerequisiteCount)
                {
                    minPrerequisiteCount = prereqCount;
                    selectedUnit = unit;
                }
            }
        }

        if (selectedUnit != null)
        {
            context.SelectedContentUnit = selectedUnit;
            context.Strategy = SequencingStrategy.PrerequisiteBased;
            context.PrerequisitesMet = true;
            context.SelectionReason = $"Content has prerequisites met ({minPrerequisiteCount} prerequisites)";
            return BehaviorTreeNodeStatus.Success;
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private bool HasTopicContinuity(CurriculumSequencingContext context)
    {
        // Check if there's a recently completed content unit to continue from
        if (context.LearningPath.Count == 0)
            return false;

        var lastUnit = context.LearningPath.Last();
        // Check if there are content units in the same topic path
        return context.AvailableContentUnits.Any(cu =>
            cu.TopicPath.Count > 0
            && lastUnit.TopicPath.Count > 0
            && cu.TopicPath[0] == lastUnit.TopicPath[0] // Same top-level topic
            && cu.Id != lastUnit.Id);
    }

    private BehaviorTreeNodeStatus SelectTopicContinuityContent(CurriculumSequencingContext context)
    {
        if (context.LearningPath.Count == 0)
            return BehaviorTreeNodeStatus.Failure;

        var lastUnit = context.LearningPath.Last();
        var relatedUnits = context.AvailableContentUnits
            .Where(cu =>
                cu.TopicPath.Count > 0
                && lastUnit.TopicPath.Count > 0
                && cu.TopicPath[0] == lastUnit.TopicPath[0]
                && cu.Id != lastUnit.Id
                && MeetsPrerequisites(cu, context.ContentMastery))
            .OrderBy(cu => cu.TopicPath.Count) // Prefer units closer in topic hierarchy
            .ToList();

        if (relatedUnits.Count > 0)
        {
            context.SelectedContentUnit = relatedUnits[0];
            context.Strategy = SequencingStrategy.TopicContinuity;
            context.SelectionReason = $"Continuing topic: {relatedUnits[0].TopicPath[0]}";
            return BehaviorTreeNodeStatus.Success;
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private bool HasAppropriateDifficulty(CurriculumSequencingContext context)
    {
        // Check if there are content units at appropriate difficulty level
        // This is a simplified check - in practice, would consider grade level, complexity, etc.
        return context.AvailableContentUnits.Count > 0;
    }

    private BehaviorTreeNodeStatus SelectDifficultyContent(CurriculumSequencingContext context)
    {
        // Select content unit based on difficulty progression
        // For now, select first unmastered unit with prerequisites met
        foreach (var unit in context.AvailableContentUnits)
        {
            if (!context.ContentMastery.TryGetValue(unit.Id, out var level) || level < MasteryLevel.Mastery)
            {
                if (MeetsPrerequisites(unit, context.ContentMastery))
                {
                    context.SelectedContentUnit = unit;
                    context.Strategy = SequencingStrategy.DifficultyProgression;
                    context.SelectionReason = "Content at appropriate difficulty level";
                    return BehaviorTreeNodeStatus.Success;
                }
            }
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private BehaviorTreeNodeStatus SelectFirstAvailable(CurriculumSequencingContext context)
    {
        // Fallback: select first available content unit
        if (context.AvailableContentUnits.Count > 0)
        {
            context.SelectedContentUnit = context.AvailableContentUnits[0];
            context.Strategy = SequencingStrategy.Unknown;
            context.SelectionReason = "Fallback: first available content unit";
            return BehaviorTreeNodeStatus.Success;
        }

        return BehaviorTreeNodeStatus.Failure;
    }

    private bool MeetsPrerequisites(
        ContentUnit contentUnit,
        IReadOnlyDictionary<Guid, TeachingAssistant.Data.Entities.MasteryLevel> contentMastery)
    {
        if (!contentUnit.Prerequisites.Any())
        {
            return true; // No prerequisites
        }

        // Check all strong prerequisites (strength >= 2)
        foreach (var prereq in contentUnit.Prerequisites.Where(p => p.Strength >= 2))
        {
            if (!contentMastery.TryGetValue(prereq.PrerequisiteUnitId, out var prereqLevel)
                || prereqLevel < TeachingAssistant.Data.Entities.MasteryLevel.Proficient)
            {
                return false; // Prerequisite not met
            }
        }

        return true;
    }
}

using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Pedagogy;

/// <summary>
/// Implementation of mastery calculation with weighted scoring and prerequisite checking.
/// </summary>
public class MasteryCalculator : IMasteryCalculator
{
    private readonly ILogger<MasteryCalculator>? _logger;
    private readonly Dictionary<ConceptId, IReadOnlyList<ConceptId>> _prerequisiteGraph;

    /// <summary>
    /// Initializes a new instance of the <see cref="MasteryCalculator"/> class.
    /// </summary>
    /// <param name="prerequisiteGraph">Optional prerequisite graph mapping concepts to their prerequisites.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public MasteryCalculator(
        Dictionary<ConceptId, IReadOnlyList<ConceptId>>? prerequisiteGraph = null,
        ILogger<MasteryCalculator>? logger = null)
    {
        _prerequisiteGraph = prerequisiteGraph ?? new Dictionary<ConceptId, IReadOnlyList<ConceptId>>();
        _logger = logger;
    }

    /// <inheritdoc/>
    public MasteryLevel CalculateMastery(ConceptId concept, IReadOnlyList<AssessmentResult> history)
    {
        if (concept == null)
            throw new ArgumentNullException(nameof(concept));
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        if (history.Count == 0)
        {
            _logger?.LogDebug("No assessment history for concept {ConceptId}, returning Novice", concept.Value);
            return MasteryLevel.Novice;
        }

        _logger?.LogDebug(
            "Calculating mastery for concept {ConceptId} with {Count} assessments",
            concept.Value,
            history.Count);

        // Calculate weighted average with recency bias
        var weights = CalculateWeights(history.Count);
        var weightedScore = 0.0;
        var totalWeight = 0.0;

        for (int i = 0; i < history.Count; i++)
        {
            var weight = weights[i];
            var score = history[i].Score;
            weightedScore += score * weight;
            totalWeight += weight;
        }

        var averageScore = totalWeight > 0 ? weightedScore / totalWeight : 0.0;

        // Apply mastery decay for older assessments
        var decayFactor = CalculateDecayFactor(history);
        var finalScore = averageScore * decayFactor;

        var masteryLevel = MasteryLevelExtensions.FromScore(finalScore);

        _logger?.LogInformation(
            "Calculated mastery for concept {ConceptId}: Score {Score}%, Level {Level}",
            concept.Value,
            finalScore,
            masteryLevel);

        return masteryLevel;
    }

    /// <inheritdoc/>
    public bool MeetsPrerequisites(
        ConceptId targetConcept,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery)
    {
        if (targetConcept == null)
            throw new ArgumentNullException(nameof(targetConcept));
        if (studentMastery == null)
            throw new ArgumentNullException(nameof(studentMastery));

        if (!_prerequisiteGraph.TryGetValue(targetConcept, out var prerequisites))
        {
            // No prerequisites defined, assume prerequisites are met
            _logger?.LogDebug(
                "No prerequisites defined for concept {ConceptId}, assuming prerequisites met",
                targetConcept.Value);
            return true;
        }

        if (prerequisites.Count == 0)
        {
            return true;
        }

        _logger?.LogDebug(
            "Checking prerequisites for concept {ConceptId}: {PrerequisiteCount} prerequisites",
            targetConcept.Value,
            prerequisites.Count);

        // Check if all prerequisites are met (at least Proficient level)
        foreach (var prereq in prerequisites)
        {
            if (!studentMastery.TryGetValue(prereq, out var mastery))
            {
                _logger?.LogDebug(
                    "Prerequisite {PrereqId} not found in student mastery",
                    prereq.Value);
                return false;
            }

            if (mastery < MasteryLevel.Proficient)
            {
                _logger?.LogDebug(
                    "Prerequisite {PrereqId} mastery level {Level} is below Proficient",
                    prereq.Value,
                    mastery);
                return false;
            }
        }

        _logger?.LogInformation(
            "All prerequisites met for concept {ConceptId}",
            targetConcept.Value);

        return true;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ConceptId> GetReadyConcepts(
        IReadOnlyList<ConceptId> availableConcepts,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery)
    {
        if (availableConcepts == null)
            throw new ArgumentNullException(nameof(availableConcepts));
        if (studentMastery == null)
            throw new ArgumentNullException(nameof(studentMastery));

        var readyConcepts = new List<ConceptId>();

        foreach (var concept in availableConcepts)
        {
            // Skip if already mastered
            if (studentMastery.TryGetValue(concept, out var mastery) && mastery >= MasteryLevel.Mastery)
            {
                continue;
            }

            // Check if prerequisites are met
            if (MeetsPrerequisites(concept, studentMastery))
            {
                readyConcepts.Add(concept);
            }
        }

        _logger?.LogDebug(
            "Found {ReadyCount} ready concepts out of {AvailableCount} available",
            readyConcepts.Count,
            availableConcepts.Count);

        return readyConcepts;
    }

    /// <summary>
    /// Calculates weights for assessments with recency bias (more recent = higher weight).
    /// </summary>
    private double[] CalculateWeights(int count)
    {
        var weights = new double[count];
        var baseWeight = 1.0;
        const double decayRate = 0.9; // 10% decay per older assessment

        // Start from most recent (last index) and work backwards
        for (int i = count - 1; i >= 0; i--)
        {
            weights[i] = baseWeight;
            baseWeight *= decayRate;
        }

        return weights;
    }

    /// <summary>
    /// Calculates a decay factor based on how old the assessments are.
    /// </summary>
    private double CalculateDecayFactor(IReadOnlyList<AssessmentResult> history)
    {
        if (history.Count == 0)
            return 1.0;

        var now = DateTimeOffset.UtcNow;
        var oldestAssessment = history.Min(h => h.Timestamp);
        var daysSinceOldest = (now - oldestAssessment).TotalDays;

        // Decay factor: 1.0 for recent assessments, decreasing over time
        // After 90 days, apply 10% decay
        const double halfLifeDays = 90.0;
        var decayFactor = Math.Exp(-daysSinceOldest / halfLifeDays * Math.Log(2));

        // Minimum decay factor of 0.5 (don't reduce mastery by more than 50%)
        return Math.Max(0.5, decayFactor);
    }
}

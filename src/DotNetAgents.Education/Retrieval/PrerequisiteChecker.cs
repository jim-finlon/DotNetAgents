using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Retrieval;

/// <summary>
/// Implementation of prerequisite checking with dependency validation.
/// </summary>
public class PrerequisiteChecker : IPrerequisiteChecker
{
    private readonly Dictionary<ConceptId, IReadOnlyList<ConceptId>> _prerequisiteGraph;
    private readonly IMasteryCalculator _masteryCalculator;
    private readonly ILogger<PrerequisiteChecker> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrerequisiteChecker"/> class.
    /// </summary>
    /// <param name="prerequisiteGraph">The prerequisite graph mapping concepts to their prerequisites.</param>
    /// <param name="masteryCalculator">The mastery calculator for checking mastery levels.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public PrerequisiteChecker(
        Dictionary<ConceptId, IReadOnlyList<ConceptId>>? prerequisiteGraph = null,
        IMasteryCalculator? masteryCalculator = null,
        ILogger<PrerequisiteChecker>? logger = null)
    {
        _prerequisiteGraph = prerequisiteGraph ?? new Dictionary<ConceptId, IReadOnlyList<ConceptId>>();
        _masteryCalculator = masteryCalculator ?? new MasteryCalculator(_prerequisiteGraph);
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PrerequisiteChecker>.Instance;
    }

    /// <inheritdoc/>
    public Task<bool> CheckPrerequisitesAsync(
        ConceptId conceptId,
        IReadOnlyDictionary<ConceptId, ConceptMastery> studentMastery,
        CancellationToken cancellationToken = default)
    {
        if (conceptId == null)
            throw new ArgumentNullException(nameof(conceptId));
        if (studentMastery == null)
            throw new ArgumentNullException(nameof(studentMastery));

        // Convert ConceptMastery to MasteryLevel dictionary for IMasteryCalculator
        var masteryLevels = studentMastery.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Level);

        var meetsPrerequisites = _masteryCalculator.MeetsPrerequisites(conceptId, masteryLevels);

        _logger.LogDebug(
            "Prerequisite check for concept {ConceptId}: {Result}",
            conceptId.Value,
            meetsPrerequisites ? "MET" : "NOT MET");

        return Task.FromResult(meetsPrerequisites);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ConceptId>> GetMissingPrerequisitesAsync(
        ConceptId conceptId,
        IReadOnlyDictionary<ConceptId, ConceptMastery> studentMastery,
        CancellationToken cancellationToken = default)
    {
        if (conceptId == null)
            throw new ArgumentNullException(nameof(conceptId));
        if (studentMastery == null)
            throw new ArgumentNullException(nameof(studentMastery));

        if (!_prerequisiteGraph.TryGetValue(conceptId, out var prerequisites))
        {
            // No prerequisites defined
            return Task.FromResult<IReadOnlyList<ConceptId>>(Array.Empty<ConceptId>());
        }

        var missing = new List<ConceptId>();

        // Convert ConceptMastery to MasteryLevel dictionary
        var masteryLevels = studentMastery.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Level);

        foreach (var prereq in prerequisites)
        {
            // Check if prerequisite is met (at least Proficient level)
            if (!masteryLevels.TryGetValue(prereq, out var mastery) ||
                mastery < MasteryLevel.Proficient)
            {
                missing.Add(prereq);
            }
        }

        _logger.LogDebug(
            "Missing prerequisites for concept {ConceptId}: {Count}",
            conceptId.Value,
            missing.Count);

        return Task.FromResult<IReadOnlyList<ConceptId>>(missing);
    }
}

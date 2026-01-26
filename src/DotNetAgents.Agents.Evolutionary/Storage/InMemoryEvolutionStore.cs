using DotNetAgents.Agents.Evolutionary.Evolution;
using DotNetAgents.Agents.Evolutionary.Genetics;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary.Storage;

/// <summary>
/// In-memory implementation of evolution store (for testing and development).
/// </summary>
public sealed class InMemoryEvolutionStore : IEvolutionStore
{
    private readonly Dictionary<Guid, EvolutionRun> _runs = new();
    private readonly Dictionary<Guid, List<GenerationSnapshot>> _snapshots = new();
    private readonly Dictionary<(Guid RunId, int Generation), List<AgentChromosome>> _chromosomes = new();
    private readonly ILogger<InMemoryEvolutionStore>? _logger;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryEvolutionStore"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    public InMemoryEvolutionStore(ILogger<InMemoryEvolutionStore>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<EvolutionRun> SaveEvolutionRunAsync(
        EvolutionRun run,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(run);

        lock (_lock)
        {
            _runs[run.Id] = run;
        }

        _logger?.LogDebug("Saved evolution run {RunId}", run.Id);
        return Task.FromResult(run);
    }

    /// <inheritdoc/>
    public Task<EvolutionRun?> GetEvolutionRunAsync(
        Guid runId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _runs.TryGetValue(runId, out var run);
            return Task.FromResult<EvolutionRun?>(run);
        }
    }

    /// <inheritdoc/>
    public Task SaveGenerationSnapshotAsync(
        GenerationSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        lock (_lock)
        {
            if (!_snapshots.TryGetValue(snapshot.RunId, out var snapshots))
            {
                snapshots = new List<GenerationSnapshot>();
                _snapshots[snapshot.RunId] = snapshots;
            }

            snapshots.Add(snapshot);
        }

        _logger?.LogDebug(
            "Saved generation snapshot for run {RunId}, generation {Generation}",
            snapshot.RunId,
            snapshot.Generation);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<List<GenerationSnapshot>> GetGenerationSnapshotsAsync(
        Guid runId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_snapshots.TryGetValue(runId, out var snapshots))
            {
                return Task.FromResult(new List<GenerationSnapshot>(snapshots));
            }

            return Task.FromResult(new List<GenerationSnapshot>());
        }
    }

    /// <inheritdoc/>
    public Task SaveChromosomeAsync(
        AgentChromosome chromosome,
        Guid runId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chromosome);

        lock (_lock)
        {
            var key = (runId, chromosome.Generation);
            if (!_chromosomes.TryGetValue(key, out var chromosomes))
            {
                chromosomes = new List<AgentChromosome>();
                _chromosomes[key] = chromosomes;
            }

            chromosomes.Add(chromosome);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<List<AgentChromosome>> GetChromosomesAsync(
        Guid runId,
        int generation,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var key = (runId, generation);
            if (_chromosomes.TryGetValue(key, out var chromosomes))
            {
                return Task.FromResult(new List<AgentChromosome>(chromosomes));
            }

            return Task.FromResult(new List<AgentChromosome>());
        }
    }
}

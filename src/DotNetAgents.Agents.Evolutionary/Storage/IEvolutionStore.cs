using DotNetAgents.Agents.Evolutionary.Evolution;
using DotNetAgents.Agents.Evolutionary.Genetics;

namespace DotNetAgents.Agents.Evolutionary.Storage;

/// <summary>
/// Interface for storing evolution runs, chromosomes, and generation snapshots.
/// </summary>
public interface IEvolutionStore
{
    /// <summary>
    /// Saves an evolution run.
    /// </summary>
    /// <param name="run">The evolution run to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved evolution run.</returns>
    Task<EvolutionRun> SaveEvolutionRunAsync(
        EvolutionRun run,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an evolution run by ID.
    /// </summary>
    /// <param name="runId">The evolution run ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The evolution run if found; otherwise, null.</returns>
    Task<EvolutionRun?> GetEvolutionRunAsync(
        Guid runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a generation snapshot.
    /// </summary>
    /// <param name="snapshot">The generation snapshot.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveGenerationSnapshotAsync(
        GenerationSnapshot snapshot,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets generation snapshots for an evolution run.
    /// </summary>
    /// <param name="runId">The evolution run ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of generation snapshots.</returns>
    Task<List<GenerationSnapshot>> GetGenerationSnapshotsAsync(
        Guid runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a chromosome.
    /// </summary>
    /// <param name="chromosome">The chromosome to save.</param>
    /// <param name="runId">The evolution run ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveChromosomeAsync(
        AgentChromosome chromosome,
        Guid runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets chromosomes for a generation.
    /// </summary>
    /// <param name="runId">The evolution run ID.</param>
    /// <param name="generation">The generation number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of chromosomes.</returns>
    Task<List<AgentChromosome>> GetChromosomesAsync(
        Guid runId,
        int generation,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an evolution run.
/// </summary>
public sealed class EvolutionRun
{
    /// <summary>
    /// Gets or sets the run ID.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the run name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration used.
    /// </summary>
    public EvolutionConfig Config { get; set; } = null!;

    /// <summary>
    /// Gets or sets when the run started.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the run completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the final result.
    /// </summary>
    public EvolutionResult? Result { get; set; }
}

/// <summary>
/// Represents a generation snapshot.
/// </summary>
public sealed class GenerationSnapshot
{
    /// <summary>
    /// Gets or sets the evolution run ID.
    /// </summary>
    public Guid RunId { get; set; }

    /// <summary>
    /// Gets or sets the generation number.
    /// </summary>
    public int Generation { get; set; }

    /// <summary>
    /// Gets or sets the generation statistics.
    /// </summary>
    public GenerationStatistics Statistics { get; set; } = null!;

    /// <summary>
    /// Gets or sets when the snapshot was taken.
    /// </summary>
    public DateTimeOffset SnapshotTime { get; set; } = DateTimeOffset.UtcNow;
}

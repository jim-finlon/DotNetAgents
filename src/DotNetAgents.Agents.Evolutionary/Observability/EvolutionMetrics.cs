using System.Diagnostics.Metrics;

namespace DotNetAgents.Agents.Evolutionary.Observability;

/// <summary>
/// Metrics for evolutionary agents.
/// </summary>
public static class EvolutionMetrics
{
    private static readonly Meter Meter = new("DotNetAgents.Evolutionary", "1.0.0");

    /// <summary>
    /// Counter for evolution runs started.
    /// </summary>
    public static readonly Counter<long> EvolutionRunsStarted = Meter.CreateCounter<long>(
        "evolution.runs.started",
        "count",
        "Number of evolution runs started");

    /// <summary>
    /// Counter for evolution runs completed.
    /// </summary>
    public static readonly Counter<long> EvolutionRunsCompleted = Meter.CreateCounter<long>(
        "evolution.runs.completed",
        "count",
        "Number of evolution runs completed");

    /// <summary>
    /// Gauge for current generation number.
    /// </summary>
    public static readonly ObservableGauge<int> CurrentGeneration = Meter.CreateObservableGauge<int>(
        "evolution.generation.current",
        () => GetCurrentGeneration(),
        "generation",
        "Current generation number");

    /// <summary>
    /// Gauge for best fitness.
    /// </summary>
    public static readonly ObservableGauge<double> BestFitness = Meter.CreateObservableGauge<double>(
        "evolution.fitness.best",
        () => GetBestFitness(),
        "fitness",
        "Best fitness achieved");

    /// <summary>
    /// Gauge for average fitness.
    /// </summary>
    public static readonly ObservableGauge<double> AverageFitness = Meter.CreateObservableGauge<double>(
        "evolution.fitness.average",
        () => GetAverageFitness(),
        "fitness",
        "Average fitness in current generation");

    /// <summary>
    /// Gauge for population diversity.
    /// </summary>
    public static readonly ObservableGauge<double> PopulationDiversity = Meter.CreateObservableGauge<double>(
        "evolution.population.diversity",
        () => GetPopulationDiversity(),
        "diversity",
        "Population diversity metric");

    /// <summary>
    /// Counter for chromosomes evaluated.
    /// </summary>
    public static readonly Counter<long> ChromosomesEvaluated = Meter.CreateCounter<long>(
        "evolution.chromosomes.evaluated",
        "count",
        "Number of chromosomes evaluated");

    /// <summary>
    /// Histogram for evaluation duration.
    /// </summary>
    public static readonly Histogram<double> EvaluationDuration = Meter.CreateHistogram<double>(
        "evolution.evaluation.duration",
        "seconds",
        "Duration of fitness evaluation");

    private static int _currentGeneration;
    private static double _bestFitness;
    private static double _averageFitness;
    private static double _populationDiversity;

    /// <summary>
    /// Updates the current generation.
    /// </summary>
    public static void UpdateGeneration(int generation)
    {
        _currentGeneration = generation;
    }

    /// <summary>
    /// Updates fitness metrics.
    /// </summary>
    public static void UpdateFitness(double best, double average)
    {
        _bestFitness = best;
        _averageFitness = average;
    }

    /// <summary>
    /// Updates population diversity.
    /// </summary>
    public static void UpdateDiversity(double diversity)
    {
        _populationDiversity = diversity;
    }

    private static Measurement<int> GetCurrentGeneration()
    {
        return new Measurement<int>(_currentGeneration);
    }

    private static Measurement<double> GetBestFitness()
    {
        return new Measurement<double>(_bestFitness);
    }

    private static Measurement<double> GetAverageFitness()
    {
        return new Measurement<double>(_averageFitness);
    }

    private static Measurement<double> GetPopulationDiversity()
    {
        return new Measurement<double>(_populationDiversity);
    }
}

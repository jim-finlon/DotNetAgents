namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Tracks innovation numbers for genes to enable meaningful crossover between chromosomes with different structures.
/// Based on NEAT (NeuroEvolution of Augmenting Topologies) innovation tracking.
/// </summary>
public sealed class InnovationTracker
{
    private readonly Dictionary<string, int> _innovationMap = new();
    private readonly object _lock = new();
    private int _nextInnovationNumber = 1;

    /// <summary>
    /// Gets the innovation number for a gene, creating a new one if it doesn't exist.
    /// </summary>
    /// <param name="geneType">The type of gene.</param>
    /// <param name="geneKey">A unique key identifying this specific gene instance.</param>
    /// <returns>The innovation number for this gene.</returns>
    public int GetInnovationNumber(string geneType, string geneKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(geneType);
        ArgumentException.ThrowIfNullOrWhiteSpace(geneKey);

        var key = $"{geneType}:{geneKey}";

        lock (_lock)
        {
            if (!_innovationMap.TryGetValue(key, out var innovationNumber))
            {
                innovationNumber = _nextInnovationNumber++;
                _innovationMap[key] = innovationNumber;
            }

            return innovationNumber;
        }
    }

    /// <summary>
    /// Gets the innovation number for a gene by its characteristics.
    /// </summary>
    /// <param name="gene">The gene to get the innovation number for.</param>
    /// <returns>The innovation number.</returns>
    public int GetInnovationNumber(IGene gene)
    {
        ArgumentNullException.ThrowIfNull(gene);

        var geneKey = gene switch
        {
            NumericGene ng => $"{ng.Name}:{ng.Min}:{ng.Max}",
            PromptGene pg => $"Prompt:{pg.Content.GetHashCode(StringComparison.Ordinal)}",
            ToolConfigGene tg => $"Tools:{string.Join(",", tg.Tools.Keys.OrderBy(k => k))}",
            StrategyGene sg => $"Strategy:{sg.RetryBackoffMultiplier}:{sg.ConfidenceThreshold}",
            ModelGene mg => $"Model:{mg.Provider}:{mg.ModelIdentifier}",
            BehaviorTreeGene btg => $"BT:{btg.RootNodeType}:{btg.Nodes.Count}",
            StateMachineGene smg => $"SM:{smg.States.Count}:{smg.Transitions.Count}",
            _ => gene.GeneType
        };

        return GetInnovationNumber(gene.GeneType, geneKey);
    }

    /// <summary>
    /// Resets the innovation tracker (for new evolution runs).
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _innovationMap.Clear();
            _nextInnovationNumber = 1;
        }
    }

    /// <summary>
    /// Gets the current innovation number (next number to be assigned).
    /// </summary>
    public int CurrentInnovationNumber
    {
        get
        {
            lock (_lock)
            {
                return _nextInnovationNumber;
            }
        }
    }
}

using System.Text.Json;

namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Represents a behavior tree configuration as a gene.
/// This enables evolution of decision-making strategies through behavior tree structures.
/// </summary>
public sealed class BehaviorTreeGene : IGene
{
    /// <summary>
    /// Gets or sets the innovation number for this gene.
    /// </summary>
    public int InnovationNumber { get; set; }

    /// <summary>
    /// Gets the gene type identifier.
    /// </summary>
    public string GeneType => "BehaviorTree";

    /// <summary>
    /// Gets or sets the behavior tree name.
    /// </summary>
    public string Name { get; set; } = "EvolvedBehaviorTree";

    /// <summary>
    /// Gets or sets the root node type (Sequence, Selector, Parallel, etc.).
    /// </summary>
    public string RootNodeType { get; set; } = "Selector";

    /// <summary>
    /// Gets or sets the list of node configurations in the tree.
    /// </summary>
    public List<BehaviorTreeNodeConfig> Nodes { get; set; } = new();

    /// <summary>
    /// Gets or sets whether this behavior tree is enabled for the agent.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets additional behavior tree metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BehaviorTreeGene"/> class.
    /// </summary>
    public BehaviorTreeGene()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BehaviorTreeGene"/> class.
    /// </summary>
    /// <param name="name">The behavior tree name.</param>
    /// <param name="rootNodeType">The root node type.</param>
    /// <param name="nodes">List of node configurations.</param>
    /// <param name="enabled">Whether the tree is enabled.</param>
    /// <param name="innovationNumber">The innovation number.</param>
    public BehaviorTreeGene(
        string name,
        string rootNodeType,
        List<BehaviorTreeNodeConfig>? nodes = null,
        bool enabled = true,
        int innovationNumber = 0)
    {
        Name = name;
        RootNodeType = rootNodeType;
        Nodes = nodes ?? new List<BehaviorTreeNodeConfig>();
        Enabled = enabled;
        InnovationNumber = innovationNumber;
    }

    /// <inheritdoc/>
    public IGene Clone()
    {
        return new BehaviorTreeGene(
            Name,
            RootNodeType,
            Nodes.Select(n => n.Clone()).ToList(),
            Enabled,
            InnovationNumber)
        {
            Metadata = new Dictionary<string, object>(Metadata)
        };
    }

    /// <inheritdoc/>
    public void Mutate(double rate, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (random.NextDouble() < rate)
        {
            Enabled = !Enabled;
        }

        if (random.NextDouble() < rate && Nodes.Count > 0)
        {
            // Mutate a random node
            var node = Nodes[random.Next(Nodes.Count)];
            node.Mutate(rate, random);
        }

        // Structural mutation: add/remove nodes
        if (random.NextDouble() < rate * 0.3) // Lower probability for structural changes
        {
            if (random.NextDouble() < 0.5 && Nodes.Count > 1)
            {
                // Remove a node
                Nodes.RemoveAt(random.Next(Nodes.Count));
            }
            else
            {
                // Add a new node
                Nodes.Add(BehaviorTreeNodeConfig.CreateRandom(random));
            }
        }
    }

    /// <inheritdoc/>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Deserializes a BehaviorTreeGene from JSON.
    /// </summary>
    public static BehaviorTreeGene FromJson(string json)
    {
        return JsonSerializer.Deserialize<BehaviorTreeGene>(json)
            ?? throw new InvalidOperationException("Failed to deserialize BehaviorTreeGene");
    }
}

/// <summary>
/// Represents a node configuration in a behavior tree.
/// </summary>
public sealed class BehaviorTreeNodeConfig
{
    /// <summary>
    /// Gets or sets the node name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the node type (Action, Condition, Sequence, Selector, etc.).
    /// </summary>
    public string NodeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets node-specific parameters.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Gets or sets child node indices (for composite nodes).
    /// </summary>
    public List<int> Children { get; set; } = new();

    /// <summary>
    /// Creates a random node configuration for mutation.
    /// </summary>
    public static BehaviorTreeNodeConfig CreateRandom(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var nodeTypes = new[] { "Action", "Condition", "Sequence", "Selector" };
        return new BehaviorTreeNodeConfig
        {
            Name = $"Node_{Guid.NewGuid():N8}",
            NodeType = nodeTypes[random.Next(nodeTypes.Length)]
        };
    }

    /// <summary>
    /// Creates a clone of this node configuration.
    /// </summary>
    public BehaviorTreeNodeConfig Clone()
    {
        return new BehaviorTreeNodeConfig
        {
            Name = Name,
            NodeType = NodeType,
            Parameters = new Dictionary<string, object>(Parameters),
            Children = new List<int>(Children)
        };
    }

    /// <summary>
    /// Mutates this node configuration.
    /// </summary>
    public void Mutate(double rate, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (random.NextDouble() < rate && Parameters.Count > 0)
        {
            var key = Parameters.Keys.ElementAt(random.Next(Parameters.Count));
            var value = Parameters[key];

            if (value is double d)
            {
                Parameters[key] = d * (0.9 + random.NextDouble() * 0.2);
            }
        }
    }
}

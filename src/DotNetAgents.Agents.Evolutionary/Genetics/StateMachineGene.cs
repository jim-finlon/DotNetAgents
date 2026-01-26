using System.Text.Json;

namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Represents a state machine configuration as a gene.
/// This enables evolution of agent lifecycle and operational state patterns.
/// </summary>
public sealed class StateMachineGene : IGene
{
    /// <summary>
    /// Gets or sets the innovation number for this gene.
    /// </summary>
    public int InnovationNumber { get; set; }

    /// <summary>
    /// Gets the gene type identifier.
    /// </summary>
    public string GeneType => "StateMachine";

    /// <summary>
    /// Gets or sets the state machine name.
    /// </summary>
    public string Name { get; set; } = "EvolvedStateMachine";

    /// <summary>
    /// Gets or sets the list of states in the state machine.
    /// </summary>
    public List<string> States { get; set; } = new();

    /// <summary>
    /// Gets or sets the initial state.
    /// </summary>
    public string InitialState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of transitions (fromState -> toState).
    /// </summary>
    public List<StateTransitionConfig> Transitions { get; set; } = new();

    /// <summary>
    /// Gets or sets whether this state machine is enabled for the agent.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets additional state machine metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StateMachineGene"/> class.
    /// </summary>
    public StateMachineGene()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateMachineGene"/> class.
    /// </summary>
    /// <param name="name">The state machine name.</param>
    /// <param name="states">List of states.</param>
    /// <param name="initialState">The initial state.</param>
    /// <param name="transitions">List of transitions.</param>
    /// <param name="enabled">Whether the state machine is enabled.</param>
    /// <param name="innovationNumber">The innovation number.</param>
    public StateMachineGene(
        string name,
        List<string>? states = null,
        string initialState = "",
        List<StateTransitionConfig>? transitions = null,
        bool enabled = true,
        int innovationNumber = 0)
    {
        Name = name;
        States = states ?? new List<string>();
        InitialState = initialState;
        Transitions = transitions ?? new List<StateTransitionConfig>();
        Enabled = enabled;
        InnovationNumber = innovationNumber;
    }

    /// <inheritdoc/>
    public IGene Clone()
    {
        return new StateMachineGene(
            Name,
            new List<string>(States),
            InitialState,
            Transitions.Select(t => t.Clone()).ToList(),
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

        // Mutate transitions
        if (random.NextDouble() < rate && Transitions.Count > 0)
        {
            var transition = Transitions[random.Next(Transitions.Count)];
            transition.Mutate(rate, random);
        }

        // Structural mutation: add/remove states or transitions
        if (random.NextDouble() < rate * 0.3) // Lower probability
        {
            if (random.NextDouble() < 0.5 && States.Count > 1)
            {
                // Remove a state (and its transitions)
                var stateToRemove = States[random.Next(States.Count)];
                States.Remove(stateToRemove);
                Transitions.RemoveAll(t => t.FromState == stateToRemove || t.ToState == stateToRemove);
            }
            else if (States.Count > 0)
            {
                // Add a new state
                var newState = $"State_{Guid.NewGuid():N8}";
                States.Add(newState);

                // Add a transition to/from existing state
                if (States.Count > 1)
                {
                    var existingState = States[random.Next(States.Count - 1)]; // Exclude the new state
                    if (random.NextDouble() < 0.5)
                    {
                        Transitions.Add(new StateTransitionConfig(existingState, newState));
                    }
                    else
                    {
                        Transitions.Add(new StateTransitionConfig(newState, existingState));
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Deserializes a StateMachineGene from JSON.
    /// </summary>
    public static StateMachineGene FromJson(string json)
    {
        return JsonSerializer.Deserialize<StateMachineGene>(json)
            ?? throw new InvalidOperationException("Failed to deserialize StateMachineGene");
    }
}

/// <summary>
/// Represents a state transition configuration.
/// </summary>
public sealed class StateTransitionConfig
{
    /// <summary>
    /// Gets or sets the source state.
    /// </summary>
    public string FromState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target state.
    /// </summary>
    public string ToState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional guard condition (as expression string).
    /// </summary>
    public string? GuardCondition { get; set; }

    /// <summary>
    /// Gets or sets transition metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StateTransitionConfig"/> class.
    /// </summary>
    public StateTransitionConfig()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StateTransitionConfig"/> class.
    /// </summary>
    /// <param name="fromState">The source state.</param>
    /// <param name="toState">The target state.</param>
    /// <param name="guardCondition">Optional guard condition.</param>
    public StateTransitionConfig(string fromState, string toState, string? guardCondition = null)
    {
        FromState = fromState;
        ToState = toState;
        GuardCondition = guardCondition;
    }

    /// <summary>
    /// Creates a clone of this transition configuration.
    /// </summary>
    public StateTransitionConfig Clone()
    {
        return new StateTransitionConfig(FromState, ToState, GuardCondition)
        {
            Metadata = new Dictionary<string, object>(Metadata)
        };
    }

    /// <summary>
    /// Mutates this transition configuration.
    /// </summary>
    public void Mutate(double rate, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        // Mutation could change guard conditions or metadata
        if (random.NextDouble() < rate && !string.IsNullOrEmpty(GuardCondition))
        {
            // Simple mutation: toggle guard or modify it
            GuardCondition = random.NextDouble() < 0.5 ? null : $"Modified_{GuardCondition}";
        }
    }
}

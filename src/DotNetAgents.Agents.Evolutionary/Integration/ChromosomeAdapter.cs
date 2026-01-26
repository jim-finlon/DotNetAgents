using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Core.Agents;
using DotNetAgents.Core.Prompts;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary.Integration;

/// <summary>
/// Adapter for converting between agent chromosomes and DotNetAgents agent configurations.
/// </summary>
public sealed class ChromosomeAdapter
{
    private readonly ILogger<ChromosomeAdapter>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromosomeAdapter"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    public ChromosomeAdapter(ILogger<ChromosomeAdapter>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts an agent chromosome to an agent executor configuration.
    /// </summary>
    /// <param name="chromosome">The chromosome to convert.</param>
    /// <param name="llm">The LLM model to use.</param>
    /// <param name="toolRegistry">The tool registry.</param>
    /// <returns>An agent executor configured from the chromosome.</returns>
    public AgentExecutor ToAgentExecutor(
        AgentChromosome chromosome,
        DotNetAgents.Abstractions.Models.ILLMModel<string, string> llm,
        DotNetAgents.Abstractions.Tools.IToolRegistry toolRegistry)
    {
        ArgumentNullException.ThrowIfNull(chromosome);
        ArgumentNullException.ThrowIfNull(llm);
        ArgumentNullException.ThrowIfNull(toolRegistry);

        // Create prompt template from prompt gene
        var fullPrompt = chromosome.SystemPrompt.GetFullPrompt();
        var promptTemplate = new PromptTemplate(fullPrompt);

        // Create agent executor with chromosome parameters
        var maxIterations = (int)chromosome.MaxRetries.Value;
        var executor = new AgentExecutor(
            llm,
            toolRegistry,
            promptTemplate,
            memory: null, // Can be enhanced to support memory genes
            maxIterations: maxIterations,
            stopSequence: "Final Answer:",
            stateMachine: null, // State machine integration handled separately
            logger: null); // Logger type mismatch - will be handled separately if needed

        _logger?.LogDebug(
            "Created agent executor from chromosome {ChromosomeId}, Generation {Generation}",
            chromosome.Id,
            chromosome.Generation);

        return executor;
    }

    /// <summary>
    /// Creates an agent chromosome from an existing agent executor configuration.
    /// Useful for seeding the population with known-good agents.
    /// </summary>
    /// <param name="systemPrompt">The system prompt.</param>
    /// <param name="temperature">The temperature setting.</param>
    /// <param name="maxTokens">The max tokens setting.</param>
    /// <param name="maxRetries">The max retries setting.</param>
    /// <param name="modelIdentifier">The model identifier.</param>
    /// <param name="provider">The provider name.</param>
    /// <param name="innovationTracker">The innovation tracker.</param>
    /// <returns>A new chromosome representing this configuration.</returns>
    public AgentChromosome FromAgentConfiguration(
        string systemPrompt,
        double temperature = 0.7,
        int maxTokens = 4096,
        int maxRetries = 3,
        string modelIdentifier = "gpt-4",
        string provider = "OpenAI",
        InnovationTracker? innovationTracker = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);

        innovationTracker ??= new InnovationTracker();

        var chromosome = new AgentChromosome
        {
            Generation = 0,
            SystemPrompt = new PromptGene(systemPrompt)
            {
                InnovationNumber = innovationTracker.GetInnovationNumber("Prompt", $"Prompt:{systemPrompt.GetHashCode(StringComparison.Ordinal)}")
            },
            Temperature = new NumericGene { Name = "Temperature", Value = temperature, Min = 0.0, Max = 2.0, InnovationNumber = innovationTracker.GetInnovationNumber("Numeric", "Temperature:0.0:2.0") },
            MaxTokens = new NumericGene { Name = "MaxTokens", Value = maxTokens, Min = 256, Max = 32768, IsInteger = true, InnovationNumber = innovationTracker.GetInnovationNumber("Numeric", "MaxTokens:256:32768") },
            MaxRetries = new NumericGene { Name = "MaxRetries", Value = maxRetries, Min = 0, Max = 10, IsInteger = true, InnovationNumber = innovationTracker.GetInnovationNumber("Numeric", "MaxRetries:0:10") },
            Model = new ModelGene(modelIdentifier, provider)
            {
                InnovationNumber = innovationTracker.GetInnovationNumber("Model", $"{provider}:{modelIdentifier}")
            },
            Strategies = new StrategyGene()
            {
                InnovationNumber = innovationTracker.GetInnovationNumber("Strategy", "Default")
            },
            ToolConfiguration = new ToolConfigGene()
            {
                InnovationNumber = innovationTracker.GetInnovationNumber("ToolConfig", "Default")
            }
        };

        _logger?.LogDebug("Created chromosome {ChromosomeId} from agent configuration", chromosome.Id);

        return chromosome;
    }

    /// <summary>
    /// Validates that a chromosome can be converted to a valid agent configuration.
    /// </summary>
    /// <param name="chromosome">The chromosome to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool ValidateChromosome(AgentChromosome chromosome)
    {
        if (chromosome == null)
            return false;

        // Validate required genes
        if (chromosome.SystemPrompt == null || string.IsNullOrWhiteSpace(chromosome.SystemPrompt.Content))
            return false;

        if (chromosome.Model == null || string.IsNullOrWhiteSpace(chromosome.Model.ModelIdentifier))
            return false;

        // Validate numeric gene bounds
        if (chromosome.Temperature.Value < chromosome.Temperature.Min || 
            chromosome.Temperature.Value > chromosome.Temperature.Max)
            return false;

        if (chromosome.MaxTokens.Value < chromosome.MaxTokens.Min || 
            chromosome.MaxTokens.Value > chromosome.MaxTokens.Max)
            return false;

        // Validate state machine if present
        if (chromosome.StateMachine != null && chromosome.StateMachine.Enabled)
        {
            if (string.IsNullOrWhiteSpace(chromosome.StateMachine.InitialState))
                return false;

            if (!chromosome.StateMachine.States.Contains(chromosome.StateMachine.InitialState))
                return false;
        }

        return true;
    }
}

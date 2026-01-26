namespace DotNetAgents.Agents.Evolutionary.Genetics;

/// <summary>
/// Factory for creating agent chromosomes, including random initialization for initial population.
/// </summary>
public static class AgentChromosomeFactory
{
    /// <summary>
    /// Creates a random chromosome for initial population seeding.
    /// </summary>
    /// <param name="innovationTracker">The innovation tracker.</param>
    /// <param name="availableModels">List of available model identifiers.</param>
    /// <param name="availableProviders">List of available providers.</param>
    /// <param name="availableTools">List of available tool names.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>A randomly initialized chromosome.</returns>
    public static AgentChromosome CreateRandom(
        InnovationTracker innovationTracker,
        List<string>? availableModels = null,
        List<string>? availableProviders = null,
        List<string>? availableTools = null,
        Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(innovationTracker);

        random ??= Random.Shared;
        availableModels ??= new List<string> { "gpt-4", "gpt-3.5-turbo", "claude-3-opus", "claude-3-sonnet" };
        availableProviders ??= new List<string> { "OpenAI", "Anthropic", "Ollama" };
        availableTools ??= new List<string> { "Calculator", "DateTime", "WebSearch", "Wikipedia" };

        var modelIndex = random.Next(availableModels.Count);
        var providerIndex = random.Next(availableProviders.Count);

        var chromosome = new AgentChromosome
        {
            Generation = 0,
            SystemPrompt = CreateRandomPromptGene(innovationTracker, random),
            Temperature = new NumericGene { Name = "Temperature", Value = random.NextDouble() * 2.0, Min = 0.0, Max = 2.0, InnovationNumber = innovationTracker.GetInnovationNumber("Numeric", "Temperature:0.0:2.0") },
            MaxTokens = new NumericGene { Name = "MaxTokens", Value = random.Next(256, 8193), Min = 256, Max = 32768, IsInteger = true, InnovationNumber = innovationTracker.GetInnovationNumber("Numeric", "MaxTokens:256:32768") },
            MaxRetries = new NumericGene { Name = "MaxRetries", Value = random.Next(0, 6), Min = 0, Max = 10, IsInteger = true, InnovationNumber = innovationTracker.GetInnovationNumber("Numeric", "MaxRetries:0:10") },
            Model = new ModelGene(
                availableModels[modelIndex],
                availableProviders[providerIndex],
                innovationTracker.GetInnovationNumber("Model", $"{availableProviders[providerIndex]}:{availableModels[modelIndex]}")),
            Strategies = CreateRandomStrategyGene(innovationTracker, random),
            ToolConfiguration = CreateRandomToolConfigGene(innovationTracker, availableTools, random)
        };

        return chromosome;
    }

    private static PromptGene CreateRandomPromptGene(InnovationTracker tracker, Random random)
    {
        var personas = new[]
        {
            "You are a helpful AI assistant.",
            "You are an expert problem solver.",
            "You are a creative thinker.",
            "You are a technical specialist.",
            "You are a research assistant."
        };

        var instructions = new[]
        {
            "Think step by step.",
            "Verify your answers.",
            "Be concise and accurate.",
            "Provide detailed explanations.",
            "Use examples when helpful."
        };

        var selectedPersona = personas[random.Next(personas.Length)];
        var selectedInstructions = instructions
            .OrderBy(_ => random.Next())
            .Take(random.Next(2, 4))
            .ToList();

        return new PromptGene(
            selectedPersona,
            selectedPersona,
            selectedInstructions,
            new List<string>(),
            tracker.GetInnovationNumber("Prompt", $"Prompt:{selectedPersona.GetHashCode(StringComparison.Ordinal)}"));
    }

    private static StrategyGene CreateRandomStrategyGene(InnovationTracker tracker, Random random)
    {
        return new StrategyGene(
            retryBackoffMultiplier: 1.5 + random.NextDouble() * 1.0, // 1.5 to 2.5
            confidenceThreshold: 0.5 + random.NextDouble() * 0.4, // 0.5 to 0.9
            escalationPolicy: random.NextDouble() < 0.5 ? "AskHuman" : "Retry",
            maxChainDepth: random.Next(3, 8),
            useReflection: random.NextDouble() < 0.7, // 70% chance
            reflectionThreshold: 0.5 + random.NextDouble() * 0.3, // 0.5 to 0.8
            innovationNumber: tracker.GetInnovationNumber("Strategy", "Default"));
    }

    private static ToolConfigGene CreateRandomToolConfigGene(
        InnovationTracker tracker,
        List<string> availableTools,
        Random random)
    {
        var tools = new Dictionary<string, ToolSettings>();

        // Randomly enable some tools
        var enabledTools = availableTools
            .OrderBy(_ => random.Next())
            .Take(random.Next(2, Math.Min(availableTools.Count, 5)))
            .ToList();

        foreach (var toolName in enabledTools)
        {
            tools[toolName] = new ToolSettings
            {
                Enabled = true,
                Parameters = new Dictionary<string, object>()
            };
        }

        return new ToolConfigGene(
            tools,
            tracker.GetInnovationNumber("ToolConfig", $"Tools:{string.Join(",", enabledTools.OrderBy(t => t))}"));
    }
}

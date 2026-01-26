using DotNetAgents.Agents.Evolutionary.Evolution;
using DotNetAgents.Agents.Evolutionary.Fitness;
using DotNetAgents.Agents.Evolutionary.Genetics;
using DotNetAgents.Agents.Evolutionary.Genetics.Operators;
using DotNetAgents.Agents.Evolutionary.Integration;
using DotNetAgents.Agents.Evolutionary.Population;
using DotNetAgents.Agents.Evolutionary.HiveMind;
using DotNetAgents.Agents.Evolutionary.Distribution;
using DotNetAgents.Agents.Evolutionary.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Evolutionary;

/// <summary>
/// Extension methods for registering Evolutionary Agents services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Evolutionary Agents services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEvolutionaryAgents(
        this IServiceCollection services,
        Action<EvolutionaryAgentsOptions>? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new EvolutionaryAgentsOptions();
        configuration?.Invoke(options);

        // Register core services
        services.AddSingleton<InnovationTracker>();
        services.AddScoped<ChromosomeAdapter>();

        // Register genetic operators
        services.AddScoped<ISelectionOperator>(sp =>
        {
            var operatorType = options.SelectionOperatorType ?? "Tournament";
            return operatorType switch
            {
                "Tournament" => new TournamentSelection(options.TournamentSize),
                "Roulette" => new RouletteWheelSelection(),
                "Rank" => new RankBasedSelection(options.SelectionPressure),
                "NSGA2" => new NSGA2Selection(),
                _ => new TournamentSelection()
            };
        });

        services.AddScoped<ICrossoverOperator>(sp =>
        {
            var operatorType = options.CrossoverOperatorType ?? "Uniform";
            return operatorType switch
            {
                "SinglePoint" => new SinglePointCrossover(),
                "Uniform" => new UniformCrossover(options.CrossoverRate),
                "NEAT" => new NEATCrossover(),
                "Semantic" => new SemanticCrossover(),
                _ => new UniformCrossover()
            };
        });

        services.AddScoped<IMutationOperator>(sp =>
        {
            var operatorType = options.MutationOperatorType ?? "Standard";
            return operatorType switch
            {
                "Standard" => new StandardMutation(),
                "Adaptive" => new AdaptiveMutation(
                    options.BaseMutationRate,
                    options.MinMutationRate,
                    options.MaxMutationRate,
                    options.DiversityThreshold),
                "Semantic" => new SemanticPromptMutation(),
                _ => new StandardMutation()
            };
        });

        // Register fitness evaluator
        services.AddScoped<IFitnessEvaluator>(sp =>
        {
            var config = new FitnessConfig
            {
                CompletionWeight = options.CompletionWeight,
                QualityWeight = options.QualityWeight,
                EfficiencyWeight = options.EfficiencyWeight,
                NoveltyWeight = options.NoveltyWeight,
                ContributionWeight = options.ContributionWeight,
                ConsistencyWeight = options.ConsistencyWeight,
                TasksPerAgent = options.TasksPerAgent,
                EvaluationTimeout = options.EvaluationTimeout
            };

            var adapter = sp.GetRequiredService<ChromosomeAdapter>();
            var llm = sp.GetRequiredService<DotNetAgents.Abstractions.Models.ILLMModel<string, string>>();
            var toolRegistry = sp.GetRequiredService<DotNetAgents.Abstractions.Tools.IToolRegistry>();
            var logger = sp.GetService<ILogger<FitnessEvaluator>>();
            var evaluationTasks = options.EvaluationTasks ?? new List<EvaluationTask>();

            return new FitnessEvaluator(config, adapter, llm, toolRegistry, evaluationTasks, logger);
        });

        // Register population manager
        services.AddScoped<IPopulationManager>(sp =>
        {
            var innovationTracker = sp.GetRequiredService<InnovationTracker>();
            var selectionOperator = sp.GetRequiredService<ISelectionOperator>();
            var crossoverOperator = sp.GetRequiredService<ICrossoverOperator>();
            var mutationOperator = sp.GetRequiredService<IMutationOperator>();
            var logger = sp.GetService<ILogger<PopulationManager>>();

            return new PopulationManager(
                innovationTracker,
                selectionOperator,
                crossoverOperator,
                mutationOperator,
                options.CrossoverRate,
                options.MutationRate,
                logger);
        });

        // Register evolution engine
        services.AddScoped<IEvolutionEngine>(sp =>
        {
            var populationManager = sp.GetRequiredService<IPopulationManager>();
            var fitnessEvaluator = sp.GetRequiredService<IFitnessEvaluator>();
            var logger = sp.GetService<ILogger<EvolutionEngine>>();

            return new EvolutionEngine(populationManager, fitnessEvaluator, logger);
        });

        // Register hive mind services
        services.AddScoped<INoveltyEvaluator>(sp =>
        {
            var knowledgeRepository = sp.GetRequiredService<DotNetAgents.Knowledge.IKnowledgeRepository>();
            var logger = sp.GetService<ILogger<NoveltyEvaluator>>();
            return new NoveltyEvaluator(knowledgeRepository, logger);
        });

        services.AddScoped<IKnowledgeExtractor>(sp =>
        {
            var logger = sp.GetService<ILogger<KnowledgeExtractor>>();
            return new KnowledgeExtractor(logger);
        });

        services.AddScoped<IHiveMind>(sp =>
        {
            var knowledgeRepository = sp.GetRequiredService<DotNetAgents.Knowledge.IKnowledgeRepository>();
            var noveltyEvaluator = sp.GetRequiredService<INoveltyEvaluator>();
            var knowledgeExtractor = sp.GetRequiredService<IKnowledgeExtractor>();
            var logger = sp.GetService<ILogger<HiveMind.HiveMind>>();
            return new HiveMind.HiveMind(knowledgeRepository, noveltyEvaluator, knowledgeExtractor, logger);
        });

        // Register distributed evaluator
        services.AddScoped<IDistributedEvaluator>(sp =>
        {
            var messageBus = sp.GetService<DotNetAgents.Agents.Messaging.IAgentMessageBus>();
            var localEvaluator = sp.GetRequiredService<IFitnessEvaluator>();
            var logger = sp.GetService<ILogger<DistributedEvaluator>>();

            if (messageBus != null)
            {
                return new DistributedEvaluator(
                    messageBus,
                    localEvaluator,
                    options.EvaluationTimeout,
                    options.MaxConcurrentEvaluations ?? 50,
                    logger);
            }

            // Fall back to local evaluator if no message bus
            var agentRegistry = sp.GetService<DotNetAgents.Agents.Registry.IAgentRegistry>();
            var fallbackLogger = sp.GetService<ILogger<DotNetAgents.Agents.Messaging.InMemoryAgentMessageBus>>();
            var fallbackMessageBus = agentRegistry != null
                ? new DotNetAgents.Agents.Messaging.InMemoryAgentMessageBus(agentRegistry, fallbackLogger)
                : new DotNetAgents.Agents.Messaging.InMemoryAgentMessageBus(
                    sp.GetRequiredService<DotNetAgents.Agents.Registry.IAgentRegistry>(),
                    fallbackLogger);
            return new DistributedEvaluator(
                fallbackMessageBus,
                localEvaluator,
                options.EvaluationTimeout,
                options.MaxConcurrentEvaluations ?? 50,
                logger);
        });

        // Register storage
        services.AddSingleton<IEvolutionStore>(sp =>
        {
            var logger = sp.GetService<ILogger<InMemoryEvolutionStore>>();
            return new InMemoryEvolutionStore(logger);
        });

        return services;
    }
}

/// <summary>
/// Configuration options for Evolutionary Agents.
/// </summary>
public sealed class EvolutionaryAgentsOptions
{
    /// <summary>
    /// Gets or sets the selection operator type.
    /// </summary>
    public string? SelectionOperatorType { get; set; }

    /// <summary>
    /// Gets or sets the tournament size for tournament selection.
    /// </summary>
    public int TournamentSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets the selection pressure for rank-based selection.
    /// </summary>
    public double SelectionPressure { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the crossover operator type.
    /// </summary>
    public string? CrossoverOperatorType { get; set; }

    /// <summary>
    /// Gets or sets the crossover rate.
    /// </summary>
    public double CrossoverRate { get; set; } = 0.8;

    /// <summary>
    /// Gets or sets the mutation operator type.
    /// </summary>
    public string? MutationOperatorType { get; set; }

    /// <summary>
    /// Gets or sets the base mutation rate.
    /// </summary>
    public double BaseMutationRate { get; set; } = 0.05;

    /// <summary>
    /// Gets or sets the minimum mutation rate.
    /// </summary>
    public double MinMutationRate { get; set; } = 0.01;

    /// <summary>
    /// Gets or sets the maximum mutation rate.
    /// </summary>
    public double MaxMutationRate { get; set; } = 0.2;

    /// <summary>
    /// Gets or sets the diversity threshold.
    /// </summary>
    public double DiversityThreshold { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the mutation rate.
    /// </summary>
    public double MutationRate { get; set; } = 0.05;

    /// <summary>
    /// Gets or sets fitness weights.
    /// </summary>
    public double CompletionWeight { get; set; } = 0.3;
    public double QualityWeight { get; set; } = 0.3;
    public double EfficiencyWeight { get; set; } = 0.2;
    public double NoveltyWeight { get; set; } = 0.1;
    public double ContributionWeight { get; set; } = 0.1;
    public double ConsistencyWeight { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the number of tasks per agent.
    /// </summary>
    public int TasksPerAgent { get; set; } = 10;

    /// <summary>
    /// Gets or sets the evaluation timeout.
    /// </summary>
    public TimeSpan EvaluationTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets the evaluation tasks.
    /// </summary>
    public List<EvaluationTask>? EvaluationTasks { get; set; }

    /// <summary>
    /// Gets or sets the maximum concurrent evaluations for distributed execution.
    /// </summary>
    public int? MaxConcurrentEvaluations { get; set; } = 50;
}

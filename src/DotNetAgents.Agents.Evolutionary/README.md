# DotNetAgents.Agents.Evolutionary

Evolutionary Agent System (EAS) plugin for DotNetAgents that enables agents to evolve through genetic algorithms, creating a self-improving ecosystem.

## Overview

This plugin implements a comprehensive evolutionary agent system that applies artificial life principles (variation, competition, evaluation, reproduction, culling) to optimize agent configurations automatically. Agents evolve over generations, improving their performance through genetic algorithms.

## Features

### Core Components

- **Agent Chromosome System**: Represents agent configurations as evolvable genomes with multiple gene types
- **Genetic Operators**: Selection, crossover, and mutation operators for evolution
- **Fitness Evaluation**: Hybrid evaluation system with automated and human-in-the-loop support
- **Evolution Engine**: Orchestrates evolutionary cycles with population management
- **Enhanced Hive Mind**: Knowledge repository with novelty detection and provenance tracking
- **Distributed Execution**: Parallel fitness evaluation using message buses
- **Storage Layer**: Persistence for evolution runs, chromosomes, and generation snapshots
- **Observability**: Prometheus metrics and distributed tracing support
- **Advanced Features**: Island model evolution, adaptive mutation, semantic operations

### Gene Types

- **PromptGene**: System prompt, persona, instructions, and examples
- **ToolConfigGene**: Enabled tools and their parameters
- **StrategyGene**: Retry policies, confidence thresholds, reflection settings
- **ModelGene**: LLM model selection
- **BehaviorTreeGene**: Behavior tree configuration (evolvable decision-making)
- **StateMachineGene**: State machine patterns (evolvable lifecycle management)
- **NumericGene**: Temperature, max tokens, retry counts, etc.

### Genetic Operators

**Selection Operators:**
- Tournament Selection
- Roulette Wheel (Fitness-Proportionate) Selection
- Rank-Based Selection
- NSGA-II (Multi-Objective) Selection

**Crossover Operators:**
- Single-Point Crossover
- Uniform Crossover
- NEAT-Style Crossover (with innovation matching)
- Semantic Crossover (LLM-based prompt merging)

**Mutation Operators:**
- Standard Mutation
- Adaptive Mutation (rate adjusts based on diversity)
- Semantic Prompt Mutation (LLM-based prompt rewriting)

### Fitness Metrics

- **Task Completion Rate**: Percentage of tasks completed successfully
- **Solution Quality**: Average quality score from task evaluations
- **Efficiency Score**: Token usage, time, resource efficiency
- **Novelty Score**: Behavioral diversity from archive
- **Hive Mind Contributions**: Unique knowledge added
- **Consistency Score**: Performance across similar tasks

## Quick Start

### Installation

```csharp
services.AddDotNetAgentsEcosystem();
services.AddEvolutionaryAgents(config =>
{
    config.PopulationSize = 100;
    config.UseSpeciation = true;
    config.FitnessConfig.CompletionWeight = 0.3;
    config.SelectionOperatorType = "Tournament";
    config.CrossoverOperatorType = "NEAT";
    config.MutationOperatorType = "Adaptive";
});
```

### Basic Usage

```csharp
var engine = serviceProvider.GetRequiredService<IEvolutionEngine>();

var result = await engine.EvolveAsync(new EvolutionConfig
{
    PopulationSize = 100,
    EliteCount = 5,
    MutationRate = 0.05,
    CrossoverRate = 0.8,
    UseSpeciation = true,
    TerminationCondition = new TerminationCondition
    {
        MaxGenerations = 50,
        TargetFitness = 0.95,
        StagnationGenerations = 10
    }
}, cancellationToken);

// Get best agent
var bestAgent = result.BestAgent;
var adapter = serviceProvider.GetRequiredService<ChromosomeAdapter>();
var agentExecutor = adapter.ToAgentExecutor(
    bestAgent,
    llm,
    toolRegistry);
```

## Architecture

### High-Level Design

```
┌─────────────────────────────────────────────────────────────┐
│              DotNetAgents.EvolutionaryAgents Plugin         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐ │
│  │   Evolution  │  │  Population  │  │   Fitness        │ │
│  │    Engine    │──│   Manager    │──│   Evaluator      │ │
│  └──────────────┘  └──────────────┘  └──────────────────┘ │
│         │                 │                    │            │
│  ┌──────┴─────────────────┴────────────────────┴──────────┐ │
│  │              Genetic Operators                         │ │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐             │ │
│  │  │Selection│  │ Crossover │  │ Mutation │             │ │
│  │  └──────────┘  └──────────┘  └──────────┘             │ │
│  └─────────────────────────────────────────────────────────┘ │
│         │                 │                    │            │
│  ┌──────┴─────────────────┴────────────────────┴──────────┐ │
│  │              Enhanced Hive Mind                         │ │
│  │  (Built on DotNetAgents.Knowledge + Vector Store)      │ │
│  └─────────────────────────────────────────────────────────┘ │
└──────────────────────────┬──────────────────────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
        ▼                  ▼                  ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Agent        │  │ Message      │  │ Observability│
│ Registry     │  │ Bus          │  │ (Metrics,    │
│              │  │              │  │  Tracing)    │
└──────────────┘  └──────────────┘  └──────────────┘
```

## Key Innovations

### 1. Behavior Tree and State Machine Evolution
Evolve not just agent configurations, but also behavior tree structures and state machine patterns.

### 2. NEAT-Style Innovation Tracking
Track gene innovation numbers to enable meaningful crossover even when chromosome structures differ.

### 3. Semantic Genetic Operations
Use LLMs for semantic-aware mutation and crossover (maintains semantic coherence while introducing variation).

### 4. Multi-Objective Optimization
Support Pareto-optimal solutions for multiple competing objectives using NSGA-II.

### 5. Adaptive Mutation Rates
Mutation rates adjust based on population diversity and convergence.

### 6. Island Model Evolution
Parallel evolution across multiple "islands" with periodic migration.

### 7. Enhanced Hive Mind
- Novelty detection using vector similarity
- Automatic knowledge extraction from agent executions
- Provenance tracking (which agent/generation contributed knowledge)
- Usage analytics

## Configuration

See `appsettings.evolutionary.json` example in the plan document for full configuration options.

## Integration Points

- **Agent Registry**: Use for population management
- **Knowledge Repository**: Extend for hive mind
- **Message Buses**: Use for distributed evaluation
- **Workflow System**: Use for complex task evaluation
- **Observability**: Use for fitness metrics and tracing
- **State Machines**: Evolve state machine patterns
- **Behavior Trees**: Evolve behavior tree structures

## Testing

The plugin includes comprehensive unit tests for:
- Chromosome operations
- Genetic operators
- Fitness calculations
- Speciation logic
- Evolution engine end-to-end

## Documentation

- See the implementation plan for detailed architecture and design decisions
- API documentation is available via XML comments
- Examples are provided in the samples directory

## License

MIT

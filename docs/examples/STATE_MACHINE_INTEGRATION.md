# State Machine Integration Examples

This guide provides practical examples for integrating state machines into various DotNetAgents components.

## Table of Contents

1. [Supervisor Agent Integration](#supervisor-agent-integration)
2. [Worker Pool Integration](#worker-pool-integration)
3. [Voice Command Processing](#voice-command-processing)
4. [Education Components](#education-components)
5. [Agent Executor Integration](#agent-executor-integration)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

## Supervisor Agent Integration

### Basic Setup

```csharp
using DotNetAgents.Agents.StateMachines;
using DotNetAgents.Agents.Supervisor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Register supervisor with state machine
services.AddSupervisorWithStateMachine(
    stateMachineFactory: sp =>
    {
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var stateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<SupervisorContext>>();
        var stateMachine = StateMachinePatterns.CreateSupervisorPattern<SupervisorContext>(
            stateMachineLogger,
            waitingTimeout: TimeSpan.FromMinutes(5));
        return new StateMachineAdapter<SupervisorContext>(stateMachine);
    });

var serviceProvider = services.BuildServiceProvider();
var supervisor = serviceProvider.GetRequiredService<ISupervisorAgent>();
```

### State Transitions

The supervisor automatically transitions through states:

```csharp
// Monitoring → Analyzing (on task received)
await supervisor.SubmitTaskAsync(task);

// Analyzing → Delegating (on task analysis complete)
// Delegating → Waiting (on task delegation complete)

// Waiting → Monitoring (on response received or timeout)
```

### Accessing Current State

```csharp
var statistics = supervisor.GetStatistics();
Console.WriteLine($"Current State: {statistics.CurrentState}");
```

## Worker Pool Integration

### Setup with State Machine Support

```csharp
using DotNetAgents.Agents.StateMachines;
using DotNetAgents.Agents.WorkerPool;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Register worker pool with state machine support
services.AddWorkerPoolWithStateMachine(
    stateProviderFactory: sp =>
    {
        var agentRegistry = sp.GetRequiredService<IAgentRegistry>();
        var logger = sp.GetRequiredService<ILogger<AgentStateMachineRegistry<object>>>();
        var stateMachineRegistry = new AgentStateMachineRegistry<object>(agentRegistry, logger);
        
        // Register state machines for workers
        var stateMachine = StateMachinePatterns.CreateWorkerPoolPattern<object>(
            logger,
            cooldownDuration: TimeSpan.FromSeconds(5));
        
        // Register for each worker
        foreach (var agent in agentRegistry.GetAllAsync().Result)
        {
            stateMachineRegistry.RegisterAsync(agent.AgentId, stateMachine).Wait();
        }
        
        return new WorkerStateProviderAdapter(stateMachineRegistry);
    });

var serviceProvider = services.BuildServiceProvider();
var workerPool = serviceProvider.GetRequiredService<IWorkerPool>();
```

### Backward Compatibility

The worker pool maintains backward compatibility with `AgentStatus` enum:

```csharp
// Works with both enum and state machine
var availableWorkers = await workerPool.GetAvailableWorkersAsync();
// Uses state machine if available, falls back to enum otherwise
```

## Voice Command Processing

### Setup with Session State Machine

```csharp
using DotNetAgents.Voice;
using DotNetAgents.Voice.StateMachines;
using DotNetAgents.Agents.StateMachines;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Register command orchestration with state machine
services.AddCommandOrchestrationWithStateMachine(
    stateMachineFactory: sp =>
    {
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var stateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<VoiceSessionContext>>();
        var stateMachine = VoiceSessionStateMachinePattern.CreateVoiceSessionPattern<VoiceSessionContext>(
            stateMachineLogger,
            listeningTimeout: TimeSpan.FromSeconds(30),
            processingTimeout: TimeSpan.FromSeconds(60));
        return new VoiceSessionStateMachineAdapter<VoiceSessionContext>(stateMachine);
    });

var serviceProvider = services.BuildServiceProvider();
var orchestrator = serviceProvider.GetRequiredService<ICommandWorkflowOrchestrator>();
```

### Session Lifecycle

```csharp
// Execute command (automatically transitions states)
var commandState = new CommandState
{
    UserId = userId,
    RawText = "create a note about the meeting",
    Source = "voice"
};

var result = await orchestrator.ExecuteAsync(commandState);

// Get session state
var sessionState = orchestrator.GetSessionState(userId);
Console.WriteLine($"Session State: {sessionState?.SessionId}");
```

## Education Components

### Learning Session State Machine

```csharp
using DotNetAgents.Education.Memory;
using DotNetAgents.Education.StateMachines;
using DotNetAgents.Agents.StateMachines;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Create state machine
var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
var stateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<LearningSessionContext>>();
var stateMachine = LearningSessionStateMachinePattern.CreateLearningSessionPattern<LearningSessionContext>(
    stateMachineLogger,
    sessionTimeout: TimeSpan.FromHours(2),
    learningTimeout: TimeSpan.FromMinutes(30));
var adapter = new LearningSessionStateMachineAdapter<LearningSessionContext>(stateMachine);

// Create session memory with state machine
var sessionMemory = new LearningSessionMemory(
    loggerFactory.CreateLogger<LearningSessionMemory>(),
    adapter);

// Create session (transitions to Initialized)
var session = await sessionMemory.CreateSessionAsync(
    studentId: "student-123",
    conceptId: new ConceptId("fractions", SubjectArea.Mathematics, GradeLevel.G3_5));

// Transition through states
await sessionMemory.TransitionToStateAsync(session.SessionId, "Learning");
await sessionMemory.TransitionToStateAsync(session.SessionId, "Assessment");
await sessionMemory.TransitionToStateAsync(session.SessionId, "Review");
await sessionMemory.CompleteSessionAsync(session.SessionId);
```

### Mastery State Machine

```csharp
// Create mastery state machine
var masteryStateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<MasteryContext>>();
var masteryStateMachine = MasteryStateMachinePattern.CreateMasteryPattern<MasteryContext>(
    masteryStateMachineLogger);
var masteryAdapter = new MasteryStateMachineAdapter<MasteryContext>(masteryStateMachine);

// Create mastery memory with state machine
var masteryMemory = new MasteryStateMemory(
    loggerFactory.CreateLogger<MasteryStateMemory>(),
    masteryAdapter);

// Update mastery (automatically transitions states based on score)
var mastery = new ConceptMastery
{
    ConceptId = conceptId,
    Score = 75.0, // Automatically transitions to "Proficient" state
    Level = MasteryLevel.Proficient,
    LastUpdated = DateTimeOffset.UtcNow
};

await masteryMemory.UpdateMasteryAsync(studentId, mastery);

// Get current mastery state
var state = masteryMemory.GetMasteryState(studentId, conceptId);
Console.WriteLine($"Mastery State: {state}"); // "Proficient"
```

## Agent Executor Integration

### Setup with Execution State Machine

```csharp
using DotNetAgents.Core.Agents;
using DotNetAgents.Core.Agents.StateMachines;
using DotNetAgents.Agents.StateMachines;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Create state machine
var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
var stateMachineLogger = loggerFactory.CreateLogger<AgentStateMachine<AgentExecutionContext>>();
var stateMachine = AgentExecutionStateMachinePattern.CreateAgentExecutionPattern<AgentExecutionContext>(
    stateMachineLogger,
    maxThinkingTime: TimeSpan.FromMinutes(5),
    maxActingTime: TimeSpan.FromMinutes(2));
var adapter = new AgentExecutionStateMachineAdapter<AgentExecutionContext>(stateMachine);

// Create agent executor with state machine
var agent = new AgentExecutor(
    llm,
    toolRegistry,
    promptTemplate,
    maxIterations: 10,
    stateMachine: adapter,
    logger: loggerFactory.CreateLogger<AgentExecutor>());

// Execute (automatically tracks state transitions)
var result = await agent.InvokeAsync("What is 15 * 23 + 42?");
// States: Initialized → Thinking → Acting → Observing → Thinking → Finalizing
```

## Best Practices

### 1. Use Adapter Pattern for Circular Dependencies

When integrating state machines into components that can't directly reference `DotNetAgents.Agents.StateMachines`, use adapter interfaces:

```csharp
// Define interface in your component
public interface IMyComponentStateMachine<TState> where TState : class
{
    string? CurrentState { get; }
    Task TransitionAsync(string toState, TState context, CancellationToken cancellationToken = default);
}

// Create adapter
public class MyComponentStateMachineAdapter<TState> : IMyComponentStateMachine<TState>
{
    private readonly object _stateMachine;
    // ... implementation using reflection
}
```

### 2. Make State Machines Optional

Always make state machine integration optional to maintain backward compatibility:

```csharp
public class MyComponent
{
    private readonly IMyComponentStateMachine<MyContext>? _stateMachine;

    public MyComponent(
        // ... other dependencies
        IMyComponentStateMachine<MyContext>? stateMachine = null)
    {
        _stateMachine = stateMachine;
    }

    public async Task DoSomethingAsync()
    {
        if (_stateMachine != null)
        {
            await _stateMachine.TransitionAsync("Working", context);
        }
        // ... rest of implementation
    }
}
```

### 3. Use State Machine Patterns

Leverage pre-built patterns for common use cases:

```csharp
// Supervisor pattern
var supervisorStateMachine = StateMachinePatterns.CreateSupervisorPattern<SupervisorContext>(logger);

// Worker pool pattern
var workerStateMachine = StateMachinePatterns.CreateWorkerPoolPattern<object>(logger, cooldownDuration);

// Error recovery pattern
var errorRecoveryStateMachine = StateMachinePatterns.CreateErrorRecoveryPattern<object>(logger);
```

### 4. Handle State Transition Errors Gracefully

```csharp
try
{
    await _stateMachine.TransitionAsync("TargetState", context, cancellationToken);
}
catch (Exception ex)
{
    _logger?.LogWarning(ex, "Failed to transition to {State}", "TargetState");
    // Continue execution without state machine
}
```

### 5. Track State Context

Maintain context objects to track state-related data:

```csharp
public class MyContext
{
    public string ExecutionId { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public int Iteration { get; set; }
    public int ErrorCount { get; set; }
    public string? LastErrorMessage { get; set; }
}
```

## Troubleshooting

### Issue: State machine not transitioning

**Possible Causes:**
1. Guard condition not met
2. Transition not defined
3. State machine not initialized

**Solution:**
```csharp
// Check if transition is allowed
if (stateMachine.CanTransition("CurrentState", "TargetState", context))
{
    await stateMachine.TransitionAsync("TargetState", context);
}
else
{
    _logger?.LogWarning("Transition from {From} to {To} not allowed", "CurrentState", "TargetState");
}
```

### Issue: Circular dependency errors

**Solution:** Use adapter pattern with interfaces defined in your component:

```csharp
// Define interface in your component (not in StateMachines project)
public interface IMyStateMachine<TState> where TState : class
{
    string? CurrentState { get; }
    Task TransitionAsync(string toState, TState context, CancellationToken cancellationToken = default);
}

// Create adapter in StateMachines project
public class MyStateMachineAdapter<TState> : IMyStateMachine<TState>
{
    // ... implementation
}
```

### Issue: State machine factory fails

**Solution:** Wrap factory calls in try-catch and continue without state machine:

```csharp
services.TryAddScoped<IMyComponent>(sp =>
{
    IMyStateMachine<MyContext>? stateMachine = null;
    try
    {
        stateMachine = stateMachineFactory?.Invoke(sp);
    }
    catch (Exception ex)
    {
        _logger?.LogWarning(ex, "State machine factory failed, continuing without state machine");
    }
    
    return new MyComponent(/* ... */, stateMachine);
});
```

## Additional Resources

- [State Machines README](../../src/DotNetAgents.Agents.StateMachines/README.md)
- [Migration Guide](../guides/MIGRATING_TO_STATE_MACHINES.md)
- [Behavior Tree Integration Examples](./BEHAVIOR_TREE_INTEGRATION.md)

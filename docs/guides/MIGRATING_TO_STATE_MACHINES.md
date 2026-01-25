# Migrating to State Machines

This guide explains how to migrate from the `AgentStatus` enum-based approach to state machine-based agent lifecycle management in DotNetAgents.

## Overview

State machines provide more powerful and flexible agent lifecycle management compared to simple enum-based status tracking. They enable:

- **Complex state transitions** with guards and conditions
- **Timed transitions** (e.g., cooldown periods)
- **Entry and exit actions** for state changes
- **Hierarchical and parallel state machines**
- **Better observability** and debugging
- **Integration with behavior trees** for autonomous decision-making

## Migration Approaches

### Approach 1: Hybrid (Recommended for Gradual Migration)

Use state machines alongside `AgentStatus` enum for backward compatibility.

```csharp
// Register state machine registry
var stateMachineRegistry = new AgentStateMachineRegistry<object>(agentRegistry, logger);

// Create worker pool with state machine support
var stateProvider = new WorkerStateProviderAdapter(stateMachineRegistry);
var workerPool = new WorkerPool(
    agentRegistry,
    stateProvider: stateProvider);

// Register state machines for agents
var stateMachine = StateMachinePatterns.CreateWorkerPoolPattern<object>(
    logger,
    cooldownDuration: TimeSpan.FromSeconds(5));

await stateMachineRegistry.RegisterAsync("worker-1", stateMachine);
```

### Approach 2: Full State Machine Migration

Replace `AgentStatus` enum usage with state machines entirely.

```csharp
// Before: Using AgentStatus enum
if (agent.Status == AgentStatus.Available)
{
    // Assign task
}

// After: Using state machine
var stateMachine = stateMachineRegistry.GetStateMachine(agent.AgentId);
if (stateMachine != null && 
    AgentStatusStateMachineAdapter.MatchesStatus(
        stateMachineRegistry.GetAgentState(agent.AgentId), 
        AgentStatus.Available))
{
    // Assign task
}
```

## Step-by-Step Migration

### Step 1: Add State Machine Support to WorkerPool

```csharp
// Before
services.AddWorkerPool();

// After
services.AddWorkerPoolWithStateMachine(stateProviderFactory: sp =>
{
    var agentRegistry = sp.GetRequiredService<IAgentRegistry>();
    var logger = sp.GetRequiredService<ILogger<AgentStateMachineRegistry<object>>>();
    var stateMachineRegistry = new AgentStateMachineRegistry<object>(agentRegistry, logger);
    return new WorkerStateProviderAdapter(stateMachineRegistry);
});
```

### Step 2: Register State Machines for Agents

```csharp
var stateMachineRegistry = serviceProvider.GetRequiredService<AgentStateMachineRegistry<object>>();

// Create state machine using pattern
var stateMachine = StateMachinePatterns.CreateWorkerPoolPattern<object>(
    logger,
    cooldownDuration: TimeSpan.FromSeconds(5));

// Register for each agent
await stateMachineRegistry.RegisterAsync("worker-1", stateMachine);
await stateMachineRegistry.RegisterAsync("worker-2", stateMachine);
```

### Step 3: Update Agent Status Checks

```csharp
// Before: Direct enum check
if (agent.Status == AgentStatus.Available)
{
    // Use agent
}

// After: Using adapter
var currentState = stateProvider.GetAgentState(agent.AgentId);
if (AgentStatusStateMachineAdapter.IsAvailable(
    currentState,
    agent.CurrentTaskCount,
    agent.Capabilities.MaxConcurrentTasks))
{
    // Use agent
}
```

### Step 4: Update State Transitions

```csharp
// Before: Direct status update
await agentRegistry.UpdateStatusAsync(agentId, AgentStatus.Busy);

// After: State machine transition
var stateMachine = stateMachineRegistry.GetStateMachine(agentId);
if (stateMachine != null)
{
    await stateMachine.TransitionAsync("Busy", context);
}
```

## Benefits of State Machines

### 1. Timed Transitions

State machines support automatic transitions after a timeout:

```csharp
var stateMachine = StateMachinePatterns.CreateWorkerPoolPattern<object>(
    logger,
    cooldownDuration: TimeSpan.FromSeconds(10));

// Agent automatically transitions from CoolingDown → Available after 10 seconds
```

### 2. Guard Conditions

Control when transitions are allowed:

```csharp
builder
    .AddTransition("Available", "Busy", 
        guard: ctx => ctx.TaskCount < ctx.MaxConcurrentTasks)
    .AddTransition("Busy", "Available",
        guard: ctx => ctx.TaskCount == 0);
```

### 3. Entry/Exit Actions

Execute code when entering or exiting states:

```csharp
builder
    .AddState("Busy",
        entryAction: ctx => { /* Log task assignment */ },
        exitAction: ctx => { /* Log task completion */ });
```

### 4. Error Recovery

Automatic error recovery patterns:

```csharp
var stateMachine = StateMachinePatterns.CreateErrorRecoveryPattern<object>(logger);
// Automatically handles: Any → Error → Recovery → Idle
```

## Migration Checklist

- [ ] Identify all `AgentStatus` enum usages
- [ ] Create state machine registry
- [ ] Register state machines for agents
- [ ] Update `WorkerPool` to use `IWorkerStateProvider`
- [ ] Replace status checks with adapter methods
- [ ] Update state transitions to use state machine API
- [ ] Test backward compatibility (enum fallback)
- [ ] Update documentation and examples
- [ ] Monitor state transitions in production

## Troubleshooting

### Issue: State machine not found

**Solution:** Ensure state machine is registered before using:

```csharp
var stateMachine = stateMachineRegistry.GetStateMachine(agentId);
if (stateMachine == null)
{
    // Fallback to enum-based check
    return agent.Status == AgentStatus.Available;
}
```

### Issue: Circular dependency errors

**Solution:** Use `IWorkerStateProvider` interface instead of direct `AgentStateMachineRegistry` reference in WorkerPool.

### Issue: State transitions not working

**Solution:** Verify state machine is properly initialized and transitions are defined:

```csharp
// Check if transition is allowed
if (stateMachine.CanTransition("Available", "Busy", context))
{
    await stateMachine.TransitionAsync("Busy", context);
}
```

## Code Examples

### Before: Enum-Based

```csharp
public class WorkerPool
{
    public async Task<AgentInfo?> GetAvailableWorkerAsync()
    {
        var agents = await _registry.GetAllAsync();
        return agents.FirstOrDefault(a => 
            a.Status == AgentStatus.Available &&
            a.CurrentTaskCount < a.Capabilities.MaxConcurrentTasks);
    }
}
```

### After: State Machine-Based

```csharp
public class WorkerPool
{
    private readonly IWorkerStateProvider? _stateProvider;

    public async Task<AgentInfo?> GetAvailableWorkerAsync()
    {
        var agents = await _registry.GetAllAsync();
        return agents.FirstOrDefault(a =>
        {
            if (_stateProvider != null)
            {
                var currentState = _stateProvider.GetAgentState(a.AgentId);
                return AgentStatusStateMachineAdapter.IsAvailable(
                    currentState,
                    a.CurrentTaskCount,
                    a.Capabilities.MaxConcurrentTasks);
            }
            
            // Fallback to enum
            return a.Status == AgentStatus.Available &&
                   a.CurrentTaskCount < a.Capabilities.MaxConcurrentTasks;
        });
    }
}
```

## Additional Resources

- [State Machines README](../../src/DotNetAgents.Agents.StateMachines/README.md)
- [State Machine Patterns](../../src/DotNetAgents.Agents.StateMachines/StateMachinePatterns.cs)
- [State-Based Worker Pool](../../src/DotNetAgents.Agents.StateMachines/StateBasedWorkerPool.cs)
- [State Machines Sample](../../samples/DotNetAgents.Samples.StateMachines/)

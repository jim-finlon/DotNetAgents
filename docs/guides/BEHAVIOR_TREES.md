# Behavior Trees Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

Behavior Trees provide hierarchical decision-making for autonomous agents. DotNetAgents includes a comprehensive behavior tree implementation with composite nodes, decorators, and integration with the agent system.

## Table of Contents

1. [Introduction](#introduction)
2. [Core Concepts](#core-concepts)
3. [Node Types](#node-types)
4. [Creating Behavior Trees](#creating-behavior-trees)
5. [Integration with Agents](#integration-with-agents)
6. [Advanced Patterns](#advanced-patterns)
7. [Examples](#examples)

## Introduction

Behavior Trees are a powerful pattern for modeling agent decision-making. They provide:
- **Hierarchical Structure**: Organize decisions in a tree
- **Reusability**: Share behavior patterns across agents
- **Composability**: Build complex behaviors from simple nodes
- **Debugging**: Easy to visualize and trace execution

## Core Concepts

### Behavior Tree Structure

```
BehaviorTree
├── Root Node (Selector/Sequence)
│   ├── Composite Node
│   │   ├── Action Node
│   │   ├── Condition Node
│   │   └── Decorator Node
│   └── Leaf Node
```

### Execution Flow

1. Tree executes from root
2. Nodes return `Success`, `Failure`, or `Running`
3. Composite nodes control execution flow
4. Decorators modify node behavior

## Node Types

### Action Nodes

Execute actions and return results:

```csharp
public class MoveToLocationAction : BehaviorTreeNode
{
    private readonly string _location;

    public MoveToLocationAction(string location)
    {
        _location = location;
    }

    protected override Task<BehaviorTreeStatus> ExecuteAsync(
        BehaviorTreeContext context,
        CancellationToken cancellationToken)
    {
        // Perform action
        Console.WriteLine($"Moving to {_location}");
        
        // Return success or failure
        return Task.FromResult(BehaviorTreeStatus.Success);
    }
}
```

### Condition Nodes

Check conditions:

```csharp
public class HasItemCondition : BehaviorTreeNode
{
    private readonly string _itemName;

    public HasItemCondition(string itemName)
    {
        _itemName = itemName;
    }

    protected override Task<BehaviorTreeStatus> ExecuteAsync(
        BehaviorTreeContext context,
        CancellationToken cancellationToken)
    {
        var hasItem = context.GetState<bool>($"has_{_itemName}");
        return Task.FromResult(hasItem 
            ? BehaviorTreeStatus.Success 
            : BehaviorTreeStatus.Failure);
    }
}
```

### Composite Nodes

#### Selector (OR Logic)

Executes children until one succeeds:

```csharp
var selector = new SelectorNode(
    new CheckInventoryAction(),
    new BuyItemAction("sword"),
    new CraftItemAction("sword")
);
```

#### Sequence (AND Logic)

Executes children until one fails:

```csharp
var sequence = new SequenceNode(
    new HasItemCondition("key"),
    new MoveToLocationAction("door"),
    new OpenDoorAction()
);
```

### Decorator Nodes

Modify child node behavior:

```csharp
// Inverter: Inverts success/failure
var inverted = new InverterNode(childNode);

// Repeater: Repeats child until failure
var repeater = new RepeaterNode(childNode, maxRepeats: 5);

// Cooldown: Adds cooldown between executions
var cooldown = new CooldownNode(childNode, TimeSpan.FromSeconds(10));
```

## Creating Behavior Trees

### Basic Tree

```csharp
using DotNetAgents.Agents.BehaviorTrees;

var tree = new BehaviorTree(
    root: new SelectorNode(
        // Try to attack
        new SequenceNode(
            new HasEnemyCondition(),
            new InRangeCondition(),
            new AttackAction()
        ),
        // Otherwise, move toward enemy
        new SequenceNode(
            new HasEnemyCondition(),
            new MoveToEnemyAction()
        ),
        // Otherwise, patrol
        new PatrolAction()
    )
);
```

### Using Builder Pattern

```csharp
var tree = BehaviorTreeBuilder
    .Create()
    .Selector()
        .Sequence()
            .Condition(new HasEnemyCondition())
            .Condition(new InRangeCondition())
            .Action(new AttackAction())
        .End()
        .Sequence()
            .Condition(new HasEnemyCondition())
            .Action(new MoveToEnemyAction())
        .End()
        .Action(new PatrolAction())
    .End()
    .Build();
```

## Integration with Agents

### Tool Selection Behavior Tree

```csharp
using DotNetAgents.Agents.BehaviorTrees;

public class ToolSelectionBehaviorTree : BehaviorTree
{
    public ToolSelectionBehaviorTree(IToolRegistry toolRegistry, ILogger logger)
    {
        Root = new SelectorNode(
            // Exact match strategy
            new SequenceNode(
                new ExactMatchCondition(toolRegistry),
                new UseExactMatchToolAction()
            ),
            // Capability match strategy
            new SequenceNode(
                new CapabilityMatchCondition(toolRegistry),
                new UseCapabilityMatchToolAction()
            ),
            // Description match strategy
            new SequenceNode(
                new DescriptionMatchCondition(toolRegistry),
                new UseDescriptionMatchToolAction()
            ),
            // Fallback: no tool found
            new NoToolFoundAction()
        );
    }
}
```

### Agent Decision Making

```csharp
var agentTree = new BehaviorTree(
    root: new SelectorNode(
        // High priority: Handle urgent tasks
        new SequenceNode(
            new HasUrgentTaskCondition(),
            new ProcessUrgentTaskAction()
        ),
        // Medium priority: Process queue
        new SequenceNode(
            new HasQueuedTaskCondition(),
            new ProcessQueuedTaskAction()
        ),
        // Low priority: Idle behavior
        new IdleAction()
    )
);

// Execute tree
var context = new BehaviorTreeContext();
var status = await agentTree.ExecuteAsync(context, cancellationToken);
```

## Advanced Patterns

### Dynamic Tree Modification

```csharp
var tree = new BehaviorTree(rootNode);

// Add node dynamically
var newNode = new CustomAction();
tree.AddNode(newNode, parentId: "selector-1");

// Remove node
tree.RemoveNode("old-node");

// Replace node
tree.ReplaceNode("old-node", newNode);
```

### State Management

```csharp
var context = new BehaviorTreeContext();

// Set state
context.SetState("health", 100);
context.SetState("has_weapon", true);

// Get state
var health = context.GetState<int>("health");
var hasWeapon = context.GetState<bool>("has_weapon");

// Execute with context
await tree.ExecuteAsync(context, cancellationToken);
```

### Blackboard Pattern

```csharp
public class BlackboardBehaviorTreeContext : BehaviorTreeContext
{
    private readonly Dictionary<string, object> _blackboard = new();

    public void SetBlackboardValue(string key, object value)
    {
        _blackboard[key] = value;
    }

    public T GetBlackboardValue<T>(string key)
    {
        return (T)_blackboard[key];
    }
}
```

## Examples

### Example 1: NPC AI

```csharp
var npcTree = BehaviorTreeBuilder
    .Create()
    .Selector()
        // Combat behavior
        .Sequence()
            .Condition(new EnemyInRangeCondition())
            .Action(new AttackAction())
        .End()
        // Gathering behavior
        .Sequence()
            .Condition(new ResourceNearbyCondition())
            .Action(new GatherResourceAction())
        .End()
        // Exploration behavior
        .Action(new ExploreAction())
    .End()
    .Build();
```

### Example 2: Task Routing

```csharp
public class TaskRoutingBehaviorTree : BehaviorTree
{
    public TaskRoutingBehaviorTree(IWorkerPool workerPool, ILogger logger)
    {
        Root = new SelectorNode(
            // High priority tasks
            new SequenceNode(
                new HighPriorityCondition(),
                new PriorityBasedRoutingAction(workerPool)
            ),
            // Capability-based routing
            new SequenceNode(
                new RequiresCapabilityCondition(),
                new CapabilityBasedRoutingAction(workerPool)
            ),
            // Load-balanced routing
            new LoadBalancedRoutingAction(workerPool)
        );
    }
}
```

### Example 3: Adaptive Learning Path

```csharp
var learningTree = BehaviorTreeBuilder
    .Create()
    .Selector()
        // Review needed
        .Sequence()
            .Condition(new ReviewNeededCondition())
            .Action(new ScheduleReviewAction())
        .End()
        // Mastery gap
        .Sequence()
            .Condition(new MasteryGapCondition())
            .Action(new FillGapAction())
        .End()
        // Prerequisite-based
        .Sequence()
            .Condition(new PrerequisiteCheckCondition())
            .Action(new PrerequisiteBasedPathAction())
        .End()
        // Default: Continue current path
        .Action(new ContinuePathAction())
    .End()
    .Build();
```

## Best Practices

1. **Keep Trees Shallow**: Avoid deep nesting (max 4-5 levels)
2. **Use Composites Wisely**: Selectors for OR, Sequences for AND
3. **State Management**: Use context for shared state
4. **Error Handling**: Handle failures gracefully
5. **Testing**: Test each node independently
6. **Documentation**: Document tree structure and purpose

## Related Documentation

- [State Machines Guide](./MIGRATING_TO_STATE_MACHINES.md)
- [Behavior Trees README](../../src/DotNetAgents.Agents.BehaviorTrees/README.md)
- [Multi-Agent Patterns](./ADVANCED_MULTI_AGENT_PATTERNS.md)

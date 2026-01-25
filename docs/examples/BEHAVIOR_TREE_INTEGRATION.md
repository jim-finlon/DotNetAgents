# Behavior Tree Integration Examples

This guide provides practical examples for integrating behavior trees into various DotNetAgents components.

## Table of Contents

1. [Supervisor Task Routing](#supervisor-task-routing)
2. [Voice Command Processing](#voice-command-processing)
3. [Adaptive Learning Paths](#adaptive-learning-paths)
4. [Tool Selection](#tool-selection)
5. [Custom Behavior Trees](#custom-behavior-trees)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

## Supervisor Task Routing

### Basic Setup

```csharp
using DotNetAgents.Agents.Supervisor.BehaviorTrees;
using DotNetAgents.Agents.Supervisor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Register supervisor with behavior tree for task routing
services.AddSupervisorWithStateMachine(
    stateMachineFactory: sp => { /* ... */ },
    taskRouterFactory: sp =>
    {
        var logger = sp.GetRequiredService<ILogger<TaskRoutingBehaviorTree>>();
        return new TaskRoutingBehaviorTree(
            supervisor: sp.GetRequiredService<ISupervisorAgent>(),
            logger: logger);
    });

var serviceProvider = services.BuildServiceProvider();
var supervisor = serviceProvider.GetRequiredService<ISupervisorAgent>();
```

### LLM-Based Task Routing

For more sophisticated routing decisions:

```csharp
services.AddSupervisorWithStateMachine(
    stateMachineFactory: sp => { /* ... */ },
    taskRouterFactory: sp =>
    {
        var llm = sp.GetRequiredService<ILLMModel<ChatMessage[], ChatMessage>>();
        var logger = sp.GetRequiredService<ILogger<LLMTaskRouter>>();
        return new LLMTaskRouter(
            supervisor: sp.GetRequiredService<ISupervisorAgent>(),
            llm: llm,
            logger: logger);
    });
```

### Using the Task Router

```csharp
var task = new WorkerTask
{
    TaskId = Guid.NewGuid(),
    Description = "Process customer order",
    Priority = TaskPriority.High,
    RequiredCapabilities = new[] { "order-processing" }
};

// Behavior tree automatically selects best worker based on:
// - Priority
// - Capability matching
// - Load balancing
var assignedWorker = await supervisor.SubmitTaskAsync(task);
```

## Voice Command Processing

### Setup with Command Processing Behavior Tree

```csharp
using DotNetAgents.Voice;
using DotNetAgents.Voice.BehaviorTrees;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Register command orchestration with behavior tree
services.AddCommandOrchestrationWithStateMachine(
    stateMachineFactory: sp => { /* ... */ },
    behaviorTreeFactory: sp =>
    {
        var logger = sp.GetRequiredService<ILogger<CommandProcessingBehaviorTree>>();
        return new CommandProcessingBehaviorTree(
            lowConfidenceThreshold: 0.6,
            logger: logger);
    });

var serviceProvider = services.BuildServiceProvider();
var orchestrator = serviceProvider.GetRequiredService<ICommandWorkflowOrchestrator>();
```

### Command Processing Strategies

The behavior tree automatically determines the processing strategy:

```csharp
var commandState = new CommandState
{
    UserId = userId,
    RawText = "create a note about the meeting",
    Source = "voice",
    Intent = new Intent
    {
        Domain = "notes",
        Action = "create",
        Confidence = 0.95,
        Parameters = new Dictionary<string, object> { { "title", "Meeting Notes" } }
    }
};

// Behavior tree determines strategy:
// - Simple: High confidence, complete parameters
// - MultiStep: Requires workflow execution
// - Ambiguous: Low confidence, needs clarification
var result = await orchestrator.ExecuteAsync(commandState);

// If ambiguous, clarification is automatically requested
if (result.Status == CommandStatus.AwaitingClarification)
{
    // Handle clarification request
}
```

## Adaptive Learning Paths

### Setup with Adaptive Learning Path Behavior Tree

```csharp
using DotNetAgents.Education.BehaviorTrees;
using DotNetAgents.Education.Pedagogy;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Create behavior tree
var masteryCalculator = services.BuildServiceProvider().GetRequiredService<IMasteryCalculator>();
var behaviorTree = new AdaptiveLearningPathBehaviorTree(
    masteryCalculator: masteryCalculator,
    logger: services.BuildServiceProvider().GetRequiredService<ILogger<AdaptiveLearningPathBehaviorTree>>());
```

### Determining Learning Path

```csharp
var availableConcepts = new List<ConceptId>
{
    new ConceptId("addition", SubjectArea.Mathematics, GradeLevel.K2),
    new ConceptId("subtraction", SubjectArea.Mathematics, GradeLevel.K2),
    new ConceptId("multiplication", SubjectArea.Mathematics, GradeLevel.G3_5)
};

var studentMastery = new Dictionary<ConceptId, ConceptMastery>
{
    [availableConcepts[0]] = new ConceptMastery
    {
        ConceptId = availableConcepts[0],
        Score = 85.0,
        Level = MasteryLevel.Advanced,
        LastUpdated = DateTimeOffset.UtcNow.AddDays(-10) // Needs review
    }
};

// Behavior tree selects concept based on:
// - Review needed (spaced repetition)
// - Mastery gaps
// - Prerequisites met
// - Difficulty progression
var context = await behaviorTree.DetermineLearningPathAsync(
    studentId: "student-123",
    availableConcepts: availableConcepts,
    studentMastery: studentMastery);

Console.WriteLine($"Selected Concept: {context.SelectedConcept?.Value}");
Console.WriteLine($"Strategy: {context.Strategy}"); // ReviewNeeded, MasteryGap, PrerequisiteBased, etc.
Console.WriteLine($"Reason: {context.SelectionReason}");
```

## Tool Selection

### Setup with Tool Selection Behavior Tree

```csharp
using DotNetAgents.Core.Agents.BehaviorTrees;
using DotNetAgents.Core.Tools;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Create behavior tree
var behaviorTree = new ToolSelectionBehaviorTree(
    logger: services.BuildServiceProvider().GetRequiredService<ILogger<ToolSelectionBehaviorTree>>());
```

### Selecting Tools

```csharp
var toolRegistry = new ToolRegistry();
toolRegistry.Register(new CalculatorTool());
toolRegistry.Register(new DateTimeTool());
toolRegistry.Register(new WebSearchTool(httpClient));

var availableTools = toolRegistry.GetAllTools().ToList();

// Select tool by exact name
var context1 = await behaviorTree.SelectToolAsync(
    requestedToolName: "Calculator",
    availableTools: availableTools);
// Strategy: ExactMatch

// Select tool by capability
var context2 = await behaviorTree.SelectToolAsync(
    requestedToolName: "calc",
    availableTools: availableTools);
// Strategy: CapabilityMatch

// Select tool by description keywords
var context3 = await behaviorTree.SelectToolAsync(
    requestedToolName: null,
    capabilitySearch: "mathematical operations",
    availableTools: availableTools);
// Strategy: DescriptionMatch

Console.WriteLine($"Selected: {context3.SelectedTool?.Name}");
Console.WriteLine($"Strategy: {context3.Strategy}");
```

## Custom Behavior Trees

### Creating a Custom Behavior Tree

```csharp
using DotNetAgents.Agents.BehaviorTrees;
using Microsoft.Extensions.Logging;

public class CustomBehaviorTree
{
    private readonly BehaviorTree<CustomContext> _tree;
    private readonly BehaviorTreeExecutor<CustomContext> _executor;
    private readonly ILogger<CustomBehaviorTree>? _logger;

    public CustomBehaviorTree(ILogger<CustomBehaviorTree>? logger = null)
    {
        _logger = logger;

        // Create loggers for nodes
        ILogger<BehaviorTreeNode<CustomContext>>? nodeLogger = null;
        ILogger<BehaviorTreeExecutor<CustomContext>>? executorLogger = null;
        if (logger != null)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            nodeLogger = loggerFactory.CreateLogger<BehaviorTreeNode<CustomContext>>();
            executorLogger = loggerFactory.CreateLogger<BehaviorTreeExecutor<CustomContext>>();
        }

        // Build behavior tree
        var root = new SelectorNode<CustomContext>("RootSelector", nodeLogger)
            // Strategy 1: High Priority Path
            .AddChild(new SequenceNode<CustomContext>("HighPrioritySequence", nodeLogger)
                .AddChild(new ConditionNode<CustomContext>(
                    "IsHighPriority",
                    ctx => ctx.Priority == Priority.High,
                    nodeLogger))
                .AddChild(new ActionNode<CustomContext>(
                    "HandleHighPriority",
                    ctx => HandleHighPriority(ctx),
                    nodeLogger)))
            // Strategy 2: Normal Path
            .AddChild(new SequenceNode<CustomContext>("NormalSequence", nodeLogger)
                .AddChild(new ConditionNode<CustomContext>(
                    "IsNormalPriority",
                    ctx => ctx.Priority == Priority.Normal,
                    nodeLogger))
                .AddChild(new ActionNode<CustomContext>(
                    "HandleNormal",
                    ctx => HandleNormal(ctx),
                    nodeLogger)))
            // Fallback
            .AddChild(new ActionNode<CustomContext>(
                "HandleDefault",
                ctx => HandleDefault(ctx),
                nodeLogger));

        _tree = new BehaviorTree<CustomContext>("CustomTree", root);
        _executor = new BehaviorTreeExecutor<CustomContext>(executorLogger);
    }

    public async Task<CustomContext> ProcessAsync(
        CustomContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await _executor.ExecuteAsync(_tree, context, cancellationToken);
        return context;
    }

    private BehaviorTreeNodeStatus HandleHighPriority(CustomContext context)
    {
        // Handle high priority
        context.Result = "High priority handled";
        return BehaviorTreeNodeStatus.Success;
    }

    private BehaviorTreeNodeStatus HandleNormal(CustomContext context)
    {
        // Handle normal priority
        context.Result = "Normal priority handled";
        return BehaviorTreeNodeStatus.Success;
    }

    private BehaviorTreeNodeStatus HandleDefault(CustomContext context)
    {
        // Handle default
        context.Result = "Default handled";
        return BehaviorTreeNodeStatus.Success;
    }
}

public class CustomContext
{
    public Priority Priority { get; set; }
    public string? Result { get; set; }
}

public enum Priority
{
    Low,
    Normal,
    High
}
```

### Using LLM Nodes

```csharp
using DotNetAgents.Agents.BehaviorTrees;

// Create LLM action node for dynamic decision-making
var llmNode = new LLMActionNode<CustomContext>(
    name: "LLMDecision",
    llm: llm,
    promptTemplate: "Analyze the context and decide: {Context}",
    resultExtractor: (response, context) =>
    {
        // Extract decision from LLM response
        if (response.Contains("high priority"))
        {
            context.Priority = Priority.High;
            return BehaviorTreeNodeStatus.Success;
        }
        return BehaviorTreeNodeStatus.Failure;
    },
    logger: logger);

// Add to behavior tree
root.AddChild(llmNode);
```

## Best Practices

### 1. Use Selector Nodes for Multiple Strategies

```csharp
// Root selector tries strategies in order until one succeeds
var root = new SelectorNode<Context>("RootSelector")
    .AddChild(new SequenceNode<Context>("Strategy1")
        .AddChild(new ConditionNode<Context>("CheckCondition1", ctx => ctx.Condition1))
        .AddChild(new ActionNode<Context>("ExecuteStrategy1", ctx => ExecuteStrategy1(ctx))))
    .AddChild(new SequenceNode<Context>("Strategy2")
        .AddChild(new ConditionNode<Context>("CheckCondition2", ctx => ctx.Condition2))
        .AddChild(new ActionNode<Context>("ExecuteStrategy2", ctx => ExecuteStrategy2(ctx))));
```

### 2. Use Sequence Nodes for Multi-Step Actions

```csharp
// Sequence executes all children in order, fails if any child fails
var sequence = new SequenceNode<Context>("MultiStepAction")
    .AddChild(new ActionNode<Context>("Step1", ctx => Step1(ctx)))
    .AddChild(new ActionNode<Context>("Step2", ctx => Step2(ctx)))
    .AddChild(new ActionNode<Context>("Step3", ctx => Step3(ctx)));
```

### 3. Use Decorator Nodes for Retry Logic

```csharp
using DotNetAgents.Agents.BehaviorTrees;

// Retry decorator retries action up to 3 times
var retryNode = new RetryDecoratorNode<Context>(
    child: new ActionNode<Context>("UnreliableAction", ctx => UnreliableAction(ctx)),
    maxRetries: 3,
    logger: logger);

// Timeout decorator limits execution time
var timeoutNode = new TimeoutDecoratorNode<Context>(
    child: new ActionNode<Context>("LongRunningAction", ctx => LongRunningAction(ctx)),
    timeout: TimeSpan.FromSeconds(30),
    logger: logger);
```

### 4. Handle Context Updates

```csharp
public class MyContext
{
    public string? SelectedStrategy { get; set; }
    public string? SelectionReason { get; set; }
    public bool ShouldExecute { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Update context in action nodes
private BehaviorTreeNodeStatus SelectStrategy(MyContext context)
{
    context.SelectedStrategy = "Strategy1";
    context.SelectionReason = "Condition met";
    context.ShouldExecute = true;
    return BehaviorTreeNodeStatus.Success;
}
```

### 5. Log Behavior Tree Execution

```csharp
// Enable detailed logging
services.AddLogging(builder =>
    builder.AddConsole()
           .SetMinimumLevel(LogLevel.Debug)
           .AddFilter("DotNetAgents.Agents.BehaviorTrees", LogLevel.Trace));
```

## Troubleshooting

### Issue: Behavior tree always returns Failure

**Possible Causes:**
1. No conditions are met
2. All action nodes are failing
3. Context not properly initialized

**Solution:**
```csharp
// Add logging to understand execution flow
var result = await _executor.ExecuteAsync(_tree, context, cancellationToken);
_logger?.LogDebug("Behavior tree execution result: {Status}", result.Status);

// Check context state
_logger?.LogDebug("Context state: {Context}", JsonSerializer.Serialize(context));
```

### Issue: LLM node not working

**Possible Causes:**
1. LLM not configured
2. Prompt template incorrect
3. Result extractor failing

**Solution:**
```csharp
// Verify LLM is available
if (llm == null)
{
    throw new InvalidOperationException("LLM not configured");
}

// Test prompt template
var prompt = promptTemplate.Format(new { Context = "test" });
_logger?.LogDebug("LLM prompt: {Prompt}", prompt);

// Verify result extractor
var extracted = resultExtractor("test response", context);
_logger?.LogDebug("Extracted result: {Result}", extracted);
```

### Issue: Behavior tree too slow

**Possible Causes:**
1. Too many LLM calls
2. Complex conditions
3. Sequential execution when parallel possible

**Solution:**
```csharp
// Use ParallelNode for independent actions
var parallel = new ParallelNode<Context>("ParallelActions")
    .AddChild(new ActionNode<Context>("Action1", ctx => Action1(ctx)))
    .AddChild(new ActionNode<Context>("Action2", ctx => Action2(ctx)));

// Cache LLM results
var cachedLLMNode = new CooldownDecoratorNode<Context>(
    child: llmNode,
    cooldownDuration: TimeSpan.FromMinutes(5),
    logger: logger);
```

## Additional Resources

- [Behavior Trees README](../../src/DotNetAgents.Agents.BehaviorTrees/README.md)
- [State Machine Integration Examples](./STATE_MACHINE_INTEGRATION.md)
- [LLM Action Node Documentation](../../src/DotNetAgents.Agents.BehaviorTrees/LLMActionNode.cs)

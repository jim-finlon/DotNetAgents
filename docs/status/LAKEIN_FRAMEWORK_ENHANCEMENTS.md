# DotNetAgents Framework Enhancements for Lakein System

This document outlines the enhancements needed in the DotNetAgents framework to fully support the Lakein Time Management System application.

## Overview

The Lakein system requires several framework enhancements to enable:
1. **Enhanced Voice Command Intent Taxonomy** - Support for time management domain
2. **Dialog Management** - Multi-turn conversations for goal setting and activity generation
3. **Scheduled Command Execution** - Commands that execute at specific times
4. **Command Templates** - Reusable command patterns
5. **Enhanced MCP Adapter Patterns** - Better integration with domain-specific adapters
6. **Workflow State Persistence** - Long-running workflows for goal exercises
7. **Agent Composition** - Composable agents for complex operations

## Phase 1: Enhanced Intent Taxonomy (Priority: High)

### 1.1 Dynamic Intent Registration

**Current State:** `IntentTaxonomy` is static with hardcoded intents.

**Enhancement:** Allow runtime registration of intents from application code.

**New Interface:**
```csharp
// DotNetAgents.Voice/IntentClassification/IIntentTaxonomyRegistry.cs
public interface IIntentTaxonomyRegistry
{
    void RegisterIntent(
        string domain,
        string action,
        string? subType,
        string[] requiredParameters,
        string[]? optionalParameters = null,
        string? description = null);
        
    void RegisterDomain(
        string domain,
        string description,
        Dictionary<string, string>? actionDescriptions = null);
        
    IntentTaxonomy GetTaxonomy();
}
```

**Implementation:**
```csharp
// DotNetAgents.Voice/IntentClassification/IntentTaxonomyRegistry.cs
public class IntentTaxonomyRegistry : IIntentTaxonomyRegistry
{
    private readonly Dictionary<string, DomainDefinition> _domains = new();
    
    public void RegisterIntent(
        string domain,
        string action,
        string? subType,
        string[] requiredParameters,
        string[]? optionalParameters = null,
        string? description = null)
    {
        if (!_domains.ContainsKey(domain))
        {
            _domains[domain] = new DomainDefinition(domain);
        }
        
        var intentName = subType != null
            ? $"{domain}.{action}.{subType}"
            : $"{domain}.{action}";
            
        _domains[domain].Intents[intentName] = new IntentDefinition(
            domain,
            action,
            subType,
            requiredParameters,
            optionalParameters ?? Array.Empty<string>(),
            description);
    }
    
    public IntentTaxonomy GetTaxonomy()
    {
        return new IntentTaxonomy(_domains);
    }
}
```

**Usage in Lakein:**
```csharp
// LakeinTimeManagement.Agents/VoiceCommands/LakeinIntentRegistration.cs
public static class LakeinIntentRegistration
{
    public static void RegisterLakeinIntents(IIntentTaxonomyRegistry registry)
    {
        registry.RegisterDomain("goals", "Goal management operations");
        registry.RegisterIntent("goals", "create", "lifetime", 
            new[] { "categories" },
            new[] { "user_id" },
            "Create a lifetime goals statement");
            
        registry.RegisterIntent("activities", "create", null,
            new[] { "description", "estimated_minutes" },
            new[] { "goal_id", "priority", "scheduled_date" },
            "Create a new activity");
            
        // ... more intents
    }
}
```

### 1.2 Intent Context Awareness

**Enhancement:** Allow intents to be context-aware (e.g., "complete this" refers to current activity).

**New Model:**
```csharp
// DotNetAgents.Voice/IntentClassification/IntentContext.cs
public record IntentContext
{
    public Guid? CurrentActivityId { get; init; }
    public Guid? CurrentGoalId { get; init; }
    public Guid? CurrentPlanId { get; init; }
    public DateTime CurrentTime { get; init; }
    public Dictionary<string, object> AdditionalContext { get; init; } = new();
}
```

**Updated Interface:**
```csharp
// DotNetAgents.Voice/IntentClassification/IIntentClassifier.cs
public interface IIntentClassifier
{
    Task<Intent> ClassifyAsync(
        string commandText,
        IntentContext? context = null,
        CancellationToken cancellationToken = default);
}
```

## Phase 2: Dialog Management (Priority: High)

### 2.1 Dialog State Management

**New Package:** `DotNetAgents.Voice.Dialog`

**Components:**
```csharp
// DotNetAgents.Voice.Dialog/IDialogManager.cs
public interface IDialogManager
{
    Task<DialogState> StartDialogAsync(
        Guid userId,
        string dialogType,
        Dictionary<string, object>? initialContext = null,
        CancellationToken ct = default);
        
    Task<DialogState> ContinueDialogAsync(
        Guid dialogId,
        string userInput,
        CancellationToken ct = default);
        
    Task<DialogState> GetDialogStateAsync(
        Guid dialogId,
        CancellationToken ct = default);
        
    Task CompleteDialogAsync(
        Guid dialogId,
        CancellationToken ct = default);
}

// DotNetAgents.Voice.Dialog/DialogState.cs
public record DialogState
{
    public Guid DialogId { get; init; }
    public Guid UserId { get; init; }
    public string DialogType { get; init; } = string.Empty;
    public DialogStatus Status { get; init; }
    public Dictionary<string, object> CollectedData { get; init; } = new();
    public List<string> PendingQuestions { get; init; } = new();
    public string? CurrentQuestion { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public enum DialogStatus
{
    Active,
    WaitingForInput,
    Completed,
    Cancelled
}
```

**Implementation:**
```csharp
// DotNetAgents.Voice.Dialog/DialogManager.cs
public class DialogManager : IDialogManager
{
    private readonly ILLMModel _llm;
    private readonly IDialogStore _store;
    private readonly Dictionary<string, IDialogHandler> _handlers;
    
    public async Task<DialogState> ContinueDialogAsync(
        Guid dialogId,
        string userInput,
        CancellationToken ct = default)
    {
        var state = await _store.GetAsync(dialogId, ct);
        var handler = _handlers[state.DialogType];
        
        var updatedState = await handler.ProcessInputAsync(state, userInput, ct);
        await _store.UpdateAsync(updatedState, ct);
        
        return updatedState;
    }
}
```

**Dialog Handlers:**
```csharp
// DotNetAgents.Voice.Dialog/IDialogHandler.cs
public interface IDialogHandler
{
    Task<DialogState> InitializeAsync(
        Dictionary<string, object>? context,
        CancellationToken ct = default);
        
    Task<DialogState> ProcessInputAsync(
        DialogState currentState,
        string userInput,
        CancellationToken ct = default);
        
    Task<bool> IsCompleteAsync(
        DialogState state,
        CancellationToken ct = default);
}
```

**Example: Lifetime Goals Dialog Handler**
```csharp
// LakeinTimeManagement.Agents/Dialogs/LifetimeGoalsDialogHandler.cs
public class LifetimeGoalsDialogHandler : IDialogHandler
{
    private readonly ILLMModel _llm;
    
    public async Task<DialogState> InitializeAsync(
        Dictionary<string, object>? context,
        CancellationToken ct = default)
    {
        return new DialogState
        {
            DialogId = Guid.NewGuid(),
            DialogType = "lifetime_goals",
            Status = DialogStatus.Active,
            CurrentQuestion = "What are your lifetime goals? You have 2 minutes. Start writing...",
            StartedAt = DateTime.UtcNow
        };
    }
    
    public async Task<DialogState> ProcessInputAsync(
        DialogState currentState,
        string userInput,
        CancellationToken ct = default)
    {
        // Use LLM to extract goals from user input
        var goals = await ExtractGoalsAsync(userInput, ct);
        
        var updatedState = currentState with
        {
            CollectedData = currentState.CollectedData
                .SetItem("lifetime_goals", goals)
        };
        
        // Move to next question if time is up or user indicates completion
        if (IsTimeUp() || UserIndicatesCompletion(userInput))
        {
            updatedState = updatedState with
            {
                CurrentQuestion = "Now, how would you like to spend the next 5 years? (2 minutes)",
                PendingQuestions = new List<string> { "six_month_lightning" }
            };
        }
        
        return updatedState;
    }
}
```

### 2.2 Dialog Store

**Interface:**
```csharp
// DotNetAgents.Voice.Dialog/IDialogStore.cs
public interface IDialogStore
{
    Task<DialogState> CreateAsync(DialogState state, CancellationToken ct = default);
    Task<DialogState?> GetAsync(Guid dialogId, CancellationToken ct = default);
    Task<DialogState> UpdateAsync(DialogState state, CancellationToken ct = default);
    Task<List<DialogState>> GetActiveDialogsAsync(Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid dialogId, CancellationToken ct = default);
}
```

**Storage Implementations:**
- `InMemoryDialogStore` (for testing)
- `SqlServerDialogStore` (via `DotNetAgents.Storage.SqlServer`)
- `PostgreSQLDialogStore` (via `DotNetAgents.Storage.PostgreSQL`)

## Phase 3: Scheduled Command Execution (Priority: Medium)

### 3.1 Command Scheduler

**New Package:** `DotNetAgents.Voice.Scheduling`

**Components:**
```csharp
// DotNetAgents.Voice.Scheduling/ICommandScheduler.cs
public interface ICommandScheduler
{
    Task<Guid> ScheduleCommandAsync(
        Guid userId,
        string commandText,
        DateTime executeAt,
        Dictionary<string, object>? context = null,
        CancellationToken ct = default);
        
    Task CancelScheduledCommandAsync(
        Guid scheduledCommandId,
        CancellationToken ct = default);
        
    Task<List<ScheduledCommand>> GetScheduledCommandsAsync(
        Guid userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default);
}

// DotNetAgents.Voice.Scheduling/ScheduledCommand.cs
public record ScheduledCommand
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string CommandText { get; init; } = string.Empty;
    public DateTime ExecuteAt { get; init; }
    public Dictionary<string, object> Context { get; init; } = new();
    public ScheduledCommandStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExecutedAt { get; init; }
    public string? ErrorMessage { get; init; }
}

public enum ScheduledCommandStatus
{
    Pending,
    Executing,
    Completed,
    Failed,
    Cancelled
}
```

**Implementation:**
```csharp
// DotNetAgents.Voice.Scheduling/CommandScheduler.cs
public class CommandScheduler : ICommandScheduler, IHostedService
{
    private readonly ICommandWorkflowOrchestrator _orchestrator;
    private readonly IScheduledCommandStore _store;
    private readonly ILogger<CommandScheduler> _logger;
    private Timer? _timer;
    
    public async Task StartAsync(CancellationToken ct)
    {
        _timer = new Timer(ExecuteScheduledCommands, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }
    
    private async void ExecuteScheduledCommands(object? state)
    {
        var now = DateTime.UtcNow;
        var commands = await _store.GetDueCommandsAsync(now, CancellationToken.None);
        
        foreach (var cmd in commands)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _store.UpdateStatusAsync(cmd.Id, ScheduledCommandStatus.Executing, CancellationToken.None);
                    
                    var commandState = new CommandState
                    {
                        UserId = cmd.UserId,
                        RawText = cmd.CommandText,
                        Source = "scheduled"
                    };
                    
                    await _orchestrator.ExecuteAsync(commandState);
                    await _store.UpdateStatusAsync(cmd.Id, ScheduledCommandStatus.Completed, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute scheduled command {CommandId}", cmd.Id);
                    await _store.UpdateStatusAsync(cmd.Id, ScheduledCommandStatus.Failed, CancellationToken.None, ex.Message);
                }
            });
        }
    }
}
```

**Usage in Lakein:**
```csharp
// Schedule daily planning reminder
await _commandScheduler.ScheduleCommandAsync(
    userId,
    "create daily plan for today",
    DateTime.Today.AddHours(7), // 7 AM
    cancellationToken);
```

## Phase 4: Command Templates (Priority: Medium)

### 4.1 Template System

**New Components:**
```csharp
// DotNetAgents.Voice/Commands/ICommandTemplate.cs
public interface ICommandTemplate
{
    string Name { get; }
    string Description { get; }
    string Template { get; }
    Dictionary<string, string> Parameters { get; }
    
    string Render(Dictionary<string, object> values);
}

// DotNetAgents.Voice/Commands/CommandTemplateRegistry.cs
public class CommandTemplateRegistry
{
    private readonly Dictionary<string, ICommandTemplate> _templates = new();
    
    public void RegisterTemplate(ICommandTemplate template)
    {
        _templates[template.Name] = template;
    }
    
    public string? RenderTemplate(string templateName, Dictionary<string, object> values)
    {
        if (!_templates.TryGetValue(templateName, out var template))
            return null;
            
        return template.Render(values);
    }
}
```

**Example Templates:**
```csharp
// LakeinTimeManagement.Agents/Commands/LakeinCommandTemplates.cs
public class CreateActivityTemplate : ICommandTemplate
{
    public string Name => "create_activity";
    public string Description => "Create a new activity";
    public string Template => "create activity '{description}' for {estimated_minutes} minutes";
    public Dictionary<string, string> Parameters => new()
    {
        { "description", "string" },
        { "estimated_minutes", "int" }
    };
    
    public string Render(Dictionary<string, object> values)
    {
        return Template
            .Replace("{description}", (string)values["description"])
            .Replace("{estimated_minutes}", values["estimated_minutes"].ToString());
    }
}
```

## Phase 5: Enhanced MCP Adapter Patterns (Priority: Medium)

### 5.1 Domain-Specific Adapter Base

**New Base Class:**
```csharp
// DotNetAgents.Mcp/Adapters/McpAdapterBase.cs
public abstract class McpAdapterBase : IMcpAdapter
{
    protected readonly ILogger Logger;
    protected readonly Dictionary<string, Func<Dictionary<string, object>, CancellationToken, Task<object>>> ToolHandlers;
    
    protected McpAdapterBase(ILogger logger)
    {
        Logger = logger;
        ToolHandlers = new Dictionary<string, Func<Dictionary<string, object>, CancellationToken, Task<object>>>();
        RegisterToolHandlers();
    }
    
    protected abstract void RegisterToolHandlers();
    
    public virtual async Task<object> ExecuteToolAsync(
        string toolName,
        Dictionary<string, object> parameters,
        CancellationToken ct = default)
    {
        if (!ToolHandlers.TryGetValue(toolName, out var handler))
        {
            throw new NotSupportedException($"Tool {toolName} is not supported by this adapter");
        }
        
        try
        {
            return await handler(parameters, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing tool {ToolName}", toolName);
            throw;
        }
    }
    
    public virtual IEnumerable<string> GetSupportedTools()
    {
        return ToolHandlers.Keys;
    }
}
```

**Usage:**
```csharp
// LakeinTimeManagement.Infrastructure/External/Mcp/LakeinMcpAdapter.cs
public class LakeinMcpAdapter : McpAdapterBase
{
    private readonly IMediator _mediator;
    
    public LakeinMcpAdapter(IMediator mediator, ILogger<LakeinMcpAdapter> logger)
        : base(logger)
    {
        _mediator = mediator;
    }
    
    protected override void RegisterToolHandlers()
    {
        ToolHandlers["create_activity"] = HandleCreateActivity;
        ToolHandlers["complete_activity"] = HandleCompleteActivity;
        ToolHandlers["ask_lakeins_question"] = HandleLakeinsQuestion;
    }
    
    private async Task<object> HandleCreateActivity(
        Dictionary<string, object> parameters,
        CancellationToken ct)
    {
        var command = new CreateActivityCommand(
            (Guid)parameters["user_id"],
            parameters.ContainsKey("goal_id") ? (Guid?)parameters["goal_id"] : null,
            (string)parameters["description"],
            (int)parameters["estimated_minutes"]);
            
        var activityId = await _mediator.Send(command, ct);
        return new { activity_id = activityId };
    }
}
```

## Phase 6: Workflow State Persistence Enhancements (Priority: Low)

### 6.1 Long-Running Workflow Support

**Enhancement:** Support workflows that span days/weeks (e.g., goal-setting exercises).

**New Interface:**
```csharp
// DotNetAgents.Workflow/Persistence/ILongRunningWorkflowStore.cs
public interface ILongRunningWorkflowStore : ICheckpointStore<TState>
{
    Task<List<WorkflowRun<TState>>> GetActiveWorkflowsAsync(
        Guid userId,
        CancellationToken ct = default);
        
    Task<List<WorkflowRun<TState>>> GetWorkflowsByStatusAsync(
        WorkflowStatus status,
        CancellationToken ct = default);
        
    Task ResumeWorkflowAsync(
        string runId,
        CancellationToken ct = default);
}
```

## Phase 7: Agent Composition (Priority: Low)

### 7.1 Composable Agents

**New Interface:**
```csharp
// DotNetAgents.Core/Agents/IComposableAgent.cs
public interface IComposableAgent
{
    Task<TResult> ExecuteAsync<TResult>(
        string operation,
        Dictionary<string, object> parameters,
        CancellationToken ct = default);
        
    void RegisterSubAgent(string name, IAgent subAgent);
}
```

**Usage:**
```csharp
// LakeinTimeManagement.Agents/Composite/LakeinCompositeAgent.cs
public class LakeinCompositeAgent : IComposableAgent
{
    private readonly IGoalAgent _goalAgent;
    private readonly IPriorityAgent _priorityAgent;
    private readonly ISchedulingAgent _schedulingAgent;
    
    public async Task<TResult> ExecuteAsync<TResult>(
        string operation,
        Dictionary<string, object> parameters,
        CancellationToken ct = default)
    {
        return operation switch
        {
            "generate_and_prioritize_activities" => await GenerateAndPrioritize(parameters, ct),
            "plan_and_schedule_day" => await PlanAndSchedule(parameters, ct),
            _ => throw new NotSupportedException()
        };
    }
    
    private async Task<TResult> GenerateAndPrioritize(
        Dictionary<string, object> parameters,
        CancellationToken ct)
    {
        var goalId = (Guid)parameters["goal_id"];
        var activities = await _goalAgent.GenerateActivitiesAsync(goalId, 3, 3, ct);
        
        var prioritized = new List<Activity>();
        foreach (var activity in activities)
        {
            var priority = await _priorityAgent.SuggestPriorityAsync(activity, ct);
            prioritized.Add(activity with { Priority = priority });
        }
        
        return (TResult)(object)prioritized;
    }
}
```

## Implementation Priority

### High Priority (Must Have)
1. ✅ Enhanced Intent Taxonomy (Dynamic Registration)
2. ✅ Dialog Management System
3. ✅ Intent Context Awareness

### Medium Priority (Should Have)
4. Scheduled Command Execution
5. Command Templates
6. Enhanced MCP Adapter Patterns

### Low Priority (Nice to Have)
7. Long-Running Workflow Support
8. Agent Composition

## Files to Create/Modify

### New Packages
- `DotNetAgents.Voice.Dialog` - Dialog management
- `DotNetAgents.Voice.Scheduling` - Scheduled commands

### Modified Packages
- `DotNetAgents.Voice` - Enhanced intent taxonomy, context awareness
- `DotNetAgents.Mcp` - Adapter base classes
- `DotNetAgents.Workflow` - Long-running workflow support

## Timeline

- **Week 1:** Enhanced Intent Taxonomy
- **Week 2:** Dialog Management System
- **Week 3:** Scheduled Commands
- **Week 4:** Command Templates & MCP Enhancements
- **Week 5:** Testing & Documentation

**Total: 5 weeks**

## Dependencies

No new external dependencies required. All enhancements use existing DotNetAgents packages and standard .NET libraries.

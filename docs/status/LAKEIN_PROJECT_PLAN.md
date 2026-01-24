# Lakein Time Management System - Project Plan

**Project Type:** Separate Solution (Sample Application)  
**Purpose:** Demonstrate JARVIS capabilities with a real-world time management application  
**Target Framework:** .NET 10  
**Architecture:** CQRS + Repository Pattern + Blazor Web App

## Executive Summary

This project implements Alan Lakein's time management methodology as a .NET application, powered by DotNetAgents/JARVIS for intelligent task management, prioritization, and user assistance. The system demonstrates how voice commands, agentic workflows, and AI-powered suggestions can enhance traditional time management tools.

## Solution Structure

```
LakeinTimeManagement/
├── src/
│   ├── LakeinTimeManagement.Domain/          # Domain models, entities, value objects
│   ├── LakeinTimeManagement.Application/     # Application layer (CQRS)
│   │   ├── Commands/                         # Command handlers
│   │   ├── Queries/                          # Query handlers
│   │   └── DTOs/                             # Data transfer objects
│   ├── LakeinTimeManagement.Infrastructure/  # Infrastructure layer
│   │   ├── Persistence/                      # Repository implementations
│   │   │   ├── SqlServer/                   # SQL Server repositories
│   │   │   └── PostgreSQL/                  # PostgreSQL repositories
│   │   ├── Messaging/                       # Event bus, SignalR
│   │   └── External/                        # External service integrations
│   ├── LakeinTimeManagement.Web/             # Blazor Server/WebAssembly app
│   │   ├── Components/                      # Blazor components
│   │   ├── Pages/                           # Blazor pages
│   │   ├── Services/                        # Client-side services
│   │   └── Hubs/                            # SignalR hubs (if needed)
│   └── LakeinTimeManagement.Agents/         # Agentic features
│       ├── GoalAgent/                       # Goal management agent
│       ├── ActivityAgent/                   # Activity generation agent
│       ├── PriorityAgent/                   # Priority suggestion agent
│       ├── SchedulingAgent/                  # Scheduling optimization agent
│       └── LakeinQuestionAgent/             # "Best use of time" agent
├── tests/
│   ├── LakeinTimeManagement.Domain.Tests/
│   ├── LakeinTimeManagement.Application.Tests/
│   ├── LakeinTimeManagement.Infrastructure.Tests/
│   └── LakeinTimeManagement.Agents.Tests/
└── LakeinTimeManagement.sln
```

## Phase 1: Foundation & Domain Model (Week 1-2)

### 1.1 Domain Models

**Lifetime Goals**
```csharp
// Domain/Goals/LifetimeGoalsStatement.cs
public class LifetimeGoalsStatement : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastModified { get; private set; }
    
    private List<GoalEntry> _lifetimeGoals = new();
    private List<GoalEntry> _threeYearGoals = new();
    private List<GoalEntry> _sixMonthGoals = new();
    
    public Goal? A1Goal { get; private set; }
    public Goal? A2Goal { get; private set; }
    public Goal? A3Goal { get; private set; }
    
    // Methods for goal management
    public void SetLifetimeGoals(List<GoalEntry> goals) { }
    public void SetTopThreeGoals(Goal a1, Goal a2, Goal a3) { }
    public void ReviseGoals(GoalsRevision revision) { }
}

// Domain/Goals/Goal.cs
public class Goal : Entity
{
    public Guid Id { get; private set; }
    public string Description { get; private set; }
    public GoalCategory Category { get; private set; }
    public PriorityLevel Priority { get; private set; }
    public DateTime CreatedDate { get; private set; }
}
```

**Activities**
```csharp
// Domain/Activities/Activity.cs
public class Activity : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? GoalId { get; private set; }
    public string Description { get; private set; }
    public PriorityLevel Priority { get; private set; }
    public int PriorityNumber { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ScheduledDate { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public ActivityStatus Status { get; private set; }
    public bool IsCommitted { get; private set; }
    private List<string> _tags = new();
    
    // Methods
    public void UpdatePriority(PriorityLevel priority, int number) { }
    public void Schedule(DateTime date) { }
    public void MarkCommitted() { }
    public void Complete() { }
    public void Defer() { }
    public void Eliminate() { }
}
```

**Daily Plans**
```csharp
// Domain/Planning/DailyPlan.cs
public class DailyPlan : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime PlanDate { get; private set; }
    private List<DailyActivity> _activities = new();
    public string? Notes { get; private set; }
    
    // Metrics
    public int TotalActivities => _activities.Count;
    public int CompletedActivities => _activities.Count(a => a.IsCompleted);
    public decimal CompletionScore => CalculateCompletionScore();
    
    // Methods
    public void AddActivity(DailyActivity activity) { }
    public void CompleteActivity(Guid activityId) { }
    public void TransferUncompletedToNextDay() { }
}
```

**Time Blocks**
```csharp
// Domain/Scheduling/TimeBlock.cs
public class TimeBlock : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public TimeBlockType Type { get; private set; }
    public bool IsRecurring { get; private set; }
    public RecurrencePattern? Recurrence { get; private set; }
    private List<Guid> _activityIds = new();
}
```

### 1.2 Value Objects

```csharp
// Domain/Shared/GoalEntry.cs
public record GoalEntry(string Text, GoalCategory Category);

// Domain/Shared/PriorityLevel.cs
public enum PriorityLevel { A, B, C }

// Domain/Shared/ActivityStatus.cs
public enum ActivityStatus
{
    NotStarted,
    InProgress,
    Completed,
    Deferred,
    Eliminated
}

// Domain/Shared/TimeBlockType.cs
public enum TimeBlockType
{
    ATime,
    PrimeTime,
    ExternalPrime,
    TransitionTime,
    Buffer,
    Routine
}
```

### 1.3 Domain Events

```csharp
// Domain/Events/GoalCreatedEvent.cs
public record GoalCreatedEvent(Guid GoalId, Guid UserId, string Description);

// Domain/Events/ActivityCompletedEvent.cs
public record ActivityCompletedEvent(Guid ActivityId, Guid UserId, DateTime CompletedAt);

// Domain/Events/DailyPlanCreatedEvent.cs
public record DailyPlanCreatedEvent(Guid PlanId, Guid UserId, DateTime PlanDate);

// Domain/Events/LakeinQuestionPromptedEvent.cs
public record LakeinQuestionPromptedEvent(Guid UserId, DateTime PromptedAt, string? SuggestedActivity);
```

## Phase 2: CQRS Implementation (Week 3-4)

### 2.1 Commands

**Goal Commands**
```csharp
// Application/Commands/Goals/CreateLifetimeGoalsStatementCommand.cs
public record CreateLifetimeGoalsStatementCommand(
    Guid UserId,
    List<GoalEntry> LifetimeGoals,
    List<GoalEntry> ThreeYearGoals,
    List<GoalEntry> SixMonthGoals) : IRequest<Guid>;

// Application/Commands/Goals/SetTopThreeGoalsCommand.cs
public record SetTopThreeGoalsCommand(
    Guid StatementId,
    Guid A1GoalId,
    Guid A2GoalId,
    Guid A3GoalId) : IRequest;

// Application/Commands/Goals/ReviseGoalsCommand.cs
public record ReviseGoalsCommand(
    Guid StatementId,
    GoalsRevision Revision) : IRequest;
```

**Activity Commands**
```csharp
// Application/Commands/Activities/CreateActivityCommand.cs
public record CreateActivityCommand(
    Guid UserId,
    Guid? GoalId,
    string Description,
    int EstimatedMinutes) : IRequest<Guid>;

// Application/Commands/Activities/UpdateActivityPriorityCommand.cs
public record UpdateActivityPriorityCommand(
    Guid ActivityId,
    PriorityLevel Priority,
    int PriorityNumber) : IRequest;

// Application/Commands/Activities/CompleteActivityCommand.cs
public record CompleteActivityCommand(Guid ActivityId) : IRequest;

// Application/Commands/Activities/GenerateActivitiesFromGoalCommand.cs
public record GenerateActivitiesFromGoalCommand(
    Guid GoalId,
    int BrainstormMinutes = 3,
    int RefinementMinutes = 3) : IRequest<List<Guid>>;
```

**Daily Plan Commands**
```csharp
// Application/Commands/Planning/CreateDailyPlanCommand.cs
public record CreateDailyPlanCommand(
    Guid UserId,
    DateTime PlanDate) : IRequest<Guid>;

// Application/Commands/Planning/AddActivityToDailyPlanCommand.cs
public record AddActivityToDailyPlanCommand(
    Guid PlanId,
    Guid ActivityId) : IRequest;

// Application/Commands/Planning/CompleteDailyActivityCommand.cs
public record CompleteDailyActivityCommand(
    Guid PlanId,
    Guid ActivityId) : IRequest;
```

### 2.2 Queries

```csharp
// Application/Queries/Goals/GetLifetimeGoalsStatementQuery.cs
public record GetLifetimeGoalsStatementQuery(Guid UserId) : IRequest<LifetimeGoalsStatementDto>;

// Application/Queries/Activities/GetActivitiesQuery.cs
public record GetActivitiesQuery(
    Guid UserId,
    ActivityStatus? Status = null,
    PriorityLevel? Priority = null,
    DateTime? ScheduledDate = null) : IRequest<List<ActivityDto>>;

// Application/Queries/Planning/GetDailyPlanQuery.cs
public record GetDailyPlanQuery(
    Guid UserId,
    DateTime Date) : IRequest<DailyPlanDto>;

// Application/Queries/Planning/GetTodaysPlanQuery.cs
public record GetTodaysPlanQuery(Guid UserId) : IRequest<DailyPlanDto>;

// Application/Queries/Analytics/GetTimeManagementMetricsQuery.cs
public record GetTimeManagementMetricsQuery(
    Guid UserId,
    DateTime StartDate,
    DateTime EndDate) : IRequest<TimeManagementMetricsDto>;
```

### 2.3 Command Handlers

```csharp
// Application/Commands/Activities/CreateActivityCommandHandler.cs
public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, Guid>
{
    private readonly IActivityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Guid> Handle(CreateActivityCommand request, CancellationToken ct)
    {
        var activity = new Activity(
            request.UserId,
            request.GoalId,
            request.Description,
            request.EstimatedMinutes);
            
        await _repository.AddAsync(activity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return activity.Id;
    }
}

// Application/Commands/Activities/GenerateActivitiesFromGoalCommandHandler.cs
public class GenerateActivitiesFromGoalCommandHandler : IRequestHandler<GenerateActivitiesFromGoalCommand, List<Guid>>
{
    private readonly IGoalAgent _goalAgent; // Uses DotNetAgents
    private readonly IActivityRepository _repository;
    
    public async Task<List<Guid>> Handle(GenerateActivitiesFromGoalCommand request, CancellationToken ct)
    {
        // Use agent to generate activities
        var activities = await _goalAgent.GenerateActivitiesAsync(
            request.GoalId,
            request.BrainstormMinutes,
            request.RefinementMinutes,
            ct);
            
        // Save activities
        var ids = new List<Guid>();
        foreach (var activity in activities)
        {
            await _repository.AddAsync(activity, ct);
            ids.Add(activity.Id);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        return ids;
    }
}
```

### 2.4 Query Handlers

```csharp
// Application/Queries/Planning/GetTodaysPlanQueryHandler.cs
public class GetTodaysPlanQueryHandler : IRequestHandler<GetTodaysPlanQuery, DailyPlanDto>
{
    private readonly IDailyPlanRepository _repository;
    private readonly IMapper _mapper;
    
    public async Task<DailyPlanDto> Handle(GetTodaysPlanQuery request, CancellationToken ct)
    {
        var plan = await _repository.GetByUserAndDateAsync(
            request.UserId,
            DateTime.Today,
            ct);
            
        if (plan == null)
        {
            // Auto-create if doesn't exist
            plan = await CreateDefaultPlanAsync(request.UserId, ct);
        }
        
        return _mapper.Map<DailyPlanDto>(plan);
    }
}
```

## Phase 3: Infrastructure & Persistence (Week 5-6)

### 3.1 Repository Interfaces

```csharp
// Domain/Repositories/IActivityRepository.cs
public interface IActivityRepository : IRepository<Activity>
{
    Task<List<Activity>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<List<Activity>> GetByPriorityAsync(Guid userId, PriorityLevel priority, CancellationToken ct = default);
    Task<List<Activity>> GetScheduledForDateAsync(Guid userId, DateTime date, CancellationToken ct = default);
    Task<List<Activity>> GetUncommittedAsync(Guid userId, CancellationToken ct = default);
}

// Domain/Repositories/IDailyPlanRepository.cs
public interface IDailyPlanRepository : IRepository<DailyPlan>
{
    Task<DailyPlan?> GetByUserAndDateAsync(Guid userId, DateTime date, CancellationToken ct = default);
    Task<List<DailyPlan>> GetByDateRangeAsync(Guid userId, DateTime start, DateTime end, CancellationToken ct = default);
}
```

### 3.2 SQL Server Implementation

```csharp
// Infrastructure/Persistence/SqlServer/SqlServerActivityRepository.cs
public class SqlServerActivityRepository : IActivityRepository
{
    private readonly LakeinDbContext _context;
    
    public async Task<List<Activity>> GetByPriorityAsync(
        Guid userId,
        PriorityLevel priority,
        CancellationToken ct = default)
    {
        return await _context.Activities
            .Where(a => a.UserId == userId && a.Priority == priority)
            .OrderBy(a => a.PriorityNumber)
            .ToListAsync(ct);
    }
}
```

### 3.3 PostgreSQL Implementation

```csharp
// Infrastructure/Persistence/PostgreSQL/PostgreSQLActivityRepository.cs
public class PostgreSQLActivityRepository : IActivityRepository
{
    private readonly LakeinDbContext _context;
    
    // Similar implementation using Npgsql
}
```

### 3.4 DbContext Configuration

```csharp
// Infrastructure/Persistence/LakeinDbContext.cs
public class LakeinDbContext : DbContext
{
    public DbSet<LifetimeGoalsStatement> LifetimeGoalsStatements { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<DailyPlan> DailyPlans { get; set; }
    public DbSet<TimeBlock> TimeBlocks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(LakeinDbContext).Assembly);
    }
}
```

## Phase 4: Agentic Features (Week 7-8)

### 4.1 Goal Agent

```csharp
// Agents/GoalAgent/IGoalAgent.cs
public interface IGoalAgent
{
    Task<List<Activity>> GenerateActivitiesAsync(
        Guid goalId,
        int brainstormMinutes,
        int refinementMinutes,
        CancellationToken ct = default);
        
    Task<List<Goal>> SuggestGoalConflictsAsync(
        List<Goal> goals,
        CancellationToken ct = default);
        
    Task<Goal> SuggestTopPriorityGoalAsync(
        Guid userId,
        CancellationToken ct = default);
}

// Agents/GoalAgent/GoalAgent.cs
public class GoalAgent : IGoalAgent
{
    private readonly ILLMModel _llm;
    private readonly IGoalRepository _goalRepository;
    private readonly IActivityRepository _activityRepository;
    
    public async Task<List<Activity>> GenerateActivitiesAsync(
        Guid goalId,
        int brainstormMinutes,
        int refinementMinutes,
        CancellationToken ct = default)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId, ct);
        
        // Use LLM to brainstorm activities
        var prompt = $@"
Generate concrete, actionable activities for this goal: {goal.Description}

Brainstorm phase ({brainstormMinutes} minutes):
- List as many activities as possible
- Think creatively, include 'impossible dreams'
- Don't evaluate, just generate

Refinement phase ({refinementMinutes} minutes):
- Break down large activities into smaller steps
- Add variations and alternatives
- Consolidate duplicates
";
        
        var response = await _llm.GenerateAsync(prompt, ct);
        var activities = ParseActivitiesFromResponse(response, goal.UserId, goalId);
        
        return activities;
    }
}
```

### 4.2 Priority Agent

```csharp
// Agents/PriorityAgent/IPriorityAgent.cs
public interface IPriorityAgent
{
    Task<PriorityLevel> SuggestPriorityAsync(
        Activity activity,
        Guid userId,
        CancellationToken ct = default);
        
    Task<List<Activity>> Apply80_20RuleAsync(
        List<Activity> activities,
        CancellationToken ct = default);
        
    Task<int> SuggestPriorityNumberAsync(
        List<Activity> activities,
        PriorityLevel priority,
        CancellationToken ct = default);
}

// Agents/PriorityAgent/PriorityAgent.cs
public class PriorityAgent : IPriorityAgent
{
    private readonly ILLMModel _llm;
    private readonly IGoalRepository _goalRepository;
    
    public async Task<PriorityLevel> SuggestPriorityAsync(
        Activity activity,
        Guid userId,
        CancellationToken ct = default)
    {
        // Use LLM to analyze activity and suggest priority
        var goal = activity.GoalId.HasValue
            ? await _goalRepository.GetByIdAsync(activity.GoalId.Value, ct)
            : null;
            
        var prompt = $@"
Analyze this activity and suggest a priority (A, B, or C):

Activity: {activity.Description}
Goal: {goal?.Description ?? "No specific goal"}
Estimated Time: {activity.EstimatedMinutes} minutes

Consider:
- How directly does this contribute to high-priority goals?
- What is the impact/value?
- Is it time-sensitive?
- Does it align with the user's A-1, A-2, or A-3 goals?

Respond with just: A, B, or C
";
        
        var response = await _llm.GenerateAsync(prompt, ct);
        return ParsePriority(response);
    }
}
```

### 4.3 Lakein Question Agent

```csharp
// Agents/LakeinQuestionAgent/ILakeinQuestionAgent.cs
public interface ILakeinQuestionAgent
{
    Task<LakeinQuestionResponse> AnswerLakeinsQuestionAsync(
        Guid userId,
        DateTime currentTime,
        CancellationToken ct = default);
        
    Task<bool> ShouldPromptAsync(
        Guid userId,
        DateTime currentTime,
        CancellationToken ct = default);
}

// Agents/LakeinQuestionAgent/LakeinQuestionAgent.cs
public class LakeinQuestionAgent : ILakeinQuestionAgent
{
    private readonly ILLMModel _llm;
    private readonly IDailyPlanRepository _dailyPlanRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IGoalRepository _goalRepository;
    
    public async Task<LakeinQuestionResponse> AnswerLakeinsQuestionAsync(
        Guid userId,
        DateTime currentTime,
        CancellationToken ct = default)
    {
        var todaysPlan = await _dailyPlanRepository.GetByUserAndDateAsync(userId, DateTime.Today, ct);
        var a1Goal = await _goalRepository.GetA1GoalAsync(userId, ct);
        var availableActivities = await _activityRepository.GetByPriorityAsync(userId, PriorityLevel.A, ct);
        
        var prompt = $@"
WHAT IS THE BEST USE OF MY TIME RIGHT NOW?

Current Time: {currentTime:HH:mm}
Current Day: {DateTime.Today:dddd, MMMM d}

My A-1 Goal: {a1Goal?.Description ?? "Not set"}

Today's A-Priority Activities:
{string.Join("\n", availableActivities.Select(a => $"- {a.Description} ({a.EstimatedMinutes} min)"))}

Consider:
- What time of day is it? (Prime time? Transition time?)
- What activities are scheduled?
- What's the highest-value activity I can do right now?
- What's realistic given my current context?

Suggest the best activity to do RIGHT NOW and explain why.
";
        
        var response = await _llm.GenerateAsync(prompt, ct);
        return ParseResponse(response);
    }
}
```

### 4.4 Scheduling Agent

```csharp
// Agents/SchedulingAgent/ISchedulingAgent.cs
public interface ISchedulingAgent
{
    Task<List<TimeBlock>> SuggestTimeBlocksAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);
        
    Task<TimeBlock?> SuggestOptimalTimeAsync(
        Guid userId,
        Activity activity,
        CancellationToken ct = default);
        
    Task<List<Activity>> OptimizeScheduleAsync(
        Guid userId,
        DateTime date,
        List<Activity> activities,
        CancellationToken ct = default);
}
```

## Phase 5: Voice Command Integration (Week 9)

### 5.1 Voice Command Handlers

```csharp
// Agents/VoiceCommands/VoiceCommandHandler.cs
public class VoiceCommandHandler
{
    private readonly ICommandWorkflowOrchestrator _orchestrator;
    private readonly IMediator _mediator;
    
    public async Task<CommandState> HandleVoiceCommandAsync(
        Guid userId,
        string commandText,
        CancellationToken ct = default)
    {
        var commandState = new CommandState
        {
            UserId = userId,
            RawText = commandText,
            Source = "voice"
        };
        
        return await _orchestrator.ExecuteAsync(commandState, ct);
    }
}
```

### 5.2 Intent Taxonomy Extensions

```csharp
// Agents/VoiceCommands/LakeinIntentTaxonomy.cs
public static class LakeinIntentTaxonomy
{
    public static void RegisterLakeinIntents(IntentTaxonomy taxonomy)
    {
        // Goals
        taxonomy.RegisterIntent("goals", "create", "lifetime", new[] { "categories" });
        taxonomy.RegisterIntent("goals", "set", "top_three", new[] { "a1", "a2", "a3" });
        
        // Activities
        taxonomy.RegisterIntent("activities", "create", null, new[] { "description", "goal_id", "estimated_minutes" });
        taxonomy.RegisterIntent("activities", "complete", null, new[] { "activity_id" });
        taxonomy.RegisterIntent("activities", "generate", "from_goal", new[] { "goal_id" });
        
        // Daily Planning
        taxonomy.RegisterIntent("planning", "create", "daily", new[] { "date" });
        taxonomy.RegisterIntent("planning", "add", "activity", new[] { "plan_id", "activity_id" });
        
        // Lakein's Question
        taxonomy.RegisterIntent("lakein", "ask", "question", Array.Empty<string>());
        
        // Priority
        taxonomy.RegisterIntent("priority", "set", null, new[] { "activity_id", "priority", "number" });
    }
}
```

### 5.3 MCP Adapter for Lakein Commands

```csharp
// Infrastructure/External/Mcp/LakeinMcpAdapter.cs
public class LakeinMcpAdapter : IMcpAdapter
{
    private readonly IMediator _mediator;
    
    public async Task<object> ExecuteToolAsync(
        string toolName,
        Dictionary<string, object> parameters,
        CancellationToken ct = default)
    {
        return toolName switch
        {
            "create_activity" => await HandleCreateActivity(parameters, ct),
            "complete_activity" => await HandleCompleteActivity(parameters, ct),
            "ask_lakeins_question" => await HandleLakeinsQuestion(parameters, ct),
            _ => throw new NotSupportedException($"Tool {toolName} not supported")
        };
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

## Phase 6: Blazor UI (Week 10-12)

### 6.1 Component Structure

```
Web/Components/
├── Goals/
│   ├── LifetimeGoalsStatement.razor
│   ├── GoalCard.razor
│   └── GoalEditor.razor
├── Activities/
│   ├── ActivityList.razor
│   ├── ActivityCard.razor
│   ├── ActivityEditor.razor
│   └── ActivityGenerator.razor
├── Planning/
│   ├── DailyPlanView.razor
│   ├── DailyActivityCard.razor
│   └── PlanMetrics.razor
├── Scheduling/
│   ├── TimeBlockCalendar.razor
│   └── TimeBlockEditor.razor
├── LakeinQuestion/
│   ├── LakeinQuestionPrompt.razor
│   └── SuggestedActivityCard.razor
└── Voice/
    ├── VoiceCommandInput.razor
    └── CommandStatusDisplay.razor
```

### 6.2 Key Pages

```razor
@* Pages/Dashboard.razor *@
@page "/"
@inject IMediator Mediator
@inject ILakeinQuestionAgent LakeinAgent

<h1>Lakein Time Management</h1>

<LakeinQuestionPrompt UserId="@UserId" />

<DailyPlanView UserId="@UserId" Date="@DateTime.Today" />

<ActivityList UserId="@UserId" Priority="PriorityLevel.A" />

<VoiceCommandInput UserId="@UserId" />
```

### 6.3 SignalR Integration

```csharp
// Web/Hubs/LakeinHub.cs
public class LakeinHub : Hub
{
    public async Task SubscribeToUser(Guid userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }
    
    public async Task SubscribeToCommand(Guid commandId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"command_{commandId}");
    }
}
```

## Phase 7: Testing (Week 13-14)

### 7.1 Unit Tests

- Domain model tests
- Command handler tests
- Query handler tests
- Agent tests
- Repository tests

### 7.2 Integration Tests

- CQRS flow tests
- Database integration tests
- Agent integration tests
- Voice command end-to-end tests

### 7.3 E2E Tests

- Blazor component tests
- User workflow tests
- Voice command workflow tests

## Phase 8: Deployment & Documentation (Week 15)

### 8.1 Deployment

- Docker containerization
- Database migration scripts
- CI/CD pipeline
- Azure/AWS deployment

### 8.2 Documentation

- User guide
- API documentation
- Agent usage guide
- Voice command reference

## Dependencies

### NuGet Packages

```xml
<!-- Core -->
<PackageReference Include="DotNetAgents.Core" Version="1.0.0" />
<PackageReference Include="DotNetAgents.Voice" Version="1.0.0" />
<PackageReference Include="DotNetAgents.Mcp" Version="1.0.0" />
<PackageReference Include="DotNetAgents.Workflow" Version="1.0.0" />
<PackageReference Include="DotNetAgents.Voice.SignalR" Version="1.0.0" />

<!-- CQRS -->
<PackageReference Include="MediatR" Version="12.0.0" />
<PackageReference Include="FluentValidation" Version="11.0.0" />

<!-- Persistence -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
<PackageReference Include="DotNetAgents.Storage.SqlServer" Version="1.0.0" />
<PackageReference Include="DotNetAgents.Storage.PostgreSQL" Version="1.0.0" />

<!-- Blazor -->
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="10.0.0" />

<!-- Other -->
<PackageReference Include="AutoMapper" Version="12.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
```

## Success Criteria

1. ✅ All domain models implemented
2. ✅ CQRS architecture fully functional
3. ✅ Repository pattern with SQL Server and PostgreSQL support
4. ✅ All agentic features working
5. ✅ Voice commands integrated
6. ✅ Blazor UI complete and functional
7. ✅ Comprehensive test coverage (>85%)
8. ✅ Documentation complete

## Timeline Summary

- **Weeks 1-2:** Domain models and foundation
- **Weeks 3-4:** CQRS implementation
- **Weeks 5-6:** Infrastructure and persistence
- **Weeks 7-8:** Agentic features
- **Week 9:** Voice command integration
- **Weeks 10-12:** Blazor UI
- **Weeks 13-14:** Testing
- **Week 15:** Deployment and documentation

**Total: 15 weeks**

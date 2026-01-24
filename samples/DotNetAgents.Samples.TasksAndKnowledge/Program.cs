using DotNetAgents.Core.Models;
using DotNetAgents.Knowledge;
using DotNetAgents.Knowledge.Models;
using DotNetAgents.Providers.OpenAI;
using DotNetAgents.Tasks;
using DotNetAgents.Tasks.Models;
using DotNetAgents.Workflow.Checkpoints;
using DotNetAgents.Workflow.Execution;
using DotNetAgents.Workflow.Graph;
using DotNetAgents.Workflow.Session;
using DotNetAgents.Workflow.Session.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using KnowledgeCategory = DotNetAgents.Knowledge.Models.KnowledgeCategory;
using KnowledgeSeverity = DotNetAgents.Knowledge.Models.KnowledgeSeverity;
using TaskStatus = DotNetAgents.Tasks.Models.TaskStatus;

namespace DotNetAgents.Samples.TasksAndKnowledge;

/// <summary>
/// Sample application demonstrating task management, knowledge capture, and bootstrap generation.
/// This sample shows how to integrate tasks and knowledge with workflows, capture learning,
/// and generate bootstrap payloads for workflow resumption.
/// </summary>
class Program
{
    private record WorkflowState
    {
        public string SessionId { get; init; } = Guid.NewGuid().ToString();
        public string? RunId { get; init; }
        public string Input { get; init; } = string.Empty;
        public string? TaskId { get; init; }
        public string? KnowledgeId { get; init; }
        public string Result { get; init; } = string.Empty;
        public bool HasError { get; init; }
        public string? ErrorMessage { get; init; }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - Tasks and Knowledge Sample");
        Console.WriteLine("==========================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Add OpenAI provider (optional - some features work without it)
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddOpenAI(apiKey, "gpt-4o-mini");
        }
        else
        {
            Console.WriteLine("Note: OPENAI_API_KEY not set. Some LLM features will be skipped.\n");
        }

        // Add Tasks and Knowledge services
        services.AddDotNetAgentsTasks();
        services.AddDotNetAgentsKnowledge();

        // Add Session Management (includes bootstrap generator)
        services.AddDotNetAgentsWorkflowSession();

        var serviceProvider = services.BuildServiceProvider();
        var taskManager = serviceProvider.GetRequiredService<ITaskManager>();
        var knowledgeRepo = serviceProvider.GetRequiredService<IKnowledgeRepository>();
        var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();

        // Create a workflow that demonstrates task management and knowledge capture
        var workflow = WorkflowBuilder<WorkflowState>.Create()
            .AddNode("create_task", async (state, ct) =>
            {
                Console.WriteLine($"  [Create Task] Creating task for session: {state.SessionId}");
                
                var task = await taskManager.CreateTaskAsync(new WorkTask
                {
                    SessionId = state.SessionId,
                    WorkflowRunId = state.RunId,
                    Content = $"Process input: {state.Input}",
                    Priority = TaskPriority.High,
                    Status = TaskStatus.Pending
                }, ct).ConfigureAwait(false);

                Console.WriteLine($"    ✓ Task created: {task.Id}");
                return state with { TaskId = task.Id.ToString() };
            })
            .AddNode("process", async (state, ct) =>
            {
                Console.WriteLine($"  [Process] Processing input...");
                
                // Update task status
                if (!string.IsNullOrEmpty(state.TaskId) && Guid.TryParse(state.TaskId, out var taskId))
                {
                    var task = await taskManager.GetTaskAsync(taskId, ct).ConfigureAwait(false);
                    if (task != null)
                    {
                        await taskManager.UpdateTaskAsync(task with
                        {
                            Status = TaskStatus.InProgress,
                            StartedAt = DateTimeOffset.UtcNow
                        }, ct).ConfigureAwait(false);
                        Console.WriteLine($"    ✓ Task status updated to InProgress");
                    }
                }

                // Simulate processing (with potential error)
                var hasError = state.Input.Contains("error", StringComparison.OrdinalIgnoreCase);
                if (hasError)
                {
                    return state with 
                    { 
                        HasError = true, 
                        ErrorMessage = "Simulated processing error",
                        Result = string.Empty
                    };
                }

                return state with { Result = $"Processed: {state.Input.ToUpper()}" };
            })
            .AddNode("capture_success", async (state, ct) =>
            {
                Console.WriteLine($"  [Capture Success] Recording successful processing...");
                
                var knowledge = await knowledgeRepo.AddKnowledgeAsync(new KnowledgeItem
                {
                    SessionId = state.SessionId,
                    WorkflowRunId = state.RunId,
                    TaskId = Guid.TryParse(state.TaskId ?? "", out var tid) ? tid : null,
                    Title = "Successful processing pattern",
                    Description = $"Successfully processed input: {state.Input}",
                    Solution = $"Result: {state.Result}",
                    Category = KnowledgeCategory.Solution,
                    Severity = KnowledgeSeverity.Info,
                    Tags = new[] { "success", "processing", "workflow" },
                    TechStack = new[] { "dotnet", "dotnetagents" }
                }, ct).ConfigureAwait(false);

                Console.WriteLine($"    ✓ Knowledge captured: {knowledge.Id}");
                return state with { KnowledgeId = knowledge.Id.ToString() };
            })
            .AddNode("capture_error", async (state, ct) =>
            {
                Console.WriteLine($"  [Capture Error] Recording error for future reference...");
                
                var knowledge = await knowledgeRepo.AddKnowledgeAsync(new KnowledgeItem
                {
                    SessionId = state.SessionId,
                    WorkflowRunId = state.RunId,
                    TaskId = Guid.TryParse(state.TaskId ?? "", out var tid) ? tid : null,
                    Title = "Processing error encountered",
                    Description = $"Error occurred while processing: {state.Input}",
                    ErrorMessage = state.ErrorMessage,
                    Category = KnowledgeCategory.Error,
                    Severity = KnowledgeSeverity.Error,
                    Tags = new[] { "error", "processing", "workflow" },
                    TechStack = new[] { "dotnet", "dotnetagents" }
                }, ct).ConfigureAwait(false);

                Console.WriteLine($"    ✓ Error knowledge captured: {knowledge.Id}");
                return state with { KnowledgeId = knowledge.Id.ToString() };
            })
            .AddNode("complete_task", async (state, ct) =>
            {
                Console.WriteLine($"  [Complete Task] Marking task as completed...");
                
                if (!string.IsNullOrEmpty(state.TaskId) && Guid.TryParse(state.TaskId, out var taskId))
                {
                    var task = await taskManager.GetTaskAsync(taskId, ct).ConfigureAwait(false);
                    if (task != null)
                    {
                        await taskManager.UpdateTaskAsync(task with
                        {
                            Status = state.HasError ? TaskStatus.Cancelled : TaskStatus.Completed,
                            CompletedAt = DateTimeOffset.UtcNow,
                            Notes = state.HasError ? $"Error: {state.ErrorMessage}" : "Successfully completed"
                        }, ct).ConfigureAwait(false);
                        Console.WriteLine($"    ✓ Task marked as {(state.HasError ? "Cancelled" : "Completed")}");
                    }
                }

                return state;
            })
            .AddEdge("create_task", "process")
            .AddEdge("process", "capture_success", state => !state.HasError)
            .AddEdge("process", "capture_error", state => state.HasError)
            .AddEdge("capture_success", "complete_task")
            .AddEdge("capture_error", "complete_task")
            .SetEntryPoint("create_task")
            .AddExitPoint("complete_task")
            .Build();

        // Create checkpoint store and serializer
        var checkpointStore = new InMemoryCheckpointStore<WorkflowState>();
        var serializer = new JsonStateSerializer<WorkflowState>();

        // Create executor
        var executor = new GraphExecutor<WorkflowState>(
            workflow,
            checkpointStore,
            serializer);

        // Execute workflow - Success case
        Console.WriteLine("=== Scenario 1: Successful Processing ===\n");
        var successState = new WorkflowState
        {
            Input = "Hello, DotNetAgents!"
        };

        try
        {
            var finalState = await executor.ExecuteAsync(successState, cancellationToken: default).ConfigureAwait(false);
            Console.WriteLine($"\n✓ Workflow completed successfully!");
            Console.WriteLine($"  Result: {finalState.Result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Workflow failed: {ex.Message}");
        }

        // Execute workflow - Error case
        Console.WriteLine("\n=== Scenario 2: Error Handling ===\n");
        var errorState = new WorkflowState
        {
            Input = "This will trigger an error"
        };

        try
        {
            var finalState = await executor.ExecuteAsync(errorState, cancellationToken: default).ConfigureAwait(false);
            Console.WriteLine($"\n✓ Workflow completed (with error captured)!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Workflow failed: {ex.Message}");
        }

        // Display task statistics
        Console.WriteLine("\n=== Task Statistics ===\n");
        var stats = await taskManager.GetTaskStatisticsAsync(successState.SessionId, cancellationToken: default).ConfigureAwait(false);
        Console.WriteLine($"Total Tasks: {stats.Total}");
        Console.WriteLine($"  Pending: {stats.Pending}");
        Console.WriteLine($"  In Progress: {stats.InProgress}");
        Console.WriteLine($"  Completed: {stats.Completed}");
        Console.WriteLine($"  Cancelled: {stats.Cancelled}");
        Console.WriteLine($"Completion Rate: {stats.CompletionPercentage:F1}%");

        // Query knowledge
        Console.WriteLine("\n=== Knowledge Repository ===\n");
        var knowledgeQuery = new KnowledgeQuery
        {
            SessionId = successState.SessionId,
            Page = 1,
            PageSize = 10
        };
        var knowledgeResults = await knowledgeRepo.QueryKnowledgeAsync(knowledgeQuery, cancellationToken: default).ConfigureAwait(false);
        Console.WriteLine($"Total Knowledge Items: {knowledgeResults.TotalCount}");
        foreach (var item in knowledgeResults.Items)
        {
            Console.WriteLine($"  - [{item.Category}] {item.Title} ({item.Severity})");
        }

        // Generate bootstrap payload
        Console.WriteLine("\n=== Bootstrap Generation ===\n");
        try
        {
            var tasks = await taskManager.GetTasksBySessionAsync(successState.SessionId, cancellationToken: default).ConfigureAwait(false);
            var taskSummary = new TaskSummaryData
            {
                Total = stats.Total,
                Pending = stats.Pending,
                InProgress = stats.InProgress,
                Completed = stats.Completed,
                Blocked = stats.Blocked,
                CompletionPercentage = stats.CompletionPercentage,
                Tasks = tasks.Select(t => new TaskItemData
                {
                    Id = t.Id,
                    Content = t.Content,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    Order = t.Order,
                    Tags = t.Tags.ToList(),
                    CompletedAt = t.CompletedAt
                }).ToList()
            };

            var knowledgeItems = knowledgeResults.Items.Select(k => new KnowledgeItemData
            {
                Id = k.Id,
                Title = k.Title,
                Description = k.Description,
                Category = k.Category.ToString(),
                Severity = k.Severity.ToString(),
                Solution = k.Solution,
                Tags = k.Tags.ToList(),
                ReferenceCount = k.ReferenceCount
            }).ToList();

            var bootstrapData = new BootstrapData
            {
                SessionId = successState.SessionId,
                WorkflowRunId = successState.RunId,
                ResumePoint = "Workflow completed successfully",
                SessionName = "Tasks and Knowledge Sample",
                SessionDescription = "Demonstration of task management and knowledge capture",
                TaskSummary = taskSummary,
                KnowledgeItems = knowledgeItems
            };

            var bootstrapGenerator = serviceProvider.GetRequiredService<IBootstrapGenerator>();
            var bootstrap = await bootstrapGenerator.GenerateAsync(
                bootstrapData,
                BootstrapFormat.Json,
                cancellationToken: default).ConfigureAwait(false);

            Console.WriteLine($"✓ Bootstrap payload generated ({bootstrap.Format})");
            Console.WriteLine($"  Size: {bootstrap.FormattedContent.Length} characters");
            Console.WriteLine($"\nPreview:\n{bootstrap.FormattedContent.Substring(0, Math.Min(500, bootstrap.FormattedContent.Length))}...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Bootstrap generation failed: {ex.Message}");
        }

        Console.WriteLine("\n=== Sample Complete ===");
    }
}

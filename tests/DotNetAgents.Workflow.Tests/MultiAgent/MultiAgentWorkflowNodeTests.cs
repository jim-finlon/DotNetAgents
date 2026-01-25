using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Supervisor;
using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.Messaging;
using DotNetAgents.Agents.WorkerPool;
using DotNetAgents.Workflow.Graph;
using DotNetAgents.Workflow.MultiAgent;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using TaskStatus = DotNetAgents.Agents.Tasks.TaskStatus;
using System.Linq;

namespace DotNetAgents.Workflow.Tests.MultiAgent;

/// <summary>
/// Test state for multi-agent workflow tests.
/// </summary>
public class TestMultiAgentState : MultiAgentWorkflowState
{
    public string Input { get; set; } = string.Empty;
    public List<string> ProcessedItems { get; set; } = new();
    public int TotalProcessed { get; set; }
}

/// <summary>
/// Integration tests for multi-agent workflow nodes.
/// </summary>
public class MultiAgentWorkflowNodeTests
{
    private readonly Mock<IAgentRegistry> _mockRegistry;
    private readonly Mock<IAgentMessageBus> _mockMessageBus;
    private readonly Mock<ISupervisorAgent> _mockSupervisor;
    private readonly ILoggerFactory _loggerFactory;

    public MultiAgentWorkflowNodeTests()
    {
        _mockRegistry = new Mock<IAgentRegistry>();
        _mockMessageBus = new Mock<IAgentMessageBus>();
        _mockSupervisor = new Mock<ISupervisorAgent>();
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
    }

    [Fact]
    public async Task DelegateToWorkerNode_WithValidTasks_SubmitsTasksToSupervisor()
    {
        // Arrange
        var state = new TestMultiAgentState
        {
            Input = "test-input"
        };

        var task1 = new WorkerTask
        {
            TaskId = "task-1",
            TaskType = "process",
            Input = new Dictionary<string, object> { ["item"] = "item1" }
        };
        var task2 = new WorkerTask
        {
            TaskId = "task-2",
            TaskType = "process",
            Input = new Dictionary<string, object> { ["item"] = "item2" }
        };

        var tasks = new[] { task1, task2 };
        var taskIds = new[] { "task-1", "task-2" };

        _mockSupervisor
            .Setup(s => s.SubmitTasksAsync(It.IsAny<IEnumerable<WorkerTask>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskIds);

        var node = new DelegateToWorkerNode<TestMultiAgentState>(
            "delegate",
            _mockSupervisor.Object,
            _ => tasks,
            _loggerFactory.CreateLogger<DelegateToWorkerNode<TestMultiAgentState>>());

        // Act
        var result = await node.ExecuteAsync(state, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SubmittedTasks.Should().HaveCount(2);
        result.PendingTaskIds.Should().HaveCount(2);
        result.PendingTaskIds.Should().Contain("task-1");
        result.PendingTaskIds.Should().Contain("task-2");

        _mockSupervisor.Verify(
            s => s.SubmitTasksAsync(It.Is<IEnumerable<WorkerTask>>(t => t.Count() == 2), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DelegateToWorkerNode_WithNoTasks_ReturnsStateUnchanged()
    {
        // Arrange
        var state = new TestMultiAgentState { Input = "test" };

        _mockSupervisor
            .Setup(s => s.SubmitTasksAsync(It.IsAny<IEnumerable<WorkerTask>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        var node = new DelegateToWorkerNode<TestMultiAgentState>(
            "delegate",
            _mockSupervisor.Object,
            _ => Array.Empty<WorkerTask>(),
            _loggerFactory.CreateLogger<DelegateToWorkerNode<TestMultiAgentState>>());

        // Act
        var result = await node.ExecuteAsync(state, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SubmittedTasks.Should().BeEmpty();
        result.PendingTaskIds.Should().BeEmpty();

        _mockSupervisor.Verify(
            s => s.SubmitTasksAsync(It.IsAny<IEnumerable<WorkerTask>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AggregateResultsNode_WithCompletedTasks_AggregatesResults()
    {
        // Arrange
        var state = new TestMultiAgentState
        {
            PendingTaskIds = new List<string> { "task-1", "task-2" },
            CompletedTaskIds = new List<string>()
        };

        var result1 = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = true,
            Output = new Dictionary<string, object> { ["processed"] = "item1" },
            WorkerAgentId = "worker-1"
        };
        var result2 = new WorkerTaskResult
        {
            TaskId = "task-2",
            Success = true,
            Output = new Dictionary<string, object> { ["processed"] = "item2" },
            WorkerAgentId = "worker-2"
        };

        _mockSupervisor
            .Setup(s => s.GetTaskStatusAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaskStatus.Completed);
        _mockSupervisor
            .Setup(s => s.GetTaskStatusAsync("task-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaskStatus.Completed);
        _mockSupervisor
            .Setup(s => s.GetTaskResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);
        _mockSupervisor
            .Setup(s => s.GetTaskResultAsync("task-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2);

        var aggregatedResults = new Dictionary<string, WorkerTaskResult>();

        var node = new AggregateResultsNode<TestMultiAgentState>(
            "aggregate",
            _mockSupervisor.Object,
            (s, results) =>
            {
                aggregatedResults = results;
                s.TotalProcessed = results.Count;
                return s;
            },
            waitForAllTasks: false,
            _loggerFactory.CreateLogger<AggregateResultsNode<TestMultiAgentState>>());

        // Act
        var result = await node.ExecuteAsync(state, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TaskResults.Should().HaveCount(2);
        result.TaskResults.Should().ContainKey("task-1");
        result.TaskResults.Should().ContainKey("task-2");
        result.TotalProcessed.Should().Be(2);
        aggregatedResults.Should().HaveCount(2);
    }

    [Fact]
    public async Task AggregateResultsNode_WithWaitForAllTasks_WaitsForCompletion()
    {
        // Arrange
        var state = new TestMultiAgentState
        {
            PendingTaskIds = new List<string> { "task-1" },
            CompletedTaskIds = new List<string>()
        };

        var result1 = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = true,
            Output = new Dictionary<string, object> { ["processed"] = "item1" },
            WorkerAgentId = "worker-1"
        };

        // First call returns InProgress, second returns Completed
        var callCount = 0;
        _mockSupervisor
            .Setup(s => s.GetTaskStatusAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1 ? TaskStatus.InProgress : TaskStatus.Completed;
            });

        _mockSupervisor
            .Setup(s => s.GetTaskResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);

        var node = new AggregateResultsNode<TestMultiAgentState>(
            "aggregate",
            _mockSupervisor.Object,
            (s, results) =>
            {
                s.TotalProcessed = results.Count;
                return s;
            },
            waitForAllTasks: true,
            _loggerFactory.CreateLogger<AggregateResultsNode<TestMultiAgentState>>());

        // Act
        var result = await node.ExecuteAsync(state, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PendingTaskIds.Should().BeEmpty();
        result.CompletedTaskIds.Should().Contain("task-1");
        result.TaskResults.Should().ContainKey("task-1");
        result.TotalProcessed.Should().Be(1);
    }

    [Fact]
    public async Task AggregateResultsNode_WithFailedTasks_TracksFailures()
    {
        // Arrange
        var state = new TestMultiAgentState
        {
            PendingTaskIds = new List<string> { "task-1", "task-2" },
            CompletedTaskIds = new List<string>()
        };

        var result1 = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = true,
            Output = new Dictionary<string, object> { ["processed"] = "item1" },
            WorkerAgentId = "worker-1"
        };
        var result2 = new WorkerTaskResult
        {
            TaskId = "task-2",
            Success = false,
            ErrorMessage = "Processing failed",
            WorkerAgentId = "worker-2"
        };

        _mockSupervisor
            .Setup(s => s.GetTaskStatusAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaskStatus.Completed);
        _mockSupervisor
            .Setup(s => s.GetTaskStatusAsync("task-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaskStatus.Failed);
        _mockSupervisor
            .Setup(s => s.GetTaskResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);
        _mockSupervisor
            .Setup(s => s.GetTaskResultAsync("task-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2);

        var node = new AggregateResultsNode<TestMultiAgentState>(
            "aggregate",
            _mockSupervisor.Object,
            (s, results) => s,
            waitForAllTasks: false,
            _loggerFactory.CreateLogger<AggregateResultsNode<TestMultiAgentState>>());

        // Act
        var result = await node.ExecuteAsync(state, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PendingTaskIds.Should().BeEmpty();
        result.CompletedTaskIds.Should().Contain("task-1");
        result.FailedTaskIds.Should().Contain("task-2");
        result.TaskResults.Should().HaveCount(2);
        result.TaskResults["task-1"].Success.Should().BeTrue();
        result.TaskResults["task-2"].Success.Should().BeFalse();
    }

    [Fact]
    public async Task Workflow_WithDelegateAndAggregateNodes_CompletesSuccessfully()
    {
        // Arrange
        var state = new TestMultiAgentState { Input = "test" };

        var task1 = new WorkerTask
        {
            TaskId = "task-1",
            TaskType = "process",
            Input = new Dictionary<string, object> { ["item"] = "item1" }
        };
        var task2 = new WorkerTask
        {
            TaskId = "task-2",
            TaskType = "process",
            Input = new Dictionary<string, object> { ["item"] = "item2" }
        };

        var tasks = new[] { task1, task2 };
        var taskIds = new[] { "task-1", "task-2" };

        var result1 = new WorkerTaskResult
        {
            TaskId = "task-1",
            Success = true,
            Output = new Dictionary<string, object> { ["processed"] = "item1" },
            WorkerAgentId = "worker-1"
        };
        var result2 = new WorkerTaskResult
        {
            TaskId = "task-2",
            Success = true,
            Output = new Dictionary<string, object> { ["processed"] = "item2" },
            WorkerAgentId = "worker-2"
        };

        _mockSupervisor
            .Setup(s => s.SubmitTasksAsync(It.IsAny<IEnumerable<WorkerTask>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskIds);
        _mockSupervisor
            .Setup(s => s.GetTaskStatusAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaskStatus.Completed);
        _mockSupervisor
            .Setup(s => s.GetTaskStatusAsync("task-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaskStatus.Completed);
        _mockSupervisor
            .Setup(s => s.GetTaskResultAsync("task-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);
        _mockSupervisor
            .Setup(s => s.GetTaskResultAsync("task-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2);

        var delegateNode = new DelegateToWorkerNode<TestMultiAgentState>(
            "delegate",
            _mockSupervisor.Object,
            _ => tasks,
            _loggerFactory.CreateLogger<DelegateToWorkerNode<TestMultiAgentState>>());

        var aggregateNode = new AggregateResultsNode<TestMultiAgentState>(
            "aggregate",
            _mockSupervisor.Object,
            (s, results) =>
            {
                s.ProcessedItems = results.Values
                    .Where(r => r.Success && r.Output != null)
                    .Select(r => r.Output is Dictionary<string, object> dict && dict.TryGetValue("processed", out var value)
                        ? value?.ToString() ?? string.Empty
                        : string.Empty)
                    .ToList();
                s.TotalProcessed = s.ProcessedItems.Count;
                return s;
            },
            waitForAllTasks: false,
            _loggerFactory.CreateLogger<AggregateResultsNode<TestMultiAgentState>>());

        // Act
        var afterDelegate = await delegateNode.ExecuteAsync(state, CancellationToken.None);
        var finalState = await aggregateNode.ExecuteAsync(afterDelegate, CancellationToken.None);

        // Assert
        finalState.Should().NotBeNull();
        finalState.SubmittedTasks.Should().HaveCount(2);
        finalState.TaskResults.Should().HaveCount(2);
        finalState.ProcessedItems.Should().HaveCount(2);
        finalState.ProcessedItems.Should().Contain("item1");
        finalState.ProcessedItems.Should().Contain("item2");
        finalState.TotalProcessed.Should().Be(2);
    }
}

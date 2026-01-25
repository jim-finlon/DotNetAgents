using Microsoft.Extensions.Logging;

namespace DotNetAgents.Agents.Tasks;

/// <summary>
/// In-memory implementation of <see cref="ITaskQueue"/>.
/// Suitable for single-instance deployments.
/// </summary>
public class InMemoryTaskQueue : ITaskQueue
{
    private readonly ILogger<InMemoryTaskQueue>? _logger;
    private readonly SortedSet<QueuedTask> _queue = new(new TaskPriorityComparer());
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryTaskQueue"/> class.
    /// </summary>
    /// <param name="logger">Optional logger instance.</param>
    public InMemoryTaskQueue(ILogger<InMemoryTaskQueue>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task EnqueueAsync(
        WorkerTask task,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var queuedTask = new QueuedTask(task, DateTimeOffset.UtcNow);
            _queue.Add(queuedTask);

            _logger?.LogDebug(
                "Enqueued task {TaskId} of type {TaskType} with priority {Priority}",
                task.TaskId,
                task.TaskType,
                task.Priority);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<WorkerTask?> DequeueAsync(
        string? agentId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (_queue.Count == 0)
            {
                return Task.FromResult<WorkerTask?>(null);
            }

            QueuedTask? queuedTask = null;

            if (agentId != null)
            {
                // Find a task that matches the agent's preferences or capabilities
                queuedTask = _queue.FirstOrDefault(qt =>
                    qt.Task.PreferredAgentId == agentId ||
                    string.IsNullOrEmpty(qt.Task.RequiredCapability));
            }

            // If no specific match found, get the highest priority task
            queuedTask ??= _queue.Max;

            if (queuedTask != null)
            {
                _queue.Remove(queuedTask);
                _logger?.LogDebug(
                    "Dequeued task {TaskId} of type {TaskType}",
                    queuedTask.Task.TaskId,
                    queuedTask.Task.TaskType);
                return Task.FromResult<WorkerTask?>(queuedTask.Task);
            }

            return Task.FromResult<WorkerTask?>(null);
        }
    }

    /// <inheritdoc />
    public Task<int> GetPendingCountAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            return Task.FromResult(_queue.Count);
        }
    }

    /// <inheritdoc />
    public Task<WorkerTask?> PeekAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (_queue.Count == 0)
            {
                return Task.FromResult<WorkerTask?>(null);
            }

            var queuedTask = _queue.Max;
            return Task.FromResult<WorkerTask?>(queuedTask?.Task);
        }
    }

    private sealed class QueuedTask
    {
        public WorkerTask Task { get; }
        public DateTimeOffset EnqueuedAt { get; }

        public QueuedTask(WorkerTask task, DateTimeOffset enqueuedAt)
        {
            Task = task;
            EnqueuedAt = enqueuedAt;
        }
    }

    private sealed class TaskPriorityComparer : IComparer<QueuedTask>
    {
        public int Compare(QueuedTask? x, QueuedTask? y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            // Compare by priority (higher priority first)
            var priorityComparison = y.Task.Priority.CompareTo(x.Task.Priority);
            if (priorityComparison != 0)
                return priorityComparison;

            // If priorities are equal, compare by creation time (older first)
            return x.EnqueuedAt.CompareTo(y.EnqueuedAt);
        }
    }
}

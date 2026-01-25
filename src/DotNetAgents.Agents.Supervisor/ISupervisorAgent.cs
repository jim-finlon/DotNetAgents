using DotNetAgents.Agents.Tasks;
using TaskStatus = DotNetAgents.Agents.Tasks.TaskStatus;

namespace DotNetAgents.Agents.Supervisor;

/// <summary>
/// Supervisor agent that delegates tasks to worker agents.
/// </summary>
public interface ISupervisorAgent
{
    /// <summary>
    /// Submits a task to be executed by a worker agent.
    /// </summary>
    /// <param name="task">The task to submit.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The ID of the submitted task.</returns>
    Task<string> SubmitTaskAsync(
        WorkerTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits multiple tasks for parallel execution.
    /// </summary>
    /// <param name="tasks">The tasks to submit.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The IDs of the submitted tasks.</returns>
    Task<IReadOnlyList<string>> SubmitTasksAsync(
        IEnumerable<WorkerTask> tasks,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a task.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The status of the task.</returns>
    Task<TaskStatus> GetTaskStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the result of a completed task.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The task result, or null if the task is not completed.</returns>
    Task<WorkerTaskResult?> GetTaskResultAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending or running task.
    /// </summary>
    /// <param name="taskId">The ID of the task to cancel.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the task was cancelled, false if it was not found or already completed.</returns>
    Task<bool> CancelTaskAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about task execution.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Statistics about supervisor task execution.</returns>
    Task<SupervisorStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
}

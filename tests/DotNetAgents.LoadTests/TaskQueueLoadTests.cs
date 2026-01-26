using DotNetAgents.Agents.Tasks;
using NBomber.CSharp;

namespace DotNetAgents.LoadTests;

/// <summary>
/// Load tests for task queue operations.
/// </summary>
public class TaskQueueLoadTests
{
    private static ITaskQueue? _taskQueue;

    public static void Setup(ITaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    public static void Run()
    {
        var enqueueScenario = Scenario.Create("task_queue_enqueue", async context =>
        {
            var step = Step.Create("enqueue_task", async context =>
            {
                if (_taskQueue == null)
                {
                    return Response.Fail("Task queue not initialized");
                }

                var task = new WorkerTask
                {
                    TaskId = Guid.NewGuid().ToString(),
                    TaskType = "load-test",
                    Priority = TaskPriority.Normal,
                    Input = new Dictionary<string, object> { ["test"] = "data" }
                };

                try
                {
                    await _taskQueue.EnqueueAsync(task, context.CancellationToken);
                    return Response.Ok();
                }
                catch (Exception ex)
                {
                    return Response.Fail(ex.Message);
                }
            });

            return await step;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 200, during: TimeSpan.FromMinutes(2))
        );

        var dequeueScenario = Scenario.Create("task_queue_dequeue", async context =>
        {
            var step = Step.Create("dequeue_task", async context =>
            {
                if (_taskQueue == null)
                {
                    return Response.Fail("Task queue not initialized");
                }

                try
                {
                    var task = await _taskQueue.DequeueAsync(cancellationToken: context.CancellationToken);
                    return task != null ? Response.Ok() : Response.Fail("No task available");
                }
                catch (Exception ex)
                {
                    return Response.Fail(ex.Message);
                }
            });

            return await step;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(2))
        );

        NBomberRunner
            .RegisterScenarios(enqueueScenario, dequeueScenario)
            .Run();
    }
}

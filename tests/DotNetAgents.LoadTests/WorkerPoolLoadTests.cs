using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.WorkerPool;
using NBomber.CSharp;

namespace DotNetAgents.LoadTests;

/// <summary>
/// Load tests for worker pool operations.
/// </summary>
public class WorkerPoolLoadTests
{
    private static IWorkerPool? _workerPool;

    public static void Setup(IWorkerPool workerPool)
    {
        _workerPool = workerPool;
    }

    public static void Run()
    {
        var getWorkerScenario = Scenario.Create("worker_pool_get_worker", async context =>
        {
            var step = Step.Create("get_available_worker", async context =>
            {
                if (_workerPool == null)
                {
                    return Response.Fail("Worker pool not initialized");
                }

                try
                {
                    var worker = await _workerPool.GetAvailableWorkerAsync(
                        requiredCapability: null,
                        context.CancellationToken);

                    return worker != null ? Response.Ok() : Response.Fail("No worker available");
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
            Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromMinutes(1))
        );

        var statisticsScenario = Scenario.Create("worker_pool_statistics", async context =>
        {
            var step = Step.Create("get_statistics", async context =>
            {
                if (_workerPool == null)
                {
                    return Response.Fail("Worker pool not initialized");
                }

                try
                {
                    var stats = await _workerPool.GetStatisticsAsync(context.CancellationToken);
                    return Response.Ok(sizeBytes: 100); // Approximate response size
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
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(getWorkerScenario, statisticsScenario)
            .Run();
    }
}

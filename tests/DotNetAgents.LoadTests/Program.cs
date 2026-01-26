using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.WorkerPool;
using DotNetAgents.Agents.Messaging.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

namespace DotNetAgents.LoadTests;

/// <summary>
/// Main entry point for load testing suite.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents Load Testing Suite");
        Console.WriteLine("================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddInMemoryAgentRegistry();
        services.AddInMemoryAgentMessageBus();
        services.AddInMemoryTaskQueue();
        services.AddWorkerPool();

        var serviceProvider = services.BuildServiceProvider();
        var taskQueue = serviceProvider.GetRequiredService<ITaskQueue>();
        var workerPool = serviceProvider.GetRequiredService<IWorkerPool>();

        // Setup test data
        await SetupTestDataAsync(serviceProvider);

        // Setup load tests
        TaskQueueLoadTests.Setup(taskQueue);
        WorkerPoolLoadTests.Setup(workerPool);

        // Parse command line arguments
        var testType = args.Length > 0 ? args[0] : "all";

        switch (testType.ToLower())
        {
            case "registry":
                AgentRegistryLoadTests.Run();
                break;
            case "taskqueue":
                TaskQueueLoadTests.Run();
                break;
            case "workerpool":
                WorkerPoolLoadTests.Run();
                break;
            case "all":
            default:
                Console.WriteLine("Running all load tests...\n");
                AgentRegistryLoadTests.Run();
                await Task.Delay(5000); // Brief pause between tests
                TaskQueueLoadTests.Run();
                await Task.Delay(5000);
                WorkerPoolLoadTests.Run();
                break;
        }

        Console.WriteLine("\nLoad testing completed!");
    }

    private static async Task SetupTestDataAsync(IServiceProvider serviceProvider)
    {
        var registry = serviceProvider.GetRequiredService<IAgentRegistry>();

        // Register test agents
        for (int i = 1; i <= 10; i++)
        {
            var capabilities = new AgentCapabilities
            {
                AgentId = $"worker-{i}",
                AgentType = "test-worker",
                SupportedTools = new[] { "tool1", "tool2" },
                MaxConcurrentTasks = 5
            };

            await registry.RegisterAsync(capabilities);
        }

        var workerPool = serviceProvider.GetRequiredService<IWorkerPool>();
        for (int i = 1; i <= 10; i++)
        {
            await workerPool.AddWorkerAsync($"worker-{i}");
        }

        Console.WriteLine("Test data setup complete: 10 workers registered\n");
    }
}

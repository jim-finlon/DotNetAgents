using DotNetAgents.Agents.Registry;
using DotNetAgents.Agents.Tasks;
using DotNetAgents.Agents.Supervisor;
using DotNetAgents.Agents.WorkerPool;
using DotNetAgents.Agents.Messaging.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.ChaosTests;

/// <summary>
/// Test fixture for chaos engineering tests.
/// </summary>
public class ChaosTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public ChaosTestFixture()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddInMemoryAgentRegistry();
        services.AddInMemoryAgentMessageBus();
        services.AddInMemoryTaskQueue();
        services.AddWorkerPool();
        services.AddSupervisorAgent();

        ServiceProvider = services.BuildServiceProvider();

        // Setup test agents
        SetupTestAgentsAsync().GetAwaiter().GetResult();
    }

    private async Task SetupTestAgentsAsync()
    {
        var registry = ServiceProvider.GetRequiredService<IAgentRegistry>();
        var workerPool = ServiceProvider.GetRequiredService<IWorkerPool>();

        // Register 10 test agents
        for (int i = 1; i <= 10; i++)
        {
            var capabilities = new AgentCapabilities
            {
                AgentId = $"worker-{i}",
                AgentType = "test-worker",
                SupportedTools = new[] { "tool1", "tool2" },
                SupportedIntents = new[] { "process", "calculate" },
                MaxConcurrentTasks = 5
            };

            await registry.RegisterAsync(capabilities);
            await workerPool.AddWorkerAsync($"worker-{i}");
        }
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

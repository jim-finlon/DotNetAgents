using DotNetAgents.Agents.Registry;
using Microsoft.Extensions.DependencyInjection;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

namespace DotNetAgents.LoadTests;

/// <summary>
/// Load tests for agent registry operations.
/// </summary>
public class AgentRegistryLoadTests
{
    public static void Run()
    {
        var scenario = Scenario.Create("agent_registry_register", async context =>
        {
            var step = Step.Create("register_agent", async context =>
            {
                // Simulate agent registration
                var capabilities = new AgentCapabilities
                {
                    AgentId = $"agent-{Guid.NewGuid()}",
                    AgentType = "test-agent",
                    SupportedTools = new[] { "tool1", "tool2" },
                    MaxConcurrentTasks = 5
                };

                // In a real test, this would call the actual registry
                await Task.Delay(10); // Simulate network call

                return Response.Ok();
            });

            return await step;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}

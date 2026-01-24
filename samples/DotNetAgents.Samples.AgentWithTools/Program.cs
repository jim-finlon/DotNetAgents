using DotNetAgents.Abstractions.Agents;
using DotNetAgents.Core.Agents;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.Prompts;
using DotNetAgents.Core.Prompts;
using DotNetAgents.Abstractions.Tools;
using DotNetAgents.Core.Tools;
using DotNetAgents.Tools.BuiltIn;
using DotNetAgents.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.AgentWithTools;

/// <summary>
/// Example demonstrating an agent using tools to answer questions and perform actions.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - Agent with Tools Example");
        Console.WriteLine("=======================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Add OpenAI provider (requires OPENAI_API_KEY environment variable)
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("ERROR: OPENAI_API_KEY environment variable is not set.");
            Console.WriteLine("Please set it to your OpenAI API key to run this example.");
            return;
        }

        services.AddOpenAI(apiKey, "gpt-3.5-turbo");

        services.AddHttpClient(); // Add HTTP client factory
        var serviceProvider = services.BuildServiceProvider();
        var llm = serviceProvider.GetRequiredService<ILLMModel<string, string>>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        // Create tool registry and register built-in tools
        var toolRegistry = new ToolRegistry();
        
        // Register some useful tools
        toolRegistry.Register(new CalculatorTool());
        toolRegistry.Register(new DateTimeTool());
        toolRegistry.Register(new WebSearchTool(httpClient));
        toolRegistry.Register(new WikipediaSearchTool(httpClient));

        // Create ReAct prompt template
        var promptTemplate = new ReActPromptTemplate();

        // Create agent executor
        var agent = new AgentExecutor(
            llm,
            toolRegistry,
            promptTemplate,
            maxIterations: 5);

        Console.WriteLine("Agent is ready with the following tools:");
        foreach (var tool in toolRegistry.GetAllTools())
        {
            Console.WriteLine($"  - {tool.Name}: {tool.Description}");
        }

        Console.WriteLine("\n");

        // Example 1: Math question
        Console.WriteLine("Example 1: Math Question");
        Console.WriteLine("-------------------------");
        await RunAgentExample(agent, "What is 15 * 23 + 42?").ConfigureAwait(false);

        Console.WriteLine("\n");

        // Example 2: Current date/time
        Console.WriteLine("Example 2: Current Date/Time");
        Console.WriteLine("----------------------------");
        await RunAgentExample(agent, "What is the current date and time in UTC?").ConfigureAwait(false);

        Console.WriteLine("\n");

        // Example 3: Web search
        Console.WriteLine("Example 3: Web Search");
        Console.WriteLine("---------------------");
        await RunAgentExample(agent, "Search for information about the latest developments in AI").ConfigureAwait(false);

        Console.WriteLine("\nExample completed!");
    }

    private static async Task RunAgentExample(AgentExecutor agent, string question)
    {
        Console.WriteLine($"Question: {question}");
        Console.WriteLine("Agent thinking...\n");

        try
        {
            var result = await agent.InvokeAsync(question, cancellationToken: default).ConfigureAwait(false);
            Console.WriteLine($"\nFinal Answer: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
        }
    }
}
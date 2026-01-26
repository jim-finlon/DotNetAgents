using DotNetAgents.Abstractions.Chains;
using DotNetAgents.Core.Chains;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Abstractions.Prompts;
using DotNetAgents.Core.Prompts;
using DotNetAgents.Ecosystem;
using DotNetAgents.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.BasicChain;

/// <summary>
/// Basic example demonstrating chain composition with LLM and prompt templates.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - Basic Chain Example");
        Console.WriteLine("==================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddDotNetAgentsEcosystem(); // Enable plugin system

        // Add OpenAI provider (requires OPENAI_API_KEY environment variable)
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("ERROR: OPENAI_API_KEY environment variable is not set.");
            Console.WriteLine("Please set it to your OpenAI API key to run this example.");
            return;
        }

        services.AddOpenAI(apiKey, "gpt-3.5-turbo");

        var serviceProvider = services.BuildServiceProvider();
        var llm = serviceProvider.GetRequiredService<ILLMModel<string, string>>();

        // Example 1: Simple LLM Chain
        Console.WriteLine("Example 1: Simple LLM Chain");
        Console.WriteLine("---------------------------");
        await RunSimpleChainExample(llm).ConfigureAwait(false);

        Console.WriteLine("\n");

        // Example 2: Chain with Prompt Template
        Console.WriteLine("Example 2: Chain with Prompt Template");
        Console.WriteLine("--------------------------------------");
        await RunPromptTemplateChainExample(llm).ConfigureAwait(false);

        Console.WriteLine("\n");

        // Example 3: Sequential Chain
        Console.WriteLine("Example 3: Sequential Chain");
        Console.WriteLine("----------------------------");
        await RunSequentialChainExample(llm).ConfigureAwait(false);

        Console.WriteLine("\nExample completed!");
    }

    private static async Task RunSimpleChainExample(ILLMModel<string, string> llm)
    {
        // Create a simple chain that just calls the LLM
        var chain = new Runnable<string, string>(async (input, ct) =>
        {
            var response = await llm.GenerateAsync(input, cancellationToken: ct).ConfigureAwait(false);
            return response;
        });

        var result = await chain.InvokeAsync("What is the capital of France?", cancellationToken: default).ConfigureAwait(false);
        Console.WriteLine($"Question: What is the capital of France?");
        Console.WriteLine($"Answer: {result}");
    }

    private static async Task RunPromptTemplateChainExample(ILLMModel<string, string> llm)
    {
        // Create a prompt template
        var template = new PromptTemplate("You are a helpful assistant. Answer the following question:\n\nQuestion: {question}\n\nAnswer:");

        // Create a chain that formats the prompt and calls the LLM
        // Note: ChainBuilder expects string input/output, so we'll format the prompt manually
        var chain = new Runnable<Dictionary<string, object>, string>(async (variables, ct) =>
        {
            var formattedPrompt = await template.FormatAsync(variables, ct).ConfigureAwait(false);
            var result = await llm.GenerateAsync(formattedPrompt, cancellationToken: ct).ConfigureAwait(false);
            return result;
        });

        var variables = new Dictionary<string, object>
        {
            ["question"] = "Explain quantum computing in simple terms."
        };

        var result = await chain.InvokeAsync(variables, options: null, cancellationToken: default).ConfigureAwait(false);
        Console.WriteLine($"Question: Explain quantum computing in simple terms.");
        Console.WriteLine($"Answer: {result}");
    }

    private static async Task RunSequentialChainExample(ILLMModel<string, string> llm)
    {
        // Create a chain that:
        // 1. Generates a summary
        // 2. Generates a follow-up question
        // 3. Answers the follow-up question

        var summaryChain = new Runnable<string, string>(async (input, ct) =>
        {
            var prompt = $"Summarize the following text in one sentence:\n\n{input}";
            return await llm.GenerateAsync(prompt, cancellationToken: ct).ConfigureAwait(false);
        });

        var questionChain = new Runnable<string, string>(async (input, ct) =>
        {
            var prompt = $"Based on this summary, generate one follow-up question:\n\nSummary: {input}\n\nQuestion:";
            return await llm.GenerateAsync(prompt, cancellationToken: ct).ConfigureAwait(false);
        });

        var answerChain = new Runnable<string, string>(async (input, ct) =>
        {
            var prompt = $"Answer this question briefly:\n\n{input}";
            return await llm.GenerateAsync(prompt, cancellationToken: ct).ConfigureAwait(false);
        });

        // Compose chains sequentially
        var sequentialChain = summaryChain
            .Pipe(questionChain)
            .Pipe(answerChain);

        var inputText = "Artificial intelligence is transforming industries by automating tasks, " +
                       "enabling predictive analytics, and creating new possibilities for innovation.";

        var result = await sequentialChain.InvokeAsync(inputText, cancellationToken: default).ConfigureAwait(false);

        Console.WriteLine($"Input: {inputText}");
        Console.WriteLine($"\nFinal Result: {result}");
    }
}
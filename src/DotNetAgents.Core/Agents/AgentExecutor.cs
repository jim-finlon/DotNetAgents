using DotNetAgents.Core.Chains;
using DotNetAgents.Core.Exceptions;
using DotNetAgents.Core.Memory;
using DotNetAgents.Core.Models;
using DotNetAgents.Core.Prompts;
using DotNetAgents.Core.Tools;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DotNetAgents.Core.Agents;

/// <summary>
/// Executes an agent with tool-calling capabilities using the ReAct pattern.
/// </summary>
public class AgentExecutor : IRunnable<string, string>
{
    private readonly ILLMModel<string, string> _llm;
    private readonly IToolRegistry _toolRegistry;
    private readonly IPromptTemplate _promptTemplate;
    private readonly IMemory? _memory;
    private readonly int _maxIterations;
    private readonly string _stopSequence;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentExecutor"/> class.
    /// </summary>
    /// <param name="llm">The LLM model to use.</param>
    /// <param name="toolRegistry">The registry of available tools.</param>
    /// <param name="promptTemplate">The prompt template for the agent.</param>
    /// <param name="memory">Optional memory for conversation history.</param>
    /// <param name="maxIterations">Maximum number of iterations before stopping (default: 10).</param>
    /// <param name="stopSequence">Sequence that indicates the agent should stop (default: "Final Answer:").</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public AgentExecutor(
        ILLMModel<string, string> llm,
        IToolRegistry toolRegistry,
        IPromptTemplate promptTemplate,
        IMemory? memory = null,
        int maxIterations = 10,
        string stopSequence = "Final Answer:")
    {
        _llm = llm ?? throw new ArgumentNullException(nameof(llm));
        _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
        _promptTemplate = promptTemplate ?? throw new ArgumentNullException(nameof(promptTemplate));
        _memory = memory;
        
        if (maxIterations <= 0)
            throw new ArgumentException("MaxIterations must be positive.", nameof(maxIterations));
        
        _maxIterations = maxIterations;
        _stopSequence = stopSequence ?? throw new ArgumentNullException(nameof(stopSequence));
    }

    /// <inheritdoc/>
    public async Task<string> InvokeAsync(
        string input,
        RunnableOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));

        var iteration = 0;
        var conversationHistory = new List<string>();

        // Add memory context if available
        if (_memory != null)
        {
            var recentMessages = await _memory.GetMessagesAsync(5, cancellationToken).ConfigureAwait(false);
            foreach (var message in recentMessages)
            {
                conversationHistory.Add($"{message.Role}: {message.Content}");
            }
        }

        var currentInput = input;

        while (iteration < _maxIterations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            iteration++;

            // Build prompt with tools and conversation history
            var promptVariables = BuildPromptVariables(currentInput, conversationHistory);
            var formattedPrompt = await _promptTemplate.FormatAsync(promptVariables, cancellationToken).ConfigureAwait(false);

            // Call LLM
            var response = await _llm.GenerateAsync(formattedPrompt, cancellationToken: cancellationToken).ConfigureAwait(false);

            // Check for stop sequence
            if (response.Contains(_stopSequence, StringComparison.OrdinalIgnoreCase))
            {
                var finalAnswer = ExtractFinalAnswer(response);
                
                // Save to memory if available
                if (_memory != null)
                {
                    await _memory.AddMessageAsync(new Memory.MemoryMessage
                    {
                        Content = input,
                        Role = "user"
                    }, cancellationToken).ConfigureAwait(false);

                    await _memory.AddMessageAsync(new Memory.MemoryMessage
                    {
                        Content = finalAnswer,
                        Role = "assistant"
                    }, cancellationToken).ConfigureAwait(false);
                }

                return finalAnswer;
            }

            // Try to parse tool call
            var toolCall = ParseToolCall(response);
            if (toolCall != null)
            {
                var tool = _toolRegistry.GetTool(toolCall.ToolName);
                if (tool == null)
                {
                    conversationHistory.Add($"Agent: {response}");
                    conversationHistory.Add($"System: Tool '{toolCall.ToolName}' not found.");
                    currentInput = $"Tool '{toolCall.ToolName}' not found. Please try again.";
                    continue;
                }

                // Execute tool
                ToolResult toolResult;
                try
                {
                    toolResult = await tool.ExecuteAsync(toolCall.Arguments, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    toolResult = ToolResult.Failure($"Tool execution failed: {ex.Message}");
                }

                // Add to conversation history
                conversationHistory.Add($"Agent: {response}");
                var argsString = toolCall.Arguments is JsonElement json ? json.ToString() : toolCall.Arguments.ToString() ?? "{}";
                conversationHistory.Add($"Tool: {toolCall.ToolName}({argsString})");
                var resultString = toolResult.IsSuccess 
                    ? (toolResult.Output?.ToString() ?? "Success")
                    : (toolResult.ErrorMessage ?? "Unknown error");
                conversationHistory.Add($"Tool Result: {resultString}");

                // Continue with tool result as input
                var toolOutput = toolResult.IsSuccess 
                    ? (toolResult.Output?.ToString() ?? "Success")
                    : (toolResult.ErrorMessage ?? "Unknown error");
                currentInput = $"Tool '{toolCall.ToolName}' returned: {toolOutput}";
            }
            else
            {
                // No tool call detected, add response to history and continue
                conversationHistory.Add($"Agent: {response}");
                currentInput = response;
            }
        }

        throw new AgentException(
            $"Agent exceeded maximum iterations ({_maxIterations}).",
            ErrorCategory.WorkflowError);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> StreamAsync(
        string input,
        RunnableOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // For streaming, we'll just yield the final result
        var result = await InvokeAsync(input, options, cancellationToken).ConfigureAwait(false);
        yield return result;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> BatchAsync(
        IEnumerable<string> inputs,
        RunnableOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (inputs == null)
            throw new ArgumentNullException(nameof(inputs));

        var results = new List<string>();
        foreach (var input in inputs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await InvokeAsync(input, options, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }

    private IDictionary<string, object> BuildPromptVariables(string input, List<string> conversationHistory)
    {
        var variables = new Dictionary<string, object>
        {
            ["input"] = input,
            ["tools"] = FormatToolsForPrompt(),
            ["tool_names"] = string.Join(", ", _toolRegistry.GetAllTools().Select(t => t.Name)),
            ["conversation_history"] = conversationHistory.Count > 0 
                ? string.Join("\n", conversationHistory) 
                : string.Empty
        };

        return variables;
    }

    private string FormatToolsForPrompt()
    {
        var toolDescriptions = _toolRegistry.GetAllTools()
            .Select(tool => $"- {tool.Name}: {tool.Description}")
            .ToList();

        return string.Join("\n", toolDescriptions);
    }

    private static string ExtractFinalAnswer(string response)
    {
        var stopIndex = response.IndexOf("Final Answer:", StringComparison.OrdinalIgnoreCase);
        if (stopIndex >= 0)
        {
            return response.Substring(stopIndex + "Final Answer:".Length).Trim();
        }

        return response;
    }

    private static ToolCall? ParseToolCall(string response)
    {
        // Simple pattern matching for tool calls
        // Format: Action: ToolName
        // Action Input: {json}
        var actionPattern = @"Action:\s*(\w+)";
        var actionInputPattern = @"Action Input:\s*(.+?)(?:\n|$)";

        var actionMatch = Regex.Match(response, actionPattern, RegexOptions.IgnoreCase);
        if (!actionMatch.Success)
        {
            return null;
        }

        var toolName = actionMatch.Groups[1].Value;
        var inputMatch = Regex.Match(response, actionInputPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        object arguments;
        if (inputMatch.Success)
        {
            var inputText = inputMatch.Groups[1].Value.Trim();
            // Try to parse as JSON
            try
            {
                arguments = JsonSerializer.Deserialize<JsonElement>(inputText);
            }
            catch
            {
                // If not valid JSON, wrap in a simple object
                arguments = JsonSerializer.Deserialize<JsonElement>($"{{\"input\": \"{inputText.Replace("\"", "\\\"", StringComparison.Ordinal)}\"}}");
            }
        }
        else
        {
            arguments = JsonSerializer.Deserialize<JsonElement>("{}");
        }

        return new ToolCall(toolName, arguments);
    }

    private sealed record ToolCall(string ToolName, object Arguments);
}
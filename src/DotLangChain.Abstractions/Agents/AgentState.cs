using DotLangChain.Abstractions.LLM;

namespace DotLangChain.Abstractions.Agents;

/// <summary>
/// Base state for agent graphs.
/// </summary>
public abstract class AgentState
{
    public List<ChatMessage> Messages { get; init; } = new();
    public Dictionary<string, object?> Values { get; init; } = new();
    public string? CurrentNode { get; set; }
    public int StepCount { get; set; }
}


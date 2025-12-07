namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// A tool/function call from the LLM.
/// </summary>
public sealed record ToolCall
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Arguments { get; init; } // JSON string
}


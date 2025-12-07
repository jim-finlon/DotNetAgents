namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// Streaming chunk from completion.
/// </summary>
public sealed record ChatCompletionChunk
{
    public string? ContentDelta { get; init; }
    public ToolCall? ToolCallDelta { get; init; }
    public string? FinishReason { get; init; }
}


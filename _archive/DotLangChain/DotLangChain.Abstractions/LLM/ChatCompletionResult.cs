namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// Result from a chat completion.
/// </summary>
public sealed record ChatCompletionResult
{
    public required ChatMessage Message { get; init; }
    public string? FinishReason { get; init; }
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens => PromptTokens + CompletionTokens;
    public string? Model { get; init; }
    public TimeSpan Duration { get; init; }
}


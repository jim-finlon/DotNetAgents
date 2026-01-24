using System.Text.Json;

namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// Options for chat completion requests.
/// </summary>
public sealed record ChatCompletionOptions
{
    public string? Model { get; init; }
    public float? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public float? TopP { get; init; }
    public float? FrequencyPenalty { get; init; }
    public float? PresencePenalty { get; init; }
    public IReadOnlyList<ToolDefinition>? Tools { get; init; }
    public string? ToolChoice { get; init; } // "auto", "none", or specific tool name
    public JsonElement? ResponseFormat { get; init; }
    public IReadOnlyList<string>? Stop { get; init; }
    public string? User { get; init; }
    public int? Seed { get; init; }
}


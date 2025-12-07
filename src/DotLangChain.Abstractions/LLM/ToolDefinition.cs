using System.Text.Json;

namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// Tool definition for function calling.
/// </summary>
public sealed record ToolDefinition
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required JsonElement ParametersSchema { get; init; }
    public bool Strict { get; init; } = false;
}


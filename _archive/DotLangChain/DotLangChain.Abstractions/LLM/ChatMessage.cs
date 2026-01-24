namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// A message in a conversation.
/// </summary>
public record ChatMessage
{
    public required MessageRole Role { get; init; }
    public required string Content { get; init; }
    public string? Name { get; init; }
    public string? ToolCallId { get; init; }
    public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
    public IReadOnlyList<ContentPart>? ContentParts { get; init; }

    /// <summary>
    /// Creates a system message.
    /// </summary>
    public static ChatMessage System(string content) => new()
    {
        Role = MessageRole.System,
        Content = content
    };

    /// <summary>
    /// Creates a user message.
    /// </summary>
    public static ChatMessage User(string content) => new()
    {
        Role = MessageRole.User,
        Content = content
    };

    /// <summary>
    /// Creates an assistant message.
    /// </summary>
    public static ChatMessage Assistant(string content) => new()
    {
        Role = MessageRole.Assistant,
        Content = content
    };

    /// <summary>
    /// Creates a tool message.
    /// </summary>
    public static ChatMessage Tool(string toolCallId, string content) => new()
    {
        Role = MessageRole.Tool,
        ToolCallId = toolCallId,
        Content = content
    };
}

/// <summary>
/// Content part for multimodal messages (text + images).
/// </summary>
public sealed record ContentPart
{
    public required string Type { get; init; } // "text" or "image_url"
    public required string Content { get; init; }
}


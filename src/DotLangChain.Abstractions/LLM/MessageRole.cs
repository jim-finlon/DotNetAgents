namespace DotLangChain.Abstractions.LLM;

/// <summary>
/// Role in a conversation.
/// </summary>
public enum MessageRole
{
    /// <summary>
    /// System instructions.
    /// </summary>
    System,

    /// <summary>
    /// User message.
    /// </summary>
    User,

    /// <summary>
    /// Assistant response.
    /// </summary>
    Assistant,

    /// <summary>
    /// Tool/function result.
    /// </summary>
    Tool
}


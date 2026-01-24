using DotLangChain.Abstractions.Common;

namespace DotLangChain.Core.Exceptions;

/// <summary>
/// Exception thrown when tool execution fails.
/// </summary>
public class ToolException : DotLangChainException
{
    /// <summary>
    /// Gets the tool name that caused the error, if applicable.
    /// </summary>
    public string? ToolName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolException"/> class.
    /// </summary>
    public ToolException(string message, string? toolName = null, Exception? innerException = null)
        : base(message, "DLC006", innerException)
    {
        ToolName = toolName;
    }

    /// <summary>
    /// Creates an exception for tool not found.
    /// </summary>
    public static ToolException ToolNotFound(string toolName)
    {
        return new ToolException(
            $"Tool '{toolName}' not found",
            toolName: toolName)
        {
            ErrorCode = "DLC006-001"
        };
    }

    /// <summary>
    /// Creates an exception for invalid tool parameters.
    /// </summary>
    public static ToolException InvalidParameters(string toolName, string reason)
    {
        return new ToolException(
            $"Invalid parameters for tool '{toolName}': {reason}",
            toolName: toolName)
        {
            ErrorCode = "DLC006-002",
            Context = new Dictionary<string, object?> { ["reason"] = reason }
        };
    }
}


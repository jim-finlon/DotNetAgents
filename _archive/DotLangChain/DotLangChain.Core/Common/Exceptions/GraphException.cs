using DotLangChain.Abstractions.Common;

namespace DotLangChain.Core.Exceptions;

/// <summary>
/// Exception thrown when graph execution fails.
/// </summary>
public class GraphException : DotLangChainException
{
    /// <summary>
    /// Gets the node name where the error occurred, if applicable.
    /// </summary>
    public string? NodeName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphException"/> class.
    /// </summary>
    public GraphException(string message, string? nodeName = null, Exception? innerException = null)
        : base(message, "DLC005", innerException)
    {
        NodeName = nodeName;
    }

    /// <summary>
    /// Creates an exception for max steps exceeded.
    /// </summary>
    public static GraphException MaxStepsExceeded(int maxSteps, int actualSteps)
    {
        return new GraphException(
            $"Graph execution exceeded maximum steps: {actualSteps} steps (max: {maxSteps})")
        {
            ErrorCode = "DLC005-001",
            Context = new Dictionary<string, object?>
            {
                ["max_steps"] = maxSteps,
                ["actual_steps"] = actualSteps
            }
        };
    }

    /// <summary>
    /// Creates an exception for node not found.
    /// </summary>
    public static GraphException NodeNotFound(string nodeName)
    {
        return new GraphException(
            $"Graph node '{nodeName}' not found",
            nodeName: nodeName)
        {
            ErrorCode = "DLC005-002"
        };
    }

    /// <summary>
    /// Creates an exception for invalid state transition.
    /// </summary>
    public static GraphException InvalidStateTransition(string fromNode, string toNode, string? reason = null)
    {
        return new GraphException(
            $"Invalid state transition from '{fromNode}' to '{toNode}'" + (reason != null ? $": {reason}" : ""))
        {
            ErrorCode = "DLC005-003",
            Context = new Dictionary<string, object?>
            {
                ["from_node"] = fromNode,
                ["to_node"] = toNode,
                ["reason"] = reason ?? "Unknown"
            }
        };
    }
}


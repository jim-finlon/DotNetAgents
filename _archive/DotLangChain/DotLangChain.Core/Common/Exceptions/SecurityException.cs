using DotLangChain.Abstractions.Common;

namespace DotLangChain.Core.Exceptions;

/// <summary>
/// Exception thrown when security violations are detected.
/// </summary>
public class SecurityException : DotLangChainException
{
    /// <summary>
    /// Gets the type of security violation.
    /// </summary>
    public string? ViolationType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityException"/> class.
    /// </summary>
    public SecurityException(string message, string? violationType = null, Exception? innerException = null)
        : base(message, "DLC007", innerException)
    {
        ViolationType = violationType;
    }

    /// <summary>
    /// Creates an exception for potential injection detected.
    /// </summary>
    public static SecurityException PotentialInjectionDetected(string? input = null)
    {
        return new SecurityException(
            "Potential prompt injection detected in input",
            violationType: "PromptInjection")
        {
            ErrorCode = "DLC007-001",
            Context = input != null ? new Dictionary<string, object?> { ["input_length"] = input.Length } : null
        };
    }

    /// <summary>
    /// Creates an exception for invalid credentials.
    /// </summary>
    public static SecurityException InvalidCredentials(string? resource = null)
    {
        return new SecurityException(
            $"Invalid credentials" + (resource != null ? $" for {resource}" : ""),
            violationType: "InvalidCredentials")
        {
            ErrorCode = "DLC007-002",
            Context = resource != null ? new Dictionary<string, object?> { ["resource"] = resource } : null
        };
    }
}


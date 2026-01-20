namespace DotNetAgents.Core.OutputParsers;

/// <summary>
/// Interface for parsing LLM output into structured formats.
/// </summary>
/// <typeparam name="T">The type to parse the output into.</typeparam>
public interface IOutputParser<T>
{
    /// <summary>
    /// Parses the raw output from an LLM into a structured format.
    /// </summary>
    /// <param name="output">The raw output string from the LLM.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The parsed output.</returns>
    /// <exception cref="ParsingException">Thrown when parsing fails.</exception>
    Task<T> ParseAsync(string output, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets instructions for the LLM on how to format its output.
    /// </summary>
    /// <returns>Format instructions as a string.</returns>
    string GetFormatInstructions();
}
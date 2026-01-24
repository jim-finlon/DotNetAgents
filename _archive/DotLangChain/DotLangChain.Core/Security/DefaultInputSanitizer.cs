using DotLangChain.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DotLangChain.Core.Security;

/// <summary>
/// Sanitizes user input to prevent prompt injection.
/// </summary>
public interface IInputSanitizer
{
    /// <summary>
    /// Sanitizes input based on the specified level.
    /// </summary>
    string Sanitize(string input, SanitizationLevel level = SanitizationLevel.Standard);

    /// <summary>
    /// Checks if input contains potential injection patterns.
    /// </summary>
    bool ContainsPotentialInjection(string input);
}

/// <summary>
/// Level of sanitization to apply.
/// </summary>
public enum SanitizationLevel
{
    /// <summary>
    /// Minimal sanitization (basic cleanup).
    /// </summary>
    Minimal,

    /// <summary>
    /// Standard sanitization (removes common injection patterns).
    /// </summary>
    Standard,

    /// <summary>
    /// Strict sanitization (aggressive filtering).
    /// </summary>
    Strict
}

/// <summary>
/// Default implementation of input sanitization.
/// </summary>
public sealed class DefaultInputSanitizer : IInputSanitizer
{
    private static readonly System.Text.RegularExpressions.Regex InjectionPatterns = new(
        @"(ignore\s+(previous|all|above)\s+instructions)|" +
        @"(system\s*:)|" +
        @"(you\s+are\s+now)|" +
        @"(pretend\s+to\s+be)|" +
        @"(disregard\s+(your|all))|" +
        @"(\[INST\])|(\[/INST\])|" +
        @"(<\|im_start\|>)|(<\|im_end\|>)|" +
        @"(###\s*(System|User|Assistant))",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex ControlCharacters = new(
        @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    private readonly ILogger<DefaultInputSanitizer>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultInputSanitizer"/> class.
    /// </summary>
    public DefaultInputSanitizer(ILogger<DefaultInputSanitizer>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string Sanitize(string input, SanitizationLevel level = SanitizationLevel.Standard)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        var result = input;

        // Always remove control characters
        result = ControlCharacters.Replace(result, "");

        if (level >= SanitizationLevel.Standard)
        {
            result = NormalizeUnicode(result);
            result = EscapeInjectionMarkers(result);
        }

        if (level == SanitizationLevel.Strict)
        {
            if (InjectionPatterns.IsMatch(result))
            {
                _logger?.LogWarning("Strict sanitization detected and filtered potential injection in input");
            }
            result = InjectionPatterns.Replace(result, "[FILTERED]");
        }

        return result;
    }

    /// <inheritdoc/>
    public bool ContainsPotentialInjection(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return InjectionPatterns.IsMatch(input);
    }

    private static string NormalizeUnicode(string input)
    {
        return input.Normalize(System.Text.NormalizationForm.FormKC);
    }

    private static string EscapeInjectionMarkers(string input)
    {
        return input
            .Replace("```", "` ` `")
            .Replace("---", "- - -")
            .Replace("###", "# # #");
    }
}


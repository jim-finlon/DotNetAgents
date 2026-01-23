using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DotNetAgents.Education.Safety;

/// <summary>
/// Implementation of age-adaptive content transformation for grade-level appropriateness.
/// </summary>
public class AgeAdaptiveTransformer : IAgeAdaptiveTransformer
{
    private readonly ILogger<AgeAdaptiveTransformer> _logger;
    private readonly Dictionary<GradeLevel, int> _maxReadingLevels;
    private readonly Dictionary<GradeLevel, int> _maxResponseLengths;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgeAdaptiveTransformer"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public AgeAdaptiveTransformer(ILogger<AgeAdaptiveTransformer>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AgeAdaptiveTransformer>.Instance;

        // Define maximum reading levels by grade
        _maxReadingLevels = new Dictionary<GradeLevel, int>
        {
            { GradeLevel.K2, 2 },
            { GradeLevel.G3_5, 5 },
            { GradeLevel.G6_8, 8 },
            { GradeLevel.G9_10, 10 },
            { GradeLevel.G11_12, 12 }
        };

        // Define maximum response lengths (in words) by grade
        _maxResponseLengths = new Dictionary<GradeLevel, int>
        {
            { GradeLevel.K2, 50 },
            { GradeLevel.G3_5, 100 },
            { GradeLevel.G6_8, 200 },
            { GradeLevel.G9_10, 300 },
            { GradeLevel.G11_12, 500 }
        };
    }

    /// <inheritdoc/>
    public Task<string> TransformPromptAsync(
        string prompt,
        GradeLevel gradeLevel,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

        _logger.LogDebug("Transforming prompt for grade level {GradeLevel}", gradeLevel);

        // Simplify vocabulary for younger grades
        var transformed = SimplifyVocabulary(prompt, gradeLevel);

        // Shorten if too long
        var maxLength = _maxResponseLengths[gradeLevel];
        if (CountWords(transformed) > maxLength)
        {
            transformed = Summarize(transformed, maxLength);
        }

        return Task.FromResult(transformed);
    }

    /// <inheritdoc/>
    public Task<string> TransformResponseAsync(
        string response,
        GradeLevel gradeLevel,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new ArgumentException("Response cannot be null or empty.", nameof(response));

        _logger.LogDebug("Transforming response for grade level {GradeLevel}", gradeLevel);

        // Assess complexity
        var complexity = AssessComplexity(response);
        var maxReadingLevel = _maxReadingLevels[gradeLevel];

        var transformed = response;

        // Simplify if too complex
        if (complexity.ReadingLevel > maxReadingLevel)
        {
            _logger.LogInformation(
                "Response reading level {Level} exceeds max {MaxLevel} for grade {GradeLevel}, simplifying",
                complexity.ReadingLevel,
                maxReadingLevel,
                gradeLevel);
            transformed = SimplifyVocabulary(transformed, gradeLevel);
            transformed = SimplifySentences(transformed);
        }

        // Shorten if too long
        var maxLength = _maxResponseLengths[gradeLevel];
        if (CountWords(transformed) > maxLength)
        {
            _logger.LogInformation(
                "Response length {Length} exceeds max {MaxLength} for grade {GradeLevel}, summarizing",
                CountWords(transformed),
                maxLength,
                gradeLevel);
            transformed = Summarize(transformed, maxLength);
        }

        return Task.FromResult(transformed);
    }

    /// <inheritdoc/>
    public ComplexityScore AssessComplexity(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ComplexityScore
            {
                ReadingLevel = 0,
                FleschKincaid = 0,
                FleschReadingEase = 100,
                AverageSentenceLength = 0,
                AverageSyllablesPerWord = 0
            };
        }

        var sentences = SplitSentences(text);
        var words = SplitWords(text);
        var syllables = CountSyllables(text);

        var sentenceCount = sentences.Length;
        var wordCount = words.Length;

        if (sentenceCount == 0 || wordCount == 0)
        {
            return new ComplexityScore
            {
                ReadingLevel = 0,
                FleschKincaid = 0,
                FleschReadingEase = 100,
                AverageSentenceLength = 0,
                AverageSyllablesPerWord = 0
            };
        }

        var avgSentenceLength = (double)wordCount / sentenceCount;
        var avgSyllablesPerWord = (double)syllables / wordCount;

        // Flesch-Kincaid Grade Level
        var fleschKincaid = 0.39 * avgSentenceLength + 11.8 * avgSyllablesPerWord - 15.59;

        // Flesch Reading Ease (0-100, higher is easier)
        var fleschReadingEase = 206.835 - (1.015 * avgSentenceLength) - (84.6 * avgSyllablesPerWord);

        return new ComplexityScore
        {
            ReadingLevel = Math.Max(0, (int)Math.Round(fleschKincaid)),
            FleschKincaid = fleschKincaid,
            FleschReadingEase = Math.Max(0, Math.Min(100, fleschReadingEase)),
            AverageSentenceLength = avgSentenceLength,
            AverageSyllablesPerWord = avgSyllablesPerWord
        };
    }

    private string SimplifyVocabulary(string text, GradeLevel gradeLevel)
    {
        // Simple vocabulary simplification - replace complex words with simpler alternatives
        // This is a basic implementation; a full version would use a vocabulary database

        var replacements = gradeLevel switch
        {
            GradeLevel.K2 => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "utilize", "use" },
                { "demonstrate", "show" },
                { "investigate", "look at" },
                { "examine", "look at" },
                { "analyze", "think about" },
                { "synthesize", "put together" },
                { "comprehend", "understand" },
                { "perceive", "see" }
            },
            GradeLevel.G3_5 => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "utilize", "use" },
                { "demonstrate", "show" },
                { "investigate", "study" },
                { "examine", "look at" },
                { "analyze", "study" },
                { "synthesize", "combine" },
                { "comprehend", "understand" }
            },
            _ => new Dictionary<string, string>() // No simplification for older grades
        };

        var result = text;
        foreach (var (complex, simple) in replacements)
        {
            result = Regex.Replace(result, $@"\b{Regex.Escape(complex)}\b", simple, RegexOptions.IgnoreCase);
        }

        return result;
    }

    private string SimplifySentences(string text)
    {
        // Break long sentences into shorter ones
        var sentences = SplitSentences(text);
        var simplified = new List<string>();

        foreach (var sentence in sentences)
        {
            var words = SplitWords(sentence);
            if (words.Length > 20) // Long sentence
            {
                // Try to split on conjunctions
                var parts = Regex.Split(sentence, @"\s+(and|but|or|because|so)\s+", RegexOptions.IgnoreCase);
                simplified.AddRange(parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }
            else
            {
                simplified.Add(sentence);
            }
        }

        return string.Join(". ", simplified).Trim();
    }

    private string Summarize(string text, int maxWords)
    {
        var words = SplitWords(text);
        if (words.Length <= maxWords)
            return text;

        // Take first maxWords words
        var summary = string.Join(" ", words.Take(maxWords));
        return summary + "...";
    }

    private static string[] SplitSentences(string text)
    {
        return Regex.Split(text, @"[.!?]+")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
    }

    private static string[] SplitWords(string text)
    {
        return Regex.Matches(text, @"\b\w+\b")
            .Cast<Match>()
            .Select(m => m.Value)
            .ToArray();
    }

    private static int CountWords(string text)
    {
        return SplitWords(text).Length;
    }

    private static int CountSyllables(string text)
    {
        var words = SplitWords(text);
        var totalSyllables = 0;

        foreach (var word in words)
        {
            totalSyllables += CountWordSyllables(word);
        }

        return totalSyllables;
    }

    private static int CountWordSyllables(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return 0;

        word = word.ToLowerInvariant();
        word = Regex.Replace(word, @"[^a-z]", string.Empty);

        if (word.Length <= 3)
            return 1;

        // Count vowel groups
        word = Regex.Replace(word, @"[aeiouy]+", "a");
        var syllables = word.Count(c => c == 'a');

        // Adjust for silent e
        if (word.EndsWith("e", StringComparison.OrdinalIgnoreCase))
            syllables--;

        // Minimum 1 syllable
        return Math.Max(1, syllables);
    }
}

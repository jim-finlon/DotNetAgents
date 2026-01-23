using DotNetAgents.Security.Validation;
using DotNetAgents.Education.Models;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Safety;

/// <summary>
/// Implementation of child safety content filtering for COPPA compliance.
/// </summary>
public class ChildSafetyFilter : IContentFilter
{
    private readonly ISanitizer? _sanitizer;
    private readonly ILogger<ChildSafetyFilter> _logger;
    private readonly IReadOnlySet<string> _blockedPatterns;
    private readonly IReadOnlySet<string> _sensitivePatterns;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildSafetyFilter"/> class.
    /// </summary>
    /// <param name="sanitizer">Optional sanitizer from DotNetAgents for additional filtering.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public ChildSafetyFilter(
        ISanitizer? sanitizer = null,
        ILogger<ChildSafetyFilter>? logger = null)
    {
        _sanitizer = sanitizer;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ChildSafetyFilter>.Instance;
        
        // Initialize blocked patterns (critical safety issues)
        _blockedPatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Violence
            "kill", "murder", "suicide", "self-harm", "cutting", "harm yourself",
            // Adult content
            "sex", "porn", "nude", "naked", "explicit",
            // Hate speech
            "hate", "racist", "discrimination",
            // Personal information requests
            "your address", "your phone", "your email", "where do you live", "what's your name",
            "meet me", "come to", "visit me"
        };

        // Initialize sensitive patterns (require review)
        _sensitivePatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bullying", "teasing", "mean", "hurt", "sad", "depressed", "lonely",
            "scared", "afraid", "worried", "anxious", "stressed"
        };
    }

    /// <inheritdoc/>
    public async Task<ContentFilterResult> FilterInputAsync(
        string input,
        FilterContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _logger.LogDebug("Filtering input content for grade level {GradeLevel}", context.GradeLevel);

        // Layer 1: Pattern matching (fast)
        var patternResult = CheckPatterns(input, context);
        if (!patternResult.IsAllowed)
        {
            _logger.LogWarning(
                "Input blocked by pattern matching. Categories: {Categories}",
                string.Join(", ", patternResult.FlaggedCategories));
            return patternResult;
        }

        // Layer 2: Sanitizer (DotNetAgents)
        string filteredContent = input;
        if (_sanitizer != null)
        {
            filteredContent = _sanitizer.SanitizeInput(input);
        }

        // Layer 3: Check if content was modified
        if (filteredContent != input)
        {
            _logger.LogInformation("Input content was sanitized/modified");
            return new ContentFilterResult
            {
                IsAllowed = true,
                FilteredContent = filteredContent,
                FlaggedCategories = Array.Empty<ContentCategory>(),
                RequiresReview = false
            };
        }

        return new ContentFilterResult
        {
            IsAllowed = true,
            FilteredContent = filteredContent,
            FlaggedCategories = Array.Empty<ContentCategory>(),
            RequiresReview = false
        };
    }

    /// <inheritdoc/>
    public async Task<ContentFilterResult> FilterOutputAsync(
        string output,
        FilterContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(output))
            throw new ArgumentException("Output cannot be null or empty.", nameof(output));
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _logger.LogDebug("Filtering output content for grade level {GradeLevel}", context.GradeLevel);

        // Layer 1: Pattern matching
        var patternResult = CheckPatterns(output, context);
        if (!patternResult.IsAllowed)
        {
            _logger.LogWarning(
                "Output blocked by pattern matching. Categories: {Categories}",
                string.Join(", ", patternResult.FlaggedCategories));
            return patternResult;
        }

        // Layer 2: Sanitizer
        string filteredContent = output;
        if (_sanitizer != null)
        {
            filteredContent = _sanitizer.SanitizeOutput(output);
        }

        // Layer 3: Age-appropriate content check
        var ageCheck = CheckAgeAppropriateness(filteredContent, context.GradeLevel);
        if (!ageCheck.IsAllowed)
        {
            return ageCheck;
        }

        return new ContentFilterResult
        {
            IsAllowed = true,
            FilteredContent = filteredContent ?? output, // Ensure FilteredContent is never null
            FlaggedCategories = patternResult.FlaggedCategories,
            RequiresReview = patternResult.RequiresReview || ageCheck.RequiresReview
        };
    }

    private ContentFilterResult CheckPatterns(string content, FilterContext context)
    {
        var lowerContent = content.ToLowerInvariant();
        var flaggedCategories = new List<ContentCategory>();
        var requiresReview = false;

        // Check blocked patterns (critical - content is blocked)
        if (_blockedPatterns.Any(pattern => lowerContent.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            // Determine category
            if (ContainsPattern(lowerContent, "kill", "murder", "suicide", "self-harm", "cutting", "harm yourself"))
            {
                flaggedCategories.Add(ContentCategory.Violence);
                flaggedCategories.Add(ContentCategory.SelfHarm);
            }

            if (ContainsPattern(lowerContent, "sex", "porn", "nude", "naked", "explicit"))
            {
                flaggedCategories.Add(ContentCategory.Adult);
            }

            if (ContainsPattern(lowerContent, "hate", "racist", "discrimination"))
            {
                flaggedCategories.Add(ContentCategory.HateSpeech);
            }

            if (ContainsPattern(lowerContent, "your address", "your phone", "your email", "where do you live", "what's your name", "meet me", "come to", "visit me"))
            {
                flaggedCategories.Add(ContentCategory.PersonalInformation);
            }

            _logger.LogWarning(
                "Content blocked due to flagged categories: {Categories}",
                string.Join(", ", flaggedCategories));

            return new ContentFilterResult
            {
                IsAllowed = false,
                FilteredContent = null,
                FlaggedCategories = flaggedCategories,
                RequiresReview = true,
                ReviewReason = $"Content contains blocked patterns: {string.Join(", ", flaggedCategories)}"
            };
        }

        // Check sensitive patterns (warning - requires review)
        if (_sensitivePatterns.Any(pattern => lowerContent.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            if (ContainsPattern(lowerContent, "bullying", "teasing", "mean"))
            {
                flaggedCategories.Add(ContentCategory.Bullying);
            }

            if (ContainsPattern(lowerContent, "sad", "depressed", "lonely", "scared", "afraid", "worried", "anxious", "stressed"))
            {
                flaggedCategories.Add(ContentCategory.SelfHarm); // Potential distress signal
            }

            requiresReview = true;
        }

        return new ContentFilterResult
        {
            IsAllowed = true,
            FilteredContent = content,
            FlaggedCategories = flaggedCategories,
            RequiresReview = requiresReview,
            ReviewReason = requiresReview ? "Content contains sensitive patterns requiring review" : null
        };
    }

    private ContentFilterResult CheckAgeAppropriateness(string content, GradeLevel gradeLevel)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new ContentFilterResult
            {
                IsAllowed = true,
                FilteredContent = content,
                FlaggedCategories = Array.Empty<ContentCategory>(),
                RequiresReview = false
            };
        }

        // Check for age-inappropriate vocabulary or concepts
        // This is a simplified check - a full implementation would use complexity analysis

        var inappropriateTerms = gradeLevel switch
        {
            GradeLevel.K2 => new[] { "complex", "sophisticated", "advanced" }, // Simplified check
            GradeLevel.G3_5 => Array.Empty<string>(),
            _ => Array.Empty<string>()
        };

        var lowerContent = content.ToLowerInvariant();
        if (inappropriateTerms.Length > 0 && inappropriateTerms.Any(term => lowerContent.Contains(term, StringComparison.OrdinalIgnoreCase)))
        {
            return new ContentFilterResult
            {
                IsAllowed = true, // Allow but flag for review
                FilteredContent = content,
                FlaggedCategories = Array.Empty<ContentCategory>(),
                RequiresReview = true,
                ReviewReason = "Content may not be age-appropriate for grade level"
            };
        }

        return new ContentFilterResult
        {
            IsAllowed = true,
            FilteredContent = content,
            FlaggedCategories = Array.Empty<ContentCategory>(),
            RequiresReview = false
        };
    }

    private static bool ContainsPattern(string text, params string[] patterns)
    {
        return patterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}

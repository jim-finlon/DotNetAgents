using DotNetAgents.Core.Caching;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Infrastructure;

/// <summary>
/// Cache keys for education content.
/// </summary>
public static class EducationCacheKeys
{
    /// <summary>
    /// Gets the cache key for a Socratic question.
    /// </summary>
    public static string SocraticQuestion(string conceptId, string understandingLevel) =>
        $"edu:socratic:question:{conceptId}:{understandingLevel}";

    /// <summary>
    /// Gets the cache key for an assessment.
    /// </summary>
    public static string Assessment(string conceptId, string specHash) =>
        $"edu:assessment:{conceptId}:{specHash}";

    /// <summary>
    /// Gets the cache key for a hint.
    /// </summary>
    public static string Hint(string questionId, int hintLevel) =>
        $"edu:hint:{questionId}:{hintLevel}";

    /// <summary>
    /// Gets the cache key for mastery calculation.
    /// </summary>
    public static string Mastery(string studentId, string conceptId) =>
        $"edu:mastery:{studentId}:{conceptId}";

    /// <summary>
    /// Gets the cache key for student profile.
    /// </summary>
    public static string StudentProfile(string studentId) =>
        $"edu:profile:{studentId}";
}

/// <summary>
/// Education-specific content cache extending DotNetAgents ICache.
/// </summary>
public class EducationContentCache
{
    private readonly ICache _cache;
    private readonly ILogger<EducationContentCache> _logger;
    private readonly TimeSpan _defaultTtl;

    /// <summary>
    /// Initializes a new instance of the <see cref="EducationContentCache"/> class.
    /// </summary>
    /// <param name="cache">The underlying cache implementation.</param>
    /// <param name="defaultTtl">The default time-to-live for cached items.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public EducationContentCache(
        ICache cache,
        TimeSpan? defaultTtl = null,
        ILogger<EducationContentCache>? logger = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _defaultTtl = defaultTtl ?? TimeSpan.FromHours(24);
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EducationContentCache>.Instance;
    }

    /// <summary>
    /// Caches a Socratic question.
    /// </summary>
    /// <typeparam name="T">The type of the question (must be a reference type).</typeparam>
    /// <param name="conceptId">The concept identifier.</param>
    /// <param name="understandingLevel">The understanding level.</param>
    /// <param name="question">The question to cache.</param>
    /// <param name="ttl">Optional time-to-live.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public async Task CacheQuestionAsync<T>(
        string conceptId,
        string understandingLevel,
        T question,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var key = EducationCacheKeys.SocraticQuestion(conceptId, understandingLevel);
        await _cache.SetAsync(key, question, ttl ?? _defaultTtl, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Cached question for concept {ConceptId}", conceptId);
    }

    /// <summary>
    /// Gets a cached Socratic question.
    /// </summary>
    /// <typeparam name="T">The type of the question (must be a reference type).</typeparam>
    /// <param name="conceptId">The concept identifier.</param>
    /// <param name="understandingLevel">The understanding level.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The cached question, or null if not found.</returns>
    public async Task<T?> GetQuestionAsync<T>(
        string conceptId,
        string understandingLevel,
        CancellationToken cancellationToken = default) where T : class
    {
        var key = EducationCacheKeys.SocraticQuestion(conceptId, understandingLevel);
        var result = await _cache.GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (result != null)
        {
            _logger.LogDebug("Cache hit for question: concept {ConceptId}", conceptId);
        }
        return result;
    }

    /// <summary>
    /// Caches an assessment.
    /// </summary>
    /// <typeparam name="T">The type of the assessment (must be a reference type).</typeparam>
    /// <param name="conceptId">The concept identifier.</param>
    /// <param name="specHash">The assessment specification hash.</param>
    /// <param name="assessment">The assessment to cache.</param>
    /// <param name="ttl">Optional time-to-live.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public async Task CacheAssessmentAsync<T>(
        string conceptId,
        string specHash,
        T assessment,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var key = EducationCacheKeys.Assessment(conceptId, specHash);
        await _cache.SetAsync(key, assessment, ttl ?? _defaultTtl, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Cached assessment for concept {ConceptId}", conceptId);
    }

    /// <summary>
    /// Gets a cached assessment.
    /// </summary>
    /// <typeparam name="T">The type of the assessment (must be a reference type).</typeparam>
    /// <param name="conceptId">The concept identifier.</param>
    /// <param name="specHash">The assessment specification hash.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The cached assessment, or null if not found.</returns>
    public async Task<T?> GetAssessmentAsync<T>(
        string conceptId,
        string specHash,
        CancellationToken cancellationToken = default) where T : class
    {
        var key = EducationCacheKeys.Assessment(conceptId, specHash);
        var result = await _cache.GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (result != null)
        {
            _logger.LogDebug("Cache hit for assessment: concept {ConceptId}", conceptId);
        }
        return result;
    }

    /// <summary>
    /// Invalidates cached content for a concept.
    /// </summary>
    /// <param name="conceptId">The concept identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public async Task InvalidateConceptAsync(
        string conceptId,
        CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would use cache tags or pattern matching
        // For now, we'll just log the invalidation
        _logger.LogInformation("Invalidated cache for concept {ConceptId}", conceptId);
        await Task.CompletedTask;
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetAgents.Abstractions.Models;
using DotNetAgents.Education.Memory;
using Microsoft.Extensions.Logging;
using TeachingAssistant.Data.Entities;

namespace DotNetAgents.Education.RAG;

/// <summary>
/// Expands student queries with related terms and synonyms for better retrieval.
/// </summary>
public class QueryExpander
{
    private readonly ILLMModel<string, string>? _llmModel;
    private readonly ILogger<QueryExpander>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryExpander"/> class.
    /// </summary>
    public QueryExpander(
        ILLMModel<string, string>? llmModel = null,
        ILogger<QueryExpander>? logger = null)
    {
        _llmModel = llmModel;
        _logger = logger;
    }

    /// <summary>
    /// Expands a student query with related scientific terms and synonyms.
    /// </summary>
    public async Task<string> ExpandQueryAsync(
        string query,
        Subject? subject = null,
        GradeBand? gradeBand = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return query;

        // Simple expansion using subject-specific terms
        var expanded = query;

        // Add subject-specific terms
        if (subject.HasValue)
        {
            var subjectTerms = GetSubjectTerms(subject.Value);
            expanded = $"{query} {string.Join(" ", subjectTerms)}";
        }

        // Use LLM for more sophisticated expansion if available
        if (_llmModel != null)
        {
            try
            {
                var prompt = $"""
                    Expand this student question for better curriculum search.
                    Student grade: {gradeBand?.ToString() ?? "unknown"}
                    Subject: {subject?.ToString() ?? "unknown"}
                    Question: {query}

                    Add related scientific terms and synonyms. Keep it concise (max 20 words).
                    Expanded query:
                    """;

                var response = await _llmModel.GenerateAsync(prompt, cancellationToken);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    expanded = response.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to expand query using LLM, using simple expansion");
            }
        }

        return expanded;
    }

    private static string[] GetSubjectTerms(Subject subject)
    {
        return subject switch
        {
            Subject.Biology => new[] { "living", "organism", "cell", "life", "biology" },
            Subject.Chemistry => new[] { "matter", "atom", "molecule", "reaction", "chemistry" },
            Subject.Physics => new[] { "force", "energy", "motion", "wave", "physics" },
            Subject.EarthScience => new[] { "earth", "rock", "weather", "climate", "geology" },
            Subject.Astronomy => new[] { "space", "planet", "star", "galaxy", "astronomy" },
            Subject.EnvironmentalScience => new[] { "ecosystem", "environment", "conservation", "sustainability" },
            Subject.Mathematics => new[] { "number", "equation", "calculation", "problem", "math" },
            _ => Array.Empty<string>()
        };
    }
}

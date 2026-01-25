using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using DotNetAgents.Abstractions.Retrieval;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.RAG;

/// <summary>
/// Reranks search results using cross-encoder models or Python service.
/// </summary>
public class RerankingService
{
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly string? _rerankingServiceUrl;
    private readonly ILogger<RerankingService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RerankingService"/> class.
    /// </summary>
    public RerankingService(
        IHttpClientFactory? httpClientFactory = null,
        string? rerankingServiceUrl = null,
        ILogger<RerankingService>? logger = null)
    {
        _httpClientFactory = httpClientFactory;
        _rerankingServiceUrl = rerankingServiceUrl ?? "http://localhost:8010";
        _logger = logger;
    }

    /// <summary>
    /// Reranks search results using a cross-encoder model.
    /// </summary>
    public async Task<IReadOnlyList<VectorSearchResult>> RerankAsync(
        string query,
        IReadOnlyList<VectorSearchResult> candidates,
        CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
            return candidates;

        // If no reranking service, return candidates sorted by score
        if (_httpClientFactory == null || string.IsNullOrWhiteSpace(_rerankingServiceUrl))
        {
            return candidates.OrderByDescending(r => r.Score).ToList();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var documents = candidates.Select(c => ExtractText(c)).ToList();

            var request = new
            {
                query = query,
                documents = documents
            };

            var response = await client.PostAsJsonAsync(
                $"{_rerankingServiceUrl}/rerank",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var scores = await response.Content.ReadFromJsonAsync<List<double>>(cancellationToken);
                if (scores != null && scores.Count == candidates.Count)
                {
                    // Reorder candidates by reranking scores
                    return candidates
                        .Zip(scores, (candidate, score) => new { Candidate = candidate, Score = score })
                        .OrderByDescending(x => x.Score)
                        .Select(x => x.Candidate)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Reranking service unavailable, using vector scores");
        }

        // Fallback: return candidates sorted by original score
        return candidates.OrderByDescending(r => r.Score).ToList();
    }

    private static string ExtractText(VectorSearchResult result)
    {
        // Extract text from metadata or use a default
        if (result.Metadata?.TryGetValue("chunk_text", out var text) == true)
        {
            return text?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }
}

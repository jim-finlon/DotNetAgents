using System.Net.Http.Json;
using TeachingAssistant.AdminUI.Models;

namespace TeachingAssistant.AdminUI.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SystemMetricsDto?> GetSystemMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement system metrics endpoint in API
            return new SystemMetricsDto(0, 0, 0, 0, new Dictionary<string, object>(), DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system metrics");
            return null;
        }
    }
}

using System.Net.Http.Json;
using TeachingAssistant.ParentUI.Models;

namespace TeachingAssistant.ParentUI.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<StudentDto>> GetStudentsByFamilyAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<StudentDto>>($"/api/students/family/{familyId}", cancellationToken);
            return result ?? new List<StudentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students for family {FamilyId}", familyId);
            return new List<StudentDto>();
        }
    }

    public async Task<StudentDto?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<StudentDto>($"/api/students/{studentId}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student {StudentId}", studentId);
            return null;
        }
    }

    public async Task<List<SubjectProgressDto>> GetStudentProgressAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<SubjectProgressDto>>($"/api/progress/students/{studentId}/subjects", cancellationToken);
            return result ?? new List<SubjectProgressDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress for student {StudentId}", studentId);
            return new List<SubjectProgressDto>();
        }
    }
}

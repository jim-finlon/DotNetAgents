using System.Net.Http.Json;
using TeachingAssistant.StudentUI.Models;

namespace TeachingAssistant.StudentUI.Services;

/// <summary>
/// HTTP client for Teaching Assistant API.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StudentDto?> GetStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
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

    public async Task<List<SubjectProgressDto>> GetSubjectProgressAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<SubjectProgressDto>>($"/api/progress/students/{studentId}/subjects", cancellationToken);
            return result ?? new List<SubjectProgressDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject progress for student {StudentId}", studentId);
            return new List<SubjectProgressDto>();
        }
    }

    public async Task<WorkflowSessionDto?> StartSocraticTutoringAsync(string studentId, Guid contentUnitId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new StartWorkflowRequest { StudentId = studentId, ContentUnitId = contentUnitId };
            var response = await _httpClient.PostAsJsonAsync("/api/workflows/socratic-tutoring/start", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WorkflowSessionDto>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting Socratic tutoring session");
            return null;
        }
    }

    public async Task<WorkflowStateDto?> GetWorkflowStateAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<WorkflowStateDto>($"/api/workflows/sessions/{sessionId}/state", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow state for session {SessionId}", sessionId);
            return null;
        }
    }
}

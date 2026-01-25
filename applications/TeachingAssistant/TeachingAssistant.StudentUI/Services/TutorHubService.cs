using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace TeachingAssistant.StudentUI.Services;

/// <summary>
/// SignalR hub service for real-time tutoring.
/// </summary>
public class TutorHubService : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger<TutorHubService> _logger;

    public TutorHubService(string hubUrl, ILogger<TutorHubService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string, object>("ReceiveMessage", (studentId, message) =>
        {
            MessageReceived?.Invoke(studentId, message);
        });

        _connection.On<object>("ReceiveTutorResponse", (response) =>
        {
            TutorResponseReceived?.Invoke(response);
        });

        _connection.On<object>("WorkflowStarted", (data) =>
        {
            WorkflowStarted?.Invoke(data);
        });
    }

    public event Action<string, object>? MessageReceived;
    public event Action<object>? TutorResponseReceived;
    public event Action<object>? WorkflowStarted;

    public async Task StartAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
            _logger.LogInformation("Connected to TutorHub");
        }
    }

    public async Task JoinTutoringSessionAsync(string studentId)
    {
        await _connection.InvokeAsync("JoinTutoringSession", studentId);
    }

    public async Task SendMessageToTutorAsync(string studentId, string message)
    {
        await _connection.InvokeAsync("SendMessageToTutor", studentId, message);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection.State != HubConnectionState.Disconnected)
        {
            await _connection.StopAsync();
        }
        await _connection.DisposeAsync();
    }
}

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace TeachingAssistant.ParentUI.Services;

public class ParentHubService : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger<ParentHubService> _logger;

    public ParentHubService(string hubUrl, ILogger<ParentHubService> logger)
    {
        _logger = logger;
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<object>("ReceiveProgressUpdate", (update) => ProgressUpdateReceived?.Invoke(update));
        _connection.On<object>("ReceiveAchievementNotification", (notification) => AchievementReceived?.Invoke(notification));
    }

    public event Action<object>? ProgressUpdateReceived;
    public event Action<object>? AchievementReceived;

    public async Task StartAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
            _logger.LogInformation("Connected to ParentHub");
        }
    }

    public async Task JoinFamilyNotificationsAsync(string familyId)
    {
        await _connection.InvokeAsync("JoinFamilyNotifications", familyId);
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

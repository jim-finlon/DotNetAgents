using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace TeachingAssistant.AdminUI.Services;

public class AdminHubService : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger<AdminHubService> _logger;

    public AdminHubService(string hubUrl, ILogger<AdminHubService> logger)
    {
        _logger = logger;
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<object>("ReceiveSystemMetrics", (metrics) => SystemMetricsReceived?.Invoke(metrics));
        _connection.On<object>("ReceiveAlert", (alert) => AlertReceived?.Invoke(alert));
    }

    public event Action<object>? SystemMetricsReceived;
    public event Action<object>? AlertReceived;

    public async Task StartAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync("JoinAdminMonitoring");
            _logger.LogInformation("Connected to AdminHub");
        }
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

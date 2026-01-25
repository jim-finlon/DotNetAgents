using Microsoft.AspNetCore.SignalR;

namespace TeachingAssistant.API.Hubs;

/// <summary>
/// SignalR hub for admin monitoring and system updates.
/// </summary>
public class AdminHub : Hub
{
    private readonly ILogger<AdminHub> _logger;

    public AdminHub(ILogger<AdminHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join admin monitoring group.
    /// </summary>
    public async Task JoinAdminMonitoring()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        _logger.LogInformation("Admin client {ConnectionId} joined monitoring", Context.ConnectionId);
    }

    /// <summary>
    /// Leave admin monitoring group.
    /// </summary>
    public async Task LeaveAdminMonitoring()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
        _logger.LogInformation("Admin client {ConnectionId} left monitoring", Context.ConnectionId);
    }

    /// <summary>
    /// Send system metrics to admins.
    /// </summary>
    public async Task SendSystemMetrics(SystemMetrics metrics)
    {
        await Clients.Group("admins").SendAsync("ReceiveSystemMetrics", metrics);
    }

    /// <summary>
    /// Send alert to admins.
    /// </summary>
    public async Task SendAlert(AdminAlert alert)
    {
        await Clients.Group("admins").SendAsync("ReceiveAlert", alert);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Admin client {ConnectionId} connected to AdminHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Admin client {ConnectionId} disconnected from AdminHub", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// System metrics model.
/// </summary>
public record SystemMetrics(
    int ActiveStudents,
    int ActiveSessions,
    int PendingAssessments,
    decimal AverageResponseTime,
    Dictionary<string, object>? AdditionalMetrics,
    DateTimeOffset Timestamp);

/// <summary>
/// Admin alert model.
/// </summary>
public record AdminAlert(
    string AlertType,
    string Severity,
    string Message,
    string? Details,
    DateTimeOffset Timestamp);

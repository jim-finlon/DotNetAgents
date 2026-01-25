using Microsoft.AspNetCore.SignalR;

namespace TeachingAssistant.API.Hubs;

/// <summary>
/// SignalR hub for parent notifications and updates.
/// </summary>
public class ParentHub : Hub
{
    private readonly ILogger<ParentHub> _logger;

    public ParentHub(ILogger<ParentHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join notifications for a specific family.
    /// </summary>
    public async Task JoinFamilyNotifications(string familyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"family_{familyId}");
        _logger.LogInformation("Client {ConnectionId} joined family notifications for {FamilyId}", Context.ConnectionId, familyId);
    }

    /// <summary>
    /// Leave family notifications.
    /// </summary>
    public async Task LeaveFamilyNotifications(string familyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"family_{familyId}");
        _logger.LogInformation("Client {ConnectionId} left family notifications for {FamilyId}", Context.ConnectionId, familyId);
    }

    /// <summary>
    /// Send progress update to parents.
    /// </summary>
    public async Task SendProgressUpdate(string familyId, ProgressNotification notification)
    {
        await Clients.Group($"family_{familyId}").SendAsync("ReceiveProgressUpdate", notification);
    }

    /// <summary>
    /// Send achievement notification to parents.
    /// </summary>
    public async Task SendAchievementNotification(string familyId, AchievementNotification notification)
    {
        await Clients.Group($"family_{familyId}").SendAsync("ReceiveAchievementNotification", notification);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to ParentHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected from ParentHub", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Progress notification model.
/// </summary>
public record ProgressNotification(
    string StudentId,
    string StudentName,
    string Subject,
    decimal ProgressPercentage,
    string? CurrentUnit,
    DateTimeOffset Timestamp);

/// <summary>
/// Achievement notification model.
/// </summary>
public record AchievementNotification(
    string StudentId,
    string StudentName,
    string AchievementType,
    string Title,
    string Description,
    DateTimeOffset Timestamp);

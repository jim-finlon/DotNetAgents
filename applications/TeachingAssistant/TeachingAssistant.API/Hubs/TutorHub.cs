using Microsoft.AspNetCore.SignalR;
using TeachingAssistant.API.Models;
using DotNetAgents.Education.Models;

namespace TeachingAssistant.API.Hubs;

/// <summary>
/// SignalR hub for real-time tutoring interactions.
/// </summary>
public class TutorHub : Hub
{
    private readonly ILogger<TutorHub> _logger;

    public TutorHub(ILogger<TutorHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a tutoring session for a specific student.
    /// </summary>
    public async Task JoinTutoringSession(string studentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"student_{studentId}");
        _logger.LogInformation("Client {ConnectionId} joined tutoring session for student {StudentId}", Context.ConnectionId, studentId);
    }

    /// <summary>
    /// Leave a tutoring session.
    /// </summary>
    public async Task LeaveTutoringSession(string studentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"student_{studentId}");
        _logger.LogInformation("Client {ConnectionId} left tutoring session for student {StudentId}", Context.ConnectionId, studentId);
    }

    /// <summary>
    /// Send a message to the tutor (from student).
    /// </summary>
    public async Task SendMessageToTutor(string studentId, string message)
    {
        await Clients.Group($"student_{studentId}").SendAsync("ReceiveMessage", new { StudentId = studentId, Message = message, Timestamp = DateTimeOffset.UtcNow });
    }

    /// <summary>
    /// Send a response from the tutor (to student).
    /// </summary>
    public async Task SendTutorResponse(string studentId, TutorResponse response)
    {
        await Clients.Group($"student_{studentId}").SendAsync("ReceiveTutorResponse", response);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to TutorHub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected from TutorHub", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

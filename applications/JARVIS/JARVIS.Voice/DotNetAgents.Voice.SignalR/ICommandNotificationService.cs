using DotNetAgents.Voice.Orchestration;
using DotNetAgents.Voice.SignalR.Models;

namespace DotNetAgents.Voice.SignalR;

/// <summary>
/// Interface for sending real-time notifications about command status.
/// </summary>
public interface ICommandNotificationService
{
    /// <summary>
    /// Sends a status update for a command.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="commandId">The command ID.</param>
    /// <param name="status">The new status.</param>
    /// <param name="message">Optional message.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task SendStatusUpdateAsync(
        Guid userId,
        Guid commandId,
        CommandStatus status,
        string? message = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a clarification request for a command.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="commandId">The command ID.</param>
    /// <param name="prompt">The clarification prompt.</param>
    /// <param name="missingParameter">The name of the missing parameter.</param>
    /// <param name="turn">The current turn number.</param>
    /// <param name="maxTurns">The maximum number of turns.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task SendClarificationRequestAsync(
        Guid userId,
        Guid commandId,
        string prompt,
        string missingParameter,
        int turn = 1,
        int maxTurns = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a confirmation request for a command.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="commandId">The command ID.</param>
    /// <param name="readBackText">The text to read back to the user.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task SendConfirmationRequestAsync(
        Guid userId,
        Guid commandId,
        string readBackText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command completion notification.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="commandId">The command ID.</param>
    /// <param name="result">The command result.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task SendCompletionAsync(
        Guid userId,
        Guid commandId,
        object? result,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command error notification.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="commandId">The command ID.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the async operation.</returns>
    Task SendErrorAsync(
        Guid userId,
        Guid commandId,
        string errorMessage,
        CancellationToken cancellationToken = default);
}

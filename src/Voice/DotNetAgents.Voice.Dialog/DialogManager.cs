using Microsoft.Extensions.Logging;

namespace DotNetAgents.Voice.Dialog;

/// <summary>
/// Default implementation of <see cref="IDialogManager"/>.
/// </summary>
public class DialogManager : IDialogManager
{
    private readonly IDialogStore _store;
    private readonly Dictionary<string, IDialogHandler> _handlers;
    private readonly ILogger<DialogManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogManager"/> class.
    /// </summary>
    /// <param name="store">The dialog store.</param>
    /// <param name="handlers">The dictionary of dialog handlers.</param>
    /// <param name="logger">The logger instance.</param>
    public DialogManager(
        IDialogStore store,
        IEnumerable<IDialogHandler> handlers,
        ILogger<DialogManager> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _handlers = handlers.ToDictionary(h => h.DialogType, h => h);
    }

    /// <inheritdoc />
    public async Task<DialogState> StartDialogAsync(
        Guid userId,
        string dialogType,
        Dictionary<string, object>? initialContext = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dialogType))
        {
            throw new ArgumentException("Dialog type cannot be null or empty", nameof(dialogType));
        }

        if (!_handlers.TryGetValue(dialogType, out var handler))
        {
            throw new InvalidOperationException($"No handler registered for dialog type: {dialogType}");
        }

        _logger.LogInformation("Starting dialog {DialogType} for user {UserId}", dialogType, userId);

        var initialState = await handler.InitializeAsync(userId, initialContext, cancellationToken)
            .ConfigureAwait(false);

        await _store.CreateAsync(initialState, cancellationToken).ConfigureAwait(false);

        return initialState;
    }

    /// <inheritdoc />
    public async Task<DialogState> ContinueDialogAsync(
        Guid dialogId,
        string userInput,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            throw new ArgumentException("User input cannot be null or empty", nameof(userInput));
        }

        var state = await _store.GetAsync(dialogId, cancellationToken).ConfigureAwait(false);
        if (state == null)
        {
            throw new InvalidOperationException($"Dialog {dialogId} not found");
        }

        if (state.Status == DialogStatus.Completed || state.Status == DialogStatus.Cancelled)
        {
            throw new InvalidOperationException($"Dialog {dialogId} is already {state.Status}");
        }

        if (!_handlers.TryGetValue(state.DialogType, out var handler))
        {
            throw new InvalidOperationException($"No handler registered for dialog type: {state.DialogType}");
        }

        _logger.LogDebug("Processing input for dialog {DialogId}", dialogId);

        var updatedState = await handler.ProcessInputAsync(state, userInput, cancellationToken)
            .ConfigureAwait(false);

        // Check if dialog is complete
        var isComplete = await handler.IsCompleteAsync(updatedState, cancellationToken).ConfigureAwait(false);
        if (isComplete)
        {
            updatedState = updatedState with
            {
                Status = DialogStatus.Completed,
                CompletedAt = DateTime.UtcNow
            };
        }
        else
        {
            // Get next question
            var nextQuestion = await handler.GetNextQuestionAsync(updatedState, cancellationToken)
                .ConfigureAwait(false);
            updatedState = updatedState with
            {
                Status = DialogStatus.WaitingForInput,
                CurrentQuestion = nextQuestion
            };
        }

        await _store.UpdateAsync(updatedState, cancellationToken).ConfigureAwait(false);

        return updatedState;
    }

    /// <inheritdoc />
    public Task<DialogState?> GetDialogStateAsync(
        Guid dialogId,
        CancellationToken cancellationToken = default)
    {
        return _store.GetAsync(dialogId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DialogState> CompleteDialogAsync(
        Guid dialogId,
        CancellationToken cancellationToken = default)
    {
        var state = await _store.GetAsync(dialogId, cancellationToken).ConfigureAwait(false);
        if (state == null)
        {
            throw new InvalidOperationException($"Dialog {dialogId} not found");
        }

        var completedState = state with
        {
            Status = DialogStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        await _store.UpdateAsync(completedState, cancellationToken).ConfigureAwait(false);

        return completedState;
    }

    /// <inheritdoc />
    public async Task CancelDialogAsync(
        Guid dialogId,
        CancellationToken cancellationToken = default)
    {
        var state = await _store.GetAsync(dialogId, cancellationToken).ConfigureAwait(false);
        if (state == null)
        {
            throw new InvalidOperationException($"Dialog {dialogId} not found");
        }

        var cancelledState = state with
        {
            Status = DialogStatus.Cancelled,
            LastUpdatedAt = DateTime.UtcNow
        };

        await _store.UpdateAsync(cancelledState, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<List<DialogState>> GetActiveDialogsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _store.GetActiveDialogsAsync(userId, cancellationToken);
    }
}

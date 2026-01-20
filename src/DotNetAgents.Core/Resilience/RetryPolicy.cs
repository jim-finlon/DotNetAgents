using Microsoft.Extensions.Logging;

namespace DotNetAgents.Core.Resilience;

/// <summary>
/// Configuration options for retry policies.
/// </summary>
public record RetryPolicyOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts (default: 3).
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Gets or sets the initial delay between retries (default: 1 second).
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets whether to use exponential backoff (default: true).
    /// </summary>
    public bool UseExponentialBackoff { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum delay between retries (default: 30 seconds).
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a function to determine if an exception should be retried.
    /// </summary>
    public Func<Exception, bool>? ShouldRetry { get; init; }
}

/// <summary>
/// Provides retry logic for async operations.
/// </summary>
public class RetryPolicy
{
    private readonly RetryPolicyOptions _options;
    private readonly ILogger<RetryPolicy>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryPolicy"/> class.
    /// </summary>
    /// <param name="options">The retry policy options.</param>
    /// <param name="logger">Optional logger for retry operations.</param>
    public RetryPolicy(RetryPolicyOptions? options = null, ILogger<RetryPolicy>? logger = null)
    {
        _options = options ?? new RetryPolicyOptions();
        _logger = logger;
    }

    /// <summary>
    /// Executes an async operation with retry logic.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Exception? lastException = null;
        var attempt = 0;

        while (attempt <= _options.MaxRetries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await operation(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < _options.MaxRetries && ShouldRetryException(ex))
            {
                lastException = ex;
                attempt++;

                var delay = CalculateDelay(attempt);
                _logger?.LogWarning(
                    ex,
                    "Operation failed (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms.",
                    attempt,
                    _options.MaxRetries,
                    delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        _logger?.LogError(
            lastException,
            "Operation failed after {MaxRetries} retries.",
            _options.MaxRetries);

        throw lastException ?? new InvalidOperationException("Operation failed with no exception captured.");
    }

    /// <summary>
    /// Executes an async operation with retry logic that returns void.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteAsync<object?>(
            async ct =>
            {
                await operation(ct).ConfigureAwait(false);
                return null;
            },
            cancellationToken).ConfigureAwait(false);
    }

    private bool ShouldRetryException(Exception exception)
    {
        if (_options.ShouldRetry != null)
        {
            return _options.ShouldRetry(exception);
        }

        // Default: retry on transient exceptions
        return exception is HttpRequestException ||
               exception is TaskCanceledException ||
               (exception is AggregateException aggEx && aggEx.InnerExceptions.Any(e => e is HttpRequestException || e is TaskCanceledException));
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        if (!_options.UseExponentialBackoff)
        {
            return _options.InitialDelay;
        }

        // Exponential backoff: delay = initialDelay * 2^(attempt-1)
        var delay = TimeSpan.FromMilliseconds(
            _options.InitialDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));

        return delay > _options.MaxDelay ? _options.MaxDelay : delay;
    }
}
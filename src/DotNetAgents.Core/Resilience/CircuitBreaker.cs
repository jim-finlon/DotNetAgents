using Microsoft.Extensions.Logging;

namespace DotNetAgents.Core.Resilience;

/// <summary>
/// States of a circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Circuit is closed - operations are allowed.
    /// </summary>
    Closed,

    /// <summary>
    /// Circuit is open - operations are blocked.
    /// </summary>
    Open,

    /// <summary>
    /// Circuit is half-open - testing if service has recovered.
    /// </summary>
    HalfOpen
}

/// <summary>
/// Configuration options for circuit breaker.
/// </summary>
public record CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the failure threshold before opening the circuit (default: 5).
    /// </summary>
    public int FailureThreshold { get; init; } = 5;

    /// <summary>
    /// Gets or sets the duration the circuit stays open before transitioning to half-open (default: 30 seconds).
    /// </summary>
    public TimeSpan OpenDuration { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the time window for counting failures (default: 60 seconds).
    /// </summary>
    public TimeSpan FailureWindow { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets a function to determine if an exception should count as a failure.
    /// </summary>
    public Func<Exception, bool>? ShouldCountAsFailure { get; init; }
}

/// <summary>
/// Circuit breaker pattern implementation for protecting against cascading failures.
/// </summary>
public class CircuitBreaker
{
    private readonly CircuitBreakerOptions _options;
    private readonly ILogger<CircuitBreaker>? _logger;
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount;
    private DateTimeOffset _lastFailureTime = DateTimeOffset.MinValue;
    private DateTimeOffset _circuitOpenedAt = DateTimeOffset.MinValue;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreaker"/> class.
    /// </summary>
    /// <param name="options">The circuit breaker options.</param>
    /// <param name="logger">Optional logger for circuit breaker operations.</param>
    public CircuitBreaker(CircuitBreakerOptions? options = null, ILogger<CircuitBreaker>? logger = null)
    {
        _options = options ?? new CircuitBreakerOptions();
        _logger = logger;
    }

    /// <summary>
    /// Gets the current state of the circuit breaker.
    /// </summary>
    public CircuitBreakerState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// Executes an async operation with circuit breaker protection.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the circuit is open.</exception>
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (!IsOperationAllowed())
        {
            _logger?.LogWarning("Circuit breaker is OPEN. Operation blocked.");
            throw new InvalidOperationException("Circuit breaker is open. Operation blocked.");
        }

        try
        {
            var result = await operation(cancellationToken).ConfigureAwait(false);
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an async operation with circuit breaker protection that returns void.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when the circuit is open.</exception>
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

    /// <summary>
    /// Resets the circuit breaker to closed state.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _state = CircuitBreakerState.Closed;
            _failureCount = 0;
            _lastFailureTime = DateTimeOffset.MinValue;
            _circuitOpenedAt = DateTimeOffset.MinValue;
            _logger?.LogInformation("Circuit breaker reset to CLOSED state.");
        }
    }

    private bool IsOperationAllowed()
    {
        lock (_lock)
        {
            var now = DateTimeOffset.UtcNow;

            switch (_state)
            {
                case CircuitBreakerState.Closed:
                    // Reset failure count if outside the failure window
                    if (now - _lastFailureTime > _options.FailureWindow)
                    {
                        _failureCount = 0;
                    }
                    return true;

                case CircuitBreakerState.Open:
                    // Check if enough time has passed to transition to half-open
                    if (now - _circuitOpenedAt >= _options.OpenDuration)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _logger?.LogInformation("Circuit breaker transitioning to HALF-OPEN state.");
                        return true;
                    }
                    return false;

                case CircuitBreakerState.HalfOpen:
                    return true;

                default:
                    return false;
            }
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            if (_state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Closed;
                _failureCount = 0;
                _lastFailureTime = DateTimeOffset.MinValue;
                _circuitOpenedAt = DateTimeOffset.MinValue;
                _logger?.LogInformation("Circuit breaker transitioning to CLOSED state after successful operation.");
            }
            else if (_state == CircuitBreakerState.Closed)
            {
                // Reset failure count on success
                if (DateTimeOffset.UtcNow - _lastFailureTime > _options.FailureWindow)
                {
                    _failureCount = 0;
                }
            }
        }
    }

    private void OnFailure(Exception exception)
    {
        lock (_lock)
        {
            if (!ShouldCountAsFailure(exception))
            {
                return;
            }

            _failureCount++;
            _lastFailureTime = DateTimeOffset.UtcNow;

            if (_state == CircuitBreakerState.HalfOpen)
            {
                // Failed in half-open state, go back to open
                _state = CircuitBreakerState.Open;
                _circuitOpenedAt = DateTimeOffset.UtcNow;
                _logger?.LogWarning(
                    exception,
                    "Circuit breaker transitioning to OPEN state after failure in HALF-OPEN state.");
            }
            else if (_state == CircuitBreakerState.Closed && _failureCount >= _options.FailureThreshold)
            {
                // Threshold reached, open the circuit
                _state = CircuitBreakerState.Open;
                _circuitOpenedAt = DateTimeOffset.UtcNow;
                _logger?.LogWarning(
                    exception,
                    "Circuit breaker opening after {FailureCount} failures (threshold: {Threshold}).",
                    _failureCount,
                    _options.FailureThreshold);
            }
        }
    }

    private bool ShouldCountAsFailure(Exception exception)
    {
        if (_options.ShouldCountAsFailure != null)
        {
            return _options.ShouldCountAsFailure(exception);
        }

        // Default: count transient exceptions as failures
        return exception is HttpRequestException ||
               exception is TaskCanceledException ||
               (exception is AggregateException aggEx && aggEx.InnerExceptions.Any(e => e is HttpRequestException || e is TaskCanceledException));
    }
}
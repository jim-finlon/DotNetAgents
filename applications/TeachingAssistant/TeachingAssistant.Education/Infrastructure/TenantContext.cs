using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Infrastructure;

/// <summary>
/// Implementation of tenant context.
/// </summary>
public class TenantContext : ITenantContext
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public string TenantId { get; }

    /// <summary>
    /// Gets the tenant configuration.
    /// </summary>
    public TenantConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantContext"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="configuration">The tenant configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when tenantId or configuration is null.</exception>
    public TenantContext(string tenantId, TenantConfiguration configuration)
    {
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        if (configuration.TenantId != tenantId)
        {
            throw new ArgumentException(
                "Configuration tenant ID must match the provided tenant ID.",
                nameof(configuration));
        }
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(TenantId) &&
               Configuration != null &&
               Configuration.IsActive;
    }
}

/// <summary>
/// Default implementation of <see cref="ITenantContextProvider"/> using AsyncLocal.
/// </summary>
public class AsyncLocalTenantContextProvider : ITenantContextProvider
{
    private static readonly AsyncLocal<ITenantContext?> _currentContext = new();
    private readonly Dictionary<string, ITenantContext> _tenantCache = new();
    private readonly object _lockObject = new();
    private readonly ILogger<AsyncLocalTenantContextProvider>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLocalTenantContextProvider"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public AsyncLocalTenantContextProvider(ILogger<AsyncLocalTenantContextProvider>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public ITenantContext? GetCurrent() => _currentContext.Value;

    /// <inheritdoc/>
    public void SetCurrent(ITenantContext? context)
    {
        _currentContext.Value = context;
        _logger?.LogDebug(
            "Set current tenant context: {TenantId}",
            context?.TenantId ?? "null");
    }

    /// <inheritdoc/>
    public Task<ITenantContext?> GetTenantContextAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be null or empty.", nameof(tenantId));

        lock (_lockObject)
        {
            if (_tenantCache.TryGetValue(tenantId, out var context))
            {
                return Task.FromResult<ITenantContext?>(context);
            }
        }

        // In a real implementation, this would load from a database or configuration store
        _logger?.LogWarning(
            "Tenant context not found for tenant {TenantId}",
            tenantId);

        return Task.FromResult<ITenantContext?>(null);
    }

    /// <summary>
    /// Registers a tenant context in the cache.
    /// </summary>
    /// <param name="context">The tenant context to register.</param>
    public void RegisterTenant(ITenantContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        lock (_lockObject)
        {
            _tenantCache[context.TenantId] = context;
            _logger?.LogInformation(
                "Registered tenant context: {TenantId}",
                context.TenantId);
        }
    }
}

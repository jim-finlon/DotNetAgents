using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Infrastructure;

/// <summary>
/// Implementation of tenant management.
/// </summary>
public class TenantManager : ITenantManager
{
    private readonly ITenantContextProvider _contextProvider;
    private readonly Dictionary<string, TenantConfiguration> _tenantConfigurations = new();
    private readonly object _lockObject = new();
    private readonly ILogger<TenantManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantManager"/> class.
    /// </summary>
    /// <param name="contextProvider">The tenant context provider.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public TenantManager(
        ITenantContextProvider contextProvider,
        ILogger<TenantManager>? logger = null)
    {
        _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TenantManager>.Instance;
    }

    /// <inheritdoc/>
    public Task<ITenantContext> CreateTenantAsync(
        string name,
        TenantConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be null or empty.", nameof(name));

        var tenantId = Guid.NewGuid().ToString();

        var config = configuration ?? new TenantConfiguration
        {
            TenantId = tenantId,
            Name = name,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdated = DateTimeOffset.UtcNow
        };

        if (config.TenantId != tenantId)
        {
            config = config with { TenantId = tenantId };
        }

        lock (_lockObject)
        {
            _tenantConfigurations[tenantId] = config;
        }

        var context = new TenantContext(tenantId, config);

        if (_contextProvider is AsyncLocalTenantContextProvider asyncLocalProvider)
        {
            asyncLocalProvider.RegisterTenant(context);
        }

        _logger.LogInformation(
            "Created tenant: {TenantId}, Name: {Name}",
            tenantId,
            name);

        return Task.FromResult<ITenantContext>(context);
    }

    /// <inheritdoc/>
    public Task<ITenantContext?> GetTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be null or empty.", nameof(tenantId));

        lock (_lockObject)
        {
            if (_tenantConfigurations.TryGetValue(tenantId, out var config))
            {
                var context = new TenantContext(tenantId, config);
                return Task.FromResult<ITenantContext?>(context);
            }
        }

        return Task.FromResult<ITenantContext?>(null);
    }

    /// <inheritdoc/>
    public Task<ITenantContext> UpdateTenantAsync(
        string tenantId,
        Func<TenantConfiguration, TenantConfiguration> updateAction,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be null or empty.", nameof(tenantId));
        if (updateAction == null)
            throw new ArgumentNullException(nameof(updateAction));

        lock (_lockObject)
        {
            if (!_tenantConfigurations.TryGetValue(tenantId, out var config))
            {
                throw new InvalidOperationException($"Tenant {tenantId} not found.");
            }

            var updated = updateAction(config) with
            {
                LastUpdated = DateTimeOffset.UtcNow
            };

            _tenantConfigurations[tenantId] = updated;

            var context = new TenantContext(tenantId, updated);

            _logger.LogInformation(
                "Updated tenant: {TenantId}",
                tenantId);

            return Task.FromResult<ITenantContext>(context);
        }
    }

    /// <inheritdoc/>
    public Task DeleteTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be null or empty.", nameof(tenantId));

        lock (_lockObject)
        {
            var removed = _tenantConfigurations.Remove(tenantId);
            if (removed)
            {
                _logger.LogWarning(
                    "Deleted tenant: {TenantId}",
                    tenantId);
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ITenantContext>> ListTenantsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var tenants = _tenantConfigurations.Values
                .Where(c => includeInactive || c.IsActive)
                .Select(c => new TenantContext(c.TenantId, c))
                .Cast<ITenantContext>()
                .ToList();

            return Task.FromResult<IReadOnlyList<ITenantContext>>(tenants);
        }
    }
}

namespace DotNetAgents.Education.Infrastructure;

/// <summary>
/// Represents tenant-specific configuration.
/// </summary>
public record TenantConfiguration
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tenant name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether the tenant is active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Gets the maximum number of students allowed.
    /// </summary>
    public int? MaxStudents { get; init; }

    /// <summary>
    /// Gets the maximum number of concurrent sessions allowed.
    /// </summary>
    public int? MaxConcurrentSessions { get; init; }

    /// <summary>
    /// Gets tenant-specific feature flags.
    /// </summary>
    public IDictionary<string, bool> FeatureFlags { get; init; } = new Dictionary<string, bool>();

    /// <summary>
    /// Gets additional tenant-specific settings.
    /// </summary>
    public IDictionary<string, object> Settings { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets when the tenant was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets when the tenant configuration was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Interface for tenant context providing tenant information and configuration.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Gets the tenant configuration.
    /// </summary>
    TenantConfiguration Configuration { get; }

    /// <summary>
    /// Validates that the tenant context is valid and active.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    bool IsValid();
}

/// <summary>
/// Provides access to the current tenant context.
/// </summary>
public interface ITenantContextProvider
{
    /// <summary>
    /// Gets the current tenant context.
    /// </summary>
    /// <returns>The current tenant context, or null if none is set.</returns>
    ITenantContext? GetCurrent();

    /// <summary>
    /// Sets the current tenant context.
    /// </summary>
    /// <param name="context">The tenant context to set.</param>
    void SetCurrent(ITenantContext? context);

    /// <summary>
    /// Gets the tenant context for a specific tenant ID.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The tenant context, or null if not found.</returns>
    Task<ITenantContext?> GetTenantContextAsync(
        string tenantId,
        CancellationToken cancellationToken = default);
}

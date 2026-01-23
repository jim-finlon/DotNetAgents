namespace DotNetAgents.Education.Infrastructure;

/// <summary>
/// Interface for tenant management operations.
/// </summary>
public interface ITenantManager
{
    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="name">The tenant name.</param>
    /// <param name="configuration">Optional initial configuration.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The created tenant context.</returns>
    Task<ITenantContext> CreateTenantAsync(
        string name,
        TenantConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The tenant context, or null if not found.</returns>
    Task<ITenantContext?> GetTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates tenant configuration.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="updateAction">Action to update the configuration.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The updated tenant context.</returns>
    Task<ITenantContext> UpdateTenantAsync(
        string tenantId,
        Func<TenantConfiguration, TenantConfiguration> updateAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tenant and all associated data.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task DeleteTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all tenants.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive tenants.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of tenant contexts.</returns>
    Task<IReadOnlyList<ITenantContext>> ListTenantsAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

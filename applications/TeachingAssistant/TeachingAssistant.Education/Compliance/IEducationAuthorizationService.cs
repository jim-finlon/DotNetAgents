namespace DotNetAgents.Education.Compliance;

/// <summary>
/// Represents a role in the education system.
/// </summary>
public enum EducationRole
{
    /// <summary>
    /// Student role.
    /// </summary>
    Student,

    /// <summary>
    /// Teacher role.
    /// </summary>
    Teacher,

    /// <summary>
    /// Administrator role.
    /// </summary>
    Administrator,

    /// <summary>
    /// Parent role.
    /// </summary>
    Parent,

    /// <summary>
    /// System role.
    /// </summary>
    System
}

/// <summary>
/// Represents a permission in the education system.
/// </summary>
public record Permission
{
    /// <summary>
    /// Gets the permission identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the permission name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resource the permission applies to.
    /// </summary>
    public string Resource { get; init; } = string.Empty;

    /// <summary>
    /// Gets the action allowed (view, modify, delete, etc.).
    /// </summary>
    public string Action { get; init; } = string.Empty;
}

/// <summary>
/// Interface for education authorization service (RBAC).
/// </summary>
public interface IEducationAuthorizationService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permissionId">The permission identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the user has the permission, false otherwise.</returns>
    Task<bool> CheckPermissionAsync(
        string userId,
        string permissionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of roles.</returns>
    Task<IReadOnlyList<EducationRole>> GetUserRolesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role to assign.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    Task AssignRoleAsync(
        string userId,
        EducationRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a role.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of permissions.</returns>
    Task<IReadOnlyList<Permission>> GetRolePermissionsAsync(
        EducationRole role,
        CancellationToken cancellationToken = default);
}

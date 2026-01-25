using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Compliance;

/// <summary>
/// Implementation of education authorization service with role-based access control.
/// </summary>
public class EducationAuthorizationService : IEducationAuthorizationService
{
    private readonly Dictionary<string, HashSet<EducationRole>> _userRoles = new();
    private readonly Dictionary<EducationRole, HashSet<Permission>> _rolePermissions = new();
    private readonly Dictionary<string, bool> _permissionCache = new();
    private readonly object _lockObject = new();
    private readonly ILogger<EducationAuthorizationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EducationAuthorizationService"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public EducationAuthorizationService(ILogger<EducationAuthorizationService>? logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EducationAuthorizationService>.Instance;
        InitializeDefaultPermissions();
    }

    /// <inheritdoc/>
    public Task<bool> CheckPermissionAsync(
        string userId,
        string permissionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(permissionId))
            throw new ArgumentException("Permission ID cannot be null or empty.", nameof(permissionId));

        // Check cache first
        var cacheKey = $"{userId}:{permissionId}";
        lock (_lockObject)
        {
            if (_permissionCache.TryGetValue(cacheKey, out var cached))
            {
                return Task.FromResult(cached);
            }
        }

        // Get user roles
        var roles = GetUserRoles(userId);

        // Check if any role has the permission
        var hasPermission = false;
        lock (_lockObject)
        {
            foreach (var role in roles)
            {
                if (_rolePermissions.TryGetValue(role, out var permissions))
                {
                    if (permissions.Any(p => p.Id == permissionId || p.Resource == permissionId))
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }

            // Cache the result
            _permissionCache[cacheKey] = hasPermission;
        }

        _logger.LogDebug(
            "Permission check: User {UserId}, Permission {PermissionId}, Result {Result}",
            userId,
            permissionId,
            hasPermission ? "GRANTED" : "DENIED");

        return Task.FromResult(hasPermission);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<EducationRole>> GetUserRolesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        var roles = GetUserRoles(userId);
        return Task.FromResult<IReadOnlyList<EducationRole>>(roles.ToList());
    }

    /// <inheritdoc/>
    public Task AssignRoleAsync(
        string userId,
        EducationRole role,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        lock (_lockObject)
        {
            if (!_userRoles.TryGetValue(userId, out var roles))
            {
                roles = new HashSet<EducationRole>();
                _userRoles[userId] = roles;
            }

            roles.Add(role);

            // Clear permission cache for this user
            var keysToRemove = _permissionCache.Keys
                .Where(k => k.StartsWith($"{userId}:", StringComparison.Ordinal))
                .ToList();
            foreach (var key in keysToRemove)
            {
                _permissionCache.Remove(key);
            }

            _logger.LogInformation(
                "Assigned role {Role} to user {UserId}",
                role,
                userId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Permission>> GetRolePermissionsAsync(
        EducationRole role,
        CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (_rolePermissions.TryGetValue(role, out var permissions))
            {
                return Task.FromResult<IReadOnlyList<Permission>>(permissions.ToList());
            }

            return Task.FromResult<IReadOnlyList<Permission>>(Array.Empty<Permission>());
        }
    }

    private HashSet<EducationRole> GetUserRoles(string userId)
    {
        lock (_lockObject)
        {
            if (_userRoles.TryGetValue(userId, out var roles))
            {
                return roles;
            }

            return new HashSet<EducationRole>();
        }
    }

    private void InitializeDefaultPermissions()
    {
        lock (_lockObject)
        {
            // Student permissions
            _rolePermissions[EducationRole.Student] = new HashSet<Permission>
            {
                new Permission { Id = "student:view:own", Name = "View Own Data", Resource = "student", Action = "view" },
                new Permission { Id = "student:modify:own", Name = "Modify Own Data", Resource = "student", Action = "modify" }
            };

            // Teacher permissions
            _rolePermissions[EducationRole.Teacher] = new HashSet<Permission>
            {
                new Permission { Id = "ferpa:view:*", Name = "View Student Data", Resource = "ferpa", Action = "view" },
                new Permission { Id = "ferpa:modify:*", Name = "Modify Student Data", Resource = "ferpa", Action = "modify" },
                new Permission { Id = "assessment:create", Name = "Create Assessments", Resource = "assessment", Action = "create" },
                new Permission { Id = "assessment:grade", Name = "Grade Assessments", Resource = "assessment", Action = "grade" }
            };

            // Administrator permissions
            _rolePermissions[EducationRole.Administrator] = new HashSet<Permission>
            {
                new Permission { Id = "*", Name = "All Permissions", Resource = "*", Action = "*" }
            };

            // Parent permissions
            _rolePermissions[EducationRole.Parent] = new HashSet<Permission>
            {
                new Permission { Id = "ferpa:view:child", Name = "View Child Data", Resource = "ferpa", Action = "view" },
                new Permission { Id = "ferpa:export:child", Name = "Export Child Data", Resource = "ferpa", Action = "export" }
            };

            // System permissions
            _rolePermissions[EducationRole.System] = new HashSet<Permission>
            {
                new Permission { Id = "*", Name = "All Permissions", Resource = "*", Action = "*" }
            };
        }
    }
}

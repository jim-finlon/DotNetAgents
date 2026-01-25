namespace TeachingAssistant.API.Services;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates an OAuth user and returns a JWT token.
    /// </summary>
    Task<string> AuthenticateOAuthUserAsync(string email, string name, string provider, CancellationToken cancellationToken = default);
}

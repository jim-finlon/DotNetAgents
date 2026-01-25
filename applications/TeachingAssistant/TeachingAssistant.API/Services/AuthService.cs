using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TeachingAssistant.Data;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.API.Services;

/// <summary>
/// Service implementation for authentication operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly TeachingAssistantDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        TeachingAssistantDbContext dbContext,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> AuthenticateOAuthUserAsync(
        string email,
        string name,
        string provider,
        CancellationToken cancellationToken = default)
    {
        // Find or create guardian/user account
        var guardian = await _dbContext.Guardians
            .FirstOrDefaultAsync(g => g.Email == email, cancellationToken);

        if (guardian == null)
        {
            // Create new guardian account
            // Note: In production, you'd want to create a Family first
            var family = new Family
            {
                Id = Guid.NewGuid(),
                SubscriptionTier = SubscriptionTier.Free,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.Families.Add(family);
            await _dbContext.SaveChangesAsync(cancellationToken);

            guardian = new Guardian
            {
                Id = Guid.NewGuid(),
                FamilyId = family.Id,
                Email = email,
                Name = name,
                Role = GuardianRole.Primary,
                AuthProviderId = $"{provider}:{email}",
                EmailVerified = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.Guardians.Add(guardian);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new guardian account for {Email} via {Provider}", email, provider);
        }
        else
        {
            // Update last login
            guardian.LastLoginAt = DateTimeOffset.UtcNow;
            guardian.UpdatedAt = DateTimeOffset.UtcNow;
            if (string.IsNullOrEmpty(guardian.AuthProviderId))
            {
                guardian.AuthProviderId = $"{provider}:{email}";
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Generate JWT token
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
        var issuer = jwtSettings["Issuer"] ?? "TeachingAssistant";
        var audience = jwtSettings["Audience"] ?? "TeachingAssistantUsers";
        var expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes", 60);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, guardian.Id.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim("family_id", guardian.FamilyId.ToString()),
            new Claim("provider", provider)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

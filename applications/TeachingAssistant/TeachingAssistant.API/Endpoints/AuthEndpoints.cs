using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using AspNet.Security.OAuth.GitHub;
using Microsoft.IdentityModel.Tokens;
using TeachingAssistant.API.Services;
using TeachingAssistant.Data;

namespace TeachingAssistant.API.Endpoints;

/// <summary>
/// Authentication endpoints for OAuth and JWT.
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // OAuth login endpoints
        group.MapGet("/login/google", () => Results.Challenge(
            new AuthenticationProperties { RedirectUri = "/api/auth/google-callback" },
            new[] { GoogleDefaults.AuthenticationScheme }))
            .WithName("LoginGoogle")
            .WithSummary("Initiate Google OAuth login")
            .AllowAnonymous();

        group.MapGet("/login/microsoft", () => Results.Challenge(
            new AuthenticationProperties { RedirectUri = "/api/auth/microsoft-callback" },
            new[] { MicrosoftAccountDefaults.AuthenticationScheme }))
            .WithName("LoginMicrosoft")
            .WithSummary("Initiate Microsoft OAuth login")
            .AllowAnonymous();

        group.MapGet("/login/github", () => Results.Challenge(
            new AuthenticationProperties { RedirectUri = "/api/auth/github-callback" },
            new[] { GitHubAuthenticationDefaults.AuthenticationScheme }))
            .WithName("LoginGitHub")
            .WithSummary("Initiate GitHub OAuth login")
            .AllowAnonymous();

        // OAuth callback endpoints
        group.MapGet("/google-callback", async (HttpContext context, IAuthService authService) =>
        {
            var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            var claims = result.Principal?.Claims.ToList() ?? new List<Claim>();
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "name")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Results.BadRequest("Email claim not found");
            }

            // Create or get user, generate JWT
            var jwtToken = await authService.AuthenticateOAuthUserAsync(email, name ?? "User", "Google");
            
            // Redirect to frontend with token
            return Results.Redirect($"/auth/callback?token={jwtToken}&provider=google");
        })
        .WithName("GoogleCallback")
        .AllowAnonymous();

        group.MapGet("/microsoft-callback", async (HttpContext context, IAuthService authService) =>
        {
            var result = await context.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            var claims = result.Principal?.Claims.ToList() ?? new List<Claim>();
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "name")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Results.BadRequest("Email claim not found");
            }

            // Create or get user, generate JWT
            var jwtToken = await authService.AuthenticateOAuthUserAsync(email, name ?? "User", "Microsoft");
            
            // Redirect to frontend with token
            return Results.Redirect($"/auth/callback?token={jwtToken}&provider=microsoft");
        })
        .WithName("MicrosoftCallback")
        .AllowAnonymous();

        group.MapGet("/github-callback", async (HttpContext context, IAuthService authService) =>
        {
            var result = await context.AuthenticateAsync(GitHubAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Results.Unauthorized();
            }

            var claims = result.Principal?.Claims.ToList() ?? new List<Claim>();
            // GitHub provides email in different claim types
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "email")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "urn:github:email")?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? claims.FirstOrDefault(c => c.Type == "name")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "urn:github:name")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "login")?.Value; // GitHub username fallback

            // GitHub may not always provide email if it's private
            // Use username@github.local as fallback if email not available
            if (string.IsNullOrEmpty(email))
            {
                var username = claims.FirstOrDefault(c => c.Type == "login")?.Value
                    ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(username))
                {
                    email = $"{username}@github.local";
                }
                else
                {
                    return Results.BadRequest("Email or username claim not found");
                }
            }

            // Create or get user, generate JWT
            var jwtToken = await authService.AuthenticateOAuthUserAsync(email, name ?? "GitHub User", "GitHub");
            
            // Redirect to frontend with token
            return Results.Redirect($"/auth/callback?token={jwtToken}&provider=github");
        })
        .WithName("GitHubCallback")
        .AllowAnonymous();

        // Logout endpoint
        group.MapPost("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { message = "Logged out successfully" });
        })
        .WithName("Logout")
        .RequireAuthorization();

        // Get current user info
        group.MapGet("/me", (HttpContext context) =>
        {
            var user = context.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                email = user.FindFirst(ClaimTypes.Email)?.Value,
                name = user.FindFirst(ClaimTypes.Name)?.Value,
                claims = user.Claims.Select(c => new { c.Type, c.Value })
            });
        })
        .WithName("GetCurrentUser")
        .RequireAuthorization();
    }
}

using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using AspNet.Security.OAuth.GitHub;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TeachingAssistant.Data;
using TeachingAssistant.API.Endpoints;
using TeachingAssistant.API.Services;
using TeachingAssistant.API.Hubs;
using DotNetAgents.Education;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file if it exists (for local development)
// Note: ASP.NET Core doesn't natively support .env files, so set environment variables
// or use appsettings.Development.json instead
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Teaching Assistant API",
        Version = "v1",
        Description = "API for K-12 Science Education AI Platform"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TeachingAssistantDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register education services
builder.Services.AddDotNetAgentsEducation();

// Configure Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
var googleSettings = builder.Configuration.GetSection("Authentication:Google");
var microsoftSettings = builder.Configuration.GetSection("Authentication:Microsoft");
var githubSettings = builder.Configuration.GetSection("Authentication:GitHub");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(jwtSettings.GetValue<int>("ExpirationMinutes", 60));
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "TeachingAssistant",
        ValidAudience = jwtSettings["Audience"] ?? "TeachingAssistantUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    if (!string.IsNullOrEmpty(googleSettings["ClientId"]) && !string.IsNullOrEmpty(googleSettings["ClientSecret"]))
    {
        options.ClientId = googleSettings["ClientId"]!;
        options.ClientSecret = googleSettings["ClientSecret"]!;
        options.CallbackPath = "/api/auth/google-callback";
        options.SaveTokens = true;
    }
})
.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
{
    if (!string.IsNullOrEmpty(microsoftSettings["ClientId"]) && !string.IsNullOrEmpty(microsoftSettings["ClientSecret"]))
    {
        options.ClientId = microsoftSettings["ClientId"]!;
        options.ClientSecret = microsoftSettings["ClientSecret"]!;
        options.CallbackPath = "/api/auth/microsoft-callback";
        options.SaveTokens = true;
    }
})
.AddGitHub(GitHubAuthenticationDefaults.AuthenticationScheme, options =>
{
    if (!string.IsNullOrEmpty(githubSettings["ClientId"]) && !string.IsNullOrEmpty(githubSettings["ClientSecret"]))
    {
        options.ClientId = githubSettings["ClientId"]!;
        options.ClientSecret = githubSettings["ClientSecret"]!;
        options.CallbackPath = "/api/auth/github-callback";
        options.SaveTokens = true;
    }
});

builder.Services.AddAuthorization();

// Add SignalR
builder.Services.AddSignalR();

// Register application services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5000" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map API endpoints
app.MapAuthEndpoints();
app.MapStudentEndpoints();
app.MapProgressEndpoints();
app.MapAssessmentEndpoints();
app.MapWorkflowEndpoints();

// Map SignalR hubs
app.MapHub<TutorHub>("/hubs/tutor");
app.MapHub<ParentHub>("/hubs/parent");
app.MapHub<AdminHub>("/hubs/admin");

app.Run();

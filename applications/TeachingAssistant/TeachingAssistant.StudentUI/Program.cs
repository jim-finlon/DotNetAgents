using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TeachingAssistant.StudentUI;
using TeachingAssistant.StudentUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API base URL
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseUrl) 
});

// Register services
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped(sp =>
{
    var hubUrl = $"{apiBaseUrl}/hubs/tutor";
    return new TutorHubService(hubUrl, sp.GetRequiredService<ILogger<TutorHubService>>());
});

await builder.Build().RunAsync();

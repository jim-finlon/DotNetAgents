using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Google;

public class GoogleProviderPlugin : PluginBase
{
    public GoogleProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-google",
            Name = "Google Gemini Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides Google Gemini integration. Supports Gemini Pro and other Google AI models.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "google", "gemini", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Google Gemini Provider plugin initialized. Use AddGoogleGemini() to configure.");
        return Task.CompletedTask;
    }
}

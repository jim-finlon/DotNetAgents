using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Groq;

public class GroqProviderPlugin : PluginBase
{
    public GroqProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-groq",
            Name = "Groq Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides Groq integration. Supports fast inference with Llama, Mixtral, and other models.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "groq", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Groq Provider plugin initialized. Use AddGroq() to configure.");
        return Task.CompletedTask;
    }
}

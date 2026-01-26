using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Mistral;

public class MistralProviderPlugin : PluginBase
{
    public MistralProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-mistral",
            Name = "Mistral Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides Mistral AI integration. Supports Mistral 7B, Mixtral, and other Mistral models.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "mistral", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Mistral Provider plugin initialized. Use AddMistral() to configure.");
        return Task.CompletedTask;
    }
}

using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Cohere;

public class CohereProviderPlugin : PluginBase
{
    public CohereProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-cohere",
            Name = "Cohere Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides Cohere LLM integration. Supports Command, Command-Light, and other Cohere models.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "cohere", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Cohere Provider plugin initialized. Use AddCohere() to configure.");
        return Task.CompletedTask;
    }
}

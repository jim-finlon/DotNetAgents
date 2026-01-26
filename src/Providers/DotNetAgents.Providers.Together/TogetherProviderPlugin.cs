using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Together;

public class TogetherProviderPlugin : PluginBase
{
    public TogetherProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-together",
            Name = "Together Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides Together AI integration. Supports various open-source models via Together's platform.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "together", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Together Provider plugin initialized. Use AddTogether() to configure.");
        return Task.CompletedTask;
    }
}

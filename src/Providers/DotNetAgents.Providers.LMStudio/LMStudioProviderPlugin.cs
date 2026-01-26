using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.LMStudio;

public class LMStudioProviderPlugin : PluginBase
{
    public LMStudioProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-lmstudio",
            Name = "LM Studio Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides LM Studio local LLM integration. Supports running models via LM Studio server.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "lmstudio", "local", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("LM Studio Provider plugin initialized. Use AddLMStudio() to configure.");
        return Task.CompletedTask;
    }
}

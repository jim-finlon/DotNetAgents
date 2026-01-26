using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Azure;

public class AzureProviderPlugin : PluginBase
{
    public AzureProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-azure",
            Name = "Azure OpenAI Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides Azure OpenAI Service integration. Supports GPT models through Azure cloud infrastructure.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "azure", "openai", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Azure OpenAI Provider plugin initialized. Use AddAzureOpenAI() to configure.");
        return Task.CompletedTask;
    }
}

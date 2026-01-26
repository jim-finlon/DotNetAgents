using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Anthropic;

/// <summary>
/// Plugin for Anthropic provider.
/// </summary>
public class AnthropicProviderPlugin : PluginBase
{
    public AnthropicProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-anthropic",
            Name = "Anthropic Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides Anthropic Claude LLM integration. Supports Claude 3 Sonnet, Opus, and Haiku models.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "anthropic", "claude", "provider", "infrastructure" },
            Dependencies = new List<string>(),
            RepositoryUrl = "https://github.com/dotnetagents/DotNetAgents"
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Anthropic Provider plugin initialized. Use AddAnthropic() to configure.");
        return Task.CompletedTask;
    }
}

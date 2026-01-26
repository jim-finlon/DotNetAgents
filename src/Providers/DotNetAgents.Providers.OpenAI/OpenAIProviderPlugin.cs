using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.OpenAI;

/// <summary>
/// Plugin for OpenAI provider.
/// </summary>
public class OpenAIProviderPlugin : PluginBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAIProviderPlugin"/> class.
    /// </summary>
    public OpenAIProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-openai",
            Name = "OpenAI Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides OpenAI LLM integration. Supports GPT-3.5, GPT-4, GPT-4 Turbo, and other OpenAI models.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "openai", "gpt", "provider", "infrastructure" },
            Dependencies = new List<string>(),
            RepositoryUrl = "https://github.com/dotnetagents/DotNetAgents",
            DocumentationUrl = "https://github.com/dotnetagents/DotNetAgents/docs/guides/providers.md"
        };
    }

    /// <inheritdoc />
    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation(
            "OpenAI Provider plugin initialized. Use AddOpenAI() to configure.");

        return Task.CompletedTask;
    }
}

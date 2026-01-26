using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.Ollama;

public class OllamaProviderPlugin : PluginBase
{
    public OllamaProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-ollama",
            Name = "Ollama Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides Ollama local LLM integration. Supports running models locally (Llama, Mistral, etc.).",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "ollama", "local", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("Ollama Provider plugin initialized. Use AddOllama() to configure.");
        return Task.CompletedTask;
    }
}

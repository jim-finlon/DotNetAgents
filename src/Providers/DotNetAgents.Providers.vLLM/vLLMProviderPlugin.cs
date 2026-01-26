using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.vLLM;

public class vLLMProviderPlugin : PluginBase
{
    public vLLMProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-vllm",
            Name = "vLLM Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides vLLM integration. Supports high-performance local inference with various models.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "vllm", "local", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("vLLM Provider plugin initialized. Use AddvLLM() to configure.");
        return Task.CompletedTask;
    }
}

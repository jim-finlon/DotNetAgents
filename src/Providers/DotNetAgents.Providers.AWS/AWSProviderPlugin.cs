using DotNetAgents.Ecosystem;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Providers.AWS;

public class AWSProviderPlugin : PluginBase
{
    public AWSProviderPlugin()
    {
        Metadata = new PluginMetadata
        {
            Id = "provider-aws",
            Name = "AWS Bedrock Provider",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Provides AWS Bedrock integration. Supports Claude, Llama, and other models via AWS infrastructure.",
            Author = "DotNetAgents",
            License = "MIT",
            Category = "Infrastructure",
            Tags = new List<string> { "llm", "aws", "bedrock", "provider", "infrastructure" },
            Dependencies = new List<string>()
        };
    }

    protected override Task OnInitializeAsync(IPluginContext context, CancellationToken cancellationToken)
    {
        Logger?.LogInformation("AWS Bedrock Provider plugin initialized. Use AddAWSBedrock() to configure.");
        return Task.CompletedTask;
    }
}

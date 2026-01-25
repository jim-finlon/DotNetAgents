using DotNetAgents.Mcp.Routing;
using DotNetAgents.Voice.IntentClassification;
using DotNetAgents.Voice.Parsing;

namespace DotNetAgents.Samples.JARVISVoice;

/// <summary>
/// Mock services for demonstration purposes.
/// </summary>
internal static class MockServices
{

    /// <summary>
    /// Mock MCP adapter router for demonstration purposes.
    /// </summary>
    internal class MockMcpAdapterRouter : IMcpAdapterRouter
    {
        public Task<object?> ExecuteIntentAsync(Intent intent, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object?>($"Mock execution result for {intent.FullName}");
        }
    }

    /// <summary>
    /// Mock command parser for demonstration purposes.
    /// </summary>
    internal class MockCommandParser : ICommandParser
    {
        public Task<Intent> ParseAsync(string rawText, IntentContext? context = null, CancellationToken cancellationToken = default)
        {
            // Create a mock intent based on the command text
            var intent = new Intent
            {
                Domain = rawText.Contains("note") ? "notes" : "unknown",
                Action = rawText.Contains("create") ? "create" : "unknown",
                Confidence = rawText.Length > 10 ? 0.85 : 0.4,
                Parameters = new Dictionary<string, object>(),
                MissingRequired = new List<string>()
            };

            return Task.FromResult(intent);
        }
    }
}

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace DotNetAgents.Observability.Tracing;

/// <summary>
/// Extension methods for OpenTelemetry integration.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Creates a tracer provider builder with DotNetAgents instrumentation.
    /// </summary>
    /// <param name="builder">The tracer provider builder.</param>
    /// <returns>The builder for method chaining.</returns>
    public static TracerProviderBuilder AddDotNetAgentsInstrumentation(this TracerProviderBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder.AddSource("DotNetAgents.Core")
                      .AddSource("DotNetAgents.Workflow")
                      .AddSource("DotNetAgents.Observability")
                      .AddSource("DotNetAgents.Agents");
    }

    /// <summary>
    /// Starts a new activity for an LLM call.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="model">The model name.</param>
    /// <param name="correlationId">Optional correlation ID.</param>
    /// <returns>The started activity, or null if tracing is disabled.</returns>
    public static Activity? StartLLMCallActivity(
        this ActivitySource activitySource,
        string model,
        string? correlationId = null)
    {
        if (activitySource == null)
            throw new ArgumentNullException(nameof(activitySource));

        var activity = activitySource.StartActivity("llm.call");
        if (activity != null)
        {
            activity.SetTag("llm.model", model);
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                activity.SetTag("correlation.id", correlationId);
            }
        }

        return activity;
    }

    /// <summary>
    /// Starts a new activity for a workflow execution.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="workflowName">The workflow name.</param>
    /// <param name="runId">The workflow run ID.</param>
    /// <param name="correlationId">Optional correlation ID.</param>
    /// <returns>The started activity, or null if tracing is disabled.</returns>
    public static Activity? StartWorkflowActivity(
        this ActivitySource activitySource,
        string workflowName,
        string runId,
        string? correlationId = null)
    {
        if (activitySource == null)
            throw new ArgumentNullException(nameof(activitySource));

        var activity = activitySource.StartActivity("workflow.execute");
        if (activity != null)
        {
            activity.SetTag("workflow.name", workflowName);
            activity.SetTag("workflow.run_id", runId);
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                activity.SetTag("correlation.id", correlationId);
            }
        }

        return activity;
    }

    /// <summary>
    /// Starts a new activity for a tool execution.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="toolName">The tool name.</param>
    /// <param name="correlationId">Optional correlation ID.</param>
    /// <returns>The started activity, or null if tracing is disabled.</returns>
    public static Activity? StartToolActivity(
        this ActivitySource activitySource,
        string toolName,
        string? correlationId = null)
    {
        if (activitySource == null)
            throw new ArgumentNullException(nameof(activitySource));

        var activity = activitySource.StartActivity("tool.execute");
        if (activity != null)
        {
            activity.SetTag("tool.name", toolName);
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                activity.SetTag("correlation.id", correlationId);
            }
        }

        return activity;
    }
}
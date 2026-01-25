using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TeachingAssistant.AI;

/// <summary>
/// Background service that periodically monitors model health.
/// </summary>
public class ModelHealthMonitor : BackgroundService
{
    private readonly IModelRouter _modelRouter;
    private readonly TimeSpan _checkInterval;
    private readonly ILogger<ModelHealthMonitor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelHealthMonitor"/> class.
    /// </summary>
    public ModelHealthMonitor(
        IModelRouter modelRouter,
        TimeSpan? checkInterval = null,
        ILogger<ModelHealthMonitor>? logger = null)
    {
        _modelRouter = modelRouter ?? throw new ArgumentNullException(nameof(modelRouter));
        _checkInterval = checkInterval ?? TimeSpan.FromMinutes(1);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _modelRouter.HealthCheckAsync(stoppingToken);
                _logger.LogDebug("Model health check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during model health check");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}

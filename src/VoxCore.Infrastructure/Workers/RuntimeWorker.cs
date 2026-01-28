using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VoxCore.Runtime.Contracts;


namespace VoxCore.Infrastructure.Workers;

public sealed class RuntimeWorker(IRuntimeEntry entry, ILogger<RuntimeWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await entry.RunAsync(stoppingToken);
            logger.LogInformation("Worker started, waiting indefinitely...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Worker stopping due to cancellation");
        }
    }
}
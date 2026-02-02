using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace VoxCore.Infrastructure.Services;

internal sealed class RabbitSubscription(
    IChannel channel,
    string consumerTag,
    ILogger logger
) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        try
        {
            await channel.BasicCancelAsync(consumerTag);
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to dispose RabbitMQ subscription cleanly");
        }
    }
}

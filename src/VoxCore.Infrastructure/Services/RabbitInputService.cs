using Microsoft.Extensions.Logging;
using VoxCore.Infrastructure.Contracts;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services;

public sealed class RabbitInputService : IInputService, IAsyncDisposable
{
    public event EventHandler<IInputService.InputData>? InputReceived;

    private readonly Task<IAsyncDisposable> _subscription;

    public RabbitInputService(IRabbitBus bus, ILogger<RabbitInputService> logger)
    {
        _subscription = bus.SubscribeAsync<TextDTO>(
            "raw_text", $"raw_text.input.*",
            async (ex, data, ct) => 
            {
                logger.LogDebug("Data: {@data}", data);
                InputReceived?.Invoke(this,
                    new IInputService.InputData(
                        data.Text ?? string.Empty
                    ));
            });
    }

    public async ValueTask DisposeAsync()
    {
        if (_subscription != null)
        {
            var s = await _subscription;
            if (s != null)
                await s.DisposeAsync();
        }

    }
}
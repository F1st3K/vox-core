using VoxCore.Infrastructure.Contracts;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services;

public sealed class RabbitInputService : IInputService
{
    public event EventHandler<IInputService.Args>? InputReceived;

    // public RabbitInputService(IRabbitBus bus)
    // {
    //     bus.SubscribeAsync(
    //         "raw_text", $"raw_text.input.*",
    //         async (ex, d, ct) => InputReceived?.Invoke(this,
    //             new IInputService.Args(
    //                 ex?.Split('.')?.LastOrDefault() ?? string.Empty,
    //                 d?.text
    //             )
    //         )
    //     );
    // }
}
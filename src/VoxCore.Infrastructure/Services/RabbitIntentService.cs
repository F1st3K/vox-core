using VoxCore.Infrastructure.Contracts;
using VoxCore.Plugins.Contracts;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services;

public sealed class RabbitIntentService(IRabbitBus bus) : IIntentService
{
    public async Task ConfigureAsync(IEnumerable<IIntentDeclaration> declaredIntents, CancellationToken ct)
    {
        // var body = declaredIntents.ToArray();
        // await bus.PublishAsync("intents", $"intents.config", body, ct: ct);
    }

    public async Task<IIntentService.Intent?> DecodeAsync(string rawText, CancellationToken ct)
    {
        var body  = new { text = rawText };
        var result = await bus.RpcAsync("intents", $"intents.decode", body, ct: ct);
        return result.text;
    }
}
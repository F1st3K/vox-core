using Microsoft.Extensions.Logging;
using VoxCore.Infrastructure.Contracts;
using VoxCore.Plugins.Contracts;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services;

public sealed class RabbitIntentService(
    IRabbitBus bus,
    ILogger<RabbitIntentService> logger
) : IIntentService
{
    public async Task ConfigureAsync(IEnumerable<IIntentDeclaration> declaredIntents, CancellationToken ct)
    {
        var body = declaredIntents.ToArray();
        await bus.PublishAsync("intents", $"intents.config", body, ct);
    }

    public async Task<IIntentService.Intent?> DecodeAsync(Guid sessionId, string rawText, CancellationToken ct)
    {
        var body  = new { session_id = sessionId, text = rawText };
        var result = new TaskCompletionSource<IIntentService.Intent?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var id = Guid.NewGuid();
        await using var sub = await bus.SubscribeAsync<RasaDTO>(
            "intents",
            $"intents.response.{id}",
            async (_, data, ct) =>
            {
                logger.LogDebug("Data: {@data}", data);
                if (string.IsNullOrEmpty(data.Intent?.Name))
                {
                    result.SetResult(null);
                    return;
                }
                var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var e in data.Entities ?? [])
                    if (e != null && e.Entity != null && e.Value != null)
                        parameters[e.Entity] = e.Value;


                result.SetResult(new IIntentService.Intent(data.Intent.Name, parameters));
            },
            ct
        );
        await bus.PublishAsync("intents", $"intents.request.{id}", body, ct);
        return await result.Task;
    }
}
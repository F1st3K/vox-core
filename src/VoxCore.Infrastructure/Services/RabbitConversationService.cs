using Microsoft.Extensions.Logging;
using VoxCore.Infrastructure.Contracts;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services;

public sealed class RabbitConversationService(
    IRabbitBus bus
) : IConversationService
{
    public async Task<string> AskAsync(Guid sessionId, string request, CancellationToken ct)
    {
        var body  = new { session_id = sessionId, text = request };
        var result = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var id = Guid.NewGuid();
        await using var sub = await bus.SubscribeAsync<TextDTO>(
            "raw_text",
            $"raw_text.response.{id}",
            async (_, data, ct) =>
            {
                result.SetResult(data.Text ?? string.Empty);
            },
            ct
        );
        await bus.PublishAsync("speech", $"speech.ask.{id}", body, ct);
        return await result.Task;
    }

    public async void Say(Guid sessionId, string message, CancellationToken ct)
    {
        var body  = new { session_id = sessionId, text = message };
        await bus.PublishAsync("speech", $"speech.say.voice", body, ct: ct);
    }
}
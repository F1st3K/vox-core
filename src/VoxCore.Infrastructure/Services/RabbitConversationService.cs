using VoxCore.Infrastructure.Contracts;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Infrastructure.Services;

public sealed class RabbitConversationService(IRabbitBus bus) : IConversationService
{
    public async Task<string> AskAsync(Guid sessionId, string source, string request, CancellationToken ct)
    {
        var body  = new { session_id = sessionId, text = request };
        var result = await bus.RpcAsync("speech", $"speech.ask.{source}", body, ct: ct);
        return result.text;
    }

    public async void Say(Guid sessionId, string source, string message, CancellationToken ct)
    {
        var body  = new { session_id = sessionId, text = message };
        await bus.PublishAsync("speech", $"speech.say.{source}", body, ct: ct);
    }
}
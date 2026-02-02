namespace VoxCore.Runtime.Contracts;

public interface IConversationService
{
    void Say(Guid sessionId, string message, CancellationToken ct);
    Task<string> AskAsync(Guid sessionId, string request, CancellationToken ct);
}
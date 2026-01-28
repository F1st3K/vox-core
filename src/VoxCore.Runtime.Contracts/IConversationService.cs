namespace VoxCore.Runtime.Contracts;

public interface IConversationService
{
    void Say(Guid sessionId, string source, string message, CancellationToken ct);
    Task<string> AskAsync(Guid sessionId, string source, string request, CancellationToken ct);
}
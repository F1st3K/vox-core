using VoxCore.Plugins.Contracts.Services;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Runtime.Services;

internal sealed class CurrentDialog : ICurrentDialog
{
    private readonly IConversationService _sessions;
    private readonly CancellationToken _ct;

    public Guid SessionId { get; }

    public CurrentDialog(IConversationService sessions, Guid sessionId, CancellationToken ct)
    {
        _sessions = sessions;
        _ct = ct;
        SessionId = sessionId;
    }

    public void Say(string text)
        => _sessions.Say(SessionId, text, _ct);

    public Task<string> AskAsync(string text)
        => _sessions.AskAsync(SessionId, text, _ct);
}
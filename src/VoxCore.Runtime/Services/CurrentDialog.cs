using VoxCore.Plugins.Contracts.Services;
using VoxCore.Runtime.Contracts;

namespace VoxCore.Runtime.Services;

internal sealed class CurrentDialog : ICurrentDialog
{
    private readonly IConversationService _sessions;
    private readonly CancellationToken _ct;

    public Guid SessionId { get; }
    public string Source { get; }

    public CurrentDialog(IConversationService sessions, Guid sessionId, string source, CancellationToken ct)
    {
        _sessions = sessions;
        _ct = ct;
        SessionId = sessionId;
        Source = source;
    }

    public void Say(string text)
        => _sessions.Say(SessionId, Source, text, _ct);

    public Task<string> AskAsync(string text)
        => _sessions.AskAsync(SessionId, Source, text, _ct);
}
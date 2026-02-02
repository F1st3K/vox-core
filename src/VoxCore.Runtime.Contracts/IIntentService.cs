using VoxCore.Plugins.Contracts;

namespace VoxCore.Runtime.Contracts;

public interface IIntentService
{
    public sealed record Intent(string Name, IDictionary<string, object> Parameters);

    Task<Intent?> DecodeAsync(Guid sessionId, string rawText, CancellationToken ct);

    Task ConfigureAsync(IEnumerable<IIntentDeclaration> declaredIntents, CancellationToken ct);
}
using VoxCore.Plugins.Contracts;

namespace VoxCore.Runtime.Contracts;

public interface IIntentService
{
    public sealed record Intent(string Name, object Parameters);

    Task<Intent?> DecodeAsync(string rawText, CancellationToken ct);

    Task ConfigureAsync(IEnumerable<IIntentDeclaration> declaredIntents, CancellationToken ct);
}
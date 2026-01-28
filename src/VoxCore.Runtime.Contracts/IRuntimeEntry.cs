namespace VoxCore.Runtime.Contracts;

public interface IRuntimeEntry
{
    Task RunAsync(CancellationToken ct);
}
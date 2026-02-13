namespace VoxCore.Runtime.Contracts;

public interface ITextNormalaizer
{
    Task<string> NormalizeAsync(string text, CancellationToken ct);
}
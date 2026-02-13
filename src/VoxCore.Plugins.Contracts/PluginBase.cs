namespace VoxCore.Plugins.Contracts;

public abstract class PluginBase<TIntent, TParams> : IPlugin<TIntent, TParams>
    where TIntent : IIntentDeclaration, new()
    where TParams : class, new()
{
    public Type ParametersType => typeof(TParams);

    public bool IsEnabled { get; set; } = true;

    public abstract TIntent Intent { get; }

    public abstract Task ExecuteAsync(
        TParams parameters,
        CancellationToken ct
    );

    async Task IPlugin.ExecuteAsync(
        object parameters,
        CancellationToken ct)
    {
        if (parameters is not TParams typed)
            throw new InvalidOperationException(
                $"Invalid parameters type. Expected {typeof(TParams).Name}"
            );

        await ExecuteAsync(typed, ct);
    }
}
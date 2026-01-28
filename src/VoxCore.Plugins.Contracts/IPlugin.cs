namespace VoxCore.Plugins.Contracts;

public interface IPlugin
{
    Task ExecuteAsync(object parameters, CancellationToken ct);
}

public interface IIntentDeclaration
{
    string Name { get; }
    IEnumerable<string> Examples { get; }
}

public interface IPlugin<TIntent> : IPlugin
    where TIntent : IIntentDeclaration, new()
{
    TIntent Intent { get; }
}

public interface IPlugin<TIntent, TParams> : IPlugin<TIntent>, IPlugin
    where TIntent : IIntentDeclaration, new()
    where TParams : class
{
    Task ExecuteAsync(TParams parameters, CancellationToken ct);
}